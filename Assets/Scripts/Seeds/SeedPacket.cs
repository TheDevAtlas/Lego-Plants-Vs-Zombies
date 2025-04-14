using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SeedPacket : MonoBehaviour, IPointerClickHandler
{
    [Header("Seed Scriptable Object")]
    public Seed seed;  // Reference to a Seed ScriptableObject assigned in the Inspector

    [Header("UI Components")]
    public Image packetImage;           // Displays the seed packet image
    public TextMeshProUGUI costText1;     // First text element for the sun cost
    public TextMeshProUGUI costText2;     // Second text element for the sun cost

    [Header("Background Images")]
    public Image mainBackground;        // Main background image component
    public Image secondBackground;      // Secondary background image component

    [Header("Background Sprites")]
    public Sprite readySpriteMain;      // Sprite when seed packet is ready (main background)
    public Sprite readySpriteSecond;    // Sprite when seed packet is ready (second background)
    public Sprite notReadySpriteMain;   // Sprite when seed packet is cooling down (main background)
    public Sprite notReadySpriteSecond; // Sprite when seed packet is cooling down (second background)

    [Header("Reload Animation")]
    public Image reloadImage;           // The reload sprite image that will animate during cooldown

    // Internal variables for cooldown.
    private RectTransform reloadRectTransform;
    public bool isReady = true;
    private float rechargeTime;
    
    // NEW: Variables for seed selection.
    public bool isSelected = false;     // Tracks whether the seed packet is currently selected.
    public bool isAnimating = false;    // Prevents multiple rapid clicks.
    public Transform originalParent;    // The available slot that originally held this packet.
    public Transform originalSlot;      // Alias for the original parent (set in Start).

    void Start()
    {
        // Initialize the seed packet from the Seed ScriptableObject values.
        if (seed != null)
        {
            if (packetImage != null)
                packetImage.sprite = seed.packetImage;

            if (costText1 != null)
                costText1.text = seed.sunCost.ToString();

            if (costText2 != null)
                costText2.text = seed.sunCost.ToString();

            rechargeTime = seed.rechargeTime;
        }

        // Set the background images to the “ready” sprites.
        if (mainBackground != null)
            mainBackground.sprite = readySpriteMain;

        if (secondBackground != null)
            secondBackground.sprite = readySpriteSecond;

        // Disable the reload image initially and cache its RectTransform.
        if (reloadImage != null)
        {
            reloadImage.gameObject.SetActive(false);
            reloadRectTransform = reloadImage.GetComponent<RectTransform>();
        }
        isReady = true;
        
        // Record the original parent (the available seed slot).
        originalParent = transform.parent;
        originalSlot = originalParent;
    }

    // Handle pointer clicks (UI button style).
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.SeedPacketClicked(this);
        }
    }

    /// <summary>
    /// Call this method when the seed packet is used (e.g., to plant the seed).
    /// If the seed is ready, execute its usage logic then start the cooldown.
    /// </summary>
    public void UseSeed()
    {
        if (isReady)
        {
            // Example: Determine desired planting position. This could be based on a mouse click, a predetermined grid cell, etc.
            Vector3 desiredPosition = Vector3.zero;

            // Call the planting controller.
            // Ensure you have a proper reference to the planting controller (could be a singleton or assigned at runtime)
            PlantingController plantingController = FindObjectOfType<PlantingController>();
            if (plantingController != null)
            {
                //plantingController.PlantSeed(seed, desiredPosition);
            }
            else
            {
                Debug.LogError("PlantingController not found in the scene!");
            }

            StartCoroutine(BeginCooldown());
        }
        else
        {
            // Optionally, provide feedback that the seed is not ready.
        }
    }


    /// <summary>
    /// Coroutine that handles the cooldown process.
    /// While cooling down, the reload image animates and the background sprites indicate the not-ready state.
    /// </summary>
    IEnumerator BeginCooldown()
    {
        isReady = false;
        if (mainBackground != null)
            mainBackground.sprite = notReadySpriteMain;
        if (secondBackground != null)
            secondBackground.sprite = notReadySpriteSecond;

        if (reloadImage != null)
        {
            reloadImage.gameObject.SetActive(true);
            reloadRectTransform.anchoredPosition = new Vector2(0, 0);
        }

        float elapsed = 0f;
        while (elapsed < rechargeTime)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / rechargeTime);
            if (reloadRectTransform != null)
            {
                float newY = Mathf.Lerp(0, 108f, progress);
                reloadRectTransform.anchoredPosition = new Vector2(0, newY);
            }
            yield return null;
        }

        isReady = true;
        if (reloadImage != null)
            reloadImage.gameObject.SetActive(false);
        if (mainBackground != null)
            mainBackground.sprite = readySpriteMain;
        if (secondBackground != null)
            secondBackground.sprite = readySpriteSecond;
    }
}
