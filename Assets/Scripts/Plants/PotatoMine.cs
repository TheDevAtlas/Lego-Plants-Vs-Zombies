using UnityEngine;
using System.Collections;

public class PotatoMine : MonoBehaviour
{
    public float armingTime = 3f;
    public float dropAmount = 0.5f;
    public float detectionRangeX = 0.8f;
    public float zRowTolerance = 0.3f;
    public float slerpSpeed = 2f;

    private Vector3 startPos;
    private Vector3 buriedPos;
    private bool isArmed = false;
    private bool risingUp = false;
    public int damage;

    void Start()
    {
        startPos = transform.position;
        buriedPos = startPos - new Vector3(0, dropAmount, 0);
        transform.position = buriedPos;
        StartCoroutine(ArmAfterDelay());
    }

    IEnumerator ArmAfterDelay()
    {
        yield return new WaitForSeconds(armingTime);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * slerpSpeed;
            transform.position = Vector3.Slerp(buriedPos, startPos, t);
            yield return null;
        }

        isArmed = true;
    }

    void Update()
    {
        if (!isArmed) return;

        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");
        foreach (GameObject zombie in zombies)
        {
            float dx = Mathf.Abs(zombie.transform.position.x - transform.position.x);
            float dz = Mathf.Abs(zombie.transform.position.z - transform.position.z);

            if (dx <= detectionRangeX && dz <= zRowTolerance)
            {
                Explode();
                break;
            }
        }
    }

    public void Explode()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");

        foreach (GameObject zombie in zombies)
        {
            float dz = Mathf.Abs(zombie.transform.position.z - transform.position.z);
            if (Vector3.Distance(zombie.transform.position, transform.position) <= 1.414f && dz <= zRowTolerance)
            {
                // Destroy(zombie);

                zombie.GetComponent<Zombie>().TakeDamage(damage, Zombie.DamageType.Explode);
            }
        }

        Destroy(gameObject);
    }
}
