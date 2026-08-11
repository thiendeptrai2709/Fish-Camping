using System.Collections.Generic;
using UnityEngine;

public class InventoryGridData : MonoBehaviour, ISaveable
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float cellSize = 64f;
    [SerializeField] private bool isTrunk = false; // Thêm tick box để phân biệt Balo và Cốp xe

    [SerializeField]
    private Vector2Int[] trunkLevelSizes = new Vector2Int[4] {
        new Vector2Int(5, 2),
        new Vector2Int(5, 4),
        new Vector2Int(7, 5),
        new Vector2Int(8, 5)
    };

    private bool[,] occupiedCells;

    private void Awake()
    {
        InitGrid();
    }

    public void InitGrid()
    {
        // Chỉ áp dụng logic đọc Level Cốp xe nếu script này đang nằm trên Cốp xe
        if (isTrunk)
        {
            int currentLevel = PlayerPrefs.GetInt("SavedTrunkLevel", 0);
            if (currentLevel >= 0 && currentLevel < trunkLevelSizes.Length)
            {
                gridWidth = trunkLevelSizes[currentLevel].x;
                gridHeight = trunkLevelSizes[currentLevel].y;
            }
        }

        if (occupiedCells == null || occupiedCells.GetLength(0) != gridWidth || occupiedCells.GetLength(1) != gridHeight)
        {
            bool[,] newCells = new bool[gridWidth, gridHeight];
            if (occupiedCells != null)
            {
                int oldW = occupiedCells.GetLength(0);
                int oldH = occupiedCells.GetLength(1);
                for (int x = 0; x < Mathf.Min(oldW, gridWidth); x++)
                {
                    for (int y = 0; y < Mathf.Min(oldH, gridHeight); y++)
                    {
                        newCells[x, y] = occupiedCells[x, y];
                    }
                }
            }
            occupiedCells = newCells;
        }
    }

    public bool CanPlaceItem(int startX, int startY, ItemShapeSO shape, bool isRotated)
    {
        if (occupiedCells == null) InitGrid();
        int itemWidth = shape.GetWidth(isRotated);
        int itemHeight = shape.GetHeight(isRotated);

        if (startX < 0 || startY < 0 || startX + itemWidth > gridWidth || startY + itemHeight > gridHeight)
        {
            return false;
        }

        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                if (shape.IsCellOccupied(x, y, isRotated))
                {
                    if (occupiedCells[startX + x, startY + y])
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void PlaceItem(int startX, int startY, ItemShapeSO shape, bool isRotated)
    {
        if (occupiedCells == null) InitGrid();
        int itemWidth = shape.GetWidth(isRotated);
        int itemHeight = shape.GetHeight(isRotated);

        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                if (shape.IsCellOccupied(x, y, isRotated))
                {
                    occupiedCells[startX + x, startY + y] = true;
                }
            }
        }
    }

    public void ClearCells(int startX, int startY, ItemShapeSO shape, bool isRotated)
    {
        if (occupiedCells == null) InitGrid();
        int itemWidth = shape.GetWidth(isRotated);
        int itemHeight = shape.GetHeight(isRotated);

        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                if (shape.IsCellOccupied(x, y, isRotated))
                {
                    if (startX + x >= 0 && startX + x < gridWidth && startY + y >= 0 && startY + y < gridHeight)
                    {
                        occupiedCells[startX + x, startY + y] = false;
                    }
                }
            }
        }
    }

    public float GetCellSize() => cellSize;
    public int GetGridWidth() => gridWidth;
    public int GetGridHeight() => gridHeight;

    // ==================== TÍCH HỢP HỆ THỐNG LƯU / TẢI BALO & CỐP XE ====================

    public void SaveData(GameSaveData data)
    {
        List<InventoryItemSave> targetList = isTrunk ? data.trunkItems : data.backpackItems;
        targetList.Clear();

        // Tìm tất cả InventoryItemUI ở GameObject cha (để quét hết các panel con ngang hàng)
        InventoryItemUI[] itemsOnGrid = transform.parent != null
            ? transform.parent.GetComponentsInChildren<InventoryItemUI>(true)
            : GetComponentsInChildren<InventoryItemUI>(true);

        Debug.Log($"[InventorySave] Tìm thấy {itemsOnGrid.Length} GameObject item trên UI {(isTrunk ? "Cốp" : "Balo")}");

        foreach (var item in itemsOnGrid)
        {
            if (item != null && !string.IsNullOrEmpty(item.ItemID))
            {
                // Kiểm tra xem item này thuộc về Balo hay Cốp xe để lưu đúng kho
                bool isItemInTrunk = item.currentOwner == InventoryItemUI.GridOwner.Trunk;
                if (isTrunk == isItemInTrunk)
                {
                    targetList.Add(new InventoryItemSave
                    {
                        itemID = item.ItemID,
                        quantity = item.Quantity,
                        gridX = item.GridX,
                        gridY = item.GridY
                    });

                    Debug.Log($" -> Đã ghi nhận item: ID={item.ItemID}, X={item.GridX}, Y={item.GridY}");
                }
            }
        }

        Debug.Log($"<color=green>[InventorySave] ĐÃ LƯU THÀNH CÔNG {(isTrunk ? "Cốp xe" : "Balo")}: {targetList.Count} món đồ!</color>");
    }

    public void LoadData(GameSaveData data)
    {
        // 1. Nếu là Cốp xe, cập nhật lại kích thước khung lưới
        if (isTrunk)
        {
            int trunkLevelIndex = data.trunkUpgradeLevel - 1;
            if (trunkLevelIndex >= 0 && trunkLevelIndex < trunkLevelSizes.Length)
            {
                gridWidth = trunkLevelSizes[trunkLevelIndex].x;
                gridHeight = trunkLevelSizes[trunkLevelIndex].y;
            }
            InitGrid();
        }

        List<InventoryItemSave> targetList = isTrunk ? data.trunkItems : data.backpackItems;
        if (targetList == null) return;

        // 2. Xóa sạch tất cả Item UI cũ đang hiển thị
        InventoryItemUI[] oldItems = transform.parent != null
            ? transform.parent.GetComponentsInChildren<InventoryItemUI>(true)
            : GetComponentsInChildren<InventoryItemUI>(true);

        foreach (var oldItem in oldItems)
        {
            bool isItemInTrunk = oldItem.currentOwner == InventoryItemUI.GridOwner.Trunk;
            if (oldItem != null && isTrunk == isItemInTrunk)
            {
                Destroy(oldItem.gameObject);
            }
        }

        // Reset lại mảng ô cờ
        if (occupiedCells != null)
        {
            System.Array.Clear(occupiedCells, 0, occupiedCells.Length);
        }

        // 3. Sinh lại từng món đồ lên Balo / Cốp
        foreach (var savedItem in targetList)
        {
            if (!isTrunk && BackpackMinigameUI.Instance != null)
            {
                BackpackMinigameUI.Instance.SpawnSavedItem(savedItem.itemID, savedItem.gridX, savedItem.gridY, savedItem.quantity);
            }
            else if (isTrunk && TrunkMinigameUI.Instance != null)
            {
                TrunkMinigameUI.Instance.SpawnSavedItem(savedItem.itemID, savedItem.gridX, savedItem.gridY, savedItem.quantity);
            }
        }

        Debug.Log($"<color=cyan>[InventorySave] ĐÃ LOAD THÀNH CÔNG {(isTrunk ? "Cốp xe" : "Balo")}: {targetList.Count} món đồ!</color>");
    }
}