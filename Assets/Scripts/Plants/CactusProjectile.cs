using UnityEngine;

public class CactusProjectile : MonoBehaviour
{
    public enum PeaType { Normal }
    public PeaType peaType = PeaType.Normal;

    public Vector3 shootDirection = Vector3.forward;
    public float speed = 10f;
    public float maxPosition = 25f;

    public Material normalMaterial;


    private Renderer rend;
    public int damage;
    public GameObject effectN;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        transform.position += shootDirection.normalized * speed * Time.deltaTime;

        if (transform.position.x >= maxPosition)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        print("hit a thing");
        if (other.CompareTag("zombie"))
        {
            Destroy(Instantiate(effectN, transform.position, Quaternion.identity),1.5f);
            DealDamage(other.gameObject);
            Destroy(gameObject);
        }
    }

    void DealDamage(GameObject zombie)
    {
        int choice = UnityEngine.Random.Range(0, 3); // 0, 1, or 2

        switch (choice)
        {
            case 0:
                AudioManager.instance.Play("PeaHit1");
                break;
            case 1:
                AudioManager.instance.Play("PeaHit2");
                break;
            case 2:
                AudioManager.instance.Play("PeaHit3");
                break;
        }

        // Replace this with actual damage logic depending on zombie health systems
        switch (peaType)
        {
            case PeaType.Normal:
                Debug.Log("Normal Cactus hit!");
                zombie.GetComponent<Zombie>().TakeDamage(damage, Zombie.DamageType.Normal);
                break;
        }
    }
}
