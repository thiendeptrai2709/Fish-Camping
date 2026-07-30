using UnityEngine;
using UnityEngine.UI;

public class TrunkMinigameUI : MonoBehaviour
{
    public static TrunkMinigameUI Instance { get; private set; }

    [Header("Link Dữ Liệu & UI")]
    [SerializeField] private InventoryGridData gridData;
    [SerializeField] private RectTransform gridRootRect;
    [SerializeField] private RectTransform itemsContainer;
    [SerializeField] private GameObject cellVisualPrefab;
    [SerializeField] private Image highlightOverlay;

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
        Canvas rootCanvas = gridRootRect.GetComponentInParent<Canvas>();
        Camera pressCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRootRect, screenPosition, pressCamera, out Vector2 localPoint))
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
        Canvas rootCanvas = gridRootRect.GetComponentInParent<Canvas>();
        Camera pressCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRootRect, screenPosition, pressCamera, out Vector2 localPoint))
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
                    canPlace = CanSwapItems(itemUI, targetX, targetY);
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
            HideHighlight();

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
                    return;
                }
                else
                {
                    if (TrySwapItems(itemUI, targetX, targetY))
                    {
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
    }

    public bool TryPlaceItemFromExternal(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        if (GetClampedGridIndex(screenPosition, itemUI.GetItemShape(), itemUI.IsRotated(), out int targetX, out int targetY))
        {
            if (gridData.CanPlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated()))
            {
                PlaceItemDirectlyToGrid(itemUI, targetX, targetY, itemUI.IsRotated());
                return true;
            }
        }
        return false;
    }

    public void PlaceItemDirectlyToGrid(InventoryItemUI itemUI, int x, int y, bool rotated)
    {
        itemUI.transform.SetParent(itemsContainer);
        RectTransform itemRect = itemUI.GetComponent<RectTransform>();
        itemRect.pivot = new Vector2(0, 1);
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(0, 1);
        gridData.PlaceItem(x, y, itemUI.GetItemShape(), rotated);
        itemUI.SetGridPosition(x, y);
        itemRect.anchoredPosition = GetAnchoredPositionFromGridIndex(x, y);
        itemUI.UpdateVisualSize();
    }
    public bool TrySwapItems(InventoryItemUI draggedItem, int targetX, int targetY)
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
        }
        else
        {
            gridData.PlaceItem(currentX, currentY, itemUI.GetItemShape(), currentRotatedState);
        }
    }
}