using UnityEngine;

public class InventoryGridData : MonoBehaviour
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
}