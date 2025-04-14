using UnityEngine;
using System.Collections;

public class Sunflower : MonoBehaviour
{
    public float timeToSpawnSun;
    public Animator flowerAnimator;
    public GameObject sunPrefab;
    public float upwardForce = 5f;
    public float sideForce = 2f;

    void Start()
    {
        // Start timer to spawn sun
        StartCoroutine("SpawnSun");
    }

    IEnumerator SpawnSun()
    {
        yield return new WaitForSeconds(timeToSpawnSun);
        print("Sun Spawned");
        flowerAnimator.SetTrigger("Sun");
        StartCoroutine("SpawnSun");
        yield return new WaitForSeconds(0.25f);
        MakeSun();
    }

    void MakeSun()
    {
        GameObject newSun = Instantiate(sunPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);

        Rigidbody rb = newSun.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Randomize direction on X and Z axis
            float xDirection = Random.value > 0.5f ? 1f : -1f;
            float zDirection = Random.value > 0.5f ? 1f : -1f;
            Vector3 force = new Vector3(xDirection * sideForce, upwardForce, zDirection * sideForce);

            rb.AddForce(force, ForceMode.Impulse);
        }
    }
}
