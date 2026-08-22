using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TrunkMinigameUI : MonoBehaviour
{
    public static TrunkMinigameUI Instance { get; private set; }

    [Header("Link Dữ Liệu & UI")]
    [SerializeField] private InventoryGridData gridData;
    [SerializeField] private RectTransform gridRootRect;
    [SerializeField] private RectTransform itemsContainer;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private GameObject cellVisualPrefab;
    [SerializeField] private Image highlightOverlay;

    [Header("--- TẤT CẢ ITEM TRONG GAME ĐỂ LOAD CỐP ---")]
    [Tooltip("Kéo toàn bộ ScriptableObject Item/Fish/Rod vào đây để load dữ liệu cốp xe lúc khởi động")]
    [SerializeField] private ItemShapeSO[] allDatabaseItems;

    private const string TRUNK_SAVE_KEY = "Saved_Trunk_Data";

    private int startDragX;
    private int startDragY;
    private bool startDragRotated;
    private int dragGridOffsetX;
    private int dragGridOffsetY;
    private bool isGridInitialized = false;
    private bool isLoading = false;

    private Canvas cachedRootCanvas;
    private RectTransform cachedRootCanvasRect;
    private Camera cachedPressCamera;

    public Canvas GetRootCanvas()
    {
        if (cachedRootCanvas == null) CacheCanvasReferences();
        return cachedRootCanvas;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CacheCanvasReferences();
    }

    private void CacheCanvasReferences()
    {
        if (gridRootRect != null)
        {
            cachedRootCanvas = gridRootRect.GetComponentInParent<Canvas>();
            if (cachedRootCanvas != null)
            {
                cachedRootCanvasRect = cachedRootCanvas.GetComponent<RectTransform>();
                cachedPressCamera = cachedRootCanvas.renderMode != RenderMode.ScreenSpaceOverlay 
                    ? (cachedRootCanvas.worldCamera != null ? cachedRootCanvas.worldCamera : Camera.main) 
                    : null;
            }
        }
    }

    private void Start()
    {
        InitializeTrunkUI();
    }

    private void OnEnable()
    {
        InitializeTrunkUI();
        LoadTrunk();
        ForcedTutorialManager.Instance?.NotifyTrunkOpened();
    }

    private void OnDisable()
    {
        if (!isLoading)
        {
            SaveTrunk();
        }
    }

    private void InitializeTrunkUI()
    {
        if (isGridInitialized) return;

        if (gridData != null)
        {
            gridData.InitGrid();
        }

        CreateVisualGrid();

        // Tự động tìm lại itemsContainer nếu bị rỗng
        if (itemsContainer == null && gridRootRect != null)
        {
            Transform found = gridRootRect.Find("ItemsContainer");
            if (found != null) itemsContainer = found.GetComponent<RectTransform>();
            else itemsContainer = gridRootRect;
        }

        if (itemsContainer != null && gridRootRect != null)
        {
            if (itemsContainer.parent != gridRootRect)
            {
                itemsContainer.SetParent(gridRootRect, false);
            }
            itemsContainer.anchorMin = Vector2.zero;
            itemsContainer.anchorMax = Vector2.one;
            itemsContainer.offsetMin = Vector2.zero;
            itemsContainer.offsetMax = Vector2.zero;
            itemsContainer.SetAsLastSibling();
        }

        if (highlightOverlay != null)
        {
            highlightOverlay.rectTransform.pivot = new Vector2(0, 1);
            highlightOverlay.rectTransform.anchorMin = new Vector2(0, 1);
            highlightOverlay.rectTransform.anchorMax = new Vector2(0, 1);
            highlightOverlay.gameObject.SetActive(false);
            highlightOverlay.transform.SetAsLastSibling();
        }

        isGridInitialized = true;
    }

    // ==========================================
    // HỆ THỐNG THÊM ĐỒ VÀO CỐP XE TỰ ĐỘNG (DÀNH CHO SHOP / TRỤ XĂNG)
    // ==========================================
    public bool AddItemToTrunk(ItemShapeSO shape)
    {
        if (shape == null) return false;

        if (!isGridInitialized || gridData == null)
        {
            InitializeTrunkUI();
            LoadTrunk();
        }

        int width = gridData.GetGridWidth();
        int height = gridData.GetGridHeight();

        // Tìm vị trí ô trống đầu tiên để nhét vừa
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridData.CanPlaceItem(x, y, shape, false))
                {
                    InventoryItemUI spawned = ForceSpawnLoadedItem(shape, x, y, false);
                    if (spawned != null)
                    {
                        spawned.currentOwner = InventoryItemUI.GridOwner.Trunk;
                    }
                    SaveTrunk();
                    Debug.Log($"<color=green>[Trunk] Đã thêm thành công {shape.itemName} vào cốp tại ô ({x}, {y})!</color>");
                    return true;
                }
            }
        }

        Debug.LogWarning("[Trunk] Cốp xe đã đầy, không còn vị trí trống!");
        return false;
    }

    // ==========================================
    // HỆ THỐNG SAVE & LOAD CỐP XE
    // ==========================================
    public void SaveTrunk()
    {
        if (isLoading || itemsContainer == null) return;

        InventoryItemUI[] allItems = itemsContainer.GetComponentsInChildren<InventoryItemUI>();
        BackpackSaveContainer container = new BackpackSaveContainer();
        HashSet<string> recordedPositions = new HashSet<string>();

        foreach (var item in allItems)
        {
            if (item == null || item.GetItemShape() == null) continue;
            if (item.transform.parent != itemsContainer) continue;

            string posKey = $"{item.GetGridX()}_{item.GetGridY()}";
            if (recordedPositions.Contains(posKey))
            {
                continue; // Tránh lưu đè 2 item cùng 1 vị trí / vật phẩm song sinh
            }
            recordedPositions.Add(posKey);

            ItemShapeSO shape = item.GetItemShape();
            string finalID = !string.IsNullOrEmpty(shape.itemID) ? shape.itemID : shape.name;

            SavedItemData data = new SavedItemData
            {
                itemID = finalID,
                gridX = item.GetGridX(),
                gridY = item.GetGridY(),
                isRotated = item.IsRotated(),
                isFish = (shape is FishSO),
                fishLength = item.GetLength(),
                fishWeight = item.GetWeight(),
                fishGrade = (int)item.GetGrade(),
                durability = item.GetDurability(),
                remainingUses = item.GetRemainingUses()
            };

            container.items.Add(data);
        }

        string json = JsonUtility.ToJson(container);
        PlayerPrefs.SetString(TRUNK_SAVE_KEY, json);
        PlayerPrefs.Save();
        GameDatabaseManager.Instance?.SaveAndSyncToCloud();
        Debug.Log($"<color=green>[Trunk] Đã lưu {container.items.Count} món đồ vào Cốp xe!</color>");
    }

    public void LoadTrunk()
    {
        isLoading = true;
        try
        {
            string json = PlayerPrefs.GetString(TRUNK_SAVE_KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                // Nếu chưa có dữ liệu lưu thì xóa các item visual đang tồn tại
                ClearAllVisualItems();
                if (gridData != null) gridData.InitGrid();
                return;
            }

            BackpackSaveContainer container = JsonUtility.FromJson<BackpackSaveContainer>(json);
            if (container == null || container.items == null)
            {
                ClearAllVisualItems();
                if (gridData != null) gridData.InitGrid();
                return;
            }

            ClearAllVisualItems();

            if (gridData != null)
            {
                gridData.InitGrid();
            }

            HashSet<string> loadedPositions = new HashSet<string>();

            foreach (var saved in container.items)
            {
                if (saved == null) continue;
                string posKey = $"{saved.gridX}_{saved.gridY}";
                if (loadedPositions.Contains(posKey)) continue; // Ngăn chặn tạo trùng lặp
                loadedPositions.Add(posKey);

                ItemShapeSO foundShape = FindItemShapeByID(saved.itemID);
                if (foundShape != null)
                {
                    InventoryItemUI spawned = ForceSpawnLoadedItem(foundShape, saved.gridX, saved.gridY, saved.isRotated);
                    if (spawned != null)
                    {
                        spawned.currentOwner = InventoryItemUI.GridOwner.Trunk;
                        if (saved.isFish)
                        {
                            spawned.SetFishInstanceData(saved.fishLength, saved.fishWeight, (FishGrade)saved.fishGrade);
                        }
                        if (saved.durability >= 0f)
                        {
                            spawned.SetDurability(saved.durability);
                        }
                        if (saved.remainingUses >= 0)
                        {
                            spawned.SetRemainingUses(saved.remainingUses);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"<color=red>[Trunk] Không tìm thấy ScriptableObject với ID: {saved.itemID}</color>");
                }
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ClearAllVisualItems()
    {
        Transform targetParent = itemsContainer != null ? itemsContainer : (gridRootRect != null ? gridRootRect : transform);
        InventoryItemUI[] oldItems = targetParent.GetComponentsInChildren<InventoryItemUI>(true);
        foreach (var old in oldItems)
        {
            if (old != null)
            {
                old.transform.SetParent(null); // Gỡ cha ngay lập tức để không bị quét trúng
                Destroy(old.gameObject);
            }
        }
    }

    // Luôn đảm bảo sinh con đúng trong itemsContainer / Canvas của Cốp
    private InventoryItemUI ForceSpawnLoadedItem(ItemShapeSO shape, int startX, int startY, bool rotated)
    {
        if (itemUIPrefab == null) return null;

        Transform targetParent = itemsContainer != null ? itemsContainer : (gridRootRect != null ? gridRootRect : transform);

        GameObject itemObj = Instantiate(itemUIPrefab, targetParent);
        RectTransform rect = itemObj.GetComponent<RectTransform>();
        rect.pivot = rotated ? new Vector2(0, 0) : new Vector2(0, 1);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);

        InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
        itemUI.currentOwner = InventoryItemUI.GridOwner.Trunk;
        itemUI.Setup(shape, null, startX, startY, rotated);

        if (gridData != null)
        {
            gridData.PlaceItem(startX, startY, shape, rotated);
        }

        rect.anchoredPosition = GetAnchoredPositionFromGridIndex(startX, startY);
        itemUI.UpdateVisualSize();

        return itemUI;
    }

    public InventoryItemUI SpawnItem(ItemShapeSO shape, int startX, int startY, bool rotated = false)
    {
        if (gridData == null || itemUIPrefab == null) return null;

        if (gridData.CanPlaceItem(startX, startY, shape, rotated))
        {
            return ForceSpawnLoadedItem(shape, startX, startY, rotated);
        }
        return null;
    }

    private ItemShapeSO FindItemShapeByID(string id)
    {
        if (allDatabaseItems != null)
        {
            foreach (var item in allDatabaseItems)
            {
                if (item != null)
                {
                    string checkID = !string.IsNullOrEmpty(item.itemID) ? item.itemID : item.name;
                    if (checkID == id || item.name == id) return item;
                }
            }
        }

        ItemShapeSO loaded = Resources.Load<ItemShapeSO>(id);
        return loaded;
    }

    [ContextMenu("Xóa dữ liệu Cốp Xe (Reset Trunk)")]
    public void ClearTrunkSave()
    {
        PlayerPrefs.DeleteKey(TRUNK_SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>[Trunk] Đã xóa dữ liệu lưu trữ Cốp Xe thành công!</color>");
    }

    public void CreateVisualGrid()
    {
        if (gridData == null || cellVisualPrefab == null || gridRootRect == null) return;

        foreach (Transform child in gridRootRect)
        {
            if (child != itemsContainer && (highlightOverlay == null || child != highlightOverlay.transform))
            {
                Destroy(child.gameObject);
            }
        }

        int width = gridData.GetGridWidth();
        int height = gridData.GetGridHeight();
        float cellSize = gridData.GetCellSize();

        gridRootRect.sizeDelta = new Vector2(width * cellSize, height * cellSize);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellVisualPrefab, gridRootRect);
                RectTransform rect = cell.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0, 1);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(cellSize, cellSize);
                rect.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
            }
        }
    }

    public InventoryGridData GetGridData() => gridData;
    public RectTransform GetGridRoot() => gridRootRect;
    public RectTransform GetItemsContainer() => itemsContainer;
    public float GetCellSize() => gridData != null ? gridData.GetCellSize() : 64f;
    public void HideHighlight() { if (highlightOverlay != null) highlightOverlay.gameObject.SetActive(false); }
    public Vector2 GetAnchoredPositionFromGridIndex(int x, int y) => new Vector2(x * GetCellSize(), -y * GetCellSize());

    public bool GetGridIndexFromScreenPosition(Vector2 screenPosition, out int x, out int y)
    {
        x = -1; y = -1;
        if (gridRootRect == null || gridData == null) return false;
        if (cachedRootCanvas == null) CacheCanvasReferences();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRootRect, screenPosition, cachedPressCamera, out Vector2 localPoint))
        {
            float cellSize = gridData.GetCellSize();
            x = Mathf.FloorToInt((localPoint.x - gridRootRect.rect.xMin) / cellSize);
            y = Mathf.FloorToInt((gridRootRect.rect.yMax - localPoint.y) / cellSize);
            return (x >= 0 && x < gridData.GetGridWidth() && y >= 0 && y < gridData.GetGridHeight());
        }
        return false;
    }

    public bool GetClampedGridIndex(Vector2 screenPosition, ItemShapeSO shape, bool isRotated, out int x, out int y)
    {
        x = 0; y = 0;
        if (gridRootRect == null || gridData == null) return false;
        if (cachedRootCanvas == null) CacheCanvasReferences();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRootRect, screenPosition, cachedPressCamera, out Vector2 localPoint))
        {
            float cellSize = gridData.GetCellSize();
            int rawMouseX = Mathf.FloorToInt((localPoint.x - gridRootRect.rect.xMin) / cellSize);
            int rawMouseY = Mathf.FloorToInt((gridRootRect.rect.yMax - localPoint.y) / cellSize);
            int maxOffsetX = shape.GetWidth(isRotated) - 1;
            int maxOffsetY = shape.GetHeight(isRotated) - 1;
            x = Mathf.Clamp(rawMouseX - Mathf.Clamp(dragGridOffsetX, 0, Mathf.Max(0, maxOffsetX)), 0, Mathf.Max(0, gridData.GetGridWidth() - shape.GetWidth(isRotated)));
            y = Mathf.Clamp(rawMouseY - Mathf.Clamp(dragGridOffsetY, 0, Mathf.Max(0, maxOffsetY)), 0, Mathf.Max(0, gridData.GetGridHeight() - shape.GetHeight(isRotated)));
            return true;
        }
        return false;
    }

    public void OnItemBeginDrag(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        startDragX = itemUI.GetGridX();
        startDragY = itemUI.GetGridY();
        startDragRotated = itemUI.IsRotated();

        if (GetGridIndexFromScreenPosition(screenPosition, out int mouseGridX, out int mouseGridY))
        {
            dragGridOffsetX = mouseGridX - startDragX;
            dragGridOffsetY = mouseGridY - startDragY;
        }
        else
        {
            dragGridOffsetX = 0;
            dragGridOffsetY = 0;
        }

        itemUI.transform.SetParent(itemsContainer != null ? itemsContainer : gridRootRect);
        itemUI.transform.SetAsLastSibling();

        gridData.ClearCells(startDragX, startDragY, itemUI.GetItemShape(), startDragRotated);

        if (highlightOverlay != null)
        {
            highlightOverlay.gameObject.SetActive(true);
            highlightOverlay.transform.SetParent(itemsContainer != null ? itemsContainer : gridRootRect);
            highlightOverlay.transform.SetSiblingIndex(itemUI.transform.GetSiblingIndex());
            highlightOverlay.rectTransform.sizeDelta = itemUI.GetComponent<RectTransform>().sizeDelta;
            highlightOverlay.rectTransform.localRotation = Quaternion.Euler(0f, 0f, startDragRotated ? -90f : 0f);
        }

        OnItemDragging(itemUI, screenPosition);
    }

    public void OnItemDragging(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        if (gridRootRect == null || gridData == null) return;

        if (GetGridIndexFromScreenPosition(screenPosition, out int rawX, out int rawY))
        {
            if (GetClampedGridIndex(screenPosition, itemUI.GetItemShape(), itemUI.IsRotated(), out int targetX, out int targetY))
            {
                bool canPlace = gridData.CanPlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated());
                if (!canPlace && !itemUI.IsEquipped())
                {
                    if (itemUI.currentOwner == InventoryItemUI.GridOwner.Trunk)
                        canPlace = CanSwapItems(itemUI, targetX, targetY);
                    else
                        canPlace = CanSwapItemsExternal(itemUI, targetX, targetY);
                }

                Vector2 snappedPosition = GetAnchoredPositionFromGridIndex(targetX, targetY);

                if (highlightOverlay != null)
                {
                    highlightOverlay.gameObject.SetActive(true);
                    highlightOverlay.rectTransform.anchoredPosition = snappedPosition;
                    highlightOverlay.rectTransform.sizeDelta = itemUI.GetComponent<RectTransform>().sizeDelta;
                    highlightOverlay.rectTransform.pivot = itemUI.GetComponent<RectTransform>().pivot;
                    highlightOverlay.rectTransform.localRotation = itemUI.GetComponent<RectTransform>().localRotation;
                    highlightOverlay.color = canPlace ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
                }

                Transform container = itemsContainer != null ? itemsContainer : gridRootRect;
                if (itemUI.transform.parent != container)
                {
                    itemUI.transform.SetParent(container, false);
                    itemUI.transform.SetAsLastSibling();
                }
                itemUI.GetComponent<RectTransform>().anchoredPosition = snappedPosition;
            }
        }
        else
        {
            HideHighlight();

            if (cachedRootCanvas == null) CacheCanvasReferences();
            if (cachedRootCanvas != null && cachedRootCanvasRect != null)
            {
                if (itemUI.transform.parent != cachedRootCanvas.transform)
                {
                    itemUI.transform.SetParent(cachedRootCanvas.transform, true);
                    itemUI.transform.SetAsLastSibling();
                }

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(cachedRootCanvasRect, screenPosition, cachedPressCamera, out Vector3 worldPoint))
                {
                    itemUI.transform.position = worldPoint;
                }
            }
        }
    }

    public void OnItemEndDrag(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        itemUI.transform.SetParent(itemsContainer != null ? itemsContainer : gridRootRect);

        RectTransform itemRect = itemUI.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(0, 1);

        HideHighlight();

        if (GetGridIndexFromScreenPosition(screenPosition, out int rawX, out int rawY))
        {
            if (GetClampedGridIndex(screenPosition, itemUI.GetItemShape(), itemUI.IsRotated(), out int targetX, out int targetY))
            {
                if (gridData.CanPlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated()))
                {
                    gridData.PlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated());
                    itemUI.SetGridPosition(targetX, targetY);
                    itemRect.anchoredPosition = GetAnchoredPositionFromGridIndex(targetX, targetY);
                    SaveTrunk();
                    return;
                }
                else
                {
                    if (TrySwapItems(itemUI, targetX, targetY))
                    {
                        SaveTrunk();
                        return;
                    }
                }
            }
        }

        if (itemUI.IsRotated() != startDragRotated)
        {
            itemUI.ToggleRotate();
        }
        gridData.PlaceItem(startDragX, startDragY, itemUI.GetItemShape(), startDragRotated);
        itemUI.SetGridPosition(startDragX, startDragY);
        itemRect.anchoredPosition = GetAnchoredPositionFromGridIndex(startDragX, startDragY);
        SaveTrunk();
    }

    public bool TryPlaceItemFromExternal(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        if (gridRootRect == null || gridData == null || itemUI == null || itemUI.GetItemShape() == null) return false;

        if (GetClampedGridIndex(screenPosition, itemUI.GetItemShape(), itemUI.IsRotated(), out int targetX, out int targetY))
        {
            if (gridData.CanPlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated()))
            {
                PlaceItemDirectlyToGrid(itemUI, targetX, targetY, itemUI.IsRotated());
                return true;
            }
            else
            {
                if (TrySwapItemsExternal(itemUI, targetX, targetY))
                {
                    return true;
                }
            }
        }

        // Dự phòng an toàn: Tự động xếp vào ô trống khả dụng bất kỳ trong Cốp xe
        return TryAutoFitItemToGrid(itemUI);
    }

    public bool TryAutoFitItemToGrid(InventoryItemUI itemUI)
    {
        if (gridData == null || itemUI == null || itemUI.GetItemShape() == null) return false;
        int width = gridData.GetGridWidth();
        int height = gridData.GetGridHeight();
        ItemShapeSO shape = itemUI.GetItemShape();

        // 1. Thử theo hướng xoay hiện tại
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridData.CanPlaceItem(x, y, shape, itemUI.IsRotated()))
                {
                    PlaceItemDirectlyToGrid(itemUI, x, y, itemUI.IsRotated());
                    return true;
                }
            }
        }

        // 2. Thử xoay 90 độ nếu chưa vừa
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridData.CanPlaceItem(x, y, shape, !itemUI.IsRotated()))
                {
                    itemUI.ToggleRotate();
                    PlaceItemDirectlyToGrid(itemUI, x, y, itemUI.IsRotated());
                    return true;
                }
            }
        }

        return false;
    }

    public void PlaceItemDirectlyToGrid(InventoryItemUI itemUI, int x, int y, bool rotated)
    {
        Transform container = itemsContainer != null ? itemsContainer : gridRootRect;
        itemUI.transform.SetParent(container, false);
        itemUI.transform.SetAsLastSibling();

        RectTransform itemRect = itemUI.GetComponent<RectTransform>();
        itemRect.pivot = rotated ? new Vector2(0, 0) : new Vector2(0, 1);
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(0, 1);

        gridData.PlaceItem(x, y, itemUI.GetItemShape(), rotated);
        itemUI.SetGridPosition(x, y);
        itemRect.anchoredPosition = GetAnchoredPositionFromGridIndex(x, y);
        itemUI.SetEquippedState(false, null);
        itemUI.currentOwner = InventoryItemUI.GridOwner.Trunk;
        itemUI.UpdateVisualSize();

        SaveTrunk();
        if (BackpackMinigameUI.Instance != null)
        {
            BackpackMinigameUI.Instance.SaveBackpack();
        }
    }

    public bool TrySwapItems(InventoryItemUI draggedItem, int targetX, int targetY)
    {
        Transform container = itemsContainer != null ? itemsContainer : gridRootRect;
        InventoryItemUI[] allItems = container.GetComponentsInChildren<InventoryItemUI>();
        InventoryItemUI targetItem = null;
        int dragWidth = draggedItem.GetItemShape().GetWidth(draggedItem.IsRotated());
        int dragHeight = draggedItem.GetItemShape().GetHeight(draggedItem.IsRotated());

        foreach (InventoryItemUI item in allItems)
        {
            if (item == draggedItem || item.IsEquipped()) continue;
            int otherX = item.GetGridX();
            int otherY = item.GetGridY();
            int otherW = item.GetItemShape().GetWidth(item.IsRotated());
            int otherH = item.GetItemShape().GetHeight(item.IsRotated());

            bool overlapX = (targetX < otherX + otherW) && (targetX + dragWidth > otherX);
            bool overlapY = (targetY < otherY + otherH) && (targetY + dragHeight > otherY);

            if (overlapX && overlapY)
            {
                if (targetItem == null) targetItem = item;
                else if (targetItem != item) return false;
            }
        }

        if (targetItem == null) return false;
        int targetW = targetItem.GetItemShape().GetWidth(targetItem.IsRotated());
        int targetH = targetItem.GetItemShape().GetHeight(targetItem.IsRotated());
        if (dragWidth == targetW && dragHeight == targetH && (targetX != targetItem.GetGridX() || targetY != targetItem.GetGridY())) return false;

        gridData.ClearCells(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
        bool canPlaceDragged = gridData.CanPlaceItem(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
        bool canPlaceTarget = false;
        bool targetRotatedState = targetItem.IsRotated();

        if (canPlaceDragged)
        {
            gridData.PlaceItem(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
            canPlaceTarget = gridData.CanPlaceItem(startDragX, startDragY, targetItem.GetItemShape(), targetRotatedState);
            if (!canPlaceTarget)
            {
                canPlaceTarget = gridData.CanPlaceItem(startDragX, startDragY, targetItem.GetItemShape(), !targetRotatedState);
                if (canPlaceTarget) targetRotatedState = !targetRotatedState;
            }
            if (!canPlaceTarget) gridData.ClearCells(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
        }

        if (canPlaceDragged && canPlaceTarget)
        {
            draggedItem.SetGridPosition(targetX, targetY);
            draggedItem.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPositionFromGridIndex(targetX, targetY);
            if (targetItem.IsRotated() != targetRotatedState) targetItem.ToggleRotate();
            gridData.PlaceItem(startDragX, startDragY, targetItem.GetItemShape(), targetItem.IsRotated());
            targetItem.SetGridPosition(startDragX, startDragY);
            targetItem.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPositionFromGridIndex(startDragX, startDragY);
            return true;
        }
        else
        {
            gridData.PlaceItem(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
            return false;
        }
    }

    public bool CanSwapItems(InventoryItemUI draggedItem, int targetX, int targetY)
    {
        Transform container = itemsContainer != null ? itemsContainer : gridRootRect;
        if (draggedItem == null || container == null || draggedItem.IsEquipped()) return false;
        InventoryItemUI[] allItems = container.GetComponentsInChildren<InventoryItemUI>();
        InventoryItemUI targetItem = null;
        int dragWidth = draggedItem.GetItemShape().GetWidth(draggedItem.IsRotated());
        int dragHeight = draggedItem.GetItemShape().GetHeight(draggedItem.IsRotated());

        foreach (InventoryItemUI item in allItems)
        {
            if (item == draggedItem || item.IsEquipped()) continue;
            int otherX = item.GetGridX();
            int otherY = item.GetGridY();
            int otherW = item.GetItemShape().GetWidth(item.IsRotated());
            int otherH = item.GetItemShape().GetHeight(item.IsRotated());
            bool overlapX = (targetX < otherX + otherW) && (targetX + dragWidth > otherX);
            bool overlapY = (targetY < otherY + otherH) && (targetY + dragHeight > otherY);
            if (overlapX && overlapY)
            {
                if (targetItem == null) targetItem = item;
                else if (targetItem != item) return false;
            }
        }

        if (targetItem == null) return false;
        int targetW = targetItem.GetItemShape().GetWidth(targetItem.IsRotated());
        int targetH = targetItem.GetItemShape().GetHeight(targetItem.IsRotated());
        if (dragWidth == targetW && dragHeight == targetH && (targetX != targetItem.GetGridX() || targetY != targetItem.GetGridY())) return false;

        gridData.ClearCells(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
        bool canPlaceDragged = gridData.CanPlaceItem(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
        bool canPlaceTarget = false;

        if (canPlaceDragged)
        {
            gridData.PlaceItem(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
            canPlaceTarget = gridData.CanPlaceItem(startDragX, startDragY, targetItem.GetItemShape(), targetItem.IsRotated());
            if (!canPlaceTarget) canPlaceTarget = gridData.CanPlaceItem(startDragX, startDragY, targetItem.GetItemShape(), !targetItem.IsRotated());
            gridData.ClearCells(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
        }
        gridData.PlaceItem(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
        return (canPlaceDragged && canPlaceTarget);
    }

    public void TryRotateItemInGrid(InventoryItemUI itemUI)
    {
        int currentX = itemUI.GetGridX();
        int currentY = itemUI.GetGridY();
        bool currentRotatedState = itemUI.IsRotated();
        bool targetRotatedState = !currentRotatedState;

        gridData.ClearCells(currentX, currentY, itemUI.GetItemShape(), currentRotatedState);

        if (gridData.CanPlaceItem(currentX, currentY, itemUI.GetItemShape(), targetRotatedState))
        {
            gridData.PlaceItem(currentX, currentY, itemUI.GetItemShape(), targetRotatedState);
            itemUI.ToggleRotate();
            itemUI.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPositionFromGridIndex(currentX, currentY);
            SaveTrunk();
        }
        else
        {
            gridData.PlaceItem(currentX, currentY, itemUI.GetItemShape(), currentRotatedState);
        }
    }

    public bool CanSwapItemsExternal(InventoryItemUI draggedItem, int targetX, int targetY)
    {
        Transform container = itemsContainer != null ? itemsContainer : gridRootRect;
        if (draggedItem == null || container == null || draggedItem.IsEquipped()) return false;
        InventoryItemUI[] allItems = container.GetComponentsInChildren<InventoryItemUI>();
        InventoryItemUI targetItem = null;
        int dragWidth = draggedItem.GetItemShape().GetWidth(draggedItem.IsRotated());
        int dragHeight = draggedItem.GetItemShape().GetHeight(draggedItem.IsRotated());

        foreach (InventoryItemUI item in allItems)
        {
            if (item == draggedItem || item.IsEquipped()) continue;
            int otherX = item.GetGridX();
            int otherY = item.GetGridY();
            int otherW = item.GetItemShape().GetWidth(item.IsRotated());
            int otherH = item.GetItemShape().GetHeight(item.IsRotated());
            if ((targetX < otherX + otherW) && (targetX + dragWidth > otherX) && (targetY < otherY + otherH) && (targetY + dragHeight > otherY))
            {
                if (targetItem == null) targetItem = item;
                else return false;
            }
        }

        if (targetItem == null) return false;
        int targetW = targetItem.GetItemShape().GetWidth(targetItem.IsRotated());
        int targetH = targetItem.GetItemShape().GetHeight(targetItem.IsRotated());
        if (dragWidth == targetW && dragHeight == targetH && (targetX != targetItem.GetGridX() || targetY != targetItem.GetGridY())) return false;

        gridData.ClearCells(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
        bool canPlaceDragged = gridData.CanPlaceItem(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
        bool canPlaceTarget = false;

        if (canPlaceDragged && draggedItem.currentOwner == InventoryItemUI.GridOwner.Backpack && BackpackMinigameUI.Instance != null)
        {
            canPlaceTarget = BackpackMinigameUI.Instance.CanPlaceItemAt(draggedItem.GetGridX(), draggedItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
            if (!canPlaceTarget) canPlaceTarget = BackpackMinigameUI.Instance.CanPlaceItemAt(draggedItem.GetGridX(), draggedItem.GetGridY(), targetItem.GetItemShape(), !targetItem.IsRotated());
        }

        gridData.PlaceItem(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
        return (canPlaceDragged && canPlaceTarget);
    }

    public bool TrySwapItemsExternal(InventoryItemUI draggedItem, int targetX, int targetY)
    {
        Transform container = itemsContainer != null ? itemsContainer : gridRootRect;
        InventoryItemUI[] allItems = container.GetComponentsInChildren<InventoryItemUI>();
        InventoryItemUI targetItem = null;
        int dragWidth = draggedItem.GetItemShape().GetWidth(draggedItem.IsRotated());
        int dragHeight = draggedItem.GetItemShape().GetHeight(draggedItem.IsRotated());

        foreach (InventoryItemUI item in allItems)
        {
            if (item == draggedItem || item.IsEquipped()) continue;
            int otherX = item.GetGridX();
            int otherY = item.GetGridY();
            int otherW = item.GetItemShape().GetWidth(item.IsRotated());
            int otherH = item.GetItemShape().GetHeight(item.IsRotated());
            if ((targetX < otherX + otherW) && (targetX + dragWidth > otherX) && (targetY < otherY + otherH) && (targetY + dragHeight > otherY))
            {
                if (targetItem == null) targetItem = item;
                else return false;
            }
        }

        if (targetItem == null) return false;
        int targetW = targetItem.GetItemShape().GetWidth(targetItem.IsRotated());
        int targetH = targetItem.GetItemShape().GetHeight(targetItem.IsRotated());
        if (dragWidth == targetW && dragHeight == targetH && (targetX != targetItem.GetGridX() || targetY != targetItem.GetGridY())) return false;

        gridData.ClearCells(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
        bool canPlaceDragged = gridData.CanPlaceItem(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
        bool canPlaceTarget = false;
        bool targetRotatedState = targetItem.IsRotated();

        if (canPlaceDragged && draggedItem.currentOwner == InventoryItemUI.GridOwner.Backpack && BackpackMinigameUI.Instance != null)
        {
            canPlaceTarget = BackpackMinigameUI.Instance.CanPlaceItemAt(draggedItem.GetGridX(), draggedItem.GetGridY(), targetItem.GetItemShape(), targetRotatedState);
            if (!canPlaceTarget)
            {
                canPlaceTarget = BackpackMinigameUI.Instance.CanPlaceItemAt(draggedItem.GetGridX(), draggedItem.GetGridY(), targetItem.GetItemShape(), !targetRotatedState);
                if (canPlaceTarget) targetRotatedState = !targetRotatedState;
            }
        }

        if (canPlaceDragged && canPlaceTarget)
        {
            int oldDragX = draggedItem.GetGridX();
            int oldDragY = draggedItem.GetGridY();

            PlaceItemDirectlyToGrid(draggedItem, targetX, targetY, draggedItem.IsRotated());
            if (targetItem.IsRotated() != targetRotatedState) targetItem.ToggleRotate();
            BackpackMinigameUI.Instance.PlaceItemDirectlyToGrid(targetItem, oldDragX, oldDragY, targetRotatedState);
            targetItem.currentOwner = InventoryItemUI.GridOwner.Backpack;
            SaveTrunk();
            return true;
        }
        else
        {
            gridData.PlaceItem(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
            return false;
        }
    }

    public bool CanPlaceItemAt(int x, int y, ItemShapeSO shape, bool rotated)
    {
        return gridData != null && gridData.CanPlaceItem(x, y, shape, rotated);
    }

    public void RefreshGridVisuals()
    {
        if (gridData != null) gridData.InitGrid();

        foreach (Transform child in gridRootRect)
        {
            if (child != itemsContainer && (highlightOverlay == null || child != highlightOverlay.transform))
            {
                Destroy(child.gameObject);
            }
        }

        CreateVisualGrid();

        if (itemsContainer != null) itemsContainer.SetAsLastSibling();
        if (highlightOverlay != null) highlightOverlay.transform.SetAsLastSibling();
    }
}