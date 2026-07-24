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
            savePath = Path.Combine(Application.persistentDataPath, "FishJournal.json");
            InitJournal();
            LoadData();
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

    public bool RecordCatch(string fishID, float length, float weight)
    {
        if (!journalData.ContainsKey(fishID)) return false;

        bool isNewRecord = false;
        FishRecord record = journalData[fishID];

        record.isUnlocked = true;
        record.totalCaught++;

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

            // Làm sạch data hiện tại trên RAM
            journalData.Clear();
            InitJournal();
        }
        else
        {
            Debug.Log("<color=yellow>[Fish Journal] Không tìm thấy file save nào để xóa.</color>");
        }
    }
}