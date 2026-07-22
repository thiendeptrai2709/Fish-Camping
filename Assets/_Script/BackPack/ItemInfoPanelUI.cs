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

    public void ShowInfo(ItemShapeSO itemShape)
    {
        if (itemShape == null)
        {
            ClearInfo();
            return;
        }

        if (contentGroup != null) contentGroup.SetActive(true);

        if (itemIconImage != null)
        {
            itemIconImage.sprite = itemShape.itemIcon;
            itemIconImage.enabled = (itemShape.itemIcon != null);
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemShape.itemName;
        }

        if (itemStatsText != null)
        {
            itemStatsText.text = itemShape.GetFormattedStats();
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