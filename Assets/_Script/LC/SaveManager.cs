using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private void Update()
    {
        // Bấm phím K trên bàn phím để LƯU (Save)
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[Test] Đang bấm K để lưu...");
            SaveGameToCloud();
        }

        // Bấm phím L trên bàn phím để TẢI (Load)
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("[Test] Đang bấm L để load...");
            LoadGameFromCloud();
        }
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ==================== LƯU TIẾN ĐỘ LÊN FIREBASE ====================
    public void SaveGameToCloud()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("[SaveManager] Chưa đăng nhập Firebase!");
            return;
        }

        // 1. Khởi tạo đối tượng lưu trữ
        GameSaveData data = new GameSaveData();

        // 2. Tự động tìm tất cả các MonoBehaviours có dùng ISaveable trong Scene
        var saveableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveable>();        // 3. Bảo từng script tự đưa dữ liệu vào "thùng" data
        foreach (var saveable in saveableObjects)
        {
            saveable.SaveData(data);
        }

        // 4. Chuyển thành chuỗi JSON
        string jsonString = JsonUtility.ToJson(data);

        // 5. Đẩy JSON lên Firebase Firestore
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference userDoc = db.Collection("users").Document(user.UserId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "saveDataJson", jsonString },
            { "lastSaveTime", Timestamp.GetCurrentTimestamp() }
        };

        userDoc.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(task => {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("[SaveManager] Lưu toàn bộ game lên Firebase thành công!");
            }
            else
            {
                Debug.LogError("[SaveManager] Lưu thất bại: " + task.Exception);
            }
        });
    }

    // ==================== TẢI TIẾN ĐỘ TỪ FIREBASE VỀ ====================
    public void LoadGameFromCloud()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference userDoc = db.Collection("users").Document(user.UserId);

        userDoc.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.ContainsField("saveDataJson"))
                {
                    string jsonString = snapshot.GetValue<string>("saveDataJson");

                    // 1. Giải mã JSON ngược lại thành Object GameSaveData
                    GameSaveData data = JsonUtility.FromJson<GameSaveData>(jsonString);

                    // 2. Tự động quét các script ISaveable và phát dữ liệu lại cho chúng
                    var saveableObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
                    foreach (var saveable in saveableObjects)
                    {
                        saveable.LoadData(data);
                    }

                    Debug.Log("[SaveManager] Tải dữ liệu từ Firebase thành công!");
                }
            }
        });
    }
}