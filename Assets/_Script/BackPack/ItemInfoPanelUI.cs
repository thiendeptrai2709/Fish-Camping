using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

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
            itemIconImage.preserveAspect = true;

            RectTransform iconRect = itemIconImage.rectTransform;
            if (iconRect != null)
            {
                if (itemShape is FishingRodSO)
                {
                    iconRect.localScale = new Vector3(1.6f, 1.6f, 1f);
                }
                else
                {
                    iconRect.localScale = new Vector3(1.3f, 1.3f, 1f);
                }
            }

            if (itemShape.itemIcon != null && itemShape.itemIcon.rect.height > 0f)
            {
                float ratio = (float)itemShape.itemIcon.rect.width / itemShape.itemIcon.rect.height;
                AspectRatioFitter fitter = itemIconImage.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                {
                    fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                    fitter.aspectRatio = ratio;
                }
            }
        }

        bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        string rawName = !string.IsNullOrEmpty(itemShape.itemName) ? itemShape.itemName : itemShape.name;
        string localizedName = GetSafeLocalizedString(rawName, rawName);

        if (itemNameText != null)
        {
            itemNameText.text = localizedName;
        }

        // ========================================================
        // HIỂN THỊ DỮ LIỆU CÁC LOẠI VẬT PHẨM
        // ========================================================
        if (itemShape is FishSO fishData)
        {
            // 1. Phân loại độ hiếm
            string rarityText = "";
            string rarityColor = "#ECEFF1";
            switch (fishData.rarity)
            {
                case FishRarity.Common:
                    rarityText = isVietnamese ? "Phổ biến" : "Common";
                    rarityColor = "#ECEFF1";
                    break;
                case FishRarity.Uncommon:
                    rarityText = isVietnamese ? "Ít gặp" : "Uncommon";
                    rarityColor = "#66BB6A";
                    break;
                case FishRarity.Rare:
                    rarityText = isVietnamese ? "Hiếm" : "Rare";
                    rarityColor = "#29B6F6";
                    break;
                case FishRarity.Legendary:
                    rarityText = isVietnamese ? "Truyền thuyết" : "Legendary";
                    rarityColor = "#FFA726";
                    break;
            }

            // 2. Phân loại phẩm chất
            string gradeString = "";
            string gradeColor = "#FFFFFF";
            float priceMultiplier = 1f;
            switch (itemUI.GetGrade())
            {
                case FishGrade.Normal:
                    gradeString = isVietnamese ? "Hạng Thường" : "Normal Grade";
                    gradeColor = "#ECEFF1";
                    priceMultiplier = 1.0f;
                    break;
                case FishGrade.Bronze:
                    gradeString = isVietnamese ? "Hạng Đồng (★)" : "Bronze Grade (★)";
                    gradeColor = "#CD7F32";
                    priceMultiplier = 1.25f;
                    break;
                case FishGrade.Silver:
                    gradeString = isVietnamese ? "Hạng Bạc (★★)" : "Silver Grade (★★)";
                    gradeColor = "#C0C0C0";
                    priceMultiplier = 1.5f;
                    break;
                case FishGrade.Gold:
                    gradeString = isVietnamese ? "Hạng Vàng Kim (★★★)" : "Gold Grade (★★★)";
                    gradeColor = "#FFD700";
                    priceMultiplier = 2.0f;
                    break;
            }

            float len = itemUI.GetLength();
            float wei = itemUI.GetWeight();
            if (len <= 0f) len = (fishData.minLength + fishData.maxLength) * 0.5f;
            if (wei <= 0f) wei = (fishData.minWeight + fishData.maxWeight) * 0.5f;

            int estimatedPrice = Mathf.RoundToInt(fishData.basePrice * priceMultiplier);

            string mapNameLocalized = GetSafeLocalizedString(fishData.mapName, !string.IsNullOrEmpty(fishData.mapName) ? fishData.mapName : "Vùng nước ngọt");

            if (itemStatsText != null)
            {
                if (isVietnamese)
                {
                    itemStatsText.text = $"• Độ hiếm: <color={rarityColor}><b>{rarityText}</b></color>\n" +
                                         $"• Phẩm chất: <color={gradeColor}><b>{gradeString}</b></color>\n" +
                                         $"• Chiều dài: <b>{len:F1} cm</b>\n" +
                                         $"• Cân nặng: <b>{wei:F1} kg</b>\n" +
                                         $"• Vùng xuất hiện: <b>{mapNameLocalized}</b>\n" +
                                         $"• Giá bán ước tính: <color=#FFD700><b>{estimatedPrice} Vàng</b></color>";
                }
                else
                {
                    itemStatsText.text = $"• Rarity: <color={rarityColor}><b>{rarityText}</b></color>\n" +
                                         $"• Quality: <color={gradeColor}><b>{gradeString}</b></color>\n" +
                                         $"• Length: <b>{len:F1} cm</b>\n" +
                                         $"• Weight: <b>{wei:F1} kg</b>\n" +
                                         $"• Habitat: <b>{mapNameLocalized}</b>\n" +
                                         $"• Estimated Price: <color=#FFD700><b>{estimatedPrice} Gold</b></color>";
                }
            }
        }
        else if (itemShape is FishingRodSO rod)
        {
            float curDur = itemUI.GetDurability();
            float maxDur = itemUI.GetMaxDurability();
            float ratio = maxDur > 0 ? Mathf.Clamp01(curDur / maxDur) : 1f;
            string durColor = (ratio > 0.5f) ? "#00FF7F" : (ratio > 0.2f ? "#FFD700" : "#FF4545");
            string mapInfo = rod.rodTier <= 4
                ? (isVietnamese ? "<color=#81C784>Map 2 & Map 3 (Nước ngọt)</color>" : "<color=#81C784>Map 2 & Map 3 (Freshwater)</color>")
                : (isVietnamese ? "<color=#4FC3F7>Map 4 (Chuyên dụng câu biển)</color>" : "<color=#4FC3F7>Map 4 (Saltwater Ocean)</color>");

            if (itemStatsText != null)
            {
                if (isVietnamese)
                {
                    itemStatsText.text = $"• Cấp độ: <b>Tier {rod.rodTier}</b>\n" +
                                         $"• Độ bền: <color={durColor}><b>{Mathf.RoundToInt(curDur)}/{Mathf.RoundToInt(maxDur)} ({Mathf.RoundToInt(ratio * 100f)}%)</b></color>\n" +
                                         $"• Khu vực: {mapInfo}\n" +
                                         $"• Lực kéo cá: <b>+{rod.fishingPower}</b>\n" +
                                         $"• Tầm ném tối đa: <b>{rod.castDistance}m</b>\n" +
                                         $"• Giảm thời gian chờ: <b>-{rod.waitTimeReductionPercentage}%</b>";
                }
                else
                {
                    itemStatsText.text = $"• Level: <b>Tier {rod.rodTier}</b>\n" +
                                         $"• Durability: <color={durColor}><b>{Mathf.RoundToInt(curDur)}/{Mathf.RoundToInt(maxDur)} ({Mathf.RoundToInt(ratio * 100f)}%)</b></color>\n" +
                                         $"• Region: {mapInfo}\n" +
                                         $"• Fishing Power: <b>+{rod.fishingPower}</b>\n" +
                                         $"• Max Cast Range: <b>{rod.castDistance}m</b>\n" +
                                         $"• Wait Reduction: <b>-{rod.waitTimeReductionPercentage}%</b>";
                }
            }
        }
        else if (itemShape is BobberSO bobber)
        {
            float curDur = itemUI.GetDurability();
            float maxDur = itemUI.GetMaxDurability();
            float ratio = maxDur > 0 ? Mathf.Clamp01(curDur / maxDur) : 1f;
            string durColor = (ratio > 0.5f) ? "#00FF7F" : (ratio > 0.2f ? "#FFD700" : "#FF4545");

            if (itemStatsText != null)
            {
                if (isVietnamese)
                {
                    itemStatsText.text = $"• Độ bền phao: <color={durColor}><b>{Mathf.RoundToInt(curDur)}/{Mathf.RoundToInt(maxDur)}</b></color>\n" +
                                         $"• Độ ổn định Minigame: <b>+{bobber.buoyancy:F1}x</b>\n" +
                                         $"• Tăng kích thước cá: <b>+{bobber.attractivenessBonus}%</b>\n" +
                                         $"• Trọng lượng ném: <b>{bobber.weight}kg</b>";
                }
                else
                {
                    itemStatsText.text = $"• Bobber Durability: <color={durColor}><b>{Mathf.RoundToInt(curDur)}/{Mathf.RoundToInt(maxDur)}</b></color>\n" +
                                         $"• Minigame Stability: <b>+{bobber.buoyancy:F1}x</b>\n" +
                                         $"• Fish Size Bonus: <b>+{bobber.attractivenessBonus}%</b>\n" +
                                         $"• Cast Weight: <b>{bobber.weight}kg</b>";
                }
            }
        }
        else if (itemShape is BaitSO bait)
        {
            int curUses = itemUI.GetRemainingUses();
            int maxUses = itemUI.GetMaxUses();
            string usesColor = (curUses > 1) ? "#00FF7F" : "#FF4545";

            if (itemStatsText != null)
            {
                if (isVietnamese)
                {
                    itemStatsText.text = $"• Số lần dùng còn lại: <color={usesColor}><b>{curUses}/{maxUses} lần</b></color>\n" +
                                         $"• Tăng độ thu hút: <b>+{bait.attractivenessBonus}%</b>\n" +
                                         $"• Giảm thời gian chờ: <b>-{bait.waitTimeReduction}s</b>\n" +
                                         $"• Tăng tỷ lệ cá hiếm: <b>+{bait.targetRarityBonus}</b>";
                }
                else
                {
                    itemStatsText.text = $"• Remaining Uses: <color={usesColor}><b>{curUses}/{maxUses} uses</b></color>\n" +
                                         $"• Attractiveness: <b>+{bait.attractivenessBonus}%</b>\n" +
                                         $"• Wait Reduction: <b>-{bait.waitTimeReduction}s</b>\n" +
                                         $"• Rare Fish Chance: <b>+{bait.targetRarityBonus}</b>";
                }
            }
        }
        else
        {
            if (itemStatsText != null) itemStatsText.text = itemShape.GetFormattedStats();
        }

        if (itemDescriptionText != null)
        {
            string rawDesc = itemShape.itemDescription;
            string localizedDesc = GetSafeLocalizedString(rawDesc, rawDesc);

            itemDescriptionText.text = string.IsNullOrEmpty(localizedDesc)
                ? (isVietnamese ? "Không có mô tả chi tiết cho vật phẩm này." : "No detailed description available.")
                : localizedDesc;
        }
    }

    private string GetSafeLocalizedString(string textOrKey, string fallback)
    {
        if (string.IsNullOrEmpty(textOrKey)) return fallback;
        try
        {
            if (LocalizationSettings.StringDatabase != null)
            {
                var table = LocalizationSettings.StringDatabase.GetTable("Game Text");
                if (table != null)
                {
                    var entry = table.GetEntry(textOrKey);
                    if (entry != null && !string.IsNullOrEmpty(entry.GetLocalizedString()))
                        return entry.GetLocalizedString();
                }
            }
        }
        catch { }
        return !string.IsNullOrEmpty(textOrKey) ? textOrKey : fallback;
    }

    public void ClearInfo()
    {
        if (contentGroup != null) contentGroup.SetActive(false);
        if (itemIconImage != null)
        {
            itemIconImage.enabled = false;
            itemIconImage.rectTransform.localScale = Vector3.one;
        }
        if (itemNameText != null) itemNameText.text = string.Empty;
        if (itemStatsText != null) itemStatsText.text = string.Empty;
        if (itemDescriptionText != null) itemDescriptionText.text = string.Empty;
    }
}