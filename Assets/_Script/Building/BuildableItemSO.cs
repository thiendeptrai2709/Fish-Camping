using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildableItem", menuName = "Building/Buildable Item")]
public class BuildableItemSO : ScriptableObject
{
    public string itemName;
    public GameObject prefab;
    public GameObject previewPrefab; // Mô hình nửa trong suốt để ngắm trước khi đặt
    public float energyCost = 3f;
    public float comfortBonus = 10f;
}