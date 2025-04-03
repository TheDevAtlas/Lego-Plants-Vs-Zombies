using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelController : MonoBehaviour
{
    [Header("Camera Movement")]
    public Transform cameraTransform;
    public Vector3 cameraStartPos;
    public Vector3 cameraTargetPos;
    public float cameraMoveDuration = 2f;

    [Header("UI Movement")]
    public RectTransform seedPickerUI;
    public Vector2 seedPickerStartPos;
    public Vector2 seedPickerTargetPos;
    public float uiMoveDuration = 2f;

    [Header("UI Button")]
    public Button selectSeedsButton;

    private bool isSeedPickerActive = false;

    public PlantingController plantingController;

    void Start()
    {
        // Set starting positions
        cameraTransform.position = cameraStartPos;
        seedPickerUI.anchoredPosition = seedPickerStartPos;

        // Start the opening animation
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        yield return StartCoroutine(MoveBoth(
            cameraTransform, cameraTargetPos, cameraMoveDuration,
            seedPickerUI, seedPickerTargetPos, uiMoveDuration
        ));

        isSeedPickerActive = true;
    }

    public void OnSelectSeedsClicked()
    {
        if (!isSeedPickerActive) return;

        // Check that all seed slots are filled
        SeedPacket[] packets = FindObjectsOfType<SeedPacket>();
        int count = 0;
        foreach (SeedPacket packet in packets)
        {
            if (packet.isPlanted)
            {
                count++;
            }
        }

        if(count >= 7)
        {
            // All slots are filled, proceed
            isSeedPickerActive = false;
            StartCoroutine(CloseSeedPicker());
        }
    }

    IEnumerator CloseSeedPicker()
    {
        // Disable all SeedPacket scripts
        SeedPacket[] packets = FindObjectsOfType<SeedPacket>();
        foreach (SeedPacket packet in packets)
        {
            packet.enabled = false;
        }

        // Disable all PlantedImage scripts
        PlantedImage[] plants = FindObjectsOfType<PlantedImage>();
        foreach (PlantedImage plant in plants)
        {
            plant.enabled = false;
            plant.gameObject.GetComponent<SidebarPlantSelector>().enabled = true;
        }

        yield return StartCoroutine(MoveBoth(
            cameraTransform, cameraStartPos, cameraMoveDuration,
            seedPickerUI, seedPickerStartPos, uiMoveDuration
        ));

        plantingController.enabled = true;
    }

    IEnumerator MoveBoth(Transform cam, Vector3 camTarget, float camDuration, RectTransform ui, Vector2 uiTarget, float uiDuration)
    {
        float time = 0f;

        Vector3 camStart = cam.position;
        Vector2 uiStart = ui.anchoredPosition;

        float duration = Mathf.Max(camDuration, uiDuration);

        while (time < duration)
        {
            float t = time / duration;
            t = EaseInOutCubic(t);

            if (time <= camDuration)
                cam.position = Vector3.LerpUnclamped(camStart, camTarget, t);
            if (time <= uiDuration)
                ui.anchoredPosition = Vector2.LerpUnclamped(uiStart, uiTarget, t);

            time += Time.deltaTime;
            yield return null;
        }

        cam.position = camTarget;
        ui.anchoredPosition = uiTarget;
    }

    float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}
