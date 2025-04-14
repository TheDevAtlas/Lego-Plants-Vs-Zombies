using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour
{
    public int SunValue = 25;
    public GameObject sunCollectionPoint;
    public float SunWait = 2f;
    public float moveDuration = 1f;

    private Rigidbody rb;
    private PlantingController plantingController;

    void Start()
    {
        sunCollectionPoint = GameObject.Find("Sun Collection Point");
        rb = GetComponent<Rigidbody>();

        plantingController = GameObject.Find("Planting Controller").GetComponent<PlantingController>();

        StartCoroutine(CollectSun());
    }

    IEnumerator CollectSun()
    {
        yield return new WaitForSeconds(SunWait);

        // Disable physics
        if (rb != null)
            rb.isKinematic = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = sunCollectionPoint.transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            transform.position = Vector3.Slerp(startPos, targetPos, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }
        
        plantingController.AddSun(SunValue);

        Destroy(gameObject);
    }
}
