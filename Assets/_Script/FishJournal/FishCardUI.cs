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
        if (record != null && record.isUnlocked)
        {
            lockedOverlay.SetActive(false);
            fishIcon.sprite = fishData.itemIcon;
            fishIcon.color = Color.white;
            fishNameText.text = fishData.itemName;

            // Xử lý chuỗi hiển thị Hạng và Màu sắc
            string gradeString = "";
            string hexColor = "#FFFFFF";

            switch (record.highestGrade)
            {
                case FishGrade.Normal: gradeString = "Hạng Thường"; hexColor = "#FFFFFF"; break;
                case FishGrade.Bronze: gradeString = "Hạng Đồng"; hexColor = "#CD7F32"; break;
                case FishGrade.Silver: gradeString = "Hạng Bạc"; hexColor = "#C0C0C0"; break;
                case FishGrade.Gold: gradeString = "Hạng Vàng"; hexColor = "#FFD700"; break;
            }

            statsText.text = $"L: {record.maxLength:F1}cm\nW: {record.maxWeight:F1}kg\n<color={hexColor}>{gradeString}</color>";
        }
        else
        {
            lockedOverlay.SetActive(true);
            fishIcon.sprite = fishData.itemIcon;
            fishIcon.color = Color.black;
            fishNameText.text = "???";
            statsText.text = "";
        }
    }
}