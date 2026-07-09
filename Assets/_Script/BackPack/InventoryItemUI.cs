using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
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
    private EquipmentSlotUI currentSlot = null;

    public void SetEquippedState(bool equipped, EquipmentSlotUI slot = null)
    {
        isEquipped = equipped;
        currentSlot = slot;
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
                // Khi bấm xoay lúc kéo, gọi thẳng OnItemDragging để ảnh xoay ngay tại ô chuột đang đứng
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
            // Luôn giữ sizeDelta theo kích thước gốc của vật phẩm (không được tráo đổi)
            rectTransform.sizeDelta = new Vector2(itemShape.width * cellSize, itemShape.height * cellSize);
            // Kỹ thuật chuyển Pivot: Khi xoay -90 độ, Pivot (0,0) giúp góc trên-trái trực quan khớp tuyệt đối với tọa độ ô lưới
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || minigameUI == null) return;
        isDragging = true;
        lastDragPosition = eventData.position;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.4f;

        if (isEquipped && currentSlot != null)
        {
            currentSlot.RemoveEquippedItem();

            Canvas rootCanvas = currentSlot.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                transform.SetParent(rootCanvas.transform);
            }
            else
            {
                transform.SetParent(currentSlot.transform.root);
            }

            RectTransform rect = GetComponent<RectTransform>();
            rect.pivot = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);

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

        if (isHandledBySlot)
        {
            isHandledBySlot = false;
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
    }

    public ItemShapeSO GetItemShape() => itemShape;
    public int GetGridX() => gridX;
    public int GetGridY() => gridY;
    public bool IsRotated() => isRotated;
}