using UnityEngine;

[System.Serializable]
public class FishData
{
    public string fishName;          // Tên cá (VD: Cá Vược, Cá Trê)
    public GameObject fishPrefab;    // Model 3D của cá

    [Range(0f, 100f)]
    public float spawnChance = 50f;  // Tỷ lệ xuất hiện (%)

    [Header("Giới Hạn Số Lượng")]
    public int maxSimultaneous = 15;  // Tối đa bao nhiêu con loại này được sống cùng lúc

    [Header("Chu Kỳ Thời Gian Xuất Hiện")]
    public bool spawnMorning = true;   // Sáng (0.2 -> 0.5)
    public bool spawnAfternoon = true; // Trưa chiều (0.5 -> 0.7)
    public bool spawnEvening = true;   // Hoàng hôn (0.7 -> 0.8)
    public bool spawnNight = true;     // Đêm (0.8 -> 1.0 và 0.0 -> 0.2)

    [HideInInspector]
    public int currentCount = 0;       // Bộ đếm tự động (ẩn khỏi Inspector)
}