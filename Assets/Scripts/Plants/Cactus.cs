using UnityEngine;
using System.Collections;

public class Cactus : MonoBehaviour
{
    public Transform shootPoint;
    public GameObject projectilePrefab;
    public Animator shootAnimator;
    public Vector3 shootDirection = Vector3.right;

    public float maxSightX = 10f;
    public float zRowTolerance = 0.3f;
    private bool isShooting = false;

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

    public void ShootProj()
    {
        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        // Optional: rotate the projectile visually
        proj.transform.Rotate(-45.623f, 0f, 0f);

        // Assign shoot direction to projectile
        CactusProjectile projDir = proj.GetComponent<CactusProjectile>();
        if (projDir != null)
        {
            projDir.shootDirection = shootDirection;
        }
    }
}
