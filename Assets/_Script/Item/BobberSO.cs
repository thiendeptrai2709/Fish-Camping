using UnityEngine;

[CreateAssetMenu(fileName = "NewBobber", menuName = "Inventory/Bobber SO")]
public class BobberSO : ItemShapeSO
{
    [Header("Bobber General Stats")]
    public float attractivenessBonus = 5f; // Tăng một chút độ thu hút cá
    public float buoyancy = 1f; // Độ nổi của phao trên mặt nước

    [Header("3D Bobber Visuals")]
    [Tooltip("Prefab 3D của phao sẽ được sinh ra bay xuống nước khi quăng cần")]
    public GameObject bobberPrefab;

    [Header("Bobber Physics")]
    [Tooltip("Trọng lượng của phao (ảnh hưởng đến tốc độ bay khi ném)")]
    public float weight = 0.5f;
    public override string GetFormattedStats()
    {
        int tier = 1;
        string n = (string.IsNullOrEmpty(itemID) ? name : itemID).ToLower();
        for (int i = 8; i >= 1; i--)
        {
            if (n.Contains(i.ToString())) { tier = i; break; }
        }

        string mapInfo = tier <= 6 
            ? "<color=#81C784>Map 2 & Map 3 (Nước ngọt)</color>" 
            : "<color=#4FC3F7>Map 4 (Phao biển chịu sóng)</color>";

        return $"• Khu vực: {mapInfo}\n" +
               $"• Độ ổn định Minigame: +{buoyancy:F1}x\n" +
               $"• Tăng kích thước cá: +{attractivenessBonus}%\n" +
               $"• Trọng lượng ném: {weight}kg";
    }
}