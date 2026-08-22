using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
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

    // --- DỮ LIỆU ĐỘ BỀN VÀ SỐ LẦN SỬ DỤNG INSTANCE ---
    private float currentDurability = -1f;
    private int remainingUses = -1;
    private GameObject durabilityBarRoot;
    private Image durabilityFillImage;
    private TMPro.TextMeshProUGUI usesBadgeText;

    public void SetFishInstanceData(float length, float weight, FishGrade grade)
    {
        currentLength = length;
        currentWeight = weight;
        currentGrade = grade;
    }

    public void SetDurability(float val)
    {
        currentDurability = Mathf.Max(0f, val);
        UpdateVisualIndicators();
    }

    public float GetDurability()
    {
        if (currentDurability < 0f)
        {
            if (itemShape is FishingRodSO rod) currentDurability = rod.maxDurability;
            else if (itemShape is BobberSO bobber) currentDurability = bobber.maxDurability;
            else currentDurability = 100f;
        }
        return currentDurability;
    }

    public float GetMaxDurability()
    {
        if (itemShape is FishingRodSO rod) return rod.maxDurability > 0 ? rod.maxDurability : 100f;
        if (itemShape is BobberSO bobber) return bobber.maxDurability > 0 ? bobber.maxDurability : 30f;
        return 100f;
    }

    public void ConsumeDurability(float amount)
    {
        float cur = GetDurability();
        SetDurability(cur - amount);
    }

    public void RepairDurability()
    {
        SetDurability(GetMaxDurability());
    }

    public void SetRemainingUses(int val)
    {
        remainingUses = Mathf.Max(0, val);
        UpdateVisualIndicators();
    }

    public int GetRemainingUses()
    {
        if (remainingUses < 0)
        {
            if (itemShape is BaitSO bait) remainingUses = bait.maxUses > 0 ? bait.maxUses : 5;
            else remainingUses = 1;
        }
        return remainingUses;
    }

    public int GetMaxUses()
    {
        if (itemShape is BaitSO bait) return bait.maxUses > 0 ? bait.maxUses : 5;
        return 1;
    }

    public void ConsumeUse(int amount = 1)
    {
        int cur = GetRemainingUses();
        SetRemainingUses(cur - amount);
    }
    public void SetOriginIngredientSlot(CookingSlotUI slot) => originIngredientSlot = slot;
    public CookingSlotUI GetOriginIngredientSlot() => originIngredientSlot;
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
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        UpdateVisualSize();
        UpdateVisualIndicators();
    }

    public void UpdateVisualSize()
    {
        if (itemShape == null) return;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        float cellSize = 64f;
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

    public void OnPointerDown(PointerEventData eventData)
    {
        // Giữ lại để thỏa mãn IPointerDownHandler của Unity EventSystem
    }

    private Canvas cachedDragCanvas;
    private RectTransform cachedDragCanvasRect;
    private Camera cachedPressCamera;

    private Camera GetEventCamera(Canvas canvas)
    {
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;
        return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
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

        if (ItemInfoPanelUI.Instance != null)
        {
            ItemInfoPanelUI.Instance.ClearInfo();
        }

        cachedDragCanvas = GetComponentInParent<Canvas>();
        if (cachedDragCanvas != null)
        {
            cachedDragCanvasRect = cachedDragCanvas.GetComponent<RectTransform>();
            cachedPressCamera = GetEventCamera(cachedDragCanvas);
        }

        if (isFromCooking || originIngredientSlot != null)
        {
            if (cachedDragCanvas != null) transform.SetParent(cachedDragCanvas.transform, true);

            rectTransform.pivot = new Vector2(0, 1);
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
            if (rootCanvas != null)
            {
                cachedDragCanvas = rootCanvas;
                cachedDragCanvasRect = rootCanvas.GetComponent<RectTransform>();
                cachedPressCamera = GetEventCamera(rootCanvas);
                transform.SetParent(rootCanvas.transform, true);
            }
            else transform.SetParent(currentSlot.transform.root, true);

            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            UpdateVisualSize();

            transform.SetAsLastSibling();
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.OnItemBeginDragFromExternal(this, eventData.position);
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

        Camera backpackCam = (BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetRootCanvas() != null)
            ? GetEventCamera(BackpackMinigameUI.Instance.GetRootCanvas())
            : eventData.pressEventCamera;

        Camera trunkCam = (TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetRootCanvas() != null)
            ? GetEventCamera(TrunkMinigameUI.Instance.GetRootCanvas())
            : eventData.pressEventCamera;

        bool overBackpack = BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetGridRoot() != null && BackpackMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(BackpackMinigameUI.Instance.GetGridRoot(), eventData.position, backpackCam);
        bool overTrunk = TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetGridRoot() != null && TrunkMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(TrunkMinigameUI.Instance.GetGridRoot(), eventData.position, trunkCam);

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

            if (cachedDragCanvas == null)
            {
                cachedDragCanvas = GetComponentInParent<Canvas>();
                if (cachedDragCanvas != null)
                {
                    cachedDragCanvasRect = cachedDragCanvas.GetComponent<RectTransform>();
                    cachedPressCamera = GetEventCamera(cachedDragCanvas);
                }
            }

            if (cachedDragCanvas != null && cachedDragCanvasRect != null)
            {
                if (transform.parent != cachedDragCanvas.transform)
                {
                    transform.SetParent(cachedDragCanvas.transform, true);
                    transform.SetAsLastSibling();
                }
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(cachedDragCanvasRect, eventData.position, cachedPressCamera, out Vector3 worldPoint))
                    transform.position = worldPoint;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (isHandledBySlot)
        {
            isHandledBySlot = false;
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();
            return;
        }

        Camera backpackCam = (BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetRootCanvas() != null)
            ? GetEventCamera(BackpackMinigameUI.Instance.GetRootCanvas())
            : eventData.pressEventCamera;

        Camera trunkCam = (TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetRootCanvas() != null)
            ? GetEventCamera(TrunkMinigameUI.Instance.GetRootCanvas())
            : eventData.pressEventCamera;

        if (isFromCooking || originIngredientSlot != null)
        {
            bool isOverBalo = BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetGridRoot() != null && BackpackMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(BackpackMinigameUI.Instance.GetGridRoot(), eventData.position, backpackCam);
            bool isOverXe = TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetGridRoot() != null && TrunkMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(TrunkMinigameUI.Instance.GetGridRoot(), eventData.position, trunkCam);
            bool placedInGrid = false;

            if (isOverXe) 
            { 
                placedInGrid = TrunkMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position); 
                if (placedInGrid) currentOwner = GridOwner.Trunk; 
            }
            else if (isOverBalo || BackpackMinigameUI.Instance != null)
            {
                placedInGrid = BackpackMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position);
                if (!placedInGrid && BackpackMinigameUI.Instance != null)
                {
                    placedInGrid = BackpackMinigameUI.Instance.TryAutoFitItemToGrid(this);
                }

                if (placedInGrid)
                {
                    currentOwner = GridOwner.Backpack;
                    if (itemShape != null && QuestManager.Instance != null)
                    {
                        string itemName = string.IsNullOrEmpty(itemShape.itemName) ? itemShape.name : itemShape.itemName;
                        QuestManager.Instance.AddProgressByItem(itemName, 1);
                    }
                }
            }

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

        if (isEquipped)
        {
            bool isOverXe = TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetGridRoot() != null && TrunkMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(TrunkMinigameUI.Instance.GetGridRoot(), eventData.position, trunkCam);
            bool isOverBalo = BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetGridRoot() != null && BackpackMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(BackpackMinigameUI.Instance.GetGridRoot(), eventData.position, backpackCam);
            bool placedInGrid = false;

            if (isOverXe)
            {
                placedInGrid = TrunkMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position);
                if (placedInGrid) currentOwner = GridOwner.Trunk;
            }
            else
            {
                if (BackpackMinigameUI.Instance != null)
                {
                    placedInGrid = BackpackMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position);
                    if (placedInGrid) currentOwner = GridOwner.Backpack;
                }
            }

            if (placedInGrid)
            {
                isEquipped = false;
                currentSlot = null;
                if (itemShape != null && QuestManager.Instance != null)
                {
                    string itemName = string.IsNullOrEmpty(itemShape.itemName) ? itemShape.name : itemShape.itemName;
                    QuestManager.Instance.AddProgressByItem(itemName, 1);
                }
            }
            else
            {
                if (currentSlot != null)
                {
                    currentSlot.ReturnItemToSlot(this);
                }
            }
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();
            return;
        }

        bool overBackpackGrid = BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.GetGridRoot() != null && BackpackMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(BackpackMinigameUI.Instance.GetGridRoot(), eventData.position, backpackCam);
        bool overTrunkGrid = TrunkMinigameUI.Instance != null && TrunkMinigameUI.Instance.GetGridRoot() != null && TrunkMinigameUI.Instance.GetGridRoot().gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(TrunkMinigameUI.Instance.GetGridRoot(), eventData.position, trunkCam);

        bool placed = false;
        if (overTrunkGrid)
        {
            placed = TrunkMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position);
            if (placed) currentOwner = GridOwner.Trunk;
        }
        else if (overBackpackGrid)
        {
            placed = BackpackMinigameUI.Instance.TryPlaceItemFromExternal(this, eventData.position);
            if (placed)
            {
                currentOwner = GridOwner.Backpack;
                if (itemShape != null && QuestManager.Instance != null)
                {
                    string itemName = string.IsNullOrEmpty(itemShape.itemName) ? itemShape.name : itemShape.itemName;
                    QuestManager.Instance.AddProgressByItem(itemName, 1);
                }
            }
        }

        if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
        if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();

        if (!placed)
        {
            // Kiểm tra xem vị trí thả chuột có nằm trên bất kỳ ô trang bị EquipmentSlotUI nào không (Fallback bắt dính cực nhạy)
            EquipmentSlotUI[] allEquipSlots = Object.FindObjectsByType<EquipmentSlotUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var slot in allEquipSlots)
            {
                if (slot == null || !slot.gameObject.activeInHierarchy) continue;
                RectTransform slotRect = slot.GetComponent<RectTransform>();
                Canvas slotCanvas = slot.GetComponentInParent<Canvas>();
                Camera slotCam = (slotCanvas != null) ? GetEventCamera(slotCanvas) : eventData.pressEventCamera;

                if (slotRect != null && RectTransformUtility.RectangleContainsScreenPoint(slotRect, eventData.position, slotCam))
                {
                    if (slot.CanEquip(itemShape))
                    {
                        slot.OnDrop(eventData);
                        if (isHandledBySlot)
                        {
                            isHandledBySlot = false;
                            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
                            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();
                            return;
                        }
                    }
                }
            }

            // Kiểm tra xem vị trí thả chuột có nằm trên bất kỳ ô HUDHotbarSlotUI nào không
            HUDHotbarSlotUI[] allHudSlots = Object.FindObjectsByType<HUDHotbarSlotUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var hudSlot in allHudSlots)
            {
                if (hudSlot == null || !hudSlot.gameObject.activeInHierarchy) continue;
                RectTransform hudRect = hudSlot.GetComponent<RectTransform>();
                Canvas hudCanvas = hudSlot.GetComponentInParent<Canvas>();
                Camera hudCam = (hudCanvas != null) ? GetEventCamera(hudCanvas) : eventData.pressEventCamera;

                if (hudRect != null && RectTransformUtility.RectangleContainsScreenPoint(hudRect, eventData.position, hudCam))
                {
                    hudSlot.OnDrop(eventData);
                    if (isHandledBySlot)
                    {
                        isHandledBySlot = false;
                        if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.HideHighlight();
                        if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.HideHighlight();
                        return;
                    }
                }
            }

            if (currentOwner == GridOwner.Backpack && BackpackMinigameUI.Instance != null)
                BackpackMinigameUI.Instance.OnItemEndDrag(this, eventData.position);
            else if (currentOwner == GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                TrunkMinigameUI.Instance.OnItemEndDrag(this, eventData.position);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemShape == null) return;
        if (isDragging) return;

        // Nếu là thức ăn đã nấu chín trên giá nấu (isFromCooking), click chuột trái sẽ tự động nhặt vào Balo một lần duy nhất
        if (isFromCooking && eventData.button == PointerEventData.InputButton.Left)
        {
            if (BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.TryAutoAddFromCooking(this))
            {
                isFromCooking = false;
                if (CookingUIManager.Instance != null)
                {
                    CookingUIManager.Instance.OnFoodCollectedSuccessfully();
                }
                if (itemShape != null && QuestManager.Instance != null)
                {
                    string itemName = string.IsNullOrEmpty(itemShape.itemName) ? itemShape.name : itemShape.itemName;
                    QuestManager.Instance.AddProgressByItem(itemName, 1);
                }
                return;
            }
            else
            {
                Debug.Log("<color=red>[Cooking] Balo đã đầy, hãy dọn chỗ trống trước khi lấy thức ăn!</color>");
                return;
            }
        }

        // HIỂN THỊ BẢNG CHI TIẾT & MENU THAO TÁC (ITEM ACTION MENU ĐỂ SỬA CẦN, ĂN, ĐỔ XĂNG)
        if (ItemInfoPanelUI.Instance != null)
        {
            ItemInfoPanelUI.Instance.ShowInfo(this);
        }

        if (ItemActionMenu.Instance != null)
        {
            ItemActionMenu.Instance.ShowMenu(this);
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
        // Giữ bảng thông tin hiển thị món đồ vừa chọn/rê chuột, không tắt vội để người chơi dễ đọc và thao tác
    }

    public ItemShapeSO GetItemShape() => itemShape;
    public int GetGridX() => gridX;
    public int GetGridY() => gridY;
    public bool IsRotated() => isRotated;

    // ==========================================
    // HÀM ĂN MÓN ĂN - CHỈ ĂN ĐƯỢC MÓN ĐÃ NẤU CHÍN
    // ==========================================
    public void ConsumeItem()
    {
        // 1. Nếu là Cá Sống (FishSO) -> Khóa tuyệt đối, không cho ăn
        if (itemShape is FishSO)
        {
            Debug.LogWarning("<color=yellow>[InventoryItemUI] Cá sống chưa thể ăn trực tiếp! Hãy đặt lên Vỉ Nướng để nấu chín.</color>");
            return;
        }

        // 2. Nếu là Món Ăn (FoodSO)
        if (itemShape is FoodSO foodData)
        {
            if (CharacterStatsManager.Instance != null)
            {
                float hunger = foodData.hungerRestore > 0 ? foodData.hungerRestore : 35f;
                float thirst = foodData.thirstRestore > 0 ? foodData.thirstRestore : 20f;
                float energy = foodData.energyRestore > 0 ? foodData.energyRestore : 30f;

                CharacterStatsManager.Instance.ModifyStat(StatType.Hunger, hunger);
                CharacterStatsManager.Instance.ModifyStat(StatType.Thirst, thirst);
                CharacterStatsManager.Instance.ModifyStat(StatType.Energy, energy);
                CharacterStatsManager.Instance.ModifyStat(StatType.Comfort, 15f);
            }

            if (currentOwner == GridOwner.Backpack && BackpackMinigameUI.Instance != null)
            {
                BackpackMinigameUI.Instance.RemoveItem(this);
            }
            ForcedTutorialManager.Instance?.NotifyEatFish();
        }
        // 3. Nếu là các món ăn đã chế biến khác (không phải cá sống)
        else if (itemShape != null)
        {
            string id = itemShape.itemID != null ? itemShape.itemID.ToLower() : "";
            string assetName = itemShape.name != null ? shapeNameClean(itemShape.name) : "";
            string itemName = itemShape.itemName != null ? itemShape.itemName.ToLower() : "";

            bool isCookedItem = id.Contains("cooked") || id.Contains("food") || assetName.Contains("food") || 
                               itemName.Contains("nướng") || itemName.Contains("thức ăn") || itemName.Contains("thịt nướng") || itemName.Contains("bánh");

            if (isCookedItem)
            {
                if (CharacterStatsManager.Instance != null)
                {
                    CharacterStatsManager.Instance.ModifyStat(StatType.Hunger, 35f);
                    CharacterStatsManager.Instance.ModifyStat(StatType.Thirst, 20f);
                    CharacterStatsManager.Instance.ModifyStat(StatType.Energy, 30f);
                    CharacterStatsManager.Instance.ModifyStat(StatType.Comfort, 15f);
                }

                if (currentOwner == GridOwner.Backpack && BackpackMinigameUI.Instance != null)
                {
                    BackpackMinigameUI.Instance.RemoveItem(this);
                }
                ForcedTutorialManager.Instance?.NotifyEatFish();
            }
        }
    }

    private string shapeNameClean(string s) => s != null ? s.ToLower() : "";

    public void UpdateVisualIndicators()
    {
        if (itemShape == null) return;

        // 1. THANH ĐỘ BỀN SIÊU MỎNG (CHO CẦN CÂU VÀ PHAO CÂU) - NẰM SÁT VIỀN ĐÁY, KHÔNG CHE ẢNH
        bool hasDurability = (itemShape is FishingRodSO || itemShape is BobberSO);
        if (hasDurability)
        {
            float curDur = GetDurability();
            float maxDur = GetMaxDurability();
            float ratio = maxDur > 0f ? Mathf.Clamp01(curDur / maxDur) : 1f;

            if (durabilityBarRoot == null)
            {
                // Tạo thanh nền độ bền siêu mỏng gọn sát mép dưới cùng
                durabilityBarRoot = new GameObject("DurabilityBarRoot", typeof(RectTransform), typeof(Image));
                durabilityBarRoot.transform.SetParent(transform, false);

                RectTransform bgRect = durabilityBarRoot.GetComponent<RectTransform>();
                bgRect.anchorMin = new Vector2(0f, 0f);
                bgRect.anchorMax = new Vector2(1f, 0f);
                bgRect.pivot = new Vector2(0.5f, 0f);
                bgRect.sizeDelta = new Vector2(-6f, 3.5f);
                bgRect.anchoredPosition = new Vector2(0f, 2f);

                Image bgImg = durabilityBarRoot.GetComponent<Image>();
                bgImg.color = new Color(0f, 0f, 0f, 0.65f);
                bgImg.raycastTarget = false;

                // Tạo thanh Fill
                GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(durabilityBarRoot.transform, false);

                RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;

                durabilityFillImage = fillObj.GetComponent<Image>();
                durabilityFillImage.type = Image.Type.Filled;
                durabilityFillImage.fillMethod = Image.FillMethod.Horizontal;
                durabilityFillImage.raycastTarget = false;
            }

            if (durabilityFillImage != null)
            {
                durabilityBarRoot.SetActive(true);
                durabilityFillImage.fillAmount = ratio;
                if (ratio > 0.5f)
                {
                    durabilityFillImage.color = new Color(0.2f, 0.95f, 0.35f, 0.95f); // Xanh neon
                }
                else if (ratio > 0.2f)
                {
                    durabilityFillImage.color = new Color(1f, 0.8f, 0.15f, 0.95f); // Vàng
                }
                else
                {
                    durabilityFillImage.color = new Color(1f, 0.25f, 0.25f, 0.95f); // Đỏ
                }
            }
        }
        else if (durabilityBarRoot != null)
        {
            durabilityBarRoot.SetActive(false);
        }

        // 2. BADGE SỐ LƯỢT DÙNG (CHO MỒI CÂU) - GÓC DƯỚI PHẢI NHỎ GỌN
        if (itemShape is BaitSO)
        {
            int uses = GetRemainingUses();
            if (usesBadgeText == null)
            {
                GameObject badgeObj = new GameObject("UsesBadge", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
                badgeObj.transform.SetParent(transform, false);

                RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(1f, 0f);
                badgeRect.anchorMax = new Vector2(1f, 0f);
                badgeRect.pivot = new Vector2(1f, 0f);
                badgeRect.sizeDelta = new Vector2(28f, 16f);
                badgeRect.anchoredPosition = new Vector2(-2f, 1f);

                usesBadgeText = badgeObj.GetComponent<TMPro.TextMeshProUGUI>();
                usesBadgeText.alignment = TMPro.TextAlignmentOptions.BottomRight;
                usesBadgeText.fontSize = 11f;
                usesBadgeText.fontStyle = TMPro.FontStyles.Bold;
                usesBadgeText.color = new Color(1f, 0.95f, 0.6f, 1f);
                usesBadgeText.raycastTarget = false;

                var outline = badgeObj.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1f, -1f);
            }

            if (usesBadgeText != null)
            {
                usesBadgeText.gameObject.SetActive(true);
                usesBadgeText.text = $"x{uses}";
                usesBadgeText.color = (uses <= 1) ? new Color(1f, 0.35f, 0.35f, 1f) : new Color(1f, 0.95f, 0.6f, 1f);
            }
        }
        else if (usesBadgeText != null)
        {
            usesBadgeText.gameObject.SetActive(false);
        }
    }
}