using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SeedPacket : MonoBehaviour, IPointerClickHandler
{
    public Transform plantImage; // Assign the actual plant image (child of this seed)
    private Canvas canvas;

    private Transform originalSlot; // Where the image should return to
    private bool isPlanted = false;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        originalSlot = plantImage.parent; // Save the original location
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
        var slots = GameObject.FindGameObjectsWithTag("SelectedSeedSlot");
        foreach (var slot in slots)
        {
            if (slot.transform.childCount == 0)
                return slot.transform;
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
