using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

[Serializable]
public class UnifiedGameSaveData
{
    public string userUID = "";
    public string email = "";
    public long timestamp = 0;

    // 1. Tiền tệ
    public int money = 1000;

    // 2. Balo & Ô trang bị (JSON string từ BackpackSaveContainer)
    public string backpackJson = "";

    // 3. Cốp xe (JSON string từ TrunkSaveContainer)
    public string trunkJson = "";

    // 4. Xe & Nâng cấp Garage
    public int equippedTireIndex = -1;
    public int savedTrunkLevel = 0;
    public float vehicleFuel = 100f;

    // 5. Sổ tay cá (JSON string từ FishJournalSaveContainer)
    public string fishJournalJson = "";

    // 6. Công trình xây dựng trên các map (List MapBuildingSaveEntry)
    public List<MapBuildingSaveEntry> mapBuildings = new List<MapBuildingSaveEntry>();

    // 7. Cốt truyện & Nhiệm vụ
    public int tutorialStage = 0;
    public string questManagerDataJson = "";

    // 8. Vị trí người chơi / xe
    public string playerLocationJson = "";

    // 9. Cài đặt game (Ngôn ngữ & Âm thanh)
    public int languageID = 0;
    public float bgmVolume = 0.75f;
    public float sfxVolume = 0.75f;
}

[Serializable]
public class MapBuildingSaveEntry
{
    public string mapName;
    public string buildingsJson;

    public MapBuildingSaveEntry(string map, string json)
    {
        mapName = map;
        buildingsJson = json;
    }
}

public class GameDatabaseManager : MonoBehaviour
{
    public static GameDatabaseManager Instance { get; private set; }

    private const string CLOUD_BACKUP_LOCAL_KEY = "Local_Cloud_Backup_Data";
    private DatabaseReference dbReference;
    private bool isFirebaseDatabaseReady = false;
    private bool isSyncing = false;

    private float autoCloudSyncInterval = 30f;
    private float cloudSyncTimer = 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("[GameDatabaseManager]");
            Instance = go.AddComponent<GameDatabaseManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            InitializeFirebaseDatabase();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // Tự động đồng bộ lên Firebase định kỳ 30 giây một lần khi đang trong gameplay
        if (isFirebaseDatabaseReady && !isSyncing)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (PlayerLocationSaveManager.IsGameplayScene(currentScene))
            {
                cloudSyncTimer += Time.deltaTime;
                if (cloudSyncTimer >= autoCloudSyncInterval)
                {
                    cloudSyncTimer = 0f;
                    SaveAndSyncToCloud();
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerLocationSaveManager.IsGameplayScene(scene.name))
        {
            // Tự động lưu và sync khi đổi scene
            SaveAndSyncToCloud();
        }
    }

