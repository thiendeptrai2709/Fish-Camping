using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BackpackMinigameUI : MonoBehaviour
{
    public static BackpackMinigameUI Instance { get; private set; }

    [SerializeField] private InventoryGridData gridData;
    [SerializeField] private RectTransform gridRootRect;
    [SerializeField] private RectTransform itemsContainer;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private GameObject cellVisualPrefab;
    [SerializeField] private Image highlightOverlay;
    [SerializeField] private ItemShapeSO[] testSpawnItems;

    [Header("--- TẤT CẢ ITEM TRONG GAME ĐỂ LOAD ---")]
    [Tooltip("Kéo toàn bộ ScriptableObject ItemShapeSO / FishSO / RodSO vào đây để hệ thống tra cứu lúc nạp game")]
    [SerializeField] private ItemShapeSO[] allDatabaseItems;

    private const string SAVE_KEY = "Saved_Backpack_Data";

    private int startDragX;
    private int startDragY;
    private bool startDragRotated;
    private int dragGridOffsetX;
    private int dragGridOffsetY;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (gridData != null)
        {
            gridData.InitGrid();
        }

        CreateVisualGrid();
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

        // Tự động Load Balo từ ổ cứng. Nếu chưa từng chơi thì mới sinh Test Items
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            LoadBackpack();
        }
        else
        {
            SpawnTestItems();
            SaveBackpack();
        }
    }

    // ==========================================
    // HỆ THỐNG SAVE & LOAD BALO
    // ==========================================
    public InventoryItemUI CreateItemUI(ItemShapeSO shape, bool rotated = false)
    {
        if (itemUIPrefab == null || shape == null || itemsContainer == null) return null;
        GameObject itemObj = Instantiate(itemUIPrefab, itemsContainer);
        InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
        itemUI.Setup(shape, this, 0, 0, rotated);
        return itemUI;
    }

    public void SaveBackpack()
    {
        if (itemsContainer == null) return;

        InventoryItemUI[] allItems = itemsContainer.GetComponentsInChildren<InventoryItemUI>();
        BackpackSaveContainer container = new BackpackSaveContainer();
        HashSet<string> recordedPositions = new HashSet<string>();

        foreach (var item in allItems)
        {
            if (item == null || item.GetItemShape() == null) continue;
            if (item.transform.parent != itemsContainer) continue;

            string posKey = $"{item.GetGridX()}_{item.GetGridY()}";
            if (recordedPositions.Contains(posKey)) continue;
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
                fishGrade = (int)item.GetGrade()
            };

            container.items.Add(data);
        }

        // LƯU CÁC Ô TRANG BỊ (EQUIPMENT SLOTS)
        EquipmentSlotUI[] allSlots = Object.FindObjectsByType<EquipmentSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var slot in allSlots)
        {
            if (slot == null) continue;
            InventoryItemUI eqItem = slot.GetEquippedItem();
            if (eqItem != null && eqItem.GetItemShape() != null)
            {
                ItemShapeSO shape = eqItem.GetItemShape();
                string finalID = !string.IsNullOrEmpty(shape.itemID) ? shape.itemID : shape.name;

                SavedEquippedSlotData slotData = new SavedEquippedSlotData
                {
                    slotRequirement = (int)slot.GetSlotRequirement(),
                    itemID = finalID,
                    isRotated = eqItem.IsRotated(),
                    isFish = (shape is FishSO),
                    fishLength = eqItem.GetLength(),
                    fishWeight = eqItem.GetWeight(),
                    fishGrade = (int)eqItem.GetGrade()
                };
                container.equippedSlots.Add(slotData);
            }
        }

        string json = JsonUtility.ToJson(container);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadBackpack()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json)) return;

        BackpackSaveContainer container = JsonUtility.FromJson<BackpackSaveContainer>(json);
        if (container == null) return;

        // Xóa sạch các item visual cũ nếu có
        if (itemsContainer != null)
        {
            InventoryItemUI[] oldItems = itemsContainer.GetComponentsInChildren<InventoryItemUI>(true);
            foreach (var old in oldItems)
            {
                if (old != null)
                {
                    old.transform.SetParent(null);
                    Destroy(old.gameObject);
                }
            }
        }

        if (gridData != null) gridData.InitGrid();

        if (container.items != null)
        {
            HashSet<string> loadedPositions = new HashSet<string>();

            foreach (var saved in container.items)
            {
                if (saved == null) continue;
                string posKey = $"{saved.gridX}_{saved.gridY}";
                if (loadedPositions.Contains(posKey)) continue;
                loadedPositions.Add(posKey);

                ItemShapeSO foundShape = FindItemShapeByID(saved.itemID);
                if (foundShape != null)
                {
                    InventoryItemUI spawned = SpawnItem(foundShape, saved.gridX, saved.gridY, saved.isRotated);
                    if (spawned != null && saved.isFish)
                    {
                        spawned.SetFishInstanceData(saved.fishLength, saved.fishWeight, (FishGrade)saved.fishGrade);
                    }
                }
            }
        }

        // Nạp lại các item đã trang bị vào các ô EquipmentSlotUI
        if (container.equippedSlots != null && container.equippedSlots.Count > 0)
        {
            EquipmentSlotUI[] allSlots = Object.FindObjectsByType<EquipmentSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            HashSet<EquipmentSlotUI> usedSlots = new HashSet<EquipmentSlotUI>();

            foreach (var eqData in container.equippedSlots)
            {
                if (eqData == null) continue;
                ItemShapeSO foundShape = FindItemShapeByID(eqData.itemID);
                if (foundShape == null) continue;

                EquipmentSlotUI targetSlot = null;
                foreach (var slot in allSlots)
                {
                    if (slot != null && !usedSlots.Contains(slot) && (int)slot.GetSlotRequirement() == eqData.slotRequirement)
                    {
                        targetSlot = slot;
                        break;
                    }
                }

                if (targetSlot != null)
                {
                    usedSlots.Add(targetSlot);
                    InventoryItemUI spawnedItem = CreateItemUI(foundShape, eqData.isRotated);
                    if (spawnedItem != null)
                    {
                        if (eqData.isFish)
                        {
                            spawnedItem.SetFishInstanceData(eqData.fishLength, eqData.fishWeight, (FishGrade)eqData.fishGrade);
                        }
                        targetSlot.EquipItemDirectly(spawnedItem);
                    }
                }
            }
        }
    }

    private ItemShapeSO FindItemShapeByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

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

        // Tự động fallback tìm trong thư mục Resources nếu chưa kéo vào Inspector
        ItemShapeSO loaded = Resources.Load<ItemShapeSO>(id);
        if (loaded != null) return loaded;

        ItemShapeSO[] allResources = Resources.LoadAll<ItemShapeSO>("");
        if (allResources != null)
        {
            foreach (var item in allResources)
            {
                if (item != null)
                {
                    string checkID = !string.IsNullOrEmpty(item.itemID) ? item.itemID : item.name;
                    if (checkID == id || item.name == id) return item;
                }
            }
        }

        return null;
    }

    [ContextMenu("Xóa dữ liệu Balo (Reset Backpack)")]
    public void ClearBackpackSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>[Backpack] Đã xóa dữ liệu lưu trữ balo thành công!</color>");
    }

    private void OnDisable()
    {
        SaveBackpack();
    }

    private void OnApplicationQuit()
    {
        SaveBackpack();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveBackpack();
    }

    public void CreateVisualGrid()
    {
        if (gridData == null || cellVisualPrefab == null || gridRootRect == null) return;

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

    public float GetCellSize()
    {
        return gridData != null ? gridData.GetCellSize() : 64f;
    }
    public RectTransform GetGridRoot() => gridRootRootRect;
    private RectTransform gridRootRootRect => gridRootRect;

    public bool GetGridIndexFromScreenPosition(Vector2 screenPosition, out int x, out int y)
    {
        x = -1;
        y = -1;
        if (gridRootRect == null || gridData == null) return false;

        Canvas rootCanvas = gridRootRect.GetComponentInParent<Canvas>();
        Camera pressCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRootRect, screenPosition, pressCamera, out Vector2 localPoint))
        {
            float cellSize = gridData.GetCellSize();
            float offsetX = localPoint.x - gridRootRect.rect.xMin;
            float offsetY = gridRootRect.rect.yMax - localPoint.y;

            x = Mathf.FloorToInt(offsetX / cellSize);
            y = Mathf.FloorToInt(offsetY / cellSize);

            if (x >= 0 && x < gridData.GetGridWidth() && y >= 0 && y < gridData.GetGridHeight())
            {
                return true;
            }
        }
        return false;
    }

    public bool GetClampedGridIndex(Vector2 screenPosition, ItemShapeSO shape, bool isRotated, out int x, out int y)
    {
        x = 0;
        y = 0;
        if (gridRootRect == null || gridData == null) return false;

        Canvas rootCanvas = gridRootRect.GetComponentInParent<Canvas>();
        Camera pressCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRootRect, screenPosition, pressCamera, out Vector2 localPoint))
        {
            float cellSize = gridData.GetCellSize();
            float offsetX = localPoint.x - gridRootRect.rect.xMin;
            float offsetY = gridRootRect.rect.yMax - localPoint.y;

            int rawMouseX = Mathf.FloorToInt(offsetX / cellSize);
            int rawMouseY = Mathf.FloorToInt(offsetY / cellSize);

            int maxOffsetX = shape.GetWidth(isRotated) - 1;
            int maxOffsetY = shape.GetHeight(isRotated) - 1;
            int effectiveOffsetX = Mathf.Clamp(dragGridOffsetX, 0, Mathf.Max(0, maxOffsetX));
            int effectiveOffsetY = Mathf.Clamp(dragGridOffsetY, 0, Mathf.Max(0, maxOffsetY));

            x = rawMouseX - effectiveOffsetX;
            y = rawMouseY - effectiveOffsetY;

            int maxValX = gridData.GetGridWidth() - shape.GetWidth(isRotated);
            int maxValY = gridData.GetGridHeight() - shape.GetHeight(isRotated);

            x = Mathf.Clamp(x, 0, Mathf.Max(0, maxValX));
            y = Mathf.Clamp(y, 0, Mathf.Max(0, maxValY));

            return true;
        }
        return false;
    }

    public Vector2 GetAnchoredPositionFromGridIndex(int x, int y)
    {
        float cellSize = GetCellSize();
        return new Vector2(x * cellSize, -y * cellSize);
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

        itemUI.transform.SetParent(itemsContainer);
        itemUI.transform.SetAsLastSibling();

        gridData.ClearCells(startDragX, startDragY, itemUI.GetItemShape(), startDragRotated);

        if (highlightOverlay != null)
        {
            highlightOverlay.gameObject.SetActive(true);
            highlightOverlay.transform.SetParent(itemsContainer);
            highlightOverlay.transform.SetSiblingIndex(itemUI.transform.GetSiblingIndex());
            highlightOverlay.rectTransform.sizeDelta = itemUI.GetComponent<RectTransform>().sizeDelta;
            highlightOverlay.rectTransform.localRotation = Quaternion.Euler(0f, 0f, startDragRotated ? -90f : 0f);
        }

        OnItemDragging(itemUI, screenPosition);
    }

    public void OnItemBeginDragFromExternal(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        dragGridOffsetX = 0;
        dragGridOffsetY = 0;

        if (highlightOverlay != null)
        {
            highlightOverlay.gameObject.SetActive(false);
        }

        Canvas rootCanvas = gridRootRect.GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            itemUI.transform.SetParent(rootCanvas.transform, true);
            itemUI.transform.SetAsLastSibling();
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
                    if (itemUI.currentOwner == InventoryItemUI.GridOwner.Backpack)
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

                if (itemUI.transform.parent != itemsContainer)
                {
                    itemUI.transform.SetParent(itemsContainer, false);
                    itemUI.transform.SetAsLastSibling();
                }
                itemUI.GetComponent<RectTransform>().anchoredPosition = snappedPosition;
            }
        }
        else
        {
            if (highlightOverlay != null)
            {
                highlightOverlay.gameObject.SetActive(false);
            }

            Canvas rootCanvas = gridRootRect.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                if (itemUI.transform.parent != rootCanvas.transform)
                {
                    itemUI.transform.SetParent(rootCanvas.transform, true);
                    itemUI.transform.SetAsLastSibling();
                }

                Camera pressCamera = null;
                if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    pressCamera = rootCanvas.worldCamera;
                    if (pressCamera == null) pressCamera = Camera.main;
                }

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rootCanvas.GetComponent<RectTransform>(), screenPosition, pressCamera, out Vector3 worldPoint))
                {
                    itemUI.transform.position = worldPoint;
                }
            }
        }
    }

    public void OnItemEndDrag(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        itemUI.transform.SetParent(itemsContainer);

        RectTransform itemRect = itemUI.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(0, 1);

        if (highlightOverlay != null)
        {
            highlightOverlay.gameObject.SetActive(false);
        }

        if (GetGridIndexFromScreenPosition(screenPosition, out int rawX, out int rawY))
        {
            if (GetClampedGridIndex(screenPosition, itemUI.GetItemShape(), itemUI.IsRotated(), out int targetX, out int targetY))
            {
                if (gridData.CanPlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated()))
                {
                    gridData.PlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated());
                    itemUI.SetGridPosition(targetX, targetY);
                    itemRect.anchoredPosition = GetAnchoredPositionFromGridIndex(targetX, targetY);
                    SaveBackpack();
                    return;
                }
                else
                {
                    if (TrySwapItems(itemUI, targetX, targetY))
                    {
                        SaveBackpack();
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
        SaveBackpack();
    }

    private bool TrySwapItems(InventoryItemUI draggedItem, int targetX, int targetY)
    {
        InventoryItemUI[] allItems = itemsContainer.GetComponentsInChildren<InventoryItemUI>();
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
                if (targetItem == null)
                {
                    targetItem = item;
                }
                else if (targetItem != item)
                {
                    return false;
                }
            }
        }

        if (targetItem == null) return false;

        int targetW = targetItem.GetItemShape().GetWidth(targetItem.IsRotated());
        int targetH = targetItem.GetItemShape().GetHeight(targetItem.IsRotated());

        if (dragWidth == targetW && dragHeight == targetH)
        {
            if (targetX != targetItem.GetGridX() || targetY != targetItem.GetGridY())
            {
                return false;
            }
        }

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
                if (canPlaceTarget)
                {
                    targetRotatedState = !targetRotatedState;
                }
            }

            if (!canPlaceTarget)
            {
                gridData.ClearCells(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
            }
        }

        if (canPlaceDragged && canPlaceTarget)
        {
            draggedItem.SetGridPosition(targetX, targetY);
            draggedItem.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPositionFromGridIndex(targetX, targetY);

            if (targetItem.IsRotated() != targetRotatedState)
            {
                targetItem.ToggleRotate();
            }

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
            SaveBackpack();
        }
        else
        {
            gridData.PlaceItem(currentX, currentY, itemUI.GetItemShape(), currentRotatedState);
        }
    }

    public InventoryItemUI SpawnItem(ItemShapeSO shape, int startX, int startY, bool rotated = false)
    {
        if (gridData == null || itemUIPrefab == null || itemsContainer == null) return null;

        if (gridData.CanPlaceItem(startX, startY, shape, rotated))
        {
            GameObject itemObj = Instantiate(itemUIPrefab, itemsContainer);
            RectTransform rect = itemObj.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);

            InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
            itemUI.Setup(shape, this, startX, startY, rotated);

            gridData.PlaceItem(startX, startY, shape, rotated);
            rect.anchoredPosition = GetAnchoredPositionFromGridIndex(startX, startY);

            return itemUI;
        }
        return null;
    }

    private void SpawnTestItems()
    {
        if (testSpawnItems == null) return;
        int currentX = 0;
        int currentY = 0;
        int maxRowHeight = 0;
        for (int i = 0; i < testSpawnItems.Length; i++)
        {
            if (testSpawnItems[i] != null)
            {
                if (testSpawnItems[i].height > maxRowHeight)
                {
                    maxRowHeight = testSpawnItems[i].height;
                }

                InventoryItemUI spawned = SpawnItem(testSpawnItems[i], currentX, currentY);
                if (spawned != null)
                {
                    currentX += testSpawnItems[i].width + 1;
                    if (currentX >= gridData.GetGridWidth())
                    {
                        currentX = 0;
                        currentY += maxRowHeight + 1;
                        maxRowHeight = 0;
                    }
                }
            }
        }
    }

    public void HideHighlight()
    {
        if (highlightOverlay != null)
        {
            highlightOverlay.gameObject.SetActive(false);
        }
    }

    public bool CanPlaceItemAt(int x, int y, ItemShapeSO shape, bool rotated)
    {
        return gridData != null && gridData.CanPlaceItem(x, y, shape, rotated);
    }

    public void PlaceItemDirectlyToGrid(InventoryItemUI itemUI, int x, int y, bool rotated)
    {
        if (gridData == null) return;

        itemUI.transform.SetParent(itemsContainer);
        RectTransform itemRect = itemUI.GetComponent<RectTransform>();
        itemRect.pivot = new Vector2(0, 1);
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(0, 1);

        gridData.PlaceItem(x, y, itemUI.GetItemShape(), rotated);
        itemUI.SetGridPosition(x, y);
        itemRect.anchoredPosition = GetAnchoredPositionFromGridIndex(x, y);
        itemUI.SetEquippedState(false, null);
        itemUI.UpdateVisualSize();

        SaveBackpack();
    }

    public bool TryPlaceItemFromExternal(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        if (gridRootRect == null || gridData == null) return false;

        if (GetClampedGridIndex(screenPosition, itemUI.GetItemShape(), itemUI.IsRotated(), out int targetX, out int targetY))
        {
            if (gridData.CanPlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated()))
            {
                PlaceItemDirectlyToGrid(itemUI, targetX, targetY, itemUI.IsRotated());
                return true;
            }
            else
            {
                return TrySwapItemsExternal(itemUI, targetX, targetY);
            }
        }
        return false;
    }

    public bool CanSwapItems(InventoryItemUI draggedItem, int targetX, int targetY)
    {
        if (draggedItem == null || itemsContainer == null || draggedItem.IsEquipped()) return false;
        InventoryItemUI[] allItems = itemsContainer.GetComponentsInChildren<InventoryItemUI>();
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
                if (targetItem == null)
                {
                    targetItem = item;
                }
                else if (targetItem != item)
                {
                    return false;
                }
            }
        }

        if (targetItem == null) return false;

        int targetW = targetItem.GetItemShape().GetWidth(targetItem.IsRotated());
        int targetH = targetItem.GetItemShape().GetHeight(targetItem.IsRotated());

        if (dragWidth == targetW && dragHeight == targetH)
        {
            if (targetX != targetItem.GetGridX() || targetY != targetItem.GetGridY())
            {
                return false;
            }
        }

        gridData.ClearCells(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());

        bool canPlaceDragged = gridData.CanPlaceItem(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
        bool canPlaceTarget = false;

        if (canPlaceDragged)
        {
            gridData.PlaceItem(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());

            canPlaceTarget = gridData.CanPlaceItem(startDragX, startDragY, targetItem.GetItemShape(), targetItem.IsRotated());
            if (!canPlaceTarget)
            {
                canPlaceTarget = gridData.CanPlaceItem(startDragX, startDragY, targetItem.GetItemShape(), !targetItem.IsRotated());
            }

            gridData.ClearCells(targetX, targetY, draggedItem.GetItemShape(), draggedItem.IsRotated());
        }

        gridData.PlaceItem(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());

        return (canPlaceDragged && canPlaceTarget);
    }

    public bool TryAutoAddFromCooking(InventoryItemUI itemUI)
    {
        if (gridData == null) return false;
        int width = gridData.GetGridWidth();
        int height = gridData.GetGridHeight();
        ItemShapeSO shape = itemUI.GetItemShape();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridData.CanPlaceItem(x, y, shape, false))
                {
                    PlaceItemDirectlyToGrid(itemUI, x, y, false);
                    itemUI.SetFromCooking(false);
                    return true;
                }
                if (gridData.CanPlaceItem(x, y, shape, true))
                {
                    PlaceItemDirectlyToGrid(itemUI, x, y, true);
                    itemUI.SetFromCooking(false);
                    return true;
                }
            }
        }
        return false;
    }

    public bool TryAutoAddItem(ItemShapeSO itemShape)
    {
        if (gridData == null) return false;

        int width = gridData.GetGridWidth();
        int height = gridData.GetGridHeight();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridData.CanPlaceItem(x, y, itemShape, false))
                {
                    SpawnItem(itemShape, x, y, false);
                    SaveBackpack();
                    return true;
                }
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridData.CanPlaceItem(x, y, itemShape, true))
                {
                    SpawnItem(itemShape, x, y, true);
                    SaveBackpack();
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryAutoAddFish(FishSO fishShape, float length, float weight, FishGrade grade)
    {
        if (gridData == null) return false;

        int width = gridData.GetGridWidth();
        int height = gridData.GetGridHeight();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridData.CanPlaceItem(x, y, fishShape, false))
                {
                    InventoryItemUI spawned = SpawnItem(fishShape, x, y, false);
                    if (spawned != null) spawned.SetFishInstanceData(length, weight, grade);
                    if (QuestManager.Instance != null && fishShape != null)
                    {
                        string fishName = string.IsNullOrEmpty(fishShape.itemName) ? fishShape.name : fishShape.itemName;
                        QuestManager.Instance.AddProgressByItem(fishName, 1);
                    }
                    SaveBackpack();
                    return true;
                }
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridData.CanPlaceItem(x, y, fishShape, true))
                {
                    InventoryItemUI spawned = SpawnItem(fishShape, x, y, true);
                    if (spawned != null) spawned.SetFishInstanceData(length, weight, grade);
                    SaveBackpack();
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanSwapItemsExternal(InventoryItemUI draggedItem, int targetX, int targetY)
    {
        if (draggedItem == null || itemsContainer == null || draggedItem.IsEquipped()) return false;
        InventoryItemUI[] allItems = itemsContainer.GetComponentsInChildren<InventoryItemUI>();
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

        if (canPlaceDragged && draggedItem.currentOwner == InventoryItemUI.GridOwner.Trunk && TrunkMinigameUI.Instance != null)
        {
            canPlaceTarget = TrunkMinigameUI.Instance.CanPlaceItemAt(draggedItem.GetGridX(), draggedItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
            if (!canPlaceTarget) canPlaceTarget = TrunkMinigameUI.Instance.CanPlaceItemAt(draggedItem.GetGridX(), draggedItem.GetGridY(), targetItem.GetItemShape(), !targetItem.IsRotated());
        }

        gridData.PlaceItem(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
        return (canPlaceDragged && canPlaceTarget);
    }

    public bool TrySwapItemsExternal(InventoryItemUI draggedItem, int targetX, int targetY)
    {
        InventoryItemUI[] allItems = itemsContainer.GetComponentsInChildren<InventoryItemUI>();
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

        if (canPlaceDragged && draggedItem.currentOwner == InventoryItemUI.GridOwner.Trunk && TrunkMinigameUI.Instance != null)
        {
            canPlaceTarget = TrunkMinigameUI.Instance.CanPlaceItemAt(draggedItem.GetGridX(), draggedItem.GetGridY(), targetItem.GetItemShape(), targetRotatedState);
            if (!canPlaceTarget)
            {
                canPlaceTarget = TrunkMinigameUI.Instance.CanPlaceItemAt(draggedItem.GetGridX(), draggedItem.GetGridY(), targetItem.GetItemShape(), !targetRotatedState);
                if (canPlaceTarget) targetRotatedState = !targetRotatedState;
            }
        }

        if (canPlaceDragged && canPlaceTarget)
        {
            int oldDragX = draggedItem.GetGridX();
            int oldDragY = draggedItem.GetGridY();

            PlaceItemDirectlyToGrid(draggedItem, targetX, targetY, draggedItem.IsRotated());
            if (targetItem.IsRotated() != targetRotatedState) targetItem.ToggleRotate();
            TrunkMinigameUI.Instance.PlaceItemDirectlyToGrid(targetItem, oldDragX, oldDragY, targetRotatedState);
            targetItem.currentOwner = InventoryItemUI.GridOwner.Trunk;
            SaveBackpack();
            return true;
        }
        else
        {
            gridData.PlaceItem(targetItem.GetGridX(), targetItem.GetGridY(), targetItem.GetItemShape(), targetItem.IsRotated());
            return false;
        }
    }

    public InventoryItemUI[] GetAllItems()
    {
        if (itemsContainer == null) return new InventoryItemUI[0];
        return itemsContainer.GetComponentsInChildren<InventoryItemUI>();
    }

    public void RemoveItem(InventoryItemUI itemUI)
    {
        if (itemUI == null || gridData == null) return;

        gridData.ClearCells(itemUI.GetGridX(), itemUI.GetGridY(), itemUI.GetItemShape(), itemUI.IsRotated());
        Destroy(itemUI.gameObject);

        // Lưu lại ngay sau khi một item bị gỡ/bán
        SaveBackpack();
    }

    public InventoryGridData GetGridData() => gridData;
}