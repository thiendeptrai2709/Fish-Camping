using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings; // Thư viện Localization

[System.Serializable]
public class QuestSaveContainer
{
    public List<Quest> quests = new List<Quest>();
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private const string QUEST_SAVE_KEY = "Saved_NPC_Quest_Data";

    [Header("Quest Data")]
    public List<Quest> questList = new List<Quest>();

    [Header("UI References (Full Panel)")]
    public Transform questContentParent; // Kéo ô Content trong ScrollView vào đây
    public GameObject questItemPrefab;   // Kéo Prefab ô Nhiệm vụ vào đây
    public GameObject questPanel;        // Kéo QuestPanel từ Hierarchy vào đây

    [Header("Mission Day HUD (Outside Widget)")]
    public GameObject missionDayPanel;      // Panel mission_day bên ngoài màn hình
    public TextMeshProUGUI missionDayText;  // Text con 'nv' trong mission_day

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject); // Giữ QuestManager không bị xóa khi đổi Scene
            LoadQuestData();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SaveQuestData();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveQuestData();
    }

    public void SaveQuestData()
    {
        try
        {
            QuestSaveContainer container = new QuestSaveContainer();
            container.quests = new List<Quest>(questList);
            string json = JsonUtility.ToJson(container);
            PlayerPrefs.SetString(QUEST_SAVE_KEY, json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestManager] Lỗi lưu dữ liệu nhiệm vụ: {e.Message}");
        }
    }

    public void LoadQuestData()
    {
        try
        {
            if (PlayerPrefs.HasKey(QUEST_SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(QUEST_SAVE_KEY);
                if (!string.IsNullOrEmpty(json))
                {
                    QuestSaveContainer container = JsonUtility.FromJson<QuestSaveContainer>(json);
                    if (container != null && container.quests != null && container.quests.Count > 0)
                    {
                        questList = container.quests;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestManager] Lỗi nạp dữ liệu nhiệm vụ: {e.Message}");
        }
    }

    private void OnEnable()
    {
        // Tự động lắng nghe sự kiện khi người chơi đổi ngôn ngữ ở menu Cài đặt
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        EnsureUIAttached();
        UpdateMissionDayHUD();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        questPanel = null;
        questContentParent = null;
        missionDayPanel = null;
        missionDayText = null;

        EnsureUIAttached();
        UpdateMissionDayHUD();
    }

    // Tự động vẽ lại toàn bộ danh sách nhiệm vụ khi đổi ngôn ngữ
    private void OnLanguageChanged(UnityEngine.Localization.Locale locale)
    {
        if (questPanel != null && questPanel.activeSelf)
        {
            RenderQuestList();
        }
        UpdateMissionDayHUD();
    }

    private void Update()
    {
        // Nhấn phím V để Bật / Tắt Panel Nhiệm vụ (Hỗ trợ cả New Input System và Legacy Input)
        bool vPressed = (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.V);
        if (vPressed)
        {
            ToggleQuestPanel();
        }
    }

    // Hàm ÉP ĐÓNG Panel (Gọi từ QuestGiver / DialogueManager khi bắt đầu nói chuyện)
    public void ClosePanel()
    {
        EnsureUIAttached();

        if (questPanel != null && questPanel.activeSelf)
        {
            questPanel.SetActive(false);
        }
    }

    // Hàm Bật / Tắt Panel Nhiệm vụ + Quản lý Con Trỏ Chuột
    public void ToggleQuestPanel()
    {
        EnsureUIAttached();

        if (questPanel == null)
        {
            Debug.LogWarning("<color=yellow>[QuestManager] questPanel chưa được gắn kết trong Scene này!</color>");
            return;
        }

        bool willOpen = !questPanel.activeSelf;

        // Nếu chuẩn bị MỞ BẢNG -> Kiểm tra xem có đang nói chuyện với NPC không
        if (willOpen)
        {
            // Nếu DialogueManager đang bật khung thoại thì CẤM KHÔNG CHO MỞ BẢNG
            if (IsDialogueShowing())
            {
                return;
            }

            questPanel.SetActive(true);
            RenderQuestList();

            // Bật con trỏ chuột
            if (PlayerCursor.Instance != null)
            {
                PlayerCursor.Instance.SetCursorState(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            // TẮT BẢNG NHIỆM VỤ
            questPanel.SetActive(false);

            // Khóa lại con trỏ chuột
            if (PlayerCursor.Instance != null)
            {
                PlayerCursor.Instance.SetCursorState(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // Kiểm tra chính xác xem khung thoại NPC có đang hiện trên màn hình không
    private bool IsDialogueShowing()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.dialogueCanvas != null)
        {
            return DialogueManager.Instance.dialogueCanvas.activeInHierarchy;
        }
        return false;
    }

    // Tự động nối lại UI khi chuyển Scene
    public void EnsureUIAttached()
    {
        if (questPanel == null || questContentParent == null || missionDayPanel == null || missionDayText == null)
        {
            QuestUIBinder binder = Object.FindFirstObjectByType<QuestUIBinder>(FindObjectsInactive.Include);
            if (binder != null)
            {
                binder.RegisterToManager();
            }
        }

        // Tự động tìm kiếm GameObject 'mission_day' trong Canvas nếu chưa được gán
        if (missionDayPanel == null)
        {
            GameObject found = GameObject.Find("mission_day") ?? GameObject.Find("Mission_Day");
            if (found == null)
            {
                GameObject[] allGos = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allGos)
                {
                    if (go != null && (go.name == "mission_day" || go.name == "Mission_Day") && go.scene.isLoaded)
                    {
                        found = go;
                        break;
                    }
                }
            }

            if (found != null)
            {
                missionDayPanel = found;
                missionDayText = missionDayPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        if (missionDayPanel != null && missionDayText == null)
        {
            missionDayText = missionDayPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        // Gắn nút bấm vào mission_day để khi click vào cũng mở panel nhiệm vụ
        if (missionDayPanel != null)
        {
            Button btn = missionDayPanel.GetComponent<Button>();
            if (btn == null)
            {
                btn = missionDayPanel.AddComponent<Button>();
            }
            btn.onClick.RemoveListener(ToggleQuestPanel);
            btn.onClick.AddListener(ToggleQuestPanel);
        }
    }

    // Đăng ký UI từ QuestUIBinder
    public void RegisterUI(GameObject panel, Transform contentParent)
    {
        questPanel = panel;
        questContentParent = contentParent;
        RenderQuestList();
    }

    public void RegisterMissionDayHUD(GameObject panel, TextMeshProUGUI text)
    {
        missionDayPanel = panel;
        missionDayText = text;
        UpdateMissionDayHUD();
    }

    // Lấy trạng thái của nhiệm vụ theo ID
    public QuestState GetQuestState(string questId)
    {
        Quest q = questList.Find(x => x.id == questId);
        return q != null ? q.state : QuestState.NotStarted;
    }

    // Thêm nhiệm vụ mới từ QuestGiver vào danh sách
    public void AddQuestToList(Quest newQuest)
    {
        Quest q = questList.Find(x => x.id == newQuest.id);
        if (q == null)
        {
            questList.Add(newQuest);
        }
        else
        {
            q.state = newQuest.state;
            q.currentAmount = newQuest.currentAmount;
            q.targetAmount = newQuest.targetAmount;
        }
        RenderQuestList();
        UpdateMissionDayHUD();
        SaveQuestData();
    }

    // Cập nhật lại UI Panel
    public void RenderQuestList()
    {
        if (questContentParent == null || questItemPrefab == null) return;

        foreach (Transform child in questContentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Quest q in questList)
        {
            GameObject item = Instantiate(questItemPrefab, questContentParent);
            item.GetComponent<QuestItemUI>().Setup(q);
        }
    }

    // Cập nhật bảng mission_day hiển thị ngay bên ngoài màn hình
    public void UpdateMissionDayHUD()
    {
        EnsureUIAttached();

        if (missionDayPanel == null) return;

        // Lấy danh sách nhiệm vụ đang làm (InProgress hoặc CanClaim)
        List<Quest> activeQuests = questList.FindAll(q => q.state == QuestState.InProgress || q.state == QuestState.CanClaim);

        if (activeQuests == null || activeQuests.Count == 0)
        {
            missionDayPanel.SetActive(false);
            return;
        }

        missionDayPanel.SetActive(true);

        if (missionDayText == null)
        {
            missionDayText = missionDayPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (missionDayText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < activeQuests.Count; i++)
            {
                var q = activeQuests[i];
                if (i > 0) sb.Append("\n------------------\n");

                string title = GetLocalizedText(q.title);
                string desc = GetLocalizedText(q.description);
                string target = GetLocalizedText(q.targetItem);

                sb.Append($"<b><color=#FFE082>{title}</color></b>\n");

                if (!string.IsNullOrEmpty(desc))
                {
                    sb.Append($"<size=85%><color=#E0E0E0>{desc}</color></size>\n");
                }

                if (q.state == QuestState.CanClaim)
                {
                    sb.Append("<color=#69F0AE><b>[Đã xong]</b> Hãy quay về gặp NPC để nhận thưởng!</color>");
                }
                else
                {
                    sb.Append($"<color=#B0BEC5>Tiến độ:</color> <color=#FFEB3B><b>{q.currentAmount}/{q.targetAmount}</b></color>");
                    if (!string.IsNullOrEmpty(target))
                    {
                        sb.Append($" <color=#90CAF9>({target})</color>");
                    }
                }
            }
            missionDayText.text = sb.ToString();
        }
    }

    // Cập nhật tiến độ khi nhặt/câu được cá
    public void AddProgressByItem(string itemName, int amount = 1)
    {
        foreach (Quest q in questList)
        {
            if (q.state == QuestState.InProgress)
            {
                bool isMatch = (!string.IsNullOrEmpty(q.targetItem) && q.targetItem.Equals(itemName, System.StringComparison.OrdinalIgnoreCase))
                            || (!string.IsNullOrEmpty(q.description) && q.description.Contains(itemName));

                if (isMatch)
                {
                    q.currentAmount += amount;
                    if (q.currentAmount >= q.targetAmount)
                    {
                        q.currentAmount = q.targetAmount;
                        q.state = QuestState.CanClaim;
                    }
                }
            }
        }
        RenderQuestList();
        UpdateMissionDayHUD();
        SaveQuestData();
    }

    // Bấm nút Nhận thưởng
    public void ClaimReward(string questId)
    {
        Quest q = questList.Find(x => x.id == questId);
        if (q != null && q.state == QuestState.CanClaim)
        {
            q.state = QuestState.Claimed;

            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.CongTien(q.goldReward);
            }

            RenderQuestList();
            UpdateMissionDayHUD();
            SaveQuestData();
        }
    }

    // Hàm tiện ích tra cứu từ điển (hỗ trợ cả Game Text và NPC Text)
    public string GetLocalizedText(string keyOrText)
    {
        if (string.IsNullOrEmpty(keyOrText)) return "";

        try
        {
            var gameTable = LocalizationSettings.StringDatabase.GetTable("Game Text");
            if (gameTable != null)
            {
                var entry = gameTable.GetEntry(keyOrText);
                if (entry != null) return entry.GetLocalizedString();
            }

            var npcTable = LocalizationSettings.StringDatabase.GetTable("NPC Text");
            if (npcTable != null)
            {
                var entry = npcTable.GetEntry(keyOrText);
                if (entry != null) return entry.GetLocalizedString();
            }

            return keyOrText;
        }
        catch
        {
            return keyOrText;
        }
    }
}