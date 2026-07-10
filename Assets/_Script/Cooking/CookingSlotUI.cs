using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CookingSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public ItemShapeSO CurrentItem { get; private set; }

    [SerializeField] private Image itemIcon; // Ảnh hiển thị nguyên liệu trong ô

    private void Awake()
    {
        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(false); // Ẩn icon khi ô trống
        }
    }

    // Hàm này tự động chạy khi có 1 UI được thả ra (thả chuột) trên vùng của object này
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            InventoryItemUI draggedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
            if (draggedItem != null)
            {
                // Nhận dữ liệu nguyên liệu
                ReceiveItem(draggedItem.GetItemShape());

                // Báo cho item biết nó đã được xử lý để tránh logic trả về kho cũ
                draggedItem.SetHandledBySlot(true);
                draggedItem.gameObject.SetActive(false);
                Destroy(draggedItem.gameObject, 0.1f);
            }
        }
    }

    private void ReceiveItem(ItemShapeSO itemShape)
    {
        CurrentItem = itemShape;
        if (itemIcon != null && itemShape != null)
        {
            itemIcon.sprite = itemShape.itemIcon;
            itemIcon.gameObject.SetActive(true);
        }
    }

    public void ClearSlot()
    {
        CurrentItem = null;
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.gameObject.SetActive(false);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && CurrentItem != null)
        {
            if (BackpackMinigameUI.Instance != null)
            {
                bool returned = BackpackMinigameUI.Instance.TryAutoAddItem(CurrentItem);

                if (returned)
                {
                    ClearSlot();
                }
                else
                {
                    Debug.Log("<color=red>[LỖI] Balo đã đầy, không thể lấy lại nguyên liệu!</color>");
                }
            }
        }
    }
}