using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FishJournalManager : MonoBehaviour, ISaveable // Thêm ISaveable
{
    public static FishJournalManager Instance { get; private set; }

    [SerializeField] private FishDatabaseSO fishDatabase;
    private Dictionary<string, FishRecord> journalData = new Dictionary<string, FishRecord>();
    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // savePath = Path.Combine(Application.persistentDataPath, "FishJournal.json"); // COMMENT: Không cần đường dẫn file cục bộ khi dùng Cloud
            InitJournal();
            // LoadData(); // COMMENT: Chuyển sang dùng LoadData(GameSaveData) của Cloud Save Firebase
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitJournal()
    {
        if (fishDatabase == null || fishDatabase.allFishes == null) return;

        foreach (FishSO fish in fishDatabase.allFishes)
        {
            if (fish != null && !string.IsNullOrEmpty(fish.itemID))
            {
                if (!journalData.ContainsKey(fish.itemID))
                {
                    journalData.Add(fish.itemID, new FishRecord(fish.itemID));
                }
            }
        }
    }

    // Thêm FishGrade vào hàm ghi nhận
    public bool RecordCatch(string fishID, float length, float weight, FishGrade grade)
    {
        if (!journalData.ContainsKey(fishID)) return false;

        bool isNewRecord = false;
        FishRecord record = journalData[fishID];

        record.isUnlocked = true;

        // So sánh hạng mới với hạng kỷ lục cũ, nếu cao hơn thì ghi đè
        if ((int)grade > (int)record.highestGrade)
        {
            record.highestGrade = grade;
        }

        if (length > record.maxLength)
        {
            record.maxLength = length;
            isNewRecord = true;
        }

        if (weight > record.maxWeight)
        {
            record.maxWeight = weight;
            isNewRecord = true;
        }

        // SaveData(); // COMMENT: Tắt hàm lưu file Json cục bộ cũ

        // LƯU CLOUD: Tự động đẩy lên Firebase nếu lập kỷ lục mới
        if (isNewRecord && SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGameToCloud();
        }

        return isNewRecord;
    }

    public FishRecord GetRecord(string fishID)
    {
        if (journalData.ContainsKey(fishID))
        {
            return journalData[fishID];
        }
        return null;
    }

    // ==================== CODE LƯU / TẢI JSON CỤC BỘ CỦ (ĐÃ COMMENT) ====================
    /*
    private void SaveData()
    {
        List<FishRecord> dataToSave = new List<FishRecord>(journalData.Values);
        string json = JsonUtility.ToJson(new SerializationWrapper { records = dataToSave });
        File.WriteAllText(savePath, json);
    }

    private void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);

            if (wrapper != null && wrapper.records != null)
            {
                foreach (FishRecord savedRecord in wrapper.records)
                {
                    if (journalData.ContainsKey(savedRecord.fishID))
                    {
                        journalData[savedRecord.fishID] = savedRecord;
                    }
                }
            }
        }
    }

    [System.Serializable]
    private class SerializationWrapper
    {
        public List<FishRecord> records;
    }

    [ContextMenu("XÓA DỮ LIỆU SỔ TAY (RESET)")]
    public void DeleteSaveData()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("<color=red>[Fish Journal] Đã xóa file save thành công! Hãy tắt Play và bật lại game.</color>");

            journalData.Clear();
            InitJournal();
        }
        else
        {
            Debug.Log("<color=yellow>[Fish Journal] Không tìm thấy file save nào để xóa.</color>");
        }
    }
    */

    // ==================== HỆ THỐNG LƯU / TẢI CLOUD MỚI (ISAVEABLE) ====================

    public void SaveData(GameSaveData data)
    {
        data.caughtFishJournal.Clear();

        // Chuyển toàn bộ kỷ lục cá từ Dictionary sang List trong GameSaveData
        foreach (var record in journalData.Values)
        {
            data.caughtFishJournal.Add(new FishRecordSave
            {
                fishID = record.fishID,
                isUnlocked = record.isUnlocked,
                highestGrade = (int)record.highestGrade,
                maxLength = record.maxLength,
                maxWeight = record.maxWeight
            });
        }

        Debug.Log($"[FishJournal] Đã đóng gói {data.caughtFishJournal.Count} loại cá lên Cloud Data!");
    }

    public void LoadData(GameSaveData data)
    {
        if (data.caughtFishJournal == null || data.caughtFishJournal.Count == 0) return;

        // Đọc dữ liệu từ Cloud khôi phục lại vào Dictionary journalData
        foreach (var savedRecord in data.caughtFishJournal)
        {
            if (journalData.ContainsKey(savedRecord.fishID))
            {
                FishRecord record = journalData[savedRecord.fishID];
                record.isUnlocked = savedRecord.isUnlocked;
                record.highestGrade = (FishGrade)savedRecord.highestGrade;
                record.maxLength = savedRecord.maxLength;
                record.maxWeight = savedRecord.maxWeight;
            }
        }

        Debug.Log($"[FishJournal] Đã khôi phục dữ liệu Sổ cá từ Cloud thành công!");
    }
}