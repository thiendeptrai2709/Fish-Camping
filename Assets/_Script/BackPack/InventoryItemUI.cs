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
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (isFromCooking)
        {
            if (minigameUI != null && minigameUI.TryAutoAddFromCooking(this))
            {
                if (CookingUIManager.Instance != null) CookingUIManager.Instance.OnFoodCollectedSuccessfully();
                if (itemShape != null && QuestManager.Instance != null)
                {
                    string itemName = string.IsNullOrEmpty(itemShape.itemName) ? itemShape.name : itemShape.itemName;
                    QuestManager.Instance.AddProgressByItem(itemName, 1);
                }
            }
        }
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
            if (currentOwner == GridOwner.Backpack && BackpackMinigameUI.Instance != null)
                BackpackMinigameUI.Instance.OnItemEndDrag(this, eventData.position);
            else if (currentOwner == GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                TrunkMinigameUI.Instance.OnItemEndDrag(this, eventData.position);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ItemInfoPanelUI.Instance != null && itemShape != null)
        {
            ItemInfoPanelUI.Instance.ShowInfo(this);
        }

        if (ItemActionMenu.Instance != null && itemShape != null)
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
        if (ItemInfoPanelUI.Instance != null)
        {
            ItemInfoPanelUI.Instance.ClearInfo();
        }
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
            string assetName = itemShape.name != null ? itemShape.name.ToLower() : "";
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
}