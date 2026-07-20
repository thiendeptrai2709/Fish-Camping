using UnityEngine;

public enum ItemType { FishingRod, Bait, Fish }

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Game/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    public string itemID;        // ID duy nhất (Ví dụ: bait_01, rod_carbon)
    public string itemName;      // Tên hiển thị (Ví dụ: Mồi bột cao cấp)
    public ItemType itemType;    // Loại vật phẩm
    public int price;            // Giá mua hoặc giá bán (Gold)
    public int requiredLevel;    // Cấp độ Fishing Level yêu cầu để mở khóa
    [TextArea(2, 4)]
    public string description;   // Mô tả công năng vật phẩm
}