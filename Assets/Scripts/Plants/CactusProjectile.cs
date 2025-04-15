using UnityEngine;

public class CactusProjectile : MonoBehaviour
{
    public enum PeaType { Normal }
    public PeaType peaType = PeaType.Normal;

    public Vector3 shootDirection = Vector3.forward;
    public float speed = 10f;
    public float maxPosition = 25f;

    public Material normalMaterial;
    public Material iceMaterial;
    public Material fireMaterial;

    private Renderer rend;

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
            DealDamage(other.gameObject);
            Destroy(gameObject);
        }
    }

    void DealDamage(GameObject zombie)
    {
        // Replace this with actual damage logic depending on zombie health systems
        switch (peaType)
        {
            case PeaType.Normal:
                Debug.Log("Normal Cactus hit!");
                break;
        }

        // Placeholder destruction — replace with proper damage system
        Destroy(zombie);
    }
}
