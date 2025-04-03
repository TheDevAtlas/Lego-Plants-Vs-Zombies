using UnityEngine;
using UnityEngine.EventSystems;

public class SidebarPlantSelector : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    // Reference to the PlantingController to notify when a plant is selected.
    public PlantingController plantingController;
    
    // (Optional) Reference to a SeedController if you need to pass specific seed data.
    SeedController seedController;

    void Start()
    {
        seedController = GetComponent<SeedController>();
    }

    // This method is called when the sidebar UI element is clicked.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (plantingController != null)
        {
            // Optionally, add any visual or sound feedback for selection here.
            if(plantingController.ghostPlant == null)
            {
                Destroy(plantingController.ghostPlant);
            }
            
            plantingController.plantSelected = false;
            plantingController.seedController = seedController;

            // Notify the planting controller that a plant has been selected.
            plantingController.OnPlantSelected();

            // (Optional) If you want to pass seedController data or perform additional logic,
            // you can add that here.
        }
        else
        {
            Debug.LogWarning("SidebarPlantSelector: PlantingController reference is not set.");
        }
    }
}
