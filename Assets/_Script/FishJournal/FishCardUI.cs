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

        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        if (isUnlocked)
        {
            // === KHI ĐÃ CÂU ĐƯỢC ===
            if (lockedOverlay != null) lockedOverlay.SetActive(false);
            if (fishIcon != null) fishIcon.color = Color.white; // Hiện màu sắc sáng rõ

            // Hiển thị tên thật của cá (qua Localization nếu có)
            if (fishNameText != null)
            {
                string localizedName = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetLocalizedString("Game Text", fishData.itemName);
                fishNameText.text = !string.IsNullOrEmpty(localizedName) ? localizedName : fishData.itemName;
            }

            // Xử lý hiển thị thông số và hạng
            string gradeString = "";
            string hexColor = "#FFFFFF";

            switch (record.highestGrade)
            {
                case FishGrade.Normal: 
                    gradeString = isVietnamese ? "Hạng Thường" : "Normal Grade"; 
                    hexColor = "#FFFFFF"; 
                    break;
                case FishGrade.Bronze: 
                    gradeString = isVietnamese ? "Hạng Đồng" : "Bronze Grade"; 
                    hexColor = "#CD7F32"; 
                    break;
                case FishGrade.Silver: 
                    gradeString = isVietnamese ? "Hạng Bạc" : "Silver Grade"; 
                    hexColor = "#C0C0C0"; 
                    break;
                case FishGrade.Gold: 
                    gradeString = isVietnamese ? "Hạng Vàng" : "Gold Grade"; 
                    hexColor = "#FFD700"; 
                    break;
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

            // Hiển thị tên Map gợi ý lấy từ FishSO
            if (fishNameText != null)
            {
                string localizedMap = !string.IsNullOrEmpty(fishData.mapName)
                    ? UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetLocalizedString("Game Text", fishData.mapName)
                    : "";

                if (string.IsNullOrEmpty(localizedMap)) localizedMap = fishData.mapName;

                fishNameText.text = !string.IsNullOrEmpty(localizedMap) 
                    ? localizedMap 
                    : (isVietnamese ? "Chưa rõ" : "Unknown Area");
            }

            if (statsText != null)
            {
                statsText.text = ""; // Ẩn phần chỉ số đi
            }
        }
    }
}