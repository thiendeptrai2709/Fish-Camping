using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BackpackMinigameUI : MonoBehaviour
{
    [SerializeField] private InventoryGridData gridData;
    [SerializeField] private RectTransform gridRootRect;
    [SerializeField] private RectTransform itemsContainer;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private GameObject cellVisualPrefab;
    [SerializeField] private Image highlightOverlay;
    [SerializeField] private ItemShapeSO[] testSpawnItems;

    private int startDragX;
    private int startDragY;
    private bool startDragRotated;
    private int dragGridOffsetX;
    private int dragGridOffsetY;
    private void Start()
    {
        // Đảm bảo lưới logic được khởi tạo trước mọi thao tác vẽ UI và spawn item
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
        SpawnTestItems();
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

            // Đảm bảo offset không vượt quá kích thước hiện tại của item (bảo vệ lỗi khi vừa xoay item lúc kéo)
            int maxOffsetX = shape.GetWidth(isRotated) - 1;
            int maxOffsetY = shape.GetHeight(isRotated) - 1;
            int effectiveOffsetX = Mathf.Clamp(dragGridOffsetX, 0, Mathf.Max(0, maxOffsetX));
            int effectiveOffsetY = Mathf.Clamp(dragGridOffsetY, 0, Mathf.Max(0, maxOffsetY));

            // Trừ đi offset để giữ nguyên vị trí tương đối giữa chuột và góc trên-trái của item
            x = rawMouseX - effectiveOffsetX;
            y = rawMouseY - effectiveOffsetY;

            // Giới hạn (Clamp) góc trên-trái nằm hoàn toàn trong lưới
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

        // Tính toán khoảng cách (offset) từ ô chuột click đến ô góc trên-trái của item
        // Loại bỏ hoàn toàn hiện tượng "giật ảnh" khi nhấp chuột
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

        // Cập nhật ngay vị trí highlight và item tại frame đầu tiên
        OnItemDragging(itemUI, screenPosition);
    }

    public void OnItemDragging(InventoryItemUI itemUI, Vector2 screenPosition)
    {
        if (gridRootRect == null || gridData == null) return;

        if (GetClampedGridIndex(screenPosition, itemUI.GetItemShape(), itemUI.IsRotated(), out int targetX, out int targetY))
        {
            bool canPlace = gridData.CanPlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated());
            Vector2 snappedPosition = GetAnchoredPositionFromGridIndex(targetX, targetY);

            if (highlightOverlay != null)
            {
                highlightOverlay.gameObject.SetActive(true);
                highlightOverlay.rectTransform.anchoredPosition = snappedPosition;
                // Đồng bộ chính xác Pivot và Rotation từ UI item sang Highlight để ôm khít ảnh xoay
                highlightOverlay.rectTransform.sizeDelta = itemUI.GetComponent<RectTransform>().sizeDelta;
                highlightOverlay.rectTransform.pivot = itemUI.GetComponent<RectTransform>().pivot;
                highlightOverlay.rectTransform.localRotation = itemUI.GetComponent<RectTransform>().localRotation;
                highlightOverlay.color = canPlace ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
            }

            itemUI.GetComponent<RectTransform>().anchoredPosition = snappedPosition;
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

        if (GetClampedGridIndex(screenPosition, itemUI.GetItemShape(), itemUI.IsRotated(), out int targetX, out int targetY))
        {
            if (gridData.CanPlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated()))
            {
                gridData.PlaceItem(targetX, targetY, itemUI.GetItemShape(), itemUI.IsRotated());
                itemUI.SetGridPosition(targetX, targetY);
                itemRect.anchoredPosition = GetAnchoredPositionFromGridIndex(targetX, targetY);
                return;
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
            // Khóa chặt vị trí trực quan vào đúng ô (currentX, currentY), không cho phép tụt hàng
            itemUI.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPositionFromGridIndex(currentX, currentY);
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
}