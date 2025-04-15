using UnityEngine;

public class CherryBomb : MonoBehaviour
{
    public void Explode()
    {

        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");

        foreach (GameObject zombie in zombies)
        {
            if (Vector3.Distance(zombie.transform.position, transform.position) <= 1.414f * 1.2f)
            {
                Destroy(zombie);
            }
        }

        Destroy(gameObject);
    }
}
