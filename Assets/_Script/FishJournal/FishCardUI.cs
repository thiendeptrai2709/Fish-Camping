using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishCardUI : MonoBehaviour
{
    [SerializeField] private Image fishIcon;
    [SerializeField] private TextMeshProUGUI fishNameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private GameObject lockedOverlay;

    public void Setup(FishSO fishData, FishRecord record)
    {
        if (fishData == null) return;

        // Luôn gán hình ảnh con cá (dù chưa câu vẫn hiện hình dáng)
        if (fishIcon != null)
        {
            fishIcon.sprite = fishData.itemIcon;
        }

        bool isUnlocked = (record != null && record.isUnlocked);

        if (isUnlocked)
        {
            // === KHI ĐÃ CÂU ĐƯỢC ===
            if (lockedOverlay != null) lockedOverlay.SetActive(false);
            if (fishIcon != null) fishIcon.color = Color.white; // Hiện màu sắc sáng rõ

            // Hiển thị tên thật của cá
            if (fishNameText != null)
            {
                fishNameText.text = fishData.itemName;
            }

            // Xử lý hiển thị thông số và hạng
            string gradeString = "";
            string hexColor = "#FFFFFF";

            switch (record.highestGrade)
            {
                case FishGrade.Normal: gradeString = "Hạng Thường"; hexColor = "#FFFFFF"; break;
                case FishGrade.Bronze: gradeString = "Hạng Đồng"; hexColor = "#CD7F32"; break;
                case FishGrade.Silver: gradeString = "Hạng Bạc"; hexColor = "#C0C0C0"; break;
                case FishGrade.Gold: gradeString = "Hạng Vàng"; hexColor = "#FFD700"; break;
            }

            if (statsText != null)
            {
                statsText.text = $"L: {record.maxLength:F1}cm\nW: {record.maxWeight:F1}kg\n<color={hexColor}>{gradeString}</color>";
            }
        }
        else
        {
            // === KHI CHƯA CÂU ĐƯỢC ===
            if (lockedOverlay != null) lockedOverlay.SetActive(false);

            if (fishIcon != null)
            {
                // Chuyển hình con cá thành màu xám tối
                fishIcon.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            // THAY ĐỔI Ở ĐÂY: Thay vì hiện "???", hiển thị tên Map lấy từ FishSO
            if (fishNameText != null)
            {
                // Nếu bạn muốn hiển thị kèm chữ gợi ý, có thể dùng: $"Khu vực: {fishData.mapName}"
                fishNameText.text = !string.IsNullOrEmpty(fishData.mapName) ? fishData.mapName : "Chưa rõ";
            }

            if (statsText != null)
            {
                statsText.text = ""; // Ẩn phần chỉ số đi
            }
        }
    }
}