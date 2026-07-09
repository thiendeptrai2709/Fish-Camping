using UnityEngine;

[CreateAssetMenu(fileName = "NewBait", menuName = "Inventory/Bait SO")]
public class BaitSO : ItemShapeSO
{
    [Header("Bait Stats")]
    public float attractivenessBonus = 20f;
    public float waitTimeReduction = 1.5f;
    public int targetRarityBonus = 1;
    public int maxUses = 5;
}