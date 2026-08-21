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

    // Dữ liệu độ bền và lượt dùng
    public float durability = -1f;
    public int remainingUses = -1;
}

[Serializable]
public class SavedEquippedSlotData
{
    public int slotRequirement; // 0: OnlyFishingRod, 1: OnlyBait, 2: OnlyBobber, 3: Universal
    public string itemID;
    public bool isRotated;
    public bool isFish;
    public float fishLength;
    public float fishWeight;
    public int fishGrade;

    // Dữ liệu độ bền và lượt dùng
    public float durability = -1f;
    public int remainingUses = -1;
}

[Serializable]
public class BackpackSaveContainer
{
    public List<SavedItemData> items = new List<SavedItemData>();
    public List<SavedEquippedSlotData> equippedSlots = new List<SavedEquippedSlotData>();
}