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
        }
    }

    void OnTriggerEnter(Collider other)
    {
        print("hit a thing");
        if (other.CompareTag("zombie"))
        {
            if(!isActive)
            {
                AudioManager.instance.Play("Lawnmower");
            }

            isActive = true;
            other.gameObject.GetComponent<Zombie>().Die(Zombie.DamageType.Normal);
        }
    }
}
