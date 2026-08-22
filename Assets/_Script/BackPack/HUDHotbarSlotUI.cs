using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class HUDHotbarSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private EquipmentSlotUI targetSyncSlot;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image placeholderIcon;

    [Header("Visual Auto-Resize Settings")]
    [SerializeField] private bool autoResizeSlotToFitItem = true;
    [SerializeField] private float hudCellSize = 64f; // Kích thước 1 ô lưới trên HUD

    [Header("--- HUD DURABILITY & USES INDICATORS ---")]
    [SerializeField] private GameObject hudDurabilityBarRoot;
    [SerializeField] private Image hudDurabilityFillImage;
    [SerializeField] private TextMeshProUGUI hudDurabilityPercentText;
    [SerializeField] private TextMeshProUGUI hudUsesBadgeText;

    private RectTransform rectTransform;
    private Vector2 originalSlotSize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalSlotSize = rectTransform != null ? rectTransform.sizeDelta : new Vector2(64f, 64f);

        Image img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (targetSyncSlot != null)
        {
            targetSyncSlot.OnDrop(eventData);
        }
    }

    private void OnEnable()
    {
        if (targetSyncSlot != null)
        {
            targetSyncSlot.OnItemEquipped += SyncEquip;
            targetSyncSlot.OnItemRemoved += SyncRemove;

            if (targetSyncSlot.GetEquippedItem() != null)
            {
                SyncEquip(targetSyncSlot.GetEquippedItem().GetItemShape());
            }
            else
            {
                SyncRemove();
            }
        }
    }

    private void OnDisable()
    {
        if (targetSyncSlot != null)
        {
            targetSyncSlot.OnItemEquipped -= SyncEquip;
            targetSyncSlot.OnItemRemoved -= SyncRemove;
        }
    }

    private void Update()
    {
        RefreshDurabilityDisplay();
    }

    // Đồng bộ hiển thị vật phẩm
    private void SyncEquip(ItemShapeSO itemShape)
    {
        if (itemShape != null && itemIcon != null)
        {
            itemIcon.sprite = itemShape.itemIcon;
            itemIcon.enabled = true;
            if (placeholderIcon != null) placeholderIcon.enabled = false;

            if (autoResizeSlotToFitItem && rectTransform != null)
            {
                InventoryItemUI equippedItem = targetSyncSlot.GetEquippedItem();
                bool isRotated = equippedItem != null && equippedItem.IsRotated();

                float width = (isRotated ? itemShape.height : itemShape.width) * hudCellSize;
                float height = (isRotated ? itemShape.width : itemShape.height) * hudCellSize;

                rectTransform.sizeDelta = new Vector2(width, height);
            }
        }
        RefreshDurabilityDisplay();
    }

    // Xóa hiển thị vật phẩm
    private void SyncRemove()
    {
        if (itemIcon != null) itemIcon.enabled = false;
        if (placeholderIcon != null) placeholderIcon.enabled = true;

        if (autoResizeSlotToFitItem && rectTransform != null)
        {
            rectTransform.sizeDelta = originalSlotSize;
        }

        if (hudDurabilityBarRoot != null) hudDurabilityBarRoot.SetActive(false);
        if (hudUsesBadgeText != null) hudUsesBadgeText.gameObject.SetActive(false);
    }

    private void EnsureHUDIndicators()
    {
        if (hudDurabilityBarRoot == null)
        {
            hudDurabilityBarRoot = new GameObject("HUD_DurabilityBarRoot", typeof(RectTransform), typeof(Image));
            hudDurabilityBarRoot.transform.SetParent(transform, false);

            RectTransform bgRect = hudDurabilityBarRoot.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0f);
            bgRect.anchorMax = new Vector2(1f, 0f);
            bgRect.pivot = new Vector2(0.5f, 0f);
            bgRect.sizeDelta = new Vector2(-6f, 3.5f);
            bgRect.anchoredPosition = new Vector2(0f, 2f);

            Image bgImg = hudDurabilityBarRoot.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.65f);
            bgImg.raycastTarget = false;

            // Fill Bar
            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(hudDurabilityBarRoot.transform, false);

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            hudDurabilityFillImage = fillObj.GetComponent<Image>();
            hudDurabilityFillImage.type = Image.Type.Filled;
            hudDurabilityFillImage.fillMethod = Image.FillMethod.Horizontal;
            hudDurabilityFillImage.raycastTarget = false;
        }

        if (hudUsesBadgeText == null)
        {
            GameObject badgeObj = new GameObject("HUD_UsesBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeObj.transform.SetParent(transform, false);

            RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 0f);
            badgeRect.anchorMax = new Vector2(1f, 0f);
            badgeRect.pivot = new Vector2(1f, 0f);
            badgeRect.sizeDelta = new Vector2(28f, 16f);
            badgeRect.anchoredPosition = new Vector2(-2f, 1f);

            hudUsesBadgeText = badgeObj.GetComponent<TextMeshProUGUI>();
            hudUsesBadgeText.alignment = TextAlignmentOptions.BottomRight;
            hudUsesBadgeText.fontSize = 11f;
            hudUsesBadgeText.fontStyle = FontStyles.Bold;
            hudUsesBadgeText.color = new Color(1f, 0.95f, 0.6f, 1f);
            hudUsesBadgeText.raycastTarget = false;

            var outline = badgeObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1f, -1f);
        }
    }

    public void RefreshDurabilityDisplay()
    {
        if (targetSyncSlot == null) return;
        InventoryItemUI equippedItem = targetSyncSlot.GetEquippedItem();

        if (equippedItem == null || equippedItem.GetItemShape() == null)
        {
            if (hudDurabilityBarRoot != null) hudDurabilityBarRoot.SetActive(false);
            if (hudUsesBadgeText != null) hudUsesBadgeText.gameObject.SetActive(false);
            return;
        }

        EnsureHUDIndicators();
        ItemShapeSO shape = equippedItem.GetItemShape();

        // 1. THANH ĐỘ BỀN (CẦN CÂU HOẶC PHAO CÂU)
        if (shape is FishingRodSO || shape is BobberSO)
        {
            float curDur = equippedItem.GetDurability();
            float maxDur = equippedItem.GetMaxDurability();
            float ratio = maxDur > 0 ? Mathf.Clamp01(curDur / maxDur) : 1f;

            if (hudDurabilityBarRoot != null)
            {
                hudDurabilityBarRoot.SetActive(true);
                if (hudDurabilityFillImage != null)
                {
                    hudDurabilityFillImage.fillAmount = ratio;
                    if (ratio > 0.5f)
                    {
                        hudDurabilityFillImage.color = new Color(0.2f, 0.92f, 0.35f, 0.95f); // Xanh lá
                    }
                    else if (ratio > 0.2f)
                    {
                        hudDurabilityFillImage.color = new Color(1f, 0.75f, 0.15f, 0.95f); // Vàng cam
                    }
                    else
                    {
                        hudDurabilityFillImage.color = new Color(0.95f, 0.2f, 0.2f, 0.95f); // Đỏ cảnh báo
                    }
                }
            }

            if (hudUsesBadgeText != null) hudUsesBadgeText.gameObject.SetActive(false);
        }
        // 2. BADGE SỐ LƯỢT DÙNG (MỒI CÂU)
        else if (shape is BaitSO)
        {
            if (hudDurabilityBarRoot != null) hudDurabilityBarRoot.SetActive(false);

            if (hudUsesBadgeText != null)
            {
                int uses = equippedItem.GetRemainingUses();
                hudUsesBadgeText.gameObject.SetActive(true);
                hudUsesBadgeText.text = $"x{uses}";
                hudUsesBadgeText.color = (uses <= 1) ? new Color(1f, 0.3f, 0.3f, 1f) : new Color(1f, 0.95f, 0.6f, 1f);
            }
        }
        else
        {
            if (hudDurabilityBarRoot != null) hudDurabilityBarRoot.SetActive(false);
            if (hudUsesBadgeText != null) hudUsesBadgeText.gameObject.SetActive(false);
        }
    }
}