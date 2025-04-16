using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkySun : MonoBehaviour
{
    public GameObject sunPrefab;
    public float spawnInterval = 5f;
    public float spawnHeight = 15f;
    public float groundY = 0.5f;
    public float fallDuration = 2f;

    public ExtendedBoardGenerator board;

    void Start()
    {

        StartCoroutine(SpawnSunRoutine());
    }

    IEnumerator SpawnSunRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnSun();
        }
    }

    void SpawnSun()
    {
        int randomX = Random.Range(board.minGrid.x, board.maxGrid.x + 1);
        int randomZ = Random.Range(board.minGrid.y, board.maxGrid.y + 1);

        float worldX = board.boardStartX + randomX * board.tileSize;
        float worldZ = board.boardStartZ + randomZ * board.tileSize;

        Vector3 spawnPos = new Vector3(worldX, spawnHeight, worldZ);
        GameObject sun = Instantiate(sunPrefab, spawnPos, Quaternion.identity);
    }
}
