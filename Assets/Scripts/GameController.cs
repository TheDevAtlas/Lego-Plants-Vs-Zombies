using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 5;
    public int gridHeight = 9;
    public float cellSize = 1f;
    public Vector3 gridCenter = Vector3.zero;

    [Header("Planting")]
    public GameObject[] plantPrefabs;
    public Image selectorIcon;
    private int selectedPlantIndex = 0;

    private GameObject[,] grid;
    private Vector3 gridOrigin;

    void Start()
    {
        InitializeGrid();
        UpdateSelectorIcon();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPlant();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            TryRemovePlant();
        }
    }

    void InitializeGrid()
    {
        grid = new GameObject[gridWidth, gridHeight];
        gridOrigin = gridCenter - new Vector3((gridWidth - 1) * cellSize / 2f, 0f, (gridHeight - 1) * cellSize / 2f);
    }

    void TryPlant()
    {
        if (RaycastToGrid(out int x, out int z))
        {
            if (grid[x, z] == null)
            {
                Vector3 plantPos = gridOrigin + new Vector3(x * cellSize, 0f, z * cellSize);
                GameObject plant = Instantiate(plantPrefabs[selectedPlantIndex], plantPos, Quaternion.identity);
                grid[x, z] = plant;
            }
            else
            {
                Debug.Log("Tile already occupied!");
            }
        }
    }

    void TryRemovePlant()
    {
        if (RaycastToGrid(out int x, out int z))
        {
            if (grid[x, z] != null)
            {
                Destroy(grid[x, z]);
                grid[x, z] = null;
            }
        }
    }

    bool RaycastToGrid(out int x, out int z)
    {
        x = -1;
        z = -1;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 hitPoint = hit.point;
            x = Mathf.RoundToInt((hitPoint.x - gridOrigin.x) / cellSize);
            z = Mathf.RoundToInt((hitPoint.z - gridOrigin.z) / cellSize);

            if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
            {
                return true;
            }
        }

        return false;
    }

    public void SelectPlant(int index)
    {
        selectedPlantIndex = index;
        UpdateSelectorIcon();
    }

    void UpdateSelectorIcon()
    {
        if (selectorIcon != null && plantPrefabs.Length > selectedPlantIndex)
        {
            SpriteRenderer sr = plantPrefabs[selectedPlantIndex].GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                selectorIcon.sprite = sr.sprite;
        }
    }
}
