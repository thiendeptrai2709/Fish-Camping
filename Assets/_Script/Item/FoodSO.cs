using UnityEngine;

[CreateAssetMenu(fileName = "NewFood", menuName = "Inventory/Food Data")]
public class FoodSO : ItemShapeSO
{
    [Header("--- THÔNG TIN MÓN ĂN ---")]
    [Tooltip("Mô tả độ ngon của món ăn")]
    public string description = "Một món ăn nóng hổi vừa ra lò.";

    [Header("--- CHỈ SỐ HỒI PHỤC ---")]
    [Tooltip("Lượng độ no (giảm đói) hồi lại")]
    public float hungerRestore = 50f;

    [Tooltip("Lượng nước (giảm khát) hồi lại")]
    public float thirstRestore = 15f;

    [Tooltip("Lượng thể lực/năng lượng hồi lại")]
    public float energyRestore = 20f;

    [Tooltip("Giá bán của món ăn này")]
    public int sellPrice = 100;
}