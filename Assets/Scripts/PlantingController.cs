using UnityEngine;
using UnityEngine.EventSystems;

public class PlantingController : MonoBehaviour
{
    [Header("Seed & Prefab References")]
    public SeedController seedController;
    public Material ghostMaterial;

    [Header("Board Setup")]
    public LayerMask boardLayer;
    public int gridWidth = 9;
    public int gridHeight = 5;
    public float cellSize = 1f;
    public Vector3 boardOrigin;

    public GameObject ghostPlant;
    private GameObject[,] gridOccupied;

    // This flag tracks whether the player has selected a plant from the sidebar.
    public bool plantSelected = false;

    void Start()
    {
        // Initialize the grid occupancy array.
        gridOccupied = new GameObject[gridWidth, gridHeight];

        // Do not instantiate the ghost plant until a plant is selected.
        if (seedController == null)
        {
            Debug.LogError("SeedController not assigned!");
        }
    }

    // This method should be called by your sidebar selection UI.
    public void OnPlantSelected()
    {
        // Only create the ghost plant if a selection hasn't been made yet.
        if (!plantSelected && seedController != null && seedController.PlantPrefab != null)
        {
            ghostPlant = Instantiate(seedController.PlantPrefab, new Vector3(-10f, 0f, 0f), Quaternion.identity);
            ghostPlant.transform.Rotate(new Vector3(0f, 90f, 0f));
            ApplyGhostMaterial(ghostPlant);
            plantSelected = true;
        }
    }

    void Update()
    {
        // If no plant has been selected, do nothing.
        if (!plantSelected || ghostPlant == null)
            return;

        // Do not process planting when the pointer is over a UI element.
        if (EventSystem.current.IsPointerOverGameObject()) 
            return;

        // Raycast from the camera using the current mouse position.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, boardLayer))
        {
            Vector3 hitPoint = hit.point;
            int xIndex = Mathf.FloorToInt((hitPoint.x - boardOrigin.x) / cellSize);
            int zIndex = Mathf.FloorToInt((hitPoint.z - boardOrigin.z) / cellSize);

            if (xIndex >= 0 && xIndex < gridWidth && zIndex >= 0 && zIndex < gridHeight)
            {
                // Calculate the center of the current grid cell.
                Vector3 cellCenter = new Vector3(
                    boardOrigin.x + xIndex * cellSize + cellSize / 2f,
                    boardOrigin.y,
                    boardOrigin.z + zIndex * cellSize + cellSize / 2f
                );

                // Move the ghost plant to follow the mouse on the grid.
                ghostPlant.transform.position = cellCenter;

                // When the player clicks, place the plant if the cell is free.
                if (Input.GetMouseButtonDown(0))
                {
                    if (gridOccupied[xIndex, zIndex] == null)
                    {
                        PlantAt(cellCenter, xIndex, zIndex);
                    }
                    else
                    {
                        Debug.Log("Cell already occupied!");
                    }
                }
            }
        }
    }

    // Instantiate the actual plant and mark the grid cell as occupied.
    void PlantAt(Vector3 position, int xIndex, int zIndex)
    {
        GameObject plant = Instantiate(seedController.PlantPrefab, position, Quaternion.identity);
        plant.transform.Rotate(0f, 90f, 0f);
        gridOccupied[xIndex, zIndex] = plant;
        plantSelected = false;
        Destroy(ghostPlant);
        // (Optional) You could reset plantSelected to false here if you only allow one planting per selection.
    }

    // Apply a ghost material to make the preview plant semi-transparent.
    void ApplyGhostMaterial(GameObject ghostObj)
    {
        Renderer[] renderers = ghostObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.material = ghostMaterial;
        }
        Color color = ghostMaterial.color;
        color.a = 0.5f; // 50% transparency for the ghost effect.
        ghostMaterial.color = color;
    }
}
