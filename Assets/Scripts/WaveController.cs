using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int NormalZombies;
        public int ConeheadZombies;
        public int BucketheadZombies;
        public int ScreenZombies;
        public int FootballZombies;
        public int FlagZombies;

        public int TotalZombies => NormalZombies + ConeheadZombies + BucketheadZombies + ScreenZombies + FootballZombies + FlagZombies;
    }

    public List<Wave> waves = new List<Wave>();
    public GameObject zombiePrefab;
    public Transform spawnPoint;
    public float spawnDelay = 0.5f;

    private int currentWaveIndex = 0;
    private int zombiesAlive = 0;
    private int zombiesKilled = 0;
    private bool spawning = false;

    private List<GameObject> previewZombies = new List<GameObject>();

    void Start()
    {
        // This would be called by GameController during seed selection camera move
        SpawnPreviewZombies();
    }

    public void SpawnPreviewZombies()
    {
        if (waves.Count == 0) return;

        Wave firstWave = waves[0];
        HashSet<Zombie.ZombieType> shownTypes = new HashSet<Zombie.ZombieType>();

        if (firstWave.NormalZombies > 0) shownTypes.Add(Zombie.ZombieType.Normal);
        if (firstWave.ConeheadZombies > 0) shownTypes.Add(Zombie.ZombieType.Conehead);
        if (firstWave.BucketheadZombies > 0) shownTypes.Add(Zombie.ZombieType.Buckethead);
        if (firstWave.ScreenZombies > 0) shownTypes.Add(Zombie.ZombieType.Screen);
        if (firstWave.FootballZombies > 0) shownTypes.Add(Zombie.ZombieType.Football);
        if (firstWave.FlagZombies > 0) shownTypes.Add(Zombie.ZombieType.Flag);

        int laneOffset = 0;
        foreach (Zombie.ZombieType type in shownTypes)
        {
            int lane = laneOffset - 2; // Spread lanes from -2 to 2
            Vector3 pos = spawnPoint.position + new Vector3(0, 0f, lane);
            GameObject zombieObj = Instantiate(zombiePrefab, pos, Quaternion.identity);
            zombieObj.transform.Rotate(0f, -90f, 0f);

            Zombie z = zombieObj.GetComponent<Zombie>();
            z.zombieType = type;
            z.SetZombie();

            previewZombies.Add(zombieObj);
            laneOffset++;
        }
    }

    public void StartLevel()
    {
        // Destroy preview zombies before real wave
        foreach (var preview in previewZombies)
        {
            if (preview != null) Destroy(preview);
        }
        previewZombies.Clear();

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        if (currentWaveIndex >= waves.Count) yield break;

        spawning = true;
        Wave wave = waves[currentWaveIndex];
        List<Zombie.ZombieType> zombiePool = new List<Zombie.ZombieType>();

        void AddZombies(Zombie.ZombieType type, int count)
        {
            for (int i = 0; i < count; i++)
                zombiePool.Add(type);
        }

        AddZombies(Zombie.ZombieType.Normal, wave.NormalZombies);
        AddZombies(Zombie.ZombieType.Conehead, wave.ConeheadZombies);
        AddZombies(Zombie.ZombieType.Buckethead, wave.BucketheadZombies);
        AddZombies(Zombie.ZombieType.Screen, wave.ScreenZombies);
        AddZombies(Zombie.ZombieType.Football, wave.FootballZombies);
        AddZombies(Zombie.ZombieType.Flag, wave.FlagZombies);

        zombiesAlive = zombiePool.Count;
        zombiesKilled = 0;
        float totalDelay = 0f;

        foreach (var zombieType in zombiePool)
        {
            totalDelay += Random.Range(1f, 2f);
            StartCoroutine(SpawnZombieWithDelay(zombieType, totalDelay));
        }

        spawning = false;
        yield return null;
    }

    private IEnumerator SpawnZombieWithDelay(Zombie.ZombieType typeToSpawn, float delay)
    {
        yield return new WaitForSeconds(delay);

        int lane = Random.Range(-2, 3); // z from -2 to 2
        Vector3 startPos = spawnPoint.position + new Vector3(0, -2f, lane);
        GameObject zombieObj = Instantiate(zombiePrefab, startPos, Quaternion.identity);
        zombieObj.transform.Rotate(0f, -90f, 0f);

        Zombie zombie = zombieObj.GetComponent<Zombie>();
        zombie.zombieType = typeToSpawn;
        zombie.SetZombie();

        StartCoroutine(SlerpZombie(zombieObj.transform));
    }

    private IEnumerator SlerpZombie(Transform zombieTransform)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 start = zombieTransform.position;
        Vector3 target = new Vector3(start.x, transform.position.y, start.z);

        while (elapsed < duration)
        {
            zombieTransform.position = Vector3.Slerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        zombieTransform.position = target;
    }

    public void ZombieDied()
    {
        zombiesKilled++;

        if (!spawning && zombiesKilled >= zombiesAlive / 2f && currentWaveIndex + 1 < waves.Count)
        {
            currentWaveIndex++;
            StartCoroutine(SpawnWave());
        }
    }
}
