using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Define game states to control gameplay flow.
    public enum GameState { SceneIntro, SeedSelect, PreGameSetup, Playing }

    [Header("Game State Settings")]
    public GameState currentState = GameState.SceneIntro;
    public PlantingController plantingController;
    public RectTransform selectorIcon;

    [Header("Camera Settings")]
    public Transform mainCamera;              // Reference to the camera transform.
    public float sceneIntroDelay = 2f;          // Time to wait for the scene/lawn to load.
    public float seedSelectCameraX = 5f;        // X coordinate the camera should move to during seed selection.
    public float cameraMoveDuration = 1.5f;     // Duration (in seconds) for the main camera movement.
    public float finalSmoothingDuration = 0.1f; // Extra smoothing time at the end of camera movement.
    private Vector3 initialCameraPosition;    // The camera’s starting position.

    [Header("Selected Seeds UI Transition Settings")]
    public RectTransform selectedSeedsUI;     // The UI element representing the selected seeds.
    public Vector3 selectedSeedsStartPos;     // The starting local position of the selected seeds UI.
    public Vector3 selectedSeedsTargetPos;    // The target local position of the selected seeds UI.
    public float selectedSeedsMoveDuration = 1.0f;         // Duration (in seconds) for the selected seeds UI move phase.
    public float selectedSeedsFinalSmoothingDuration = 0.1f; // Extra smoothing time for the selected seeds UI movement.

    [Header("Available Seeds UI Transition Settings")]
    public RectTransform availableSeedsUI;    // The UI element representing the available seeds.
    public Vector3 availableSeedsStartPos;    // The starting local position of the available seeds UI.
    public Vector3 availableSeedsTargetPos;   // The target local position of the available seeds UI.
    public float availableSeedsMoveDuration = 1.0f;         // Duration (in seconds) for the available seeds UI move phase.
    public float availableSeedsFinalSmoothingDuration = 0.1f; // Extra smoothing time for the available seeds UI movement.

    [Header("Seed Instantiation Settings")]
    public Seed[] seeds;                // Array of Seed ScriptableObjects to assign to each seed packet.
    public GameObject seedPacketPrefab; // Seed packet prefab to instantiate (should have the SeedPacket component).

    [Header("Seed Packet Selection Settings")]
    public float seedPacketMoveDuration = 1.0f; // Duration for seed packet move animation.

    [Header("Other References")]
    public MoveCam moveCamScript;             // Reference to the MoveCam script to enable during gameplay.

    [Header("Seed Movement Control")]
    // This flag locks seed movement after the seed selection is confirmed.
    public bool seedMovementLocked = false;

    // Singleton instance for easy access from SeedPacket clicks.
    public static GameController Instance;

    // HashSet to reserve selected slots during the animation phase.
    private HashSet<Transform> reservedSelectedSlots = new HashSet<Transform>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // If not explicitly assigned, use Camera.main.
        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }

        // Save the initial camera position.
        initialCameraPosition = mainCamera.position;

        // Instantiate seed packets inside the available seeds UI.
        InstantiateSeedPackets();

        // Start in SceneIntro state and disable MoveCam until gameplay starts.
        currentState = GameState.SceneIntro;
        if (moveCamScript != null)
            moveCamScript.enabled = false;

        // Begin the scene intro sequence.
        StartCoroutine(SceneIntroSequence());
    }

    /// <summary>
    /// Searches for child objects with the tag "availableSeedSlot" under the available seeds UI.
    /// For each slot found (up to the number of seeds provided), instantiate a seed packet prefab,
    /// set its SeedPacket component's seed variable, parent it to the slot, and reset its local position.
    /// </summary>
    private void InstantiateSeedPackets()
    {
        if (availableSeedsUI == null)
        {
            Debug.LogWarning("AvailableSeedsUI is not assigned in the Inspector.");
            return;
        }

        // Find all direct child transforms of availableSeedsUI that have the tag "availableSeedSlot".
        List<Transform> availableSeedSlots = new List<Transform>();
        foreach (Transform child in availableSeedsUI)
        {
            if (child.CompareTag("availableSeedSlot"))
            {
                availableSeedSlots.Add(child);
            }
        }

        if (availableSeedSlots.Count == 0)
        {
            Debug.LogWarning("No children with tag 'availableSeedSlot' were found under the available seeds UI.");
            return;
        }

        // Instantiate a seed packet for each seed up to the number of available slots.
        int numSeeds = seeds != null ? seeds.Length : 0;
        int count = Mathf.Min(numSeeds, availableSeedSlots.Count);
        for (int i = 0; i < count; i++)
        {
            Transform slot = availableSeedSlots[i];
            GameObject newSeedPacket = Instantiate(seedPacketPrefab, slot);
            newSeedPacket.transform.localPosition = Vector3.zero;

            SeedPacket sp = newSeedPacket.GetComponent<SeedPacket>();
            if (sp != null)
            {
                sp.seed = seeds[i];
            }
            else
            {
                Debug.LogError("The instantiated seed packet prefab does not have a SeedPacket component attached.");
            }
        }
    }

    /// <summary>
    /// Wait for the scene to load, then start moving the camera, the selected seeds UI,
    /// and the available seeds UI concurrently. Once all are finished, transition the game state to SeedSelect.
    /// </summary>
    IEnumerator SceneIntroSequence()
    {
        yield return new WaitForSeconds(sceneIntroDelay);

        bool cameraFinished = false;
        bool selectedUIFinished = false;
        bool availableUIFinished = false;

        // Start the camera movement.
        StartCoroutine(MoveCameraTo(seedSelectCameraX, () => { cameraFinished = true; }));

        // Start moving the selected seeds UI.
        if (selectedSeedsUI != null)
        {
            StartCoroutine(MoveUITo(selectedSeedsUI, selectedSeedsStartPos, selectedSeedsTargetPos,
                                    selectedSeedsMoveDuration, selectedSeedsFinalSmoothingDuration, () => { selectedUIFinished = true; }));
        }
        else
        {
            selectedUIFinished = true;
        }

        // Start moving the available seeds UI.
        if (availableSeedsUI != null)
        {
            StartCoroutine(MoveUITo(availableSeedsUI, availableSeedsStartPos, availableSeedsTargetPos,
                                    availableSeedsMoveDuration, availableSeedsFinalSmoothingDuration, () => { availableUIFinished = true; }));
        }
        else
        {
            availableUIFinished = true;
        }

        // Wait until camera, selected seeds UI, and available seeds UI transitions are complete.
        yield return new WaitUntil(() => cameraFinished && selectedUIFinished && availableUIFinished);

        // Transition the game state to SeedSelect.
        currentState = GameState.SeedSelect;
    }

    /// <summary>
    /// Public method called by seed packets when clicked. Determines whether to select or deselect a seed packet.
    /// </summary>
    /// <param name="packet">The SeedPacket that was clicked.</param>
    public void SeedPacketClicked(SeedPacket packet)
    {
        if (packet.isAnimating)
            return;

        // If seed movement is locked, only process clicks that are intended for planting/other functions.
        if (seedMovementLocked)
        {
            if (packet.isSelected)
            {
                plantingController.SetSelectedSeed(packet);
                selectorIcon.gameObject.SetActive(true);
                selectorIcon.position = packet.GetComponent<RectTransform>().position;
            }
            return;
        }

        if (!packet.isSelected)
        {
            Transform targetSlot = GetNextAvailableSelectedSlot();
            if (targetSlot == null)
            {
                Debug.Log("No available selected seed slot.");
                return;
            }
            // Reserve this slot immediately so that other fast clicks cannot choose it.
            reservedSelectedSlots.Add(targetSlot);

            packet.isAnimating = true;
            StartCoroutine(AnimateSeedPacketToSlot(packet.GetComponent<RectTransform>(), targetSlot, seedPacketMoveDuration, () =>
            {
                packet.isSelected = true;
                packet.isAnimating = false;
                reservedSelectedSlots.Remove(targetSlot);

                // Record the original slot (if not already recorded).
                if (packet.originalSlot == null)
                    packet.originalSlot = packet.originalParent;
            }));
        }
        else // Handling deselection.
        {
            // Immediately free this seed packet's slot and trigger a reorganization
            // without waiting for the returning animation.
            Transform currentSlot = packet.transform.parent;
            reservedSelectedSlots.Add(currentSlot);

            // Detach the packet from the selected seeds UI so that the slot becomes empty.
            RectTransform packetRect = packet.GetComponent<RectTransform>();
            // Move the packet to the canvas's parent to keep it visible.
            packetRect.SetParent(selectedSeedsUI.transform.parent, worldPositionStays: true);

            // Immediately mark as deselected so that other seeds reflow.
            packet.isSelected = false;
            reservedSelectedSlots.Remove(currentSlot);

            // Instantly reposition any remaining selected seed packets.
            RepositionSelectedSeedsInstant();

            // Now animate the returning seed packet to its original slot.
            packet.isAnimating = true;
            StartCoroutine(AnimateSeedPacketToSlot(packetRect, packet.originalSlot, seedPacketMoveDuration, () =>
            {
                packet.isAnimating = false;
            }));
        }
    }

    /// <summary>
    /// Searches the selectedSeedsUI for child objects (with tag "selectedSeedSlot") that are empty.
    /// Returns the one with the highest y coordinate (largest localPosition.y) if available.
    /// </summary>
    /// <returns>The Transform of the next available selected seed slot, or null if none available.</returns>
    private Transform GetNextAvailableSelectedSlot()
    {
        List<Transform> availableSlots = new List<Transform>();
        foreach (Transform child in selectedSeedsUI)
        {
            if (child.CompareTag("selectedSeedSlot") && child.childCount == 0 && !reservedSelectedSlots.Contains(child))
                availableSlots.Add(child);
        }
        if (availableSlots.Count > 0)
        {
            availableSlots.Sort((a, b) => b.localPosition.y.CompareTo(a.localPosition.y));
            return availableSlots[0];
        }
        return null;
    }

    /// <summary>
    /// Animates a seed packet's RectTransform from its current world position to the destination slot's position.
    /// Once the movement is complete, the seed packet is reparented to the destination slot.
    /// </summary>
    /// <param name="seedPacketRect">The RectTransform of the seed packet.</param>
    /// <param name="destinationSlot">The target slot's Transform.</param>
    /// <param name="duration">Animation duration.</param>
    /// <param name="onComplete">Callback upon completion.</param>
    IEnumerator AnimateSeedPacketToSlot(RectTransform seedPacketRect, Transform destinationSlot, float duration, System.Action onComplete)
    {
        // Temporarily set parent to the main canvas's parent to ensure visibility.
        seedPacketRect.SetParent(selectedSeedsUI.transform.parent, worldPositionStays: true);
        seedPacketRect.SetAsLastSibling();

        // Record the starting and ending world positions.
        Vector3 startWorldPos = seedPacketRect.position;
        Vector3 endWorldPos = destinationSlot.position;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeT = Mathf.SmoothStep(0f, 1f, t);
            seedPacketRect.position = Vector3.Lerp(startWorldPos, endWorldPos, easeT);
            yield return null;
        }

        // Re-parent to the destination slot once the animation is complete.
        seedPacketRect.SetParent(destinationSlot, false);
        seedPacketRect.localPosition = Vector3.zero;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Public function called via a button when the player confirms seed selection.
    /// Checks if every selected seed slot (child with tag "selectedSeedSlot") is filled.
    /// If not, a message is printed; otherwise, the game proceeds.
    /// </summary>
    public void UseSelectedSeed()
    {
        if (currentState == GameState.SeedSelect)
        {
            int requiredCount = 0;
            int selectedCount = 0;
            foreach (Transform child in selectedSeedsUI)
            {
                if (child.CompareTag("selectedSeedSlot"))
                {
                    requiredCount++;
                    if (child.childCount > 0)
                        selectedCount++;
                }
            }
            if (selectedCount < requiredCount)
            {
                Debug.Log("Not enough seeds selected!");
                return;
            }
            seedMovementLocked = true;
            currentState = GameState.PreGameSetup;
            StartCoroutine(PreGameSetupSequenceExtended());
        }
    }

    /// <summary>
    /// Moves the camera back to its initial x position and returns the available seeds UI to its original starting position.
    /// Once both transitions are complete, switches the game state to Playing and enables the MoveCam script.
    /// </summary>
    IEnumerator PreGameSetupSequenceExtended()
    {
        bool cameraFinished = false;
        bool availableUIFinished = false;

        StartCoroutine(MoveCameraTo(initialCameraPosition.x, () => { cameraFinished = true; }));

        if (availableSeedsUI != null)
        {
            StartCoroutine(MoveUITo(availableSeedsUI, availableSeedsTargetPos, availableSeedsStartPos,
                                    availableSeedsMoveDuration, availableSeedsFinalSmoothingDuration, () => { availableUIFinished = true; }));
        }
        else
        {
            availableUIFinished = true;
        }

        yield return new WaitUntil(() => cameraFinished && availableUIFinished);

        currentState = GameState.Playing;
        if (moveCamScript != null)
            moveCamScript.enabled = true;

        // Enable the planting controller (if it is attached and was disabled)
        if (plantingController != null)
        {
            plantingController.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Smoothly moves the camera along the x-axis to a target x coordinate using SmoothStep easing.
    /// After the main movement, an extra smoothing phase is applied.
    /// </summary>
    /// <param name="targetX">Target x coordinate.</param>
    /// <param name="onComplete">Callback when done.</param>
    IEnumerator MoveCameraTo(float targetX, System.Action onComplete)
    {
        Vector3 startPos = mainCamera.position;
        Vector3 targetPos = new Vector3(targetX, startPos.y, startPos.z);
        float elapsed = 0f;
        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cameraMoveDuration);
            float easeT = Mathf.SmoothStep(0f, 1f, t);
            mainCamera.position = Vector3.Lerp(startPos, targetPos, easeT);
            yield return null;
        }
        float smoothElapsed = 0f;
        Vector3 currentPos = mainCamera.position;
        while (smoothElapsed < finalSmoothingDuration)
        {
            smoothElapsed += Time.deltaTime;
            float tSmooth = Mathf.Clamp01(smoothElapsed / finalSmoothingDuration);
            mainCamera.position = Vector3.Lerp(currentPos, targetPos, tSmooth);
            yield return null;
        }
        mainCamera.position = targetPos;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Smoothly moves a UI element from a start position to a target position using SmoothStep easing.
    /// An extra final smoothing phase is applied once the main movement concludes.
    /// </summary>
    /// <param name="uiElement">The UI element's RectTransform.</param>
    /// <param name="startPos">Starting local position.</param>
    /// <param name="targetPos">Target local position.</param>
    /// <param name="moveDuration">Main move duration.</param>
    /// <param name="finalSmoothDuration">Extra smoothing duration.</param>
    /// <param name="onComplete">Callback when done.</param>
    IEnumerator MoveUITo(RectTransform uiElement, Vector3 startPos, Vector3 targetPos, float moveDuration, float finalSmoothDuration, System.Action onComplete)
    {
        uiElement.localPosition = startPos;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float easeT = Mathf.SmoothStep(0f, 1f, t);
            uiElement.localPosition = Vector3.Lerp(startPos, targetPos, easeT);
            yield return null;
        }
        float smoothElapsed = 0f;
        Vector3 currentPos = uiElement.localPosition;
        while (smoothElapsed < finalSmoothDuration)
        {
            smoothElapsed += Time.deltaTime;
            float tSmooth = Mathf.Clamp01(smoothElapsed / finalSmoothDuration);
            uiElement.localPosition = Vector3.Lerp(currentPos, targetPos, tSmooth);
            yield return null;
        }
        uiElement.localPosition = targetPos;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Instantly repositions selected seed packets. It scans through the selected seed slots (tagged "selectedSeedSlot"),
    /// and for any empty higher slot, it moves a seed packet from a lower, occupied slot upward.
    /// This repositioning happens instantly without any animation.
    /// </summary>
    private void RepositionSelectedSeedsInstant()
    {
        bool movedSomething = false;
        do
        {
            movedSomething = false;
            List<Transform> slots = new List<Transform>();
            foreach (Transform child in selectedSeedsUI)
            {
                if (child.CompareTag("selectedSeedSlot"))
                    slots.Add(child);
            }
            // Sort slots by descending y (highest first).
            slots.Sort((a, b) => b.localPosition.y.CompareTo(a.localPosition.y));

            // Look for an empty higher slot and a candidate from a lower slot.
            for (int i = 0; i < slots.Count; i++)
            {
                Transform higherSlot = slots[i];
                if (higherSlot.childCount == 0 && !reservedSelectedSlots.Contains(higherSlot))
                {
                    Transform candidateSlot = null;
                    for (int j = i + 1; j < slots.Count; j++)
                    {
                        Transform lowerSlot = slots[j];
                        if (lowerSlot.childCount > 0)
                        {
                            candidateSlot = lowerSlot;
                            break;
                        }
                    }
                    if (candidateSlot != null)
                    {
                        RectTransform candidatePacket = candidateSlot.GetChild(0).GetComponent<RectTransform>();
                        reservedSelectedSlots.Add(higherSlot);
                        candidatePacket.SetParent(higherSlot, false);
                        candidatePacket.localPosition = Vector3.zero;
                        reservedSelectedSlots.Remove(higherSlot);
                        movedSomething = true;
                        break;
                    }
                }
            }
        } while (movedSomething);
    }
}
