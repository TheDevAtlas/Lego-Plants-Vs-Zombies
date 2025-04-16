using UnityEngine;

public class LawnMower : MonoBehaviour
{
    public float distanceToDetect = 0.5f;
    public float zRowTolerance = 0.3f;
    public float speed = 5f;
    public float maxPosition = 10f;
    public bool isActive = false;
    void Update()
    {
        if(isActive)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;

            if(transform.position.x >= maxPosition)
            {
                Destroy(gameObject);
            }
            //return;
        }

        // GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");
        // foreach (GameObject zombie in zombies)
        // {
        //     float dx = Mathf.Abs(zombie.transform.position.x - transform.position.x);
        //     float dz = Mathf.Abs(zombie.transform.position.z - transform.position.z);

        //     if (dx <= distanceToDetect - 0.2f && dz <= zRowTolerance)
        //     {
        //         isActive = true;
        //         break;
        //     }
        // }
        
    }

    void OnTriggerEnter(Collider other)
    {
        print("hit a thing");
        if (other.CompareTag("zombie"))
        {
            isActive = true;
            other.gameObject.GetComponent<Zombie>().Die(Zombie.DamageType.Normal);
        }
    }
}
