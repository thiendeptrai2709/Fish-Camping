using UnityEngine;

public interface ISaveable
{
    // Hàm này được gọi khi Save Game: gom dữ liệu từ Script vào Data Object
    void SaveData(GameSaveData data);

    // Hàm này được gọi khi Load Game: lấy dữ liệu từ Data Object áp dụng vào Script
    void LoadData(GameSaveData data);
}