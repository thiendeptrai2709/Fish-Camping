using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    // ==================== 1. TIẾN ĐỘ NGƯỜI CHƠI ====================
    public List<string> unlockedMapIDs = new List<string>(); // Danh sách ID map đã mở
    public int tireUpgradeLevel = 1;                         // Cấp độ lốp
    public int trunkUpgradeLevel = 1;                        // Cấp độ cốp

    // ==================== 2. BALO & CỐP XE (INVENTORY) ====================
    public List<InventoryItemSave> backpackItems = new List<InventoryItemSave>(); // Đồ trong Balo
    public List<InventoryItemSave> trunkItems = new List<InventoryItemSave>();    // Đồ trong Cốp xe

    // ==================== 3. VỊ TRÍ & SCENE ====================
    public string currentSceneName = "Map_1_Town";  // Tên Scene hiện tại
    public float playerPosX, playerPosY, playerPosZ; // Tọa độ X, Y, Z

    // ==================== 4. SỔ TAY CÂU CÁ ====================
    public List<FishRecordSave> caughtFishJournal = new List<FishRecordSave>();
    public float vehiclePosX, vehiclePosY, vehiclePosZ;
    public float vehicleRotX, vehicleRotY, vehicleRotZ;
}

// Class phụ cho Balo & Cốp xe (Hỗ trợ Lưới Grid Inventory)
[Serializable]
public class InventoryItemSave
{
    public string itemID;  // Mã định danh vật phẩm
    public int quantity;   // Số lượng xếp chồng
    public int gridX;      // Tọa độ cột X trên lưới Inventory
    public int gridY;      // Tọa độ hàng Y trên lưới Inventory
}

// Class phụ cho Sổ tay câu cá
[Serializable]
public class FishRecordSave
{
    public string fishID;
    public bool isUnlocked;
    public int highestGrade; // Ép kiểu enum FishGrade sang int để lưu Json
    public float maxLength;
    public float maxWeight;
}