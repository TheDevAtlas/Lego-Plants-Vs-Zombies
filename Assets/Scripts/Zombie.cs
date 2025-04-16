using UnityEngine;
using System.Collections;

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

    void Start()
    {
        gc = GameObject.Find("Game Controller").GetComponent<GameController>();
        SetZombie();
    }

    void Update()
    {
        if (gc.currentState != GameController.GameState.Playing)
            return;

        if (state == ZombieState.Walking)
        {
            transform.position += walkDirection.normalized * speed * Time.deltaTime;
            CheckForPlant();
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

            yield return new WaitForSeconds(eatInterval);
        }

        // fallback in case plant becomes null unexpectedly
        state = ZombieState.Walking;
    }

    public void TakeDamage(int damage, DamageType type)
    {
        switch (type)
        {
            case DamageType.Normal: health -= damage; break;
            case DamageType.Fire: health -= damage * 2; break;
            case DamageType.Ice: health -= damage; break;
            case DamageType.Explode: health -= damage; break;
        }

        if (health <= 0){
            GameObject.FindObjectOfType<WaveController>().ZombieDied();
            Destroy(gameObject);
        }
    }

    public ZombieType zombieType;

    public GameObject[] coneHeadPieces;
    public GameObject[] bucketHeadPieces;
    public GameObject[] screenPieces;
    public GameObject[] footballPieces;
    public GameObject[] flagPieces;

    public void SetZombie()
    {
        switch (zombieType)
        {
            case ZombieType.Normal: health = 190; break;
            case ZombieType.Conehead: health = 560; foreach (var p in coneHeadPieces) p.SetActive(true); break;
            case ZombieType.Buckethead: health = 1290; foreach (var p in bucketHeadPieces) p.SetActive(true); break;
            case ZombieType.Screen: health = 1290; foreach (var p in screenPieces) p.SetActive(true); break;
            case ZombieType.Football: health = 1100; speed *= 2f; foreach (var p in footballPieces) p.SetActive(true); break;
            case ZombieType.Flag: health = 190; foreach (var p in flagPieces) p.SetActive(true); break;
        }
    }
}
