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
