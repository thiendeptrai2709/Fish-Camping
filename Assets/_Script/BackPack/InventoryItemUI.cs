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
    public enum GridOwner { Backpack, Trunk }
    public GridOwner currentOwner = GridOwner.Backpack;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private PlayerInputHandler inputHandler;
    private bool isDragging = false;
    private Vector2 lastDragPosition;
    private Vector2 dragOffset;
    private bool isHoveringTrunk = false;
    private bool isHoveringBackpack = false;

    private int gridX;
    private int gridY;
    private bool isRotated = false;
    private bool isEquipped = false;
    private bool isHandledBySlot = false;
    private bool isFromCooking = false;
    private EquipmentSlotUI currentSlot = null;
    private float currentLength = 0f;
    private float currentWeight = 0f;
    private FishGrade currentGrade = FishGrade.Normal;

    private CookingSlotUI originIngredientSlot = null;

    public void SetFishInstanceData(float length, float weight, FishGrade grade)
    {
        currentLength = length;
        currentWeight = weight;
        currentGrade = grade;
    }
    public void SetOriginIngredientSlot(CookingSlotUI slot) => originIngredientSlot = slot;
    public float GetLength() => currentLength;
    public float GetWeight() => currentWeight;
    public FishGrade GetGrade() => currentGrade;

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
            // Ưu tiên gọi hàm UI của cái lưới mà chuột đang bay trên đầu
            if (isHoveringTrunk && TrunkMinigameUI.Instance != null)
                TrunkMinigameUI.Instance.OnItemDragging(this, lastDragPosition);
            else if (isHoveringBackpack && BackpackMinigameUI.Instance != null)
                BackpackMinigameUI.Instance.OnItemDragging(this, lastDragPosition);
            else
            {
                if (currentOwner == GridOwner.Backpack && BackpackMinigameUI.Instance != null)
                    BackpackMinigameUI.Instance.OnItemDragging(this, lastDragPosition);
                else if (currentOwner == GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                    TrunkMinigameUI.Instance.OnItemDragging(this, lastDragPosition);
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
            itemImage.raycastTarget = true;
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
        if (itemShape == null) return;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        float cellSize = 64f;
        // Khi đang kéo thả, kích thước item phải scale theo ô lưới mà chuột đang chỉ vào
        if (isDragging)
        {
            if (isHoveringTrunk && TrunkMinigameUI.Instance != null) cellSize = TrunkMinigameUI.Instance.GetCellSize();
            else if (isHoveringBackpack && BackpackMinigameUI.Instance != null) cellSize = BackpackMinigameUI.Instance.GetCellSize();
            else cellSize = (currentOwner == GridOwner.Trunk && TrunkMinigameUI.Instance != null) ? TrunkMinigameUI.Instance.GetCellSize() : (BackpackMinigameUI.Instance != null ? BackpackMinigameUI.Instance.GetCellSize() : 64f);
        }
        else
        {
            cellSize = (currentOwner == GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                ? TrunkMinigameUI.Instance.GetCellSize()
                : (BackpackMinigameUI.Instance != null ? BackpackMinigameUI.Instance.GetCellSize() : 64f);
        }

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
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isDragging = true;
        lastDragPosition = eventData.position;
        isHoveringTrunk = false;
        isHoveringBackpack = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.4f;

        // Ẩn bảng thông tin khi bắt đầu nhấc đồ lên kéo đi
        if (ItemInfoPanelUI.Instance != null)
        {
            ItemInfoPanelUI.Instance.ClearInfo();
        }

        if (isFromCooking || originIngredientSlot != null)
        {
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null) transform.SetParent(rootCanvas.transform, true);

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            UpdateVisualSize();

            transform.SetAsLastSibling();
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.OnItemBeginDragFromExternal(this, eventData.position);
        }
        else if (isEquipped && currentSlot != null)
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
            BackpackMinigameUI.Instance.OnItemBeginDragFromExternal(this, eventData.position);
        }
        else
        {
            if (currentOwner == GridOwner.Backpack && BackpackMinigameUI.Instance != null)
                BackpackMinigameUI.Instance.OnItemBeginDrag(this, eventData.position);
            else if (currentOwner == GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                TrunkMinigameUI.Instance.OnItemBeginDrag(this, eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        lastDragPosition = eventData.position;

        bool overBackpack = BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetGridRoot() != null && BackpackMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(BackpackMinigameUI.Instance.GetGridRoot(), eventData.position, eventData.pressEventCamera);
        bool overTrunk = TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetGridRoot() != null && TrunkMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(TrunkMinigameUI.Instance.GetGridRoot(), eventData.position, eventData.pressEventCamera);

        isHoveringBackpack = overBackpack;
        isHoveringTrunk = overTrunk;

        if (overTrunk)
        {
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
            TrunkMinigameUI.Instance.OnItemDragging(this, eventData.position);
        }
        else if (overBackpack)
        {
            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();
            BackpackMinigameUI.Instance.OnItemDragging(this, eventData.position);
        }
        else
        {
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();

            // Cho phép hình ảnh bay theo chuột khi ra ngoài vùng lưới
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                if (transform.parent != rootCanvas.transform) { transform.SetParent(rootCanvas.transform, true); transform.SetAsLastSibling(); }
                Camera pressCamera = rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? rootCanvas.worldCamera : null;
                if (pressCamera == null) pressCamera = Camera.main;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rootCanvas.GetComponent<RectTransform>(), eventData.position, pressCamera, out Vector3 worldPoint))
                    transform.position = worldPoint;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. Chỉ kiểm tra nút chuột trái (BỎ 'minigameUI == null' ở đây để không bị chặn code Cốp xe)
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // 2. Luôn khôi phục lại trạng thái giao diện trước tiên
        isDragging = false;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        Debug.Log($"<color=lime>[DEBUG KÉO] ĐÃ THẢ TAY!</color>");

        // Tự động tìm lại BackpackMinigameUI nếu item thuộc Balo mà minigameUI đang bị null
        if (minigameUI == null && currentOwner == GridOwner.Backpack && BackpackMinigameUI.Instance != null)
        {
            minigameUI = BackpackMinigameUI.Instance;
        }

        if (isHandledBySlot)
        {
            isHandledBySlot = false;
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();
            return;
        }

        // 3. Xử lý đồ ăn từ Nồi / Nguyên liệu nấu ăn
        if (isFromCooking || originIngredientSlot != null)
        {
            bool isOverBalo = BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetGridRoot() != null && BackpackMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(BackpackMinigameUI.Instance.GetGridRoot(), eventData.position, eventData.pressEventCamera);
            bool isOverXe = TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetGridRoot() != null && TrunkMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(TrunkMinigameUI.Instance.GetGridRoot(), eventData.position, eventData.pressEventCamera);
            bool placedInGrid = false;

            if (isOverXe && TrunkMinigameUI.Instance != null) { placedInGrid = TrunkMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position); if (placedInGrid) currentOwner = GridOwner.Trunk; }
            else if (isOverBalo && BackpackMinigameUI.Instance != null) { placedInGrid = BackpackMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position); if (placedInGrid) currentOwner = GridOwner.Backpack; }

            if (placedInGrid)
            {
                if (isFromCooking)
                {
                    isFromCooking = false;
                    if (CookingUIManager.Instance != null) CookingUIManager.Instance.OnFoodCollectedSuccessfully();
                }
                originIngredientSlot = null;
            }
            else
            {
                if (isFromCooking && CookingUIManager.Instance != null) CookingUIManager.Instance.ReturnFoodToSlot(this);
                else if (originIngredientSlot != null) originIngredientSlot.ReturnIngredient(this);
            }

            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();
            return;
        }

        // 4. Xử lý trang bị đang mặc
        if (isEquipped)
        {
            bool placedInGrid = minigameUI != null && minigameUI.TryPlaceItemFromExternal(this, eventData.position);
            if (!placedInGrid && currentSlot != null)
            {
                currentSlot.ReturnItemToSlot(this);
            }
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
            return;
        }

        // 5. Xử lý kéo thả bình thường giữa Balo và Cốp xe
        bool overBackpack = BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetGridRoot() != null && BackpackMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(BackpackMinigameUI.Instance.GetGridRoot(), eventData.position, eventData.pressEventCamera);
        bool overTrunk = TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetGridRoot() != null && TrunkMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(TrunkMinigameUI.Instance.GetGridRoot(), eventData.position, eventData.pressEventCamera);

        bool placed = false;
        if (overTrunk && TrunkMinigameUI.Instance != null)
        {
            placed = TrunkMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position);
            if (placed) currentOwner = GridOwner.Trunk;
        }
        else if (overBackpack && BackpackMinigameUI.Instance != null)
        {
            placed = BackpackMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position);
            if (placed) currentOwner = GridOwner.Backpack;
        }

        if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
        if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();

        if (!placed)
        {
            // Trả vật phẩm về đúng ô lưới cũ dựa theo chủ sở hữu (Balo hoặc Cốp xe)
            if (currentOwner == GridOwner.Backpack && BackpackMinigameUI.Instance != null)
                BackpackMinigameUI.Instance.OnItemEndDrag(this, eventData.position);
            else if (currentOwner == GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                TrunkMinigameUI.Instance.OnItemEndDrag(this, eventData.position);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // Truyền "this" (chứa toàn bộ data động) thay vì chỉ truyền itemShape (dữ liệu tĩnh)
        if (ItemInfoPanelUI.Instance != null && itemShape != null)
        {
            ItemInfoPanelUI.Instance.ShowInfo(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging && ItemInfoPanelUI.Instance != null && itemShape != null)
        {
            ItemInfoPanelUI.Instance.ShowInfo(this);
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
    public string ItemID => itemShape != null ? itemShape.itemID : ""; // Tự động lấy ID từ ItemShapeSO
    public int Quantity => 1; // Mặc định là 1 (hoặc đổi thành biến số lượng nếu có xếp chồng)
    public int GridX => gridX; // Trả về vị trí X trên lưới
    public int GridY => gridY; // Trả về vị trí Y trên lưới
}