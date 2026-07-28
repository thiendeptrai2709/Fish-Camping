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
        return $"• Độ nổi: {buoyancy}\n" +
               $"• Tăng thu hút: +{attractivenessBonus}%\n" +
               $"• Trọng lượng ném: {weight}kg";
    }
}