    private void InitializeFirebaseDatabase()
    {
        try
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            if (app != null)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                isFirebaseDatabaseReady = true;
                Debug.Log("<color=green>[GameDatabaseManager] Firebase Realtime Database đã sẵn sàng!</color>");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameDatabaseManager] Không thể khởi tạo Firebase Database trực tiếp: {e.Message}");
        }
    }

    public string GetCurrentUserID()
    {
        if (FirebaseAuth.DefaultInstance != null && FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            return FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }
        return PlayerPrefs.GetString("Firebase_User_UID", PlayerPrefs.GetString("Last_Active_User_UID", "local_player"));
    }

    public string GetCurrentUserEmail()
    {
        if (FirebaseAuth.DefaultInstance != null && FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            return FirebaseAuth.DefaultInstance.CurrentUser.Email ?? "";
        }
        return PlayerPrefs.GetString("Firebase_User_Email", "");
    }

    /// <summary>
    /// Thu thập toàn bộ dữ liệu hiện tại từ các hệ thống và trả về đối tượng UnifiedGameSaveData
    /// </summary>
    public UnifiedGameSaveData ExportAllGameData()
    {
        UnifiedGameSaveData data = new UnifiedGameSaveData
        {
            userUID = GetCurrentUserID(),
            email = GetCurrentUserEmail(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),

            // 1. Tiền
            money = MoneyManager.Instance != null ? MoneyManager.Instance.tongTien : PlayerPrefs.GetInt("PlayerMoney", PlayerPrefs.GetInt("Saved_Player_Money", 5000)),

            // 2. Balo & Ô trang bị
            backpackJson = PlayerPrefs.GetString("Saved_Backpack_Data", ""),

            // 3. Cốp xe
            trunkJson = PlayerPrefs.GetString("Saved_Trunk_Data", ""),

            // 4. Xe & Nâng cấp Garage
            equippedTireIndex = PlayerPrefs.GetInt("EquippedTireIndex", -1),
            savedTrunkLevel = PlayerPrefs.GetInt("SavedTrunkLevel", 0),

            // 5. Cốt truyện & Nhiệm vụ
            tutorialStage = ForcedTutorialManager.Instance != null ? (int)ForcedTutorialManager.Instance.GetCurrentStage() : PlayerPrefs.GetInt("ForcedTutorial_Stage", 0),

            // 6. Vị trí người chơi / xe
            playerLocationJson = PlayerPrefs.GetString("Saved_Player_Location_Data", ""),

            // 7. Cài đặt game
            languageID = PlayerPrefs.GetInt("LanguageID", 0),
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.75f),
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f)
        };

        // Nạp dữ liệu Sổ tay cá từ file JSON
        try
        {
            string journalPath = Path.Combine(Application.persistentDataPath, "FishJournal.json");
            if (File.Exists(journalPath))
            {
                data.fishJournalJson = File.ReadAllText(journalPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameDatabaseManager] Lỗi đọc FishJournal.json: {e.Message}");
        }

        // Nạp danh sách công trình xây dựng của tất cả các map
        string[] mapNames = new string[] { "Map_1_Town", "Map_2_PineLake", "Map3_Swamp", "Map4__Ocean Coast" };
        foreach (var map in mapNames)
        {
            try
            {
                string bPath = Path.Combine(Application.persistentDataPath, $"{map}_placed_buildings.json");
                if (File.Exists(bPath))
                {
                    string bJson = File.ReadAllText(bPath);
                    data.mapBuildings.Add(new MapBuildingSaveEntry(map, bJson));
                }
            }
            catch { }
        }

        return data;
    }

    /// <summary>
    /// Nhập và phân phối gói UnifiedGameSaveData vào các hệ thống Local
    /// </summary>
    public void ImportAllGameData(UnifiedGameSaveData data)
    {
        if (data == null) return;

        // 1. Tiền
        if (data.money >= 0)
        {
            PlayerPrefs.SetInt("PlayerMoney", data.money);
            PlayerPrefs.SetInt("Saved_Player_Money", data.money);
            PlayerPrefs.SetInt("HasInitializedMoney", 1);
            PlayerPrefs.SetInt("Has_Initialized_Money", 1);
            if (MoneyManager.Instance != null) MoneyManager.Instance.RefreshMoneyFromSave();
        }

        // 2. Balo & Ô trang bị
        if (!string.IsNullOrEmpty(data.backpackJson))
        {
            PlayerPrefs.SetString("Saved_Backpack_Data", data.backpackJson);
            if (BackpackMinigameUI.Instance != null) BackpackMinigameUI.Instance.LoadBackpack();
        }

        // 3. Cốp xe
        if (!string.IsNullOrEmpty(data.trunkJson))
        {
            PlayerPrefs.SetString("Saved_Trunk_Data", data.trunkJson);
            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.LoadTrunk();
        }

        // 4. Xe & Garage
        if (data.equippedTireIndex >= 0)
        {
            PlayerPrefs.SetInt("EquippedTireIndex", data.equippedTireIndex);
        }
        if (data.savedTrunkLevel >= 0)
        {
            PlayerPrefs.SetInt("SavedTrunkLevel", data.savedTrunkLevel);
        }

        // 5. Sổ tay cá
        if (!string.IsNullOrEmpty(data.fishJournalJson))
        {
            try
            {
                string journalPath = Path.Combine(Application.persistentDataPath, "FishJournal.json");
                File.WriteAllText(journalPath, data.fishJournalJson);
                if (FishJournalManager.Instance != null) FishJournalManager.Instance.ReloadFromDisk();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameDatabaseManager] Lỗi ghi FishJournal.json: {e.Message}");
            }
        }

        // 6. Công trình xây dựng
        if (data.mapBuildings != null)
        {
            foreach (var entry in data.mapBuildings)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.mapName) && !string.IsNullOrEmpty(entry.buildingsJson))
                {
                    try
                    {
                        string bPath = Path.Combine(Application.persistentDataPath, $"{entry.mapName}_placed_buildings.json");
                        File.WriteAllText(bPath, entry.buildingsJson);
                    }
                    catch { }
                }
            }
            if (BuildingSaveManager.Instance != null) BuildingSaveManager.Instance.LoadSceneBuildings();
        }

        // 7. Cốt truyện & Tutorial
        if (data.tutorialStage >= 0)
        {
            PlayerPrefs.SetInt("ForcedTutorial_Stage", data.tutorialStage);
            if (ForcedTutorialManager.Instance != null)
            {
                ForcedTutorialManager.Instance.SyncFromSavedStage(data.tutorialStage);
            }
        }

        // 8. Vị trí
        if (!string.IsNullOrEmpty(data.playerLocationJson))
        {
            PlayerPrefs.SetString("Saved_Player_Location_Data", data.playerLocationJson);
        }

        // 9. Cài đặt
        PlayerPrefs.SetInt("LanguageID", data.languageID);
        PlayerPrefs.SetFloat("BGMVolume", data.bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", data.sfxVolume);

        PlayerPrefs.Save();
        Debug.Log("<color=green>[GameDatabaseManager] Đã đồng bộ dữ liệu vào hệ thống Local thành công!</color>");
    }

    /// <summary>
    /// Lưu toàn bộ dữ liệu xuống Local và đồng thời đẩy lên Firebase Realtime Database
    /// </summary>
    public void SaveAndSyncToCloud()
    {
        if (isSyncing) return;

        UnifiedGameSaveData data = ExportAllGameData();
        string json = JsonUtility.ToJson(data, true);

        // 1. Lưu bản backup Local
        PlayerPrefs.SetString(CLOUD_BACKUP_LOCAL_KEY, json);
        PlayerPrefs.Save();

        string uid = GetCurrentUserID();
        if (string.IsNullOrEmpty(uid) || uid == "local_player")
        {
            return;
        }

        // 2. Đẩy lên Firebase Database nếu đã sẵn sàng
        if (isFirebaseDatabaseReady && dbReference != null)
        {
            isSyncing = true;
            dbReference.Child("users").Child(uid).Child("saveData").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                isSyncing = false;
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"<color=cyan>[GameDatabaseManager] Đã đồng bộ Cloud Database Firebase thành công cho UID: {uid}</color>");
                }
                else
                {
                    Debug.LogWarning($"[GameDatabaseManager] Không thể lưu lên Firebase Database: {task.Exception?.Message}");
                }
            });
        }
    }

    /// <summary>
    /// Tải toàn bộ dữ liệu từ Firebase Database về và cập nhật Local khi đăng nhập
    /// </summary>
    public void LoadFromCloud(string userUID, Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(userUID))
        {
            onComplete?.Invoke(false);
            return;
        }

        if (!isFirebaseDatabaseReady || dbReference == null)
        {
            InitializeFirebaseDatabase();
        }

        if (dbReference != null)
        {
            dbReference.Child("users").Child(userUID).Child("saveData").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully && task.Result.Exists)
                {
                    string rawJson = task.Result.GetRawJsonValue();
                    if (!string.IsNullOrEmpty(rawJson))
                    {
                        try
                        {
                            UnifiedGameSaveData cloudData = JsonUtility.FromJson<UnifiedGameSaveData>(rawJson);
                            ImportAllGameData(cloudData);
                            Debug.Log($"<color=green>[GameDatabaseManager] Tải dữ liệu Cloud thành công cho UID: {userUID}</color>");
                            onComplete?.Invoke(true);
                            return;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[GameDatabaseManager] Lỗi giải mã dữ liệu Cloud: {e.Message}");
                        }
                    }
                }
                onComplete?.Invoke(false);
            });
        }
        else
        {
            onComplete?.Invoke(false);
        }
    }
}
