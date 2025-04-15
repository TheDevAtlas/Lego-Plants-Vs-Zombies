using UnityEngine;
using System.Collections;

public class PeaShooter : MonoBehaviour
{
    public Transform shootPoint;               // Where the projectile spawns
    public GameObject peaPrefab;               // The projectile prefab
    public Animator shootAnimator;             // Animator component
    public Vector3 shootDirection = Vector3.right; // Global direction to shoot
    public float maxSightX = 10f;              // Maximum x-distance for detecting zombies
    public float zRowTolerance = 0.3f;         // Max Z-difference to allow shooting

    private bool isShooting = false;

    public PeaProjectile.PeaType PeaType;

    void Update()
    {
        if (!isShooting && ZombieInSight())
        {
            StartCoroutine(Shoot());
        }
    }

    bool ZombieInSight()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");

        foreach (GameObject zombie in zombies)
        {
            Vector3 toZombie = zombie.transform.position - transform.position;

            // Check if zombie is in direction of shootDirection, within X range, and on the same row (Z)
            if (Vector3.Dot(toZombie.normalized, shootDirection.normalized) > 0.9f &&
                zombie.transform.position.x <= maxSightX &&
                Mathf.Abs(zombie.transform.position.z - transform.position.z) <= zRowTolerance)
            {
                return true;
            }
        }
        shootAnimator.ResetTrigger("Shoot");
        return false;
        
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        shootAnimator.SetTrigger("Shoot");

        yield return new WaitForSeconds(1f); // Cooldown before next shot
        isShooting = false;
    }

    public void ShootPea()
    {
        GameObject pea = Instantiate(peaPrefab, shootPoint.position, Quaternion.identity);
        pea.GetComponent<PeaProjectile>().SetPeaType(PeaType);

        // Optional: rotate the projectile visually
        pea.transform.Rotate(-45.623f, 0f, 0f);

        // Assign shoot direction to projectile
        PeaProjectile proj = pea.GetComponent<PeaProjectile>();
        if (proj != null)
        {
            proj.shootDirection = shootDirection;
        }
    }
}
