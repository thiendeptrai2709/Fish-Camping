using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoPanelUI : MonoBehaviour
{
    public static ItemInfoPanelUI Instance { get; private set; }

    [Header("--- UI REFERENCES ---")]
    [SerializeField] private GameObject contentGroup;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ClearInfo();
    }

    public void ShowInfo(InventoryItemUI itemUI)
    {
        if (itemUI == null || itemUI.GetItemShape() == null)
        {
            ClearInfo();
            return;
        }

        ItemShapeSO itemShape = itemUI.GetItemShape();
        if (contentGroup != null) contentGroup.SetActive(true);

        if (itemIconImage != null)
        {
            itemIconImage.sprite = itemShape.itemIcon;
            itemIconImage.enabled = (itemShape.itemIcon != null);
        }

        if (itemNameText != null) itemNameText.text = itemShape.itemName;

        // HIỂN THỊ DỮ LIỆU PHÂN LOẠI
        if (itemShape is FishSO fishData)
        {
            // Nếu là Cá -> Sử dụng dữ liệu động được sinh ra lúc mới câu
            string rarityColor = fishData.rarity == FishRarity.Common ? "#FFFFFF" :
                                 fishData.rarity == FishRarity.Uncommon ? "#00FF00" : // Xanh lá
                                 fishData.rarity == FishRarity.Rare ? "#00BFFF" : "#FF00FF"; // Xanh dương hoặc Tím

            string gradeString = "";
            string gradeColor = "#FFFFFF";
            switch (itemUI.GetGrade())
            {
                case FishGrade.Normal: gradeString = "Hạng Thường"; gradeColor = "#FFFFFF"; break;
                case FishGrade.Bronze: gradeString = "Hạng Đồng"; gradeColor = "#CD7F32"; break;
                case FishGrade.Silver: gradeString = "Hạng Bạc"; gradeColor = "#C0C0C0"; break;
                case FishGrade.Gold: gradeString = "Hạng Vàng"; gradeColor = "#FFD700"; break;
            }

            if (itemStatsText != null)
            {
                itemStatsText.text = $"<color={rarityColor}>Độ hiếm: {fishData.rarity}</color>\n" +
                                     $"<color={gradeColor}>Cấp độ: {gradeString}</color>\n" +
                                     $"Dài: {itemUI.GetLength():F1}cm\n" +
                                     $"Nặng: {itemUI.GetWeight():F1}kg\n" +
                                     $"Giá cơ bản: {fishData.basePrice} Vàng";
            }
        }
        else
        {
            // Nếu là Mồi/Cần câu/Phao -> Sử dụng Data tĩnh gốc
            if (itemStatsText != null) itemStatsText.text = itemShape.GetFormattedStats();
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = string.IsNullOrEmpty(itemShape.itemDescription)
                ? "Không có mô tả chi tiết cho vật phẩm này."
                : itemShape.itemDescription;
        }
    }

    public void ClearInfo()
    {
        if (contentGroup != null) contentGroup.SetActive(false);
        if (itemIconImage != null) itemIconImage.enabled = false;
        if (itemNameText != null) itemNameText.text = string.Empty;
        if (itemStatsText != null) itemStatsText.text = string.Empty;
        if (itemDescriptionText != null) itemDescriptionText.text = string.Empty;
    }
}