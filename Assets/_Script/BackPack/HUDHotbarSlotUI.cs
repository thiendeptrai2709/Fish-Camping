using UnityEngine;
using UnityEngine.UI;

public class HUDHotbarSlotUI : MonoBehaviour
{
    [SerializeField] private EquipmentSlotUI targetSyncSlot;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image placeholderIcon;

    [Header("Visual Auto-Resize Settings")]
    [SerializeField] private bool autoResizeSlotToFitItem = true;
    [SerializeField] private float hudCellSize = 64f; // Kích thước 1 ô lưới trên HUD (có thể chỉnh to/nhỏ tùy ý)

    private RectTransform rectTransform;
    private Vector2 originalSlotSize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalSlotSize = rectTransform.sizeDelta;
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
                // Truy xuất vào Slot gốc để lấy trạng thái xoay của vật phẩm (nằm ngang hay dọc)
                InventoryItemUI equippedItem = targetSyncSlot.GetEquippedItem();
                bool isRotated = equippedItem != null && equippedItem.IsRotated();

                // Tính toán kích thước tự động (đảo ngược width/height nếu bị xoay 90 độ)
                float width = (isRotated ? itemShape.height : itemShape.width) * hudCellSize;
                float height = (isRotated ? itemShape.width : itemShape.height) * hudCellSize;

                rectTransform.sizeDelta = new Vector2(width, height);
            }
        }
    }

    // Xóa hiển thị vật phẩm
    private void SyncRemove()
    {
        if (itemIcon != null) itemIcon.enabled = false;
        if (placeholderIcon != null) placeholderIcon.enabled = true;

        if (autoResizeSlotToFitItem && rectTransform != null)
        {
            // Trả slot về kích thước mặc định khi không có đồ
            rectTransform.sizeDelta = originalSlotSize;
        }
    }
}