using UnityEngine;
using UnityEngine.EventSystems;

public class PlantingController : MonoBehaviour
{
    [Header("Seed & Prefab References")]
    // Reference to the SeedController script holding the PlantPrefab.
    public SeedController seedController;
    // The material to use for the ghost (preview) plant.
    public Material ghostMaterial;

    [Header("Board Setup")]
    // Layer on which the game board resides (make sure your board collider is set to this layer).
    public LayerMask boardLayer;
    // How many cells across (x) and rows (z) the board has.
    public int gridWidth = 9;
    public int gridHeight = 5;
    // The size (width/depth) of each cell. Adjust if your board cells aren’t 1 unit in size.
    public float cellSize = 1f;
    // The world position of the board’s bottom-left corner (or you can adjust this as needed).
    public Vector3 boardOrigin;

    // The ghost plant instance that follows the mouse.
    private GameObject ghostPlant;
    // 2D array to track the plant objects in each grid cell.
    private GameObject[,] gridOccupied;

    void Start()
    {
        // Initialize the grid occupancy array.
        gridOccupied = new GameObject[gridWidth, gridHeight];

        // Ensure the SeedController and PlantPrefab are assigned.
        if (seedController != null && seedController.PlantPrefab != null)
        {
            // Instantiate the ghost plant from the PlantPrefab.
            ghostPlant = Instantiate(seedController.PlantPrefab);
            // Apply the ghost material so it appears semi-transparent.
            ApplyGhostMaterial(ghostPlant);
        }
        else
        {
            Debug.LogError("SeedController or PlantPrefab not assigned!");
        }
    }

    void Update()
    {
        // If the ghost plant hasn't been created, exit.
        if (ghostPlant == null)
            return;

        // Prevent planting if the pointer is over a UI element.
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Raycast from the camera into the scene using the mouse position.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, boardLayer))
        {
            // Get the hit point on the board.
            Vector3 hitPoint = hit.point;
            // Convert the hit point to grid cell indices (assuming boardOrigin is at the bottom left).
            int xIndex = Mathf.FloorToInt((hitPoint.x - boardOrigin.x) / cellSize);
            int zIndex = Mathf.FloorToInt((hitPoint.z - boardOrigin.z) / cellSize);

            // Check that the indices are within the grid boundaries.
            if (xIndex >= 0 && xIndex < gridWidth && zIndex >= 0 && zIndex < gridHeight)
            {
                // Calculate the center of the current grid cell.
                Vector3 cellCenter = new Vector3(
                    boardOrigin.x + xIndex * cellSize + cellSize / 2f,
                    boardOrigin.y,  // Alternatively, set a fixed y-value if needed.
                    boardOrigin.z + zIndex * cellSize + cellSize / 2f
                );

                // Move the ghost plant to the cell center.
                ghostPlant.transform.position = cellCenter;

                // If the player clicks (left mouse button) and the cell is not already occupied...
                if (Input.GetMouseButtonDown(0))
                {
                    if (gridOccupied[xIndex, zIndex] == null)
                    {
                        // Place the real plant.
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

    // Instantiates the real plant at the given position and marks the grid cell as occupied.
    void PlantAt(Vector3 position, int xIndex, int zIndex)
    {
        GameObject plant = Instantiate(seedController.PlantPrefab, position, Quaternion.identity);
        plant.transform.Rotate(0f, 90f, 0f);
        gridOccupied[xIndex, zIndex] = plant;
        // (Optional) Play a planting sound or animation here.
    }

    // Applies the ghost material to all renderers on the ghost object.
    void ApplyGhostMaterial(GameObject ghostObj)
    {
        Renderer[] renderers = ghostObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            // Replace the material with the ghost material.
            rend.material = ghostMaterial;
        }
        // Optionally, adjust the ghost material's transparency.
        Color color = ghostMaterial.color;
        color.a = 0.5f; // 50% transparent for ghost effect.
        ghostMaterial.color = color;
    }
}
