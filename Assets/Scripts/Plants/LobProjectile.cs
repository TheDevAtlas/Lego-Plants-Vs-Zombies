using UnityEngine;

public class LobProjectile : MonoBehaviour
{
    public enum LobType { Normal, Ice }
    public LobType lobType = LobType.Normal;
    public Transform zombieTarget;
    public float arcHeight = 2f;
    public float speed = 5f;

    private Vector3 startPos;
    private float journeyLength;
    private float progress = 0f;
    private Vector3 lastPosition;
    public int damage;

    public GameObject effectN;
    public GameObject effectI;

    void Start()
    {
        if (zombieTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        startPos = transform.position;
        journeyLength = Vector3.Distance(startPos, zombieTarget.position);
    }

    void Update()
    {
        

        // Progress between 0 and 1
        progress += (speed / journeyLength) * Time.deltaTime;
        progress = Mathf.Clamp01(progress);

        if (zombieTarget)
        {
            lastPosition = zombieTarget.position;
        }
        // Linear interpolation between start and target
        Vector3 flatPos = Vector3.Lerp(startPos, lastPosition, progress);

        // Add arc
        float arc = arcHeight * Mathf.Sin(progress * Mathf.PI);
        flatPos.y += arc;

        transform.position = flatPos;

        if (progress >= 1f)
        {
            // Hit logic
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        print("hit a thing");
        if (other.CompareTag("zombie"))
        {
            int choice = UnityEngine.Random.Range(0, 4); // 0, 1, or 2

            switch (choice)
            {
                case 0:
                    AudioManager.instance.Play("Melon1");
                    break;
                case 1:
                    AudioManager.instance.Play("Melon2");
                    break;
                case 2:
                    AudioManager.instance.Play("Melon3");
                    break;
                case 3:
                    AudioManager.instance.Play("Melon4");
                    break;
            }

            switch (lobType)
            {
                case LobType.Normal:
                    Destroy(Instantiate(effectN, transform.position, Quaternion.identity),1.5f);
                    break;
                case LobType.Ice:
                    AudioManager.instance.Play("SnowPea");
                    Destroy(Instantiate(effectI, transform.position, Quaternion.identity),1.5f);
                    break;
            }
            DealDamage(other.gameObject);
            Destroy(gameObject);
        }
    }

    void DealDamage(GameObject zombie)
    {
        print("Lob hit Zombie");
        // Replace this with actual damage logic depending on zombie health systems
        switch (lobType)
        {
            case LobType.Normal:
                Debug.Log("Normal lob hit!");
                zombie.GetComponent<Zombie>().TakeDamage(damage, Zombie.DamageType.Normal);
                break;
            case LobType.Ice:
                Debug.Log("Ice lob hit! Apply slow effect.");
                zombie.GetComponent<Zombie>().TakeDamage(damage, Zombie.DamageType.Ice);
                break;
        }
    }
}
