using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FishJournalManager : MonoBehaviour
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
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "FishJournal.json");

            if (fishDatabase == null)
            {
                fishDatabase = Resources.Load<FishDatabaseSO>("FishDatabase") ?? Resources.Load<FishDatabaseSO>("FishDatabaseSO");
                if (fishDatabase == null)
                {
                    FishDatabaseSO[] all = Resources.FindObjectsOfTypeAll<FishDatabaseSO>();
                    if (all != null && all.Length > 0) fishDatabase = all[0];
                }
            }

            InitJournal();
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReloadFromDisk()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Path.Combine(Application.persistentDataPath, "FishJournal.json");
        }
        InitJournal();
        LoadData();
    }

    private void InitJournal()
    {
        if (fishDatabase == null)
        {
            fishDatabase = Resources.Load<FishDatabaseSO>("FishDatabase") ?? Resources.Load<FishDatabaseSO>("FishDatabaseSO");
            if (fishDatabase == null)
            {
                FishDatabaseSO[] all = Resources.FindObjectsOfTypeAll<FishDatabaseSO>();
                if (all != null && all.Length > 0) fishDatabase = all[0];
            }
        }

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

        SaveData();
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

    /// <summary>
    /// Đếm tổng số lượng loài cá đã câu được trong toàn bộ Sổ Tay
    /// </summary>
    public int GetTotalUnlockedFishCount()
    {
        int count = 0;
        foreach (var kvp in journalData)
        {
            if (kvp.Value != null && kvp.Value.isUnlocked)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Đếm số loài cá đã câu theo tên khu vực/map (ví dụ: "Hồ", "Đầm Lầy", "Biển")
    /// </summary>
    public int GetUnlockedFishCountByMap(string mapKeyword)
    {
        if (fishDatabase == null || fishDatabase.allFishes == null) return GetTotalUnlockedFishCount();
        if (string.IsNullOrEmpty(mapKeyword)) return GetTotalUnlockedFishCount();

        int count = 0;
        string kw = mapKeyword.Trim().ToLowerInvariant();

        foreach (FishSO fish in fishDatabase.allFishes)
        {
            if (fish == null || string.IsNullOrEmpty(fish.itemID)) continue;

            string fMap = (fish.mapName ?? "").ToLowerInvariant();
            bool mapMatch = false;

            if (kw.Contains("hồ") || kw.Contains("lake") || kw.Contains("pine"))
            {
                mapMatch = fMap.Contains("lake") || fMap.Contains("hồ") || fMap.Contains("pine");
            }
            else if (kw.Contains("đầm") || kw.Contains("swamp"))
            {
                mapMatch = fMap.Contains("swamp") || fMap.Contains("đầm");
            }
            else if (kw.Contains("biển") || kw.Contains("ocean") || kw.Contains("coast"))
            {
                mapMatch = fMap.Contains("ocean") || fMap.Contains("coast") || fMap.Contains("biển");
            }
            else
            {
                mapMatch = fMap.Contains(kw);
            }

            if (mapMatch)
            {
                if (journalData.TryGetValue(fish.itemID, out FishRecord record) && record.isUnlocked)
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// Kiểm tra xem đã câu được con cá nào đạt phẩm chất tối thiểu (vd: Rare, Legendary) hay chưa
    /// </summary>
    public bool HasCaughtFishWithRarity(FishRarity minRarity)
    {
        if (fishDatabase == null || fishDatabase.allFishes == null) return false;

        foreach (FishSO fish in fishDatabase.allFishes)
        {
            if (fish == null || string.IsNullOrEmpty(fish.itemID)) continue;
            if (fish.rarity >= minRarity)
            {
                if (journalData.TryGetValue(fish.itemID, out FishRecord record) && record.isUnlocked)
                {
                    return true;
                }
            }
        }
        return false;
    }

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

    public void ResetJournalData()
    {
        journalData.Clear();
        InitJournal();
    }
}