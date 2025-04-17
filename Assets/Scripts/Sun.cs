using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour
{
    public int SunValue = 25;
    public GameObject sunCollectionPoint;
    public float moveDuration = 1f;
    public float collectRadius = 1.5f;

    private Rigidbody rb;
    private PlantingController plantingController;
    private bool isBeingCollected = false;

    void Start()
    {
        sunCollectionPoint = GameObject.Find("Sun Collection Point");
        rb = GetComponent<Rigidbody>();
        plantingController = GameObject.Find("Planting Controller").GetComponent<PlantingController>();
        StartCoroutine(CollectSun(4f));
    }

    void Update()
    {
        if (isBeingCollected) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();

        float distance = Vector3.Distance(transform.position, mouseWorldPos);
        //print(distance);
        if (distance <= collectRadius || transform.position.x <= -3.5f || transform.position.x >= 3.5f || transform.position.z <= -2.5f || transform.position.z >= 2.5f)
        {
            StartCoroutine(CollectSun(0f));
            isBeingCollected = true;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position); // Plane at sun height

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }


    IEnumerator CollectSun(float timer)
    {
        yield return new WaitForSeconds(timer);
        isBeingCollected = true;
        if (rb != null)
            rb.isKinematic = true;
        AudioManager.instance.Play("Sun");
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }

}
