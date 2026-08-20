using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerLocationData
{
    public string sceneName = "Map_1_Town";
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float playerRotY;

    public bool isInVehicle;
    public float vehiclePosX;
    public float vehiclePosY;
    public float vehiclePosZ;
    public float vehicleRotY;

    public bool hasSavedPosition;
}

public class PlayerLocationSaveManager : MonoBehaviour
{
    public static PlayerLocationSaveManager Instance { get; private set; }

    private const string LOCATION_SAVE_KEY = "Saved_Player_Location_Data";
    private float autoSaveInterval = 3f;
    private float autoSaveTimer = 0f;

    private static bool isInitialBoot = true;
    private bool isRestoring = false;

    private static readonly HashSet<string> NonGameplayScenes = new HashSet<string>
    {
        "Intro_game",
        "LoginScene",
        "Play_Dki",
        "Play_Setting",
        "Scene_ShopPopup_Test",
        "Play1",
        "NPC"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("[PlayerLocationSaveManager]");
            Instance = go.AddComponent<PlayerLocationSaveManager>();
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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Update()
    {
        if (isRestoring) return;

        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveCurrentLocation();
        }
    }

    public static bool IsGameplayScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        return !NonGameplayScenes.Contains(sceneName);
    }

    public static string GetSavedSceneName(string defaultScene = "Map_1_Town")
    {
        if (PlayerPrefs.HasKey(LOCATION_SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(LOCATION_SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    PlayerLocationData data = JsonUtility.FromJson<PlayerLocationData>(json);
                    if (data != null && !string.IsNullOrEmpty(data.sceneName) && IsGameplayScene(data.sceneName))
                    {
                        return data.sceneName;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PlayerLocationSaveManager] Lỗi đọc scene đã lưu: {e.Message}");
                }
            }
        }
        return defaultScene;
    }

    public void SetTargetSceneOnTravel(string targetScene)
    {
        if (!IsGameplayScene(targetScene)) return;

        PlayerLocationData data = LoadSavedData();
        if (data == null) data = new PlayerLocationData();
        data.sceneName = targetScene;
        data.hasSavedPosition = false; // Đánh dấu false để khi sang map mới dùng điểm Spawn mặc định của map mới

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LOCATION_SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"<color=cyan>[PlayerLocationSaveManager] Đã lưu trước Map đích khi di chuyển: {targetScene}</color>");
    }

    public void SaveCurrentLocation(bool force = false)
    {
        if (isRestoring) return;

        string currentScene = SceneManager.GetActiveScene().name;
        if (!IsGameplayScene(currentScene)) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        VehicleEnterExit vehicleEnterExit = Object.FindFirstObjectByType<VehicleEnterExit>(FindObjectsInactive.Include);
        VehicleController vehicleController = Object.FindFirstObjectByType<VehicleController>(FindObjectsInactive.Include);

        Transform vehicleTransform = null;
        if (vehicleEnterExit != null) vehicleTransform = vehicleEnterExit.transform;
        else if (vehicleController != null) vehicleTransform = vehicleController.transform;

        if (player == null && vehicleTransform == null) return;

        bool inVehicle = false;
        if (vehicleEnterExit != null && vehicleEnterExit.IsInCar) inVehicle = true;

        PlayerLocationData data = new PlayerLocationData
        {
            sceneName = currentScene,
            hasSavedPosition = true,
            isInVehicle = inVehicle
        };

        if (player != null)
        {
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
            data.playerPosZ = player.transform.position.z;
            data.playerRotY = player.transform.eulerAngles.y;
        }

        if (vehicleTransform != null)
        {
            data.vehiclePosX = vehicleTransform.position.x;
            data.vehiclePosY = vehicleTransform.position.y;
            data.vehiclePosZ = vehicleTransform.position.z;
            data.vehicleRotY = vehicleTransform.eulerAngles.y;

            if (inVehicle)
            {
                data.playerPosX = data.vehiclePosX;
                data.playerPosY = data.vehiclePosY;
                data.playerPosZ = data.vehiclePosZ;
                data.playerRotY = data.vehicleRotY;
            }
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LOCATION_SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private PlayerLocationData LoadSavedData()
    {
        if (!PlayerPrefs.HasKey(LOCATION_SAVE_KEY)) return null;
        string json = PlayerPrefs.GetString(LOCATION_SAVE_KEY, "");
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            return JsonUtility.FromJson<PlayerLocationData>(json);
        }
        catch
        {
            return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsGameplayScene(scene.name)) return;

        // Nếu vừa khởi động game và mở vào Map mặc định (Map_1_Town) nhưng trước đó đang ở Map khác (ví dụ: Map_2_PineLake)
        if (isInitialBoot)
        {
            isInitialBoot = false;
            string savedScene = GetSavedSceneName(scene.name);
            if (!string.IsNullOrEmpty(savedScene) && savedScene != scene.name)
            {
                Debug.Log($"<color=yellow>[PlayerLocationSaveManager] Khởi động game: Tự động chuyển tiếp từ [{scene.name}] sang Map đã lưu [{savedScene}]...</color>");
                SceneManager.LoadScene(savedScene);
                return;
            }
        }

        // Đảm bảo GameplayCorePrefab (Player, Car, UI) luôn tồn tại trong Scene
        GameplayCoreManager.EnsureGameplayCoreExists();

        StartCoroutine(RestoreLocationRoutine(scene.name));
    }

    private IEnumerator RestoreLocationRoutine(string sceneName)
    {
        isRestoring = true;

        // Chờ 2 frame để đảm bảo tất cả GameObject, Terrain, NavMesh đã load hoàn tất
        yield return null;
        yield return new WaitForSeconds(0.15f);

        PlayerLocationData data = LoadSavedData();

        if (data != null && data.hasSavedPosition && data.sceneName == sceneName)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            VehicleEnterExit vehicleEnterExit = Object.FindFirstObjectByType<VehicleEnterExit>(FindObjectsInactive.Include);
            VehicleController vehicleController = Object.FindFirstObjectByType<VehicleController>(FindObjectsInactive.Include);

            Transform vehicleTransform = null;
            if (vehicleEnterExit != null) vehicleTransform = vehicleEnterExit.transform;
            else if (vehicleController != null) vehicleTransform = vehicleController.transform;

            // 1. Phục hồi vị trí Xe nếu có
            if (vehicleTransform != null && (data.vehiclePosX != 0 || data.vehiclePosZ != 0))
            {
                Rigidbody carRb = vehicleTransform.GetComponent<Rigidbody>();
                if (carRb != null)
                {
                    carRb.isKinematic = true;
                    carRb.linearVelocity = Vector3.zero;
                    carRb.angularVelocity = Vector3.zero;
                }

                vehicleTransform.position = new Vector3(data.vehiclePosX, data.vehiclePosY, data.vehiclePosZ);
                vehicleTransform.rotation = Quaternion.Euler(0, data.vehicleRotY, 0);
                Physics.SyncTransforms();

                yield return new WaitForSeconds(0.2f);
                if (carRb != null) carRb.isKinematic = false;
            }

            // 2. Phục hồi trạng thái Người chơi
            if (data.isInVehicle && vehicleEnterExit != null)
            {
                CarDoor carDoor = vehicleEnterExit.GetComponentInChildren<CarDoor>();
                Transform exitPoint = (carDoor != null) ? carDoor.ExitPoint : null;
                vehicleEnterExit.EnterVehicle(exitPoint);
            }
            else
            {
                if (vehicleEnterExit != null && vehicleEnterExit.IsInCar)
                {
                    vehicleEnterExit.ForceExitVehicle();
                }

                if (player != null && (data.playerPosX != 0 || data.playerPosZ != 0))
                {
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;

                    player.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
                    player.transform.rotation = Quaternion.Euler(0, data.playerRotY, 0);
                    Physics.SyncTransforms();

                    if (cc != null) cc.enabled = true;
                }
            }

            Debug.Log($"<color=green>[PlayerLocationSaveManager] Đã phục hồi vị trí map [{sceneName}] - InVehicle: {data.isInVehicle}</color>");
        }

        isRestoring = false;

        // Lưu lại vị trí hiện tại sau khi đã restore xong
        SaveCurrentLocation();
    }

    [ContextMenu("Xóa dữ liệu Vị trí người chơi (Reset Location)")]
    public void ClearSavedLocation()
    {
        PlayerPrefs.DeleteKey(LOCATION_SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>[PlayerLocationSaveManager] Đã xóa dữ liệu vị trí người chơi!</color>");
    }

    private void OnApplicationQuit()
    {
        if (!isRestoring) SaveCurrentLocation(true);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && !isRestoring) SaveCurrentLocation(true);
    }
}
