using UnityEngine;

public class CherryBomb : MonoBehaviour
{
    public int damage;
    public GameObject explodePrefab;
    public void Explode()
    {

        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");

        AudioManager.instance.Play("Cherry");

        foreach (GameObject zombie in zombies)
        {
            if (Vector3.Distance(zombie.transform.position, transform.position) <= 1.414f * 1.2f)
            {
                //Destroy(zombie);

                zombie.GetComponent<Zombie>().Die(Zombie.DamageType.Explode);
            }
        }

        Destroy(Instantiate(explodePrefab, transform.position, Quaternion.identity),1.5f);

        Destroy(gameObject);
    }
}
