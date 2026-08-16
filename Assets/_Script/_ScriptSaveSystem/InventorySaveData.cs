using System;
using System.Collections.Generic;

[Serializable]
public class SavedItemData
{
    public string itemID;
    public int gridX;
    public int gridY;
    public bool isRotated;

    // Dữ liệu riêng dành cho Cá (nếu là cá)
    public bool isFish;
    public float fishLength;
    public float fishWeight;
    public int fishGrade;
}

[Serializable]
public class BackpackSaveContainer
{
    public List<SavedItemData> items = new List<SavedItemData>();
}