using UnityEngine;

public class PeaProjectile : MonoBehaviour
{
    public enum PeaType { Normal, Ice, Fire }
    public PeaType peaType = PeaType.Normal;

    public Vector3 shootDirection = Vector3.forward;
    public float speed = 10f;
    public float maxPosition = 25f;

    public Material normalMaterial;
    public Material iceMaterial;
    public Material fireMaterial;

    private Renderer rend;
    public int damage;

    public GameObject effectN;
    public GameObject effectI;
    public GameObject effectF;

    void Start()
    {
        rend = GetComponent<Renderer>();
        ApplyMaterial();
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

            
            switch (peaType)
            {
                case PeaType.Normal:
                    Destroy(Instantiate(effectN, transform.position, Quaternion.identity),1.5f);
                    break;
                case PeaType.Ice:
                    Destroy(Instantiate(effectI, transform.position, Quaternion.identity),1.5f);
                    AudioManager.instance.Play("SnowPea");
                    break;
                case PeaType.Fire:
                    Destroy(Instantiate(effectF, transform.position, Quaternion.identity),1.5f);
                    AudioManager.instance.Play("FirePea");
                    break;
            }
            DealDamage(other.gameObject);
            Destroy(gameObject);
        }

        if (other.CompareTag("torchwood"))
        {
            switch (peaType)
            {
                case PeaType.Normal:
                    SetPeaType(PeaType.Fire);
                    break;
                case PeaType.Ice:
                    SetPeaType(PeaType.Normal);
                    break;
                case PeaType.Fire:
                    SetPeaType(PeaType.Fire);
                    break;
            }
        }
    }

    void DealDamage(GameObject zombie)
    {
        // Replace this with actual damage logic depending on zombie health systems
        switch (peaType)
        {
            case PeaType.Normal:
                Debug.Log("Normal pea hit!");
                zombie.GetComponent<Zombie>().TakeDamage(damage, Zombie.DamageType.Normal);
                break;
            case PeaType.Ice:
                Debug.Log("Ice pea hit! Apply slow effect.");
                zombie.GetComponent<Zombie>().TakeDamage(damage, Zombie.DamageType.Ice);
                break;
            case PeaType.Fire:
                Debug.Log("Fire pea hit! Extra damage or burn.");
                zombie.GetComponent<Zombie>().TakeDamage(damage, Zombie.DamageType.Fire);
                break;
        }

        // Placeholder destruction — replace with proper damage system
        // Destroy(zombie);
    }

    public void SetPeaType(PeaType type)
    {
        peaType = type;
        ApplyMaterial();
    }

    void ApplyMaterial()
    {
        if (rend == null) rend = GetComponent<Renderer>();

        switch (peaType)
        {
            case PeaType.Normal:
                rend.material = normalMaterial;
                break;
            case PeaType.Ice:
                rend.material = iceMaterial;
                break;
            case PeaType.Fire:
                rend.material = fireMaterial;
                break;
        }
    }
}
