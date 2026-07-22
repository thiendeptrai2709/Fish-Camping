using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
// Thêm IPointerDownHandler vào đây để bắt được phát click đầu tiên
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image itemImage;

    private ItemShapeSO itemShape;
    private BackpackMinigameUI minigameUI;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private PlayerInputHandler inputHandler;
    private bool isDragging = false;
    private Vector2 lastDragPosition;
    private Vector2 dragOffset;

    private int gridX;
    private int gridY;
    private bool isRotated = false;
    private bool isEquipped = false;
    private bool isHandledBySlot = false;
    private bool isFromCooking = false;
    private EquipmentSlotUI currentSlot = null;


    public void SetEquippedState(bool equipped, EquipmentSlotUI slot = null)
    {
        isEquipped = equipped;
        currentSlot = slot;
    }

    public void SetFromCooking(bool fromCooking)
    {
        isFromCooking = fromCooking;
    }

    public void SetHandledBySlot(bool handled)
    {
        isHandledBySlot = handled;
    }

    public bool IsEquipped() => isEquipped;
    public EquipmentSlotUI GetCurrentSlot() => currentSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
        }
        inputHandler = FindFirstObjectByType<PlayerInputHandler>();
    }

    private void Update()
    {
        if (isDragging && inputHandler != null && inputHandler.RotateItemTriggered)
        {
            ToggleRotate();
            if (minigameUI != null)
            {
                minigameUI.OnItemDragging(this, lastDragPosition);
            }
        }
    }

    public void Setup(ItemShapeSO shape, BackpackMinigameUI uiController, int startX, int startY, bool rotated = false)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (itemImage == null) itemImage = GetComponent<Image>();

        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);

        itemShape = shape;
        minigameUI = uiController;
        gridX = startX;
        gridY = startY;
        isRotated = rotated;

        if (itemImage != null && itemShape != null)
        {
            itemImage.sprite = itemShape.itemIcon;
            itemImage.color = Color.white;
            itemImage.preserveAspect = true;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            // Ép buộc nó phải nhận va chạm chuột, phòng trường hợp đẻ ra từ nồi bị lỗi
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        UpdateVisualSize();
    }

    public void UpdateVisualSize()
    {
        if (itemShape == null || minigameUI == null) return;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        float cellSize = minigameUI.GetCellSize();

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(itemShape.width * cellSize, itemShape.height * cellSize);
            rectTransform.pivot = isRotated ? new Vector2(0, 0) : new Vector2(0, 1);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, isRotated ? -90f : 0f);
        }
    }

    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    public void ToggleRotate()
    {
        isRotated = !isRotated;
        UpdateVisualSize();
    }

    // --- LOGIC BẮT CHUỘT MỚI ---
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Nếu là cá chín từ Nồi: BẤM LÀ THU HOẠCH LUÔN
        if (isFromCooking)
        {
            if (minigameUI != null && minigameUI.TryAutoAddFromCooking(this))
            {
                Debug.Log("<color=green>[THU HOẠCH] Đã click lấy đồ ăn thẳng vào Balo thành công!</color>");
                if (CookingUIManager.Instance != null) CookingUIManager.Instance.OnFoodCollectedSuccessfully();
            }
            else
            {
                Debug.Log("<color=red>[THU HOẠCH LỖI] Balo đã đầy, dọn bớt đồ đi!</color>");
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Nếu là đồ từ nồi, CẤM TUYỆT ĐỐI kéo thả để né lỗi Unity
        if (isFromCooking) return;

        if (eventData.button != PointerEventData.InputButton.Left || minigameUI == null) return;

        isDragging = true;
        lastDragPosition = eventData.position;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.4f;

        // Ẩn bảng thông tin khi bắt đầu nhấc đồ lên kéo đi
        if (ItemInfoPanelUI.Instance != null)
        {
            ItemInfoPanelUI.Instance.ClearInfo();
        }

        if (isEquipped && currentSlot != null)
        {
            currentSlot.RemoveEquippedItem();

            Canvas rootCanvas = currentSlot.GetComponentInParent<Canvas>();
            if (rootCanvas != null) transform.SetParent(rootCanvas.transform, true);
            else transform.SetParent(currentSlot.transform.root, true);

            // --- BẮT BUỘC THÊM ĐOẠN NÀY ĐỂ TRỊ BỆNH LỆCH Ô XANH ---
            // Trả Anchor về (0, 1) và gọi UpdateVisualSize để set lại Pivot chuẩn của Balo
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            UpdateVisualSize();

            transform.SetAsLastSibling();
            minigameUI.OnItemBeginDragFromExternal(this, eventData.position);
        }
        else
        {
            minigameUI.OnItemBeginDrag(this, eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || minigameUI == null) return;
        lastDragPosition = eventData.position;
        minigameUI.OnItemDragging(this, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || minigameUI == null) return;
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Debug.Log($"<color=lime>[DEBUG KÉO] ĐÃ THẢ TAY!</color>");

        if (isHandledBySlot)
        {
            isHandledBySlot = false;
            if (minigameUI != null) minigameUI.HideHighlight();
            return;
        }

        if (isFromCooking)
        {
            bool placedInGrid = minigameUI.TryPlaceItemFromExternal(this, eventData.position);
            if (placedInGrid)
            {
                isFromCooking = false;
                if (CookingUIManager.Instance != null) CookingUIManager.Instance.OnFoodCollectedSuccessfully();
            }
            else
            {
                if (CookingUIManager.Instance != null) CookingUIManager.Instance.ReturnFoodToSlot(this);
            }
            if (minigameUI != null) minigameUI.HideHighlight();
            return;
        }

        if (isEquipped)
        {
            bool placedInGrid = minigameUI.TryPlaceItemFromExternal(this, eventData.position);
            if (!placedInGrid)
            {
                if (currentSlot != null)
                {
                    currentSlot.ReturnItemToSlot(this);
                }
            }
            if (minigameUI != null) minigameUI.HideHighlight();
            return;
        }

        minigameUI.OnItemEndDrag(this, eventData.position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Hỗ trợ hiển thị khi click chuột trực tiếp vào vật phẩm
        if (ItemInfoPanelUI.Instance != null && itemShape != null)
        {
            ItemInfoPanelUI.Instance.ShowInfo(itemShape);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Khi di chuột vào item (và không trong trạng thái đang kéo đồ) -> Hiện thông tin
        if (!isDragging && ItemInfoPanelUI.Instance != null && itemShape != null)
        {
            ItemInfoPanelUI.Instance.ShowInfo(itemShape);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Khi rời chuột khỏi item -> Xóa sạch thông tin trên bảng
        if (ItemInfoPanelUI.Instance != null)
        {
            ItemInfoPanelUI.Instance.ClearInfo();
        }
    }

    public ItemShapeSO GetItemShape() => itemShape;
    public int GetGridX() => gridX;
    public int GetGridY() => gridY;
    public bool IsRotated() => isRotated;
}