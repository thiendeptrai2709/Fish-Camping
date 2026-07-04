using UnityEngine;

[CreateAssetMenu(fileName = "NewFishingRod", menuName = "Inventory/Fishing Rod SO")]
public class FishingRodSO : ItemShapeSO
{
    [Header("Rod General Stats")]
    public int rodTier = 1;
    public float maxDurability = 100f;

    [Header("Fishing Performance")]
    public float fishingPower = 15f;
    public float castDistance = 5f;
    public float waitTimeReductionPercentage = 0f;
    public int maxCatchableRarity = 1;

    [Header("Line & Reel Stats")]
    public float lineBreakResistance = 20f;
    public float reelSpeed = 2f;

    public bool CanCatchFishRarity(int fishRarity)
    {
        return fishRarity <= maxCatchableRarity;
    }

    public float CalculateEffectiveWaitTime(float baseFishWaitTime)
    {
        float reduction = baseFishWaitTime * (waitTimeReductionPercentage / 100f);
        return Mathf.Max(0.5f, baseFishWaitTime - reduction);
    }

    public float CalculateDamageToFish(float playerBonusPower = 0f)
    {
        return fishingPower + playerBonusPower;
    }
}