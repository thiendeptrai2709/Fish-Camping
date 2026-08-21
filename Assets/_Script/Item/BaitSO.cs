using UnityEngine;

[CreateAssetMenu(fileName = "NewBait", menuName = "Inventory/Bait SO")]
public class BaitSO : ItemShapeSO
{
    [Header("Bait Stats")]
    public float attractivenessBonus = 20f;
    public float waitTimeReduction = 1.5f;
    public int targetRarityBonus = 1;
    public int maxUses = 5;

    public override string GetFormattedStats()
    {
        int tier = 1;
        string n = (string.IsNullOrEmpty(itemID) ? name : itemID).ToLower();
        for (int i = 6; i >= 1; i--)
        {
            if (n.Contains(i.ToString())) { tier = i; break; }
        }

        string mapInfo = tier <= 4 
            ? "<color=#81C784>Map 2 & Map 3 (Nước ngọt)</color>" 
            : "<color=#4FC3F7>Map 4 (Mồi câu biển)</color>";

        return $"• Khu vực: {mapInfo}\n" +
               $"• Tăng độ thu hút: +{attractivenessBonus}%\n" +
               $"• Giảm thời gian chờ: -{waitTimeReduction}s\n" +
               $"• Tăng tỷ lệ cá hiếm: +{targetRarityBonus}\n" +
               $"• Số lần dùng tối đa: {maxUses}";
    }
}