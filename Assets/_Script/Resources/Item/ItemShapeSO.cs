using UnityEngine;

[CreateAssetMenu(fileName = "NewItemShape", menuName = "Inventory/Item Shape")]
public class ItemShapeSO : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 5)] public string itemDescription;
    public GameObject equippedModelPrefab;
    public int width = 1;
    public int height = 1;
    public bool[] shapeCells;

    public virtual string GetFormattedStats()
    {
        return string.Empty;
    }

    public int GetWidth(bool isRotated)
    {
        return isRotated ? height : width;
    }

    public int GetHeight(bool isRotated)
    {
        return isRotated ? width : height;
    }

    public bool IsCellOccupied(int x, int y, bool isRotated = false)
    {
        int currentWidth = GetWidth(isRotated);
        int currentHeight = GetHeight(isRotated);

        if (x < 0 || x >= currentWidth || y < 0 || y >= currentHeight)
        {
            return false;
        }

        if (shapeCells == null || shapeCells.Length == 0)
        {
            return true;
        }

        int originalX = isRotated ? y : x;
        int originalY = isRotated ? (height - 1 - x) : y;
        int index = originalY * width + originalX;

        if (index < 0 || index >= shapeCells.Length)
        {
            return true;
        }

        return shapeCells[index];
    }
}