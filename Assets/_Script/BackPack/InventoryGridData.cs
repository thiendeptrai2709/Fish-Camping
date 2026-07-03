using UnityEngine;

public class InventoryGridData : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float cellSize = 64f;

    private bool[,] occupiedCells;

    private void Awake()
    {
        InitGrid();
    }

    public void InitGrid()
    {
        if (occupiedCells == null || occupiedCells.GetLength(0) != gridWidth || occupiedCells.GetLength(1) != gridHeight)
        {
            occupiedCells = new bool[gridWidth, gridHeight];
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