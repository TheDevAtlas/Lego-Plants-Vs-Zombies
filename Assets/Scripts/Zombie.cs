using UnityEngine;

public class Zombie : MonoBehaviour
{
    public enum DamageType {Normal, Fire, Ice, Explode};
    public int health;
    public float speed;
    public Vector3 walkDirection = Vector3.left;

    private GameController gc;

    void Start()
    {
        gc = GameObject.Find("Game Controller").GetComponent<GameController>();
        SetZombie();
    }

    void Update()
    {
        if(gc.currentState == GameController.GameState.Playing)
        {
            transform.position += walkDirection.normalized * speed * Time.deltaTime;
        }
        
    }

    public void TakeDamage(int damage, DamageType type)
    {
        switch (type)
        {
            case DamageType.Normal:
                health -= damage;
                break;
            case DamageType.Fire:
                health -= damage * 2;
                break;
            case DamageType.Ice:
                health -= damage;
                break;
            case DamageType.Explode:
                health -= damage;
                break;
        }

        if (health <= 0) {
            Destroy(gameObject);
        }
    }

    public enum ZombieType {Normal, Conehead, Buckethead, Screen, Football, Flag};
    public ZombieType zombieType;

    // Set pieces to be active / get knocked off //
    // Will divide evenly / get knocked off at certain points //
    public GameObject[] coneHeadPieces;
    public GameObject[] bucketHeadPieces;
    public GameObject[] screenPieces;
    public GameObject[] footballPieces;
    public GameObject[] flagPieces;

    public void SetZombie()
    {
        switch(zombieType)
        {
            case ZombieType.Normal:
                health = 190;
                break;
            case ZombieType.Conehead:
                health = 560;
                foreach(GameObject p in coneHeadPieces)
                {
                    p.SetActive(true);
                }
                break;
            case ZombieType.Buckethead:
                health = 1290;
                foreach(GameObject p in bucketHeadPieces)
                {
                    p.SetActive(true);
                }
                break;
            case ZombieType.Screen:
                health = 1290;
                foreach(GameObject p in screenPieces)
                {
                    p.SetActive(true);
                }
                break;
            case ZombieType.Football:
                health = 1100;
                foreach(GameObject p in footballPieces)
                {
                    p.SetActive(true);
                }
                break;
            case ZombieType.Flag:
                health = 190;
                foreach(GameObject p in flagPieces)
                {
                    p.SetActive(true);
                }
                break;
        }
    }
}
