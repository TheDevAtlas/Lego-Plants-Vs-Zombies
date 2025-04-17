using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Zombie : MonoBehaviour
{
    public enum DamageType { Normal, Fire, Ice, Explode };
    public enum ZombieType { Normal, Conehead, Buckethead, Screen, Football, Flag };
    public enum ZombieState { Walking, Eating }

    public int health;
    public float speed;
    public float plantDetectDistance = 0.75f;
    public float eatInterval = 1f;
    public int eatDamage = 20;
    public Vector3 walkDirection = Vector3.left;

    private GameController gc;
    private ZombieState state = ZombieState.Walking;
    private PlantHealth currentPlant;

    [Header("Freeze Settings")]
    public float freezeDuration = 3f;
    private bool isFrozen = false;
    private float originalSpeed;
    private Coroutine freezeCoroutine;
    private List<Material> originalMaterials = new List<Material>();


    Animator anim;

    // New equipment-related fields
    // equipmentHealth represents the extra health provided by attached equipment,
    // and equipmentEquipped signals if the equipment is still in place.
    private int equipmentHealth = 0;
    private bool equipmentEquipped = false;

    public ZombieType zombieType;

    // Visual equipment pieces for various zombie types
    public GameObject[] coneHeadPieces;
    public GameObject[] bucketHeadPieces;
    public GameObject[] screenPieces;
    public GameObject[] footballPieces;
    public GameObject[] flagPieces;

    // You can adjust the explosion force as needed.
    public float explosionForce = 0.2f;

    void Start()
    {
        anim = GetComponent<Animator>();
        gc = GameObject.Find("Game Controller").GetComponent<GameController>();
        originalSpeed = speed;

        SetZombie();

    }

    void Update()
    {
        if (gc.currentState != GameController.GameState.Playing)
            return;

        if (state == ZombieState.Walking)
        {
            anim.SetTrigger("DontEat");
            transform.position += walkDirection.normalized * speed * Time.deltaTime;
            CheckForPlant();
            
        }
        else
        {
            anim.ResetTrigger("DontEat");
        }
    }

    void CheckForPlant()
    {
        Vector3 checkPosition = transform.position + Vector3.left * plantDetectDistance;
        Collider[] hits = Physics.OverlapSphere(checkPosition, 0.2f, LayerMask.GetMask("Plant"));

        foreach (Collider hit in hits)
        {
            PlantHealth plant = hit.GetComponent<PlantHealth>();
            if (plant != null)
            {
                state = ZombieState.Eating;
                currentPlant = plant;
                StartCoroutine(EatPlant());
                anim.SetTrigger("Eat");
                break;
            }
        }
    }

    IEnumerator EatPlant()
    {
        while (currentPlant != null)
        {
            currentPlant.TakeDamage(eatDamage);

            if (currentPlant.health <= 0)
            {
                state = ZombieState.Walking;
                currentPlant = null;
                yield break;
            }

            int choice = UnityEngine.Random.Range(0, 3); // 0, 1, or 2

            switch (choice)
            {
                case 0:
                    AudioManager.instance.Play("Chomp3");
                    break;
                case 1:
                    AudioManager.instance.Play("Chomp2");
                    break;
            }
            AudioManager.instance.Play("ZChew");
            yield return new WaitForSeconds(eatInterval);

            
        }

        // Fallback in case the plant becomes null unexpectedly.
        state = ZombieState.Walking;
        anim.ResetTrigger("Eat");
    }

    public void TakeDamage(int damage, DamageType type)
    {
        // Compute effective damage based on the damage type
        int effectiveDamage = damage;
        switch (type)
        {
            case DamageType.Normal:
                effectiveDamage = damage;
                break;
            case DamageType.Fire:
                if (freezeCoroutine != null)
                    StopCoroutine(freezeCoroutine);
                    RevertMats();
                effectiveDamage = damage * 2;
                break;
            case DamageType.Ice:
                if (freezeCoroutine != null)
                    StopCoroutine(freezeCoroutine);
                freezeCoroutine = StartCoroutine(FreezeEffect());

                effectiveDamage = damage;
                break;
            case DamageType.Explode:
                effectiveDamage = damage;
                break;
        }

        // If equipment is still attached, apply damage to it first.
        if (equipmentEquipped && equipmentHealth > 0)
        {
            if (equipmentHealth > effectiveDamage)
            {
                equipmentHealth -= effectiveDamage;
                effectiveDamage = 0;
            }
            else
            {
                // Calculate any leftover damage after equipment is depleted.
                effectiveDamage -= equipmentHealth;
                equipmentHealth = 0;
                // Detach the equipment now that its health is gone.
                DetachEquipment();
                equipmentEquipped = false;
            }
        }

        // Apply any remaining damage to the zombie's base health.
        if (effectiveDamage > 0)
        {
            health -= effectiveDamage;
            if (health <= 0)
            {
                // Instead of destroying immediately, call Die with the damage type.
                Die(type);
            }
        }
    }

    public void Die(DamageType type)
    {
        // If the zombie takes fire or explosive damage, tint it grey.
        if (type == DamageType.Fire)
        {
            // Tints every renderer in the zombie (including child pieces) grey.
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true)){ // include self
            
                if(r.gameObject.activeSelf)
                {
                    r.material.color = Color.grey;
                }
            }

            anim.enabled = false;
        }
        else if (type == DamageType.Explode)
        {
            // Tints every renderer in the zombie (including child pieces) grey.
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true)){  // include self
                if(r.gameObject.activeSelf)
                {
                    r.material.color = Color.grey;
                }
            }

            anim.enabled = false;

            ExplodePieces(type);
        }
        else // Any other type of death, the head, and only the head, should pop off. 
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                Transform head = child.Find("Head");
                if (head != null)
                {
                    head.parent = null;

                    Rigidbody rb = head.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = head.gameObject.AddComponent<Rigidbody>();
                        rb.linearDamping = 2;
                        rb.angularDamping = 2;
                    }

                    if (head.GetComponent<Collider>() == null)
                    {
                        head.gameObject.AddComponent<SphereCollider>();
                    }

                    // Generate a random explosion direction
                    Vector3 randomDir = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(0.5f, 1f),  // Upward bias
                        Random.Range(-1f, 1f)
                    ).normalized;

                    rb.AddForce(randomDir * explosionForce / 2f); // Half force for dramatic pop
                    rb.AddTorque(randomDir * explosionForce / 2f);
                    anim.SetTrigger("Die");
                    Destroy(head.gameObject, 5f);

                    break;
                }
            }
            
        }


        // Notify the wave controller that a zombie died.
        GameObject.FindObjectOfType<WaveController>().ZombieDied();

        // Destroy the zombie game object.
        Destroy(gameObject, 2f);
        
        GetComponent<Collider>().enabled = false;
        this.enabled = false;
    }

    private void ExplodePieces(DamageType type)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true)) // include self
        {
            if (child == transform) continue; // skip the root zombie object

            // Detach from parent
            child.parent = null;

            // Add Rigidbody if not already present
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = child.gameObject.AddComponent<Rigidbody>();
                rb.linearDamping = 2;
                rb.angularDamping = 2;
                child.gameObject.AddComponent<SphereCollider>();
            }

            // Generate a random explosion direction
            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),  // Upward bias
                Random.Range(-1f, 1f)
            ).normalized;

            // Apply the explosion force
            if(type == DamageType.Explode)
            {
                rb.AddForce(randomDir * explosionForce);
                rb.AddTorque(randomDir * explosionForce);
            }
            else
            {
                rb.AddForce(randomDir * explosionForce / 10f);
                rb.AddTorque(randomDir * explosionForce / 10f);
            }
            

            // Destroy after 2 seconds
            Destroy(child.gameObject, 5f);
        }
    }


    public void SetZombie()
    {
        // Set the base zombie health always to 190.
        health = 190;

        // Assign extra equipment health based on zombie type and activate visuals.
        // The extra health equals the original total health minus the base health.
        switch (zombieType)
        {
            case ZombieType.Normal:
                equipmentHealth = 0;
                equipmentEquipped = false;
                break;
            case ZombieType.Conehead:
                equipmentHealth = 560 - 190; // 370 extra equipment health.
                equipmentEquipped = true;
                foreach (var p in coneHeadPieces)
                    p.SetActive(true);
                break;
            case ZombieType.Buckethead:
                equipmentHealth = 1290 - 190; // 1100 extra equipment health.
                equipmentEquipped = true;
                foreach (var p in bucketHeadPieces)
                    p.SetActive(true);
                break;
            case ZombieType.Screen:
                equipmentHealth = 1290 - 190; // 1100 extra equipment health.
                equipmentEquipped = true;
                foreach (var p in screenPieces)
                    p.SetActive(true);
                break;
            case ZombieType.Football:
                equipmentHealth = 1100 - 190; // 910 extra equipment health.
                equipmentEquipped = true;
                speed *= 2f;
                foreach (var p in footballPieces)
                    p.SetActive(true);
                break;
            case ZombieType.Flag:
                equipmentHealth = 0; // No extra equipment health.
                equipmentEquipped = false;
                foreach (var p in flagPieces)
                    p.SetActive(true);
                break;
        }
    }

    private void DetachEquipment()
    {
        // This function detaches any currently equipped visual pieces
        // by unparenting them and adding a Rigidbody so that they can pop off.
        GameObject[] piecesToDetach = null;
        switch (zombieType)
        {
            case ZombieType.Conehead:
                piecesToDetach = coneHeadPieces;
                break;
            case ZombieType.Buckethead:
                piecesToDetach = bucketHeadPieces;
                break;
            case ZombieType.Screen:
                piecesToDetach = screenPieces;
                break;
            case ZombieType.Football:
                piecesToDetach = footballPieces;
                break;
            case ZombieType.Flag:
                piecesToDetach = flagPieces;
                break;
            default:
                break;
        }

        if (piecesToDetach != null)
        {
            foreach (var p in piecesToDetach)
            {
                if (p.activeInHierarchy)
                {
                    p.transform.parent = null;
                    if (p.GetComponent<Rigidbody>() == null)
                        p.AddComponent<Rigidbody>();
                }
            }
        }
    }

    public void RemoveEquipment()
    {
        bucketHeadPieces = null;
        screenPieces = null;
        footballPieces = null;
        zombieType = ZombieType.Normal;
    }

    IEnumerator FreezeEffect()
    {
        isFrozen = true;

        // Cache original materials and set them to blue
        List<Renderer> renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
        originalMaterials.Clear();

        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                originalMaterials.Add(r.material);
                r.material.color = Color.blue;
            }
        }

        // Halve speed
        speed = originalSpeed / 2f;

        yield return new WaitForSeconds(freezeDuration);

        // Revert material colors and speed
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Count)
            {
                renderers[i].material.color = originalMaterials[i].color;
            }
        }

        speed = originalSpeed;
        isFrozen = false;
        freezeCoroutine = null;
    }

    void RevertMats()
    {
        List<Renderer> renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
        // Revert material colors and speed
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Count)
            {
                renderers[i].material.color = originalMaterials[i].color;
            }
        }
    }


}
