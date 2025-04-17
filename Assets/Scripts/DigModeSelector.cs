using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DigModeSelector : MonoBehaviour, IPointerClickHandler
{
    public Material redTransparentMaterial;
    public PlantingController plantingController;

    private bool digModeActive = false;
    private GameObject lastHoveredPlant;
    private List<Material[]> originalMaterials = new List<Material[]>();
    private LayerMask plantLayerMask;

    public Color selectedColor;

    void Start()
    {
        plantLayerMask = LayerMask.GetMask("Plant"); // Make sure your plants are on this layer
    }

    void Update()
    {
        if (!digModeActive || plantingController.selectedSeed != null)
        {
            digModeActive = false;
            GetComponent<Image>().color = Color.white;
            return;
        }
        else
        {
            GetComponent<Image>().color = selectedColor;
        } 


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check for plant under cursor
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, plantLayerMask))
        {
            GameObject plant = hit.collider.gameObject;

            if (plant != lastHoveredPlant)
            {
                ResetLastPlant(); // Reset any previously hovered plant
                HighlightPlant(plant); // Highlight the new one
                lastHoveredPlant = plant;
            }

            // If clicked, delete plant
            if (Input.GetMouseButtonDown(0))
            {
                int choice = UnityEngine.Random.Range(0, 2); // 0, 1, or 2

                switch (choice)
                {
                    case 0:
                        AudioManager.instance.Play("Plant");
                        break;
                    case 1:
                        AudioManager.instance.Play("Plant2");
                        break;
                }
                Destroy(plant);
                lastHoveredPlant = null;
                digModeActive = false;
            }
        }
        else
        {
            ResetLastPlant();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        digModeActive = !digModeActive;

        GameController.Instance.selectorIcon.gameObject.SetActive(false);

        plantingController.selectedSeed = null;
        plantingController.selectedSeedPacket = null;

        if (plantingController.ghostMaterial != null && plantingController.transform.childCount > 0)
        {
            Destroy(plantingController.transform.GetChild(0).gameObject);
        }

        Debug.Log("Dig Mode " + (digModeActive ? "Activated" : "Deactivated"));
    }

    void HighlightPlant(GameObject plant)
    {
        Renderer[] renderers = plant.GetComponentsInChildren<Renderer>();
        originalMaterials.Clear();

        foreach (Renderer renderer in renderers)
        {
            originalMaterials.Add(renderer.materials);
            Material[] redMats = new Material[renderer.materials.Length];
            for (int i = 0; i < redMats.Length; i++)
            {
                redMats[i] = redTransparentMaterial;
            }
            renderer.materials = redMats;
        }
    }

    void ResetLastPlant()
    {
        if (lastHoveredPlant == null) return;

        Renderer[] renderers = lastHoveredPlant.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length && i < originalMaterials.Count; i++)
        {
            renderers[i].materials = originalMaterials[i];
        }

        lastHoveredPlant = null;
        originalMaterials.Clear();
    }
}
