using UnityEngine;

[CreateAssetMenu(fileName = "NewSeed", menuName = "Plant/Seed")]
public class Seed : ScriptableObject
{
    [Header("Seed Info")]
    public string plantName;         // The name of the plant.
    public int sunCost;              // The sun cost to deploy this plant.
    public float rechargeTime;       // Cooldown time before the seed is ready again.
    
    [Header("Seed Visuals")]
    public Sprite packetImage;       // Image for the seed packet (shown in UI).

    [Header("In-Game References")]
    public GameObject plantPrefab;   // The plant prefab that will be instantiated in game.
    public Vector3 offset;
    public Vector3 rotateOffset;
}
