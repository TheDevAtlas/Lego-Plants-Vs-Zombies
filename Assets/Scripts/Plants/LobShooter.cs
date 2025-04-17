using UnityEngine;
using System.Collections;

public class LobShooter : MonoBehaviour
{
    public Transform shootPoint;
    public GameObject projectilePrefab;
    public Animator shootAnimator;
    public Vector3 shootDirection = Vector3.up;
    public float maxSightX = 10f;
    public float zRowTolerance = 0.3f;

    public bool isShooting = false;
    public GameObject currentTarget;
    public int damage;

    void Update()
    {
        if (!isShooting)
        {
            currentTarget = GetClosestZombieInLane();
            if (currentTarget != null)
            {
                StartCoroutine(Shoot());
            }
        }
    }

    GameObject GetClosestZombieInLane()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");
        GameObject closestZombie = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject zombie in zombies)
        {
            Vector3 toZombie = zombie.transform.position - transform.position;

            // Make sure zombie is in roughly the shootDirection and on the same row
            //bool inDirection = Vector3.Dot(toZombie.normalized, shootDirection.normalized) > 0.5f;
            bool inRange = zombie.transform.position.x <= maxSightX;
            bool sameRow = Mathf.Abs(zombie.transform.position.z - transform.position.z) <= zRowTolerance;

            print("inRange " + inRange + " sameRow " + sameRow);

            if (inRange && sameRow)
            {
                float distance = toZombie.sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestZombie = zombie;
                    closestDistance = distance;
                }
            }
        }

        return closestZombie;
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        shootAnimator.SetTrigger("Shoot");

        yield return new WaitForSeconds(1f); // Cooldown
        isShooting = false;
    }

    public void ShootProjectile()
    {
        int choice = UnityEngine.Random.Range(0, 2); // 0, 1, or 2

        switch (choice)
        {
            case 0:
                AudioManager.instance.Play("Throw");
                break;
            case 1:
                AudioManager.instance.Play("Throw2");
                break;
        }
        if (currentTarget == null) return;

        GameObject pea = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        pea.transform.Rotate(-45.623f, 0f, 0f);

        LobProjectile proj = pea.GetComponent<LobProjectile>();
        if (proj != null)
        {
            proj.zombieTarget = currentTarget.transform;
            proj.damage = damage;
        }
    }
}
