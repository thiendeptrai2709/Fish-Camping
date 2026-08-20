using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CookingSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemShapeSO CurrentItem { get; private set; }

    [SerializeField] private Image itemIcon; // Ảnh hiển thị nguyên liệu trong ô
    private InventoryItemUI draggedProxyItem; // Vật thế thân khi kéo thả
    private ItemShapeSO itemBackup; // Giữ data phòng khi thả trượt

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

                // Thông báo nhiệm vụ kéo cá vào UI nấu ăn
                ForcedTutorialManager.Instance?.NotifyCookFish();
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
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || CurrentItem == null) return;

        itemBackup = CurrentItem;
        ClearSlot(); // Tạm xóa ảnh ở nồi đi

        // Sinh ra một item bám theo chuột
        GameObject prefab = CookingUIManager.Instance.GetItemPrefab();
        if (prefab != null)
        {
            GameObject itemObj = Instantiate(prefab, transform.root);
            draggedProxyItem = itemObj.GetComponent<InventoryItemUI>();
            draggedProxyItem.Setup(itemBackup, BackpackMinigameUI.Instance, 0, 0, false);
            draggedProxyItem.SetOriginIngredientSlot(this); // Khai báo xuất xứ để biết đường về

            draggedProxyItem.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedProxyItem != null) draggedProxyItem.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedProxyItem != null)
        {
            draggedProxyItem.OnEndDrag(eventData);
            draggedProxyItem = null;
        }
    }

    public void ReturnIngredient(InventoryItemUI proxyItem)
    {
        // Bị thả rơi ra ngoài vũ trụ -> Hủy item trên chuột và hồi sinh vào nồi
        ReceiveItem(proxyItem.GetItemShape());
        Destroy(proxyItem.gameObject);
    }
}