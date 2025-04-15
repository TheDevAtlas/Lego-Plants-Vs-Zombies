using UnityEngine;
using System.Collections;
using TMPro;

public class PlantingController : MonoBehaviour
{
    public int SunCount = 0;
    public TextMeshProUGUI SunCountText;

    // Singleton instance for easy access.
    public static PlantingController Instance { get; private set; }

    // The currently selected seed (its ScriptableObject) and seed packet.
    public Seed selectedSeed;
    public SeedPacket selectedSeedPacket;

    // Ghost material to apply to the preview object.
    public Material ghostMaterial;

    // The current ghost preview instance (if any).
    private GameObject ghostInstance;
    public GameController gameController;

    // Called when the script instance is being loaded.
    void Awake()
    {
        // Implementing a simple singleton pattern.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddSun(int ammount)
    {
        SunCount += ammount;
        SunCountText.text = SunCount + "";
    }

    /// <summary>
    /// Called by another script (such as GameController or directly from the seed packet)
    /// when a seed packet is clicked and selected for planting.
    /// </summary>
    /// <param name="packet">The SeedPacket that was clicked.</param>
    public void SetSelectedSeed(SeedPacket packet)
    {
        selectedSeedPacket = packet;
        selectedSeed = packet.seed;
    }

    void Update()
    {
        // Only proceed if a seed has been selected.
        if (selectedSeed != null)
        {
            // Cast a ray from the camera to the current mouse position.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // If the ray hits an object in the scene…
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object is tagged as "tile".
                if (hit.collider.CompareTag("tile"))
                {
                    // Get the tile’s transform position.
                    Vector3 tilePosition = hit.collider.transform.position;

                    // Show the ghost preview if it does not already exist.
                    if (ghostInstance == null && selectedSeed.plantPrefab != null)
                    {
                        ghostInstance = Instantiate(selectedSeed.plantPrefab, tilePosition + selectedSeed.offset, Quaternion.identity);

                        ghostInstance.transform.Rotate(selectedSeed.rotateOffset);
                        ApplyGhostMaterial(ghostInstance);

                        // Disable scripts so that ghost plants do not work as real plants //
                        Collider ghostCollider = ghostInstance.GetComponent<Collider>();
                        if (ghostCollider) {ghostCollider.enabled = false;}

                        PeaShooter ghostShooter = ghostInstance.GetComponent<PeaShooter>();
                        if (ghostShooter) {ghostShooter.enabled = false;}

                        LobProjectile ghostLob = ghostInstance.GetComponent<LobProjectile>();
                        if (ghostLob) {ghostLob.enabled = false;}

                        CherryBomb ghostCherry = ghostInstance.GetComponent<CherryBomb>();
                        if (ghostCherry) 
                        {
                            ghostCherry.enabled = false;
                            ghostInstance.GetComponent<Animator>().enabled = false;
                        }
                    }
                    else if (ghostInstance != null)
                    {
                        // Update the ghost position to follow the current tile.
                        ghostInstance.transform.position = tilePosition + selectedSeed.offset;
                    }

                    // If the player clicks while over this valid tile…
                    if (Input.GetMouseButtonDown(0))
                    {
                        bool tileOccupied = false;

                        // Check if the tile already has a plant.
                        // Option 1: Use a dedicated Tile component (recommended).
                        if (hit.collider.transform.childCount > 0)
                        {
                            tileOccupied = true;
                            return;
                        }

                        

                        if (!tileOccupied && selectedSeedPacket.isReady)
                        {
                            if (SunCount >= selectedSeed.sunCost)
                            {
                                AddSun(-selectedSeed.sunCost);

                                // Plant the seed by instantiating the actual plant prefab.
                                GameObject newPlant = Instantiate(selectedSeed.plantPrefab, tilePosition + selectedSeed.offset, Quaternion.identity);
                                newPlant.transform.Rotate(selectedSeed.rotateOffset);
                                newPlant.transform.SetParent(hit.collider.transform);
                            }
                            else
                            {
                                return;
                            }

                                
                            

                            // Notify the seed packet that its seed was used.
                            // You could call a dedicated method on the seed packet.
                            if (selectedSeedPacket != null)
                            {

                                // For example, call a method that handles cooldown, deselection, etc.
                                selectedSeedPacket.UseSeed();
                                gameController.selectorIcon.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            Debug.Log("Cannot plant here – a plant already exists on this tile.");
                        }

                        // Clear the planting selection and destroy the ghost preview.
                        selectedSeed = null;
                        selectedSeedPacket = null;
                        if (ghostInstance != null)
                        {
                            Destroy(ghostInstance);
                        }
                    }
                }
                else
                {
                    // If the raycast did not hit a tile, remove the ghost preview (if it exists).
                    if (ghostInstance != null)
                    {
                        Destroy(ghostInstance);
                    }
                }
            }
            else
            {
                // Optionally, if nothing is hit, destroy any ghost preview.
                if (ghostInstance != null)
                {
                    Destroy(ghostInstance);
                }
            }
        }
    }

    /// <summary>
    /// Applies the ghost material to all Renderer components in the given object.
    /// This gives the preview a translucent (or otherwise modified) appearance.
    /// </summary>
    /// <param name="obj">The GameObject to apply the ghost material to.</param>
    private void ApplyGhostMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Apply the ghost material.
            renderer.material = ghostMaterial;
        }
    }
}
