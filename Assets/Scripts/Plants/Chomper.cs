using UnityEngine;
using System.Collections;

public class Chomper : MonoBehaviour
{
    public float timeToChew;
    public float detectionRangeX = 1.5f;
    public bool isChewing;
    public Animator animator;
    GameObject currentTarget;
    public float zRowTolerance = 0.3f;
    void Update()
    {
        if (!isChewing)
        {
            currentTarget = GetClosestZombieInLane();
            if (currentTarget != null)
            {
                StartCoroutine(Chew());
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
            bool inRange = zombie.transform.position.x - transform.position.x <= detectionRangeX;
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

    IEnumerator Chew()
    {
        // Start animator
        animator.SetTrigger("Chew");
        animator.ResetTrigger("Unchew");

        print("Chewing");

        yield return new WaitForSeconds(timeToChew);

        print("Done Chewing");

        isChewing = false;

        AudioManager.instance.Play("Gulp");

        // End animator
        animator.SetTrigger("Unchew");
        animator.ResetTrigger("Chew");
    }

    public void Eat()
    {
        int choice = UnityEngine.Random.Range(0, 4); // 0, 1, or 2

        switch (choice)
        {
            case 0:
                AudioManager.instance.Play("Chomp1");
                break;
            case 1:
                AudioManager.instance.Play("Chomp2");
                break;
            case 2:
                AudioManager.instance.Play("Chomp3");
                break;
            case 3:
                AudioManager.instance.Play("Chomp4");
                break;
        }
        isChewing = true;
        Destroy(currentTarget);
    }
}
