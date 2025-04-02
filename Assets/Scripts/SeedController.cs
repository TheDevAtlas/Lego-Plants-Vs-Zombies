using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeedController : MonoBehaviour
{

    // Game Relevent Stuff //
    public int SeedCost;
    public GameObject PlantPrefab;
    public Sprite plantImage;
    
    // UI //
    public Image UIPlantImage;
    public Image UIPlantImageShadow;
    public TextMeshProUGUI UISeedCost;

    void Start()
    {
        UISeedCost.text = "" + SeedCost;

        UIPlantImage.sprite = plantImage;
        UIPlantImageShadow.sprite = plantImage;
    }
}
