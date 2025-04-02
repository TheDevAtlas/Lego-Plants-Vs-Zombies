using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SeedPacket : MonoBehaviour, IPointerClickHandler
{
    public Transform plantImage; // Assign the actual plant image (child of this seed)
    private Canvas canvas;

    public Transform originalSlot; // Where the image should return to
    public bool isPlanted = false;

    private List<Transform> sortedSlots;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        originalSlot = plantImage.parent; // Save the original location

        var slots = GameObject.FindGameObjectsWithTag("SelectedSeedSlot");
        sortedSlots = slots
            .Select(slot => slot.transform)
            .OrderBy(slot => -slot.position.y)
            .ToList();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPlanted)
        {
            var targetSlot = FindAvailableSlot();
            if (targetSlot != null)
            {
                StartCoroutine(FlyToSlot(plantImage, targetSlot));
            }
        }
        else
        {
            StartCoroutine(FlyBack(plantImage, originalSlot));
        }
    }

    Transform FindAvailableSlot()
    {
        foreach (var slot in sortedSlots)
        {
            if (slot.childCount == 0)
                return slot;
        }
        return null;
    }

    IEnumerator FlyToSlot(Transform movingObject, Transform targetSlot)
    {
        isPlanted = true;

        movingObject.SetParent(canvas.transform); // Move to root canvas for animation
        GameObject holder = new GameObject();
        holder.transform.SetParent(targetSlot);

        Vector3 start = movingObject.GetComponent<RectTransform>().position;
        Vector3 end = targetSlot.GetComponent<RectTransform>().position + new Vector3(238f * 0.8f, 0f, 0f);
        float duration = 0.3f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);
            float eased = CubicEaseInOut(progress);
            movingObject.GetComponent<RectTransform>().position = Vector3.Lerp(start, end, eased);
            yield return null;
        }

        movingObject.SetParent(targetSlot);
        Destroy(holder);
        movingObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        movingObject.localScale = Vector3.one;
    }

    IEnumerator FlyBack(Transform movingObject, Transform returnSlot)
    {
        isPlanted = false;

        movingObject.SetParent(canvas.transform); // Move to root canvas for animation
        // After returning, shift the remaining packets up
        ShiftPacketsUp();

        Vector3 start = movingObject.GetComponent<RectTransform>().position;
        Vector3 end = returnSlot.GetComponent<RectTransform>().position + new Vector3(238f * 0.8f, 0f, 0f);
        float duration = 0.3f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);
            float eased = CubicEaseInOut(progress);
            movingObject.GetComponent<RectTransform>().position = Vector3.Lerp(start, end, eased);
            yield return null;
        }

        movingObject.SetParent(returnSlot);
        movingObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        movingObject.localScale = Vector3.one;

        
    }

    void ShiftPacketsUp()
    {
        var plantedImages = sortedSlots
            .Where(slot => slot.childCount > 0)
            .Select(slot => slot.GetChild(0))
            .ToList();

        for (int i = 0; i < plantedImages.Count; i++)
        {
            Transform targetSlot = sortedSlots[i];
            Transform currentPlant = plantedImages[i];

            if (currentPlant.parent != targetSlot)
            {
                currentPlant.SetParent(targetSlot);
                currentPlant.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                currentPlant.localScale = Vector3.one;
            }
        }
    }

    float CubicEaseInOut(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    public void ReturnPlantToSeed()
    {
        if (isPlanted)
        {
            StartCoroutine(FlyBack(plantImage, originalSlot));
        }
        else
        {
            var targetSlot = FindAvailableSlot();
            if (targetSlot != null)
            {
                StartCoroutine(FlyToSlot(plantImage, targetSlot));
            }
        }
    }
}
