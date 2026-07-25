using UnityEngine;

[CreateAssetMenu(fileName = "NewFishingRod", menuName = "Inventory/Fishing Rod SO")]
public class FishingRodSO : ItemShapeSO
{
    [Header("Rod General Stats")]
    public int rodTier = 1;
    public float maxDurability = 100f;

    [Header("Fishing Performance")]
    public float fishingPower = 15f;
    [Tooltip("Khoảng cách ném tối đa của cần (tính bằng mét/đơn vị Unity)")]
    public float castDistance = 15f;
    [Tooltip("Hệ số nhân độ xa cho từng Zone (0: Xịt, 1: Nhẹ, 2: Vừa, 3: Mạnh)")]
    public float[] zoneDistanceMultipliers = new float[] { 0f, 0.35f, 0.7f, 1.0f };
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
    public override string GetFormattedStats()
    {
        return $"• Cấp độ: Tier {rodTier}\n" +
               $"• Lực kéo cá: {fishingPower}\n" +
               $"• Tầm ném tối đa: {castDistance}m\n" +
               $"• Giảm thời gian chờ: {waitTimeReductionPercentage}%\n" +
               $"• Độ bền: {maxDurability}";
    }
}