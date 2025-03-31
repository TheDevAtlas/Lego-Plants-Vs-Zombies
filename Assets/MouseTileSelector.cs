using UnityEngine;

public class MouseTileSelector : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject selector;
    public Vector2Int gridSize = new Vector2Int(9, 5);
    public float tileSize = 1f;
    public Vector3 gridOrigin = Vector3.zero; // Set in inspector or code

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (selector != null)
            selector.SetActive(false);
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, gridOrigin); // assumes flat ground

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 localHit = hitPoint - gridOrigin;

            int gridX = Mathf.FloorToInt(localHit.x / tileSize);
            int gridZ = Mathf.FloorToInt(localHit.z / tileSize);

            if (gridX >= 0 && gridX < gridSize.x && gridZ >= 0 && gridZ < gridSize.y)
            {
                Vector3 tilePos = new Vector3(
                    gridOrigin.x + gridX * tileSize + tileSize / 2f,
                    gridOrigin.y + 0.01f,
                    gridOrigin.z + gridZ * tileSize + tileSize / 2f
                );

                selector.SetActive(true);
                selector.transform.position = tilePos;
                selector.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
            }
            else
            {
                selector.SetActive(false);
            }
        }
        else
        {
            selector.SetActive(false);
        }
    }
}
