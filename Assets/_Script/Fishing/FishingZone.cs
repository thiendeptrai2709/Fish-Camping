using UnityEngine;

public class FishingZone : MonoBehaviour
{
    [Header("--- FISHING ZONE INFO ---")]
    public string zoneName = "Hồ Nước Ngọt";

    [Tooltip("Danh sách các loại cá có thể câu được ở vùng nước này")]
    public FishSO[] zoneFishes;

    // Hàm trả về ngẫu nhiên 1 con cá trong danh sách của vùng này
    public FishSO GetRandomFish()
    {
        if (zoneFishes == null || zoneFishes.Length == 0) return null;
        int randomIndex = Random.Range(0, zoneFishes.Length);
        return zoneFishes[randomIndex];
    }
}