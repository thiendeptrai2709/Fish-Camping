using UnityEngine;

// Định nghĩa độ hiếm của cá
public enum FishRarity
{
    Common,     // Phổ biến
    Uncommon,   // Ít gặp
    Rare,       // Hiếm
    Legendary   // Truyền thuyết
}
public enum FishGrade
{
    Normal,
    Bronze,
    Silver,
    Gold
}
[CreateAssetMenu(fileName = "NewFish", menuName = "Inventory/Fish Data")]
public class FishSO : ItemShapeSO
{
    [Header("--- FISH SPECIFIC DATA ---")]
    public FishRarity rarity = FishRarity.Common;

    [Tooltip("Prefab mô hình 3D của con cá khi hiển thị trên tay lúc câu được")]
    public GameObject caughtFishPrefab;

    [Tooltip("Độ khó khi kéo (ảnh hưởng đến tốc độ của kim trong Minigame)")]
    [Range(1f, 10f)]
    public float difficulty = 1f;

    [Tooltip("Giá bán cơ bản của con cá này")]
    public int basePrice = 50;

    [Header("--- FISH SIZE (cm/kg) ---")]
    public float minLength = 10f;
    public float maxLength = 30f;
    public float minWeight = 0.5f;
    public float maxWeight = 2.0f;

    [Header("--- FISH GRADE CHANCES (%) ---")]
    public float normalChance = 60f;
    public float bronzeChance = 25f;
    public float silverChance = 10f;
    public float goldChance = 5f;
    // Hàm tiện ích để random kích thước cá mỗi khi câu được
    public void GenerateRandomSize(out float length, out float weight)
    {
        length = Random.Range(minLength, maxLength);
        weight = Random.Range(minWeight, maxWeight);
    }
    public FishGrade GenerateRandomGrade()
    {
        float randomVal = Random.Range(0f, 100f);
        if (randomVal <= goldChance) return FishGrade.Gold;
        if (randomVal <= goldChance + silverChance) return FishGrade.Silver;
        if (randomVal <= goldChance + silverChance + bronzeChance) return FishGrade.Bronze;
        return FishGrade.Normal;
    }
}