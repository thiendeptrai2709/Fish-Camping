using UnityEngine;

public class ComfortStatController : MonoBehaviour
{
    public CharacterStatsManager statsManager;

    // Hàm này làm nền tảng để hệ thống Xây dựng (Building System) trong tương lai gọi vào
    // Mỗi khi xây hoặc phá công trình/đồ trang trí, truyền điểm thoải mái thay đổi vào đây
    public void ModifyCampComfort(float comfortChangeAmount)
    {
        statsManager.ModifyStat(StatType.Comfort, comfortChangeAmount);
    }

    // Hàm này làm nền tảng cho hệ thống Câu cá (Fishing System) sau này
    // Dùng để lấy ra tỷ lệ/hệ số gia tăng câu cá hiếm dựa theo điểm thoải mái hiện tại
    public float GetRareFishingBonusMultiplier()
    {
        float currentComfort = statsManager.GetStatValue(StatType.Comfort);
        float maxComfort = 100f;

        CharacterStat comfortStat = statsManager.GetStat(StatType.Comfort);
        if (comfortStat != null)
        {
            maxComfort = comfortStat.maxValue;
        }

        // Tính ra tỷ lệ từ 0 đến 1 tùy thuộc vào độ thoải mái (có thể nhân với hệ số sau này)
        return currentComfort / maxComfort;
    }
}