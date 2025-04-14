using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum WaveDirection { North, South, East, West }

public class ExtendedBoardGenerator : MonoBehaviour
{
    [Header("Initial Board Settings")]
    public int initialColumns = 9;
    public int initialRows = 5;
    public float tileSize = 1f;

    [Header("Animation Settings (Initial Board)")]
    public float startY = 3f;
    public float lowestY = -0.5f;
    public float endY = 0f;
    public float fallDuration = 0.5f;
    public float delayMultiplier = 0.1f;
    public float flashEaseInDuration = 0.15f;
    public float flashEaseOutDuration = 0.15f;

    [Header("Prefab & Materials")]
    public GameObject tilePrefab;
    public Material material1;
    public Material material2;

    [Header("Wave Event (Optional)")]
    public UnityEvent onWaveAdded;

    [Header("Wave Spawn Timing")]
    public float waveStartDelay = 0f;
    public float waveTileFallDuration = 0.5f;
    public float waveDelayMultiplier = 0.1f;

    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();
    private float boardStartX;
    private float boardStartZ;
    private Vector2Int minGrid;
    private Vector2Int maxGrid;

    void Start()
    {
        GenerateInitialBoard();
    }

    void GenerateInitialBoard()
    {
        boardStartX = -((initialColumns - 1) * tileSize) / 2f;
        boardStartZ = -((initialRows - 1) * tileSize) / 2f;
        minGrid = new Vector2Int(0, 0);
        maxGrid = new Vector2Int(initialColumns - 1, initialRows - 1);

        for (int z = 0; z < initialRows; z++)
        {
            for (int x = 0; x < initialColumns; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);
                Vector3 worldPos = new Vector3(boardStartX + x * tileSize, startY, boardStartZ + z * tileSize);
                float delay = (x + z) * delayMultiplier;
                CreateTile(gridPos, worldPos, delay, fallDuration);
            }
        }
    }

    void CreateTile(Vector2Int gridPos, Vector3 worldSpawnPos, float delay, float duration)
    {
        if (tiles.ContainsKey(gridPos))
            return;

        GameObject tile = tilePrefab != null
            ? Instantiate(tilePrefab, worldSpawnPos, Quaternion.identity)
            : GameObject.CreatePrimitive(PrimitiveType.Quad);

        if (tilePrefab == null)
        {
            tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        tile.transform.parent = this.transform;

        Material chosenMat = ((gridPos.x + gridPos.y) % 2 == 0) ? material1 : material2;
        MeshRenderer[] renderers = tile.GetComponentsInChildren<MeshRenderer>();

        foreach (var rend in renderers)
        {
            rend.material = new Material(chosenMat);
            Color col = rend.material.color;
            col.a = 0f;
            rend.material.color = col;
        }

        tiles.Add(gridPos, tile);

        Vector3 targetPos = new Vector3(boardStartX + gridPos.x * tileSize, endY, boardStartZ + gridPos.y * tileSize);
        StartCoroutine(AnimateTile(tile, targetPos, duration, delay));
    }

    public void AddWave(WaveDirection direction, int waveColumns, int waveRows, Vector2Int extraGridOffset = default(Vector2Int))
    {
        StartCoroutine(AddWaveCoroutine(direction, waveColumns, waveRows, extraGridOffset));
    }

    IEnumerator AddWaveCoroutine(WaveDirection direction, int waveColumns, int waveRows, Vector2Int extraGridOffset)
    {
        yield return new WaitForSeconds(waveStartDelay);

        int newStartX = 0;
        int newStartY = 0;

        switch (direction)
        {
            case WaveDirection.North:
                newStartY = maxGrid.y + 1;
                newStartX = Mathf.RoundToInt((minGrid.x + maxGrid.x - (waveColumns - 1)) / 2f);
                break;
            case WaveDirection.South:
                newStartY = minGrid.y - waveRows;
                newStartX = Mathf.RoundToInt((minGrid.x + maxGrid.x - (waveColumns - 1)) / 2f);
                break;
            case WaveDirection.East:
                newStartX = maxGrid.x + 1;
                newStartY = Mathf.RoundToInt((minGrid.y + maxGrid.y - (waveRows - 1)) / 2f);
                break;
            case WaveDirection.West:
                newStartX = minGrid.x - waveColumns;
                newStartY = Mathf.RoundToInt((minGrid.y + maxGrid.y - (waveRows - 1)) / 2f);
                break;
        }

        Vector2Int waveStartGrid = new Vector2Int(newStartX, newStartY) + extraGridOffset;

        for (int row = 0; row < waveRows; row++)
        {
            for (int col = 0; col < waveColumns; col++)
            {
                Vector2Int gridPos = new Vector2Int(waveStartGrid.x + col, waveStartGrid.y + row);
                if (!tiles.ContainsKey(gridPos))
                {
                    Vector3 worldPos = new Vector3(boardStartX + gridPos.x * tileSize, startY, boardStartZ + gridPos.y * tileSize);
                    float tileDelay = (col + row) * waveDelayMultiplier;
                    CreateTile(gridPos, worldPos, tileDelay, waveTileFallDuration);
                    yield return null;
                }
            }
        }

        UpdateBoardBounds();

        if (onWaveAdded != null)
        {
            onWaveAdded.Invoke();
        }
    }

    void UpdateBoardBounds()
    {
        foreach (Vector2Int key in tiles.Keys)
        {
            if (key.x < minGrid.x) minGrid.x = key.x;
            if (key.y < minGrid.y) minGrid.y = key.y;
            if (key.x > maxGrid.x) maxGrid.x = key.x;
            if (key.y > maxGrid.y) maxGrid.y = key.y;
        }
    }

    IEnumerator AnimateTile(GameObject tile, Vector3 targetPos, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float controlY = (4 * lowestY - startY - endY) / 2f;

        MeshRenderer[] renderers = tile.GetComponentsInChildren<MeshRenderer>();

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float newY = Mathf.Pow(1 - t, 2) * startY + 2 * (1 - t) * t * controlY + Mathf.Pow(t, 2) * endY;
            tile.transform.position = new Vector3(targetPos.x, newY, targetPos.z);

            foreach (var rend in renderers)
            {
                Color col = rend.material.color;
                col.a = Mathf.Lerp(0f, 1f, t);
                rend.material.color = col;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        tile.transform.position = targetPos;

        foreach (var rend in renderers)
        {
            Color col = rend.material.color;
            col.a = 1f;
            rend.material.color = col;
        }

        yield return StartCoroutine(FlashTile(renderers));
    }

    IEnumerator FlashTile(MeshRenderer[] renderers)
    {
        float elapsed = 0f;
        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;

        while (elapsed < flashEaseInDuration)
        {
            float t = elapsed / flashEaseInDuration;
            float easeT = t * t;
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].material.color = Color.Lerp(originalColors[i], Color.white, easeT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var rend in renderers)
            rend.material.color = Color.white;

        elapsed = 0f;
        while (elapsed < flashEaseOutDuration)
        {
            float t = elapsed / flashEaseOutDuration;
            float easeT = 1 - Mathf.Pow(1 - t, 2);
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].material.color = Color.Lerp(Color.white, originalColors[i], easeT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++){
            renderers[i].material.color = originalColors[i];
            SetMaterialToOpaque(renderers[i].material);
            //renderers[i].material.translucent = false;
        }


    }

    void SetMaterialToOpaque(Material mat)
    {
        mat.SetFloat("_Mode", 0); // 0 = Opaque
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;
    }


    public void ExtendUp()
    {
        int boardWidth = maxGrid.x - minGrid.x + 1;
        AddWave(WaveDirection.North, boardWidth, 1);
    }

    public void ExtendDown()
    {
        int boardWidth = maxGrid.x - minGrid.x + 1;
        AddWave(WaveDirection.South, boardWidth, 1);
    }

    public void ExtendRight()
    {
        int boardHeight = maxGrid.y - minGrid.y + 1;
        AddWave(WaveDirection.East, 1, boardHeight);
    }

    public void ExtendLeft()
    {
        int boardHeight = maxGrid.y - minGrid.y + 1;
        AddWave(WaveDirection.West, 1, boardHeight);
    }
}
