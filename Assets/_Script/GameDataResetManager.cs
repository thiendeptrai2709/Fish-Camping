using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// GameDataResetManager:
/// Cung cấp tính năng Reset toàn diện dữ liệu game (PlayerPrefs, File Save JSON, Balo, Cốp xe, Tiền, Tutorial, Nhật ký cá...)
/// để đưa game về trạng thái User mới vào chơi 100%.
/// - Có thể gọi từ Menu Unity Editor: Tools -> Game Data -> Reset Toàn Bộ Dữ Liệu (New User) [Ctrl+Shift+R].
/// - Có thể bấm phím nóng F10 khi đang chơi Play Mode.
/// - Tự động dọn dẹp các Singleton đang sống trong RAM để tránh lưu đè dữ liệu cũ.
/// </summary>
public class GameDataResetManager : MonoBehaviour
{
    public static GameDataResetManager Instance { get; private set; }

    [Header("--- Phím Tắt Reset Trong Game ---")]
    [Tooltip("Phím nóng để Reset toàn bộ dữ liệu khi đang chơi game")]
    [SerializeField] private KeyCode resetHotkey = KeyCode.F10;

    [Tooltip("Tự động tải lại màn chơi sau khi Reset")]
    [SerializeField] private bool reloadSceneAfterReset = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Phím nóng F10 reset tức thì trong lúc test Play Mode (Chỉ hoạt động trong Unity Editor)
        if (Input.GetKeyDown(resetHotkey))
        {
            ResetAllGameData(reloadSceneAfterReset);
        }
#endif
    }

    /// <summary>
    /// Hàm xóa sạch 100% dữ liệu game và đưa về trạng thái user mới
    /// </summary>
    public static void ResetAllGameData(bool reloadScene = true)
    {
        Debug.Log("<color=yellow>[GameDataResetManager] Bắt đầu xóa sạch toàn bộ dữ liệu game...</color>");

        // 1. Xóa toàn bộ PlayerPrefs (Tiền, Tutorial, Balo, Cốp xe, Vị trí xe/player, Cài đặt...)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. Xóa toàn bộ file save JSON / DAT trong persistentDataPath
        try
        {
            string savePath = Application.persistentDataPath;
            if (Directory.Exists(savePath))
            {
                string[] jsonFiles = Directory.GetFiles(savePath, "*.json", SearchOption.AllDirectories);
                foreach (string file in jsonFiles)
                {
                    try
                    {
                        File.Delete(file);
                        Debug.Log($"[GameDataResetManager] Đã xóa file JSON: {Path.GetFileName(file)}");
                    }
                    catch { }
                }

                string[] datFiles = Directory.GetFiles(savePath, "*.dat", SearchOption.AllDirectories);
                foreach (string file in datFiles)
                {
                    try
                    {
                        File.Delete(file);
                        Debug.Log($"[GameDataResetManager] Đã xóa file DAT: {Path.GetFileName(file)}");
                    }
                    catch { }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[GameDataResetManager] Lỗi khi xóa file save: {ex.Message}");
        }

        // 3. Reset các singleton in-memory nếu đang sống trong RAM
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.RefreshMoneyFromSave();
        }

        if (FishJournalManager.Instance != null)
        {
            FishJournalManager.Instance.ResetJournalData();
        }

        if (ForcedTutorialManager.Instance != null)
        {
            ForcedTutorialManager.Instance.ResetTutorialProgress();
        }

        if (BackpackMinigameUI.Instance != null)
        {
            BackpackMinigameUI.Instance.ClearBackpackSave();
        }

        if (TrunkMinigameUI.Instance != null)
        {
            TrunkMinigameUI.Instance.ClearTrunkSave();
        }

        Debug.Log("<color=green><b>[GameDataResetManager] ĐÃ RESET TOÀN BỘ DỮ LIỆU GAME THÀNH CÔNG! Trạng thái: User mới 100%.</b></color>");

        // 4. Tải lại scene nếu đang trong Play Mode
        if (reloadScene && Application.isPlaying)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Game Data/Reset Toàn Bộ Dữ Liệu (New User) %#r", priority = 0)]
    [MenuItem("Game/Reset Toàn Bộ Dữ Liệu (User Mới)", priority = 0)]
    public static void ResetFromEditorMenu()
    {
        ResetAllGameData(reloadScene: Application.isPlaying);
        EditorUtility.DisplayDialog("Reset Game Data Thành Công",
            "Đã xóa sạch toàn bộ dữ liệu lưu:\n- PlayerPrefs (Tiền, Quest Tutorial, Vị trí xe/Player, Balo, Cốp xe...)\n- File save JSON/DAT trong persistentDataPath.\n\nGame đã trở về trạng thái User mới 100%!",
            "OK");
    }

    [MenuItem("Tools/Game Data/Chỉ Reset Tiến Độ Tutorial", priority = 1)]
    public static void ResetOnlyTutorial()
    {
        PlayerPrefs.DeleteKey("Saved_TutorialStage");
        PlayerPrefs.DeleteKey("QuestSystem_Unlocked");
        PlayerPrefs.Save();
        if (ForcedTutorialManager.Instance != null)
        {
            ForcedTutorialManager.Instance.ResetTutorialProgress();
        }
        EditorUtility.DisplayDialog("Reset Tutorial", "Đã đưa tiến độ Tutorial về nhiệm vụ đầu tiên!", "OK");
    }

    [MenuItem("Tools/Game Data/Chỉ Reset Tiền & Balo", priority = 2)]
    public static void ResetMoneyAndBackpack()
    {
        PlayerPrefs.DeleteKey("PlayerCurrentMoney");
        PlayerPrefs.DeleteKey("MoneyManager_Initialized");
        PlayerPrefs.DeleteKey("Saved_BackpackGridData");
        PlayerPrefs.DeleteKey("Saved_TrunkGridData");
        PlayerPrefs.Save();
        if (MoneyManager.Instance != null) MoneyManager.Instance.RefreshMoneyFromSave();
        EditorUtility.DisplayDialog("Reset Tiền & Balo", "Đã xóa tiền và làm trống Balo/Cốp xe!", "OK");
    }

    [MenuItem("Tools/Game Data/Mở Thư Mục Save (PersistentDataPath)", priority = 20)]
    public static void OpenSaveDirectory()
    {
        string path = Application.persistentDataPath;
        if (Directory.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
    }
#endif
}
