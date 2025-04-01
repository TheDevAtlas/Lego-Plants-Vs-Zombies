using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PlantedImage : MonoBehaviour, IPointerClickHandler
{
    private SeedPacket seedPacket;

    private void Start()
    {
        seedPacket = GetComponentInParent<SeedPacket>();
        if (seedPacket == null)
        {
            Debug.LogWarning("PlantedImage: Could not find SeedPacket in parent.");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (seedPacket != null)
        {
            seedPacket.ReturnPlantToSeed();
        }
    }
}
