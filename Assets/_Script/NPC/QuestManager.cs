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
            GameDatabaseManager.Instance?.SaveAndSyncToCloud();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestManager] Lỗi lưu dữ liệu nhiệm vụ: {e.Message}");
        }
    }

    public bool HasUnlockedQuestSystem()
    {
        if (PlayerPrefs.GetInt("QuestSystem_Unlocked", 0) == 1) return true;
        if (questList != null && questList.Exists(q => !q.isDaily)) return true;
        return false;
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
                        container.quests.RemoveAll(q => q.id == "Quest1" || q.id == "Quest2");
                        questList = container.quests;
                    }
                }
            }

            // Nếu chưa được NPC giao nhiệm vụ cốt truyện, xóa bỏ daily quest tạm
            if (!HasUnlockedQuestSystem())
            {
                questList.RemoveAll(q => q.isDaily);
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
        if (HasUnlockedQuestSystem())
        {
            GenerateDailyQuestsIfNeed();
            UpdateMissionDayHUD();
        }
        else
        {
            if (missionDayPanel != null) missionDayPanel.SetActive(false);
            if (questPanel != null) questPanel.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        questPanel = null;
        questContentParent = null;
        missionDayPanel = null;
        missionDayText = null;

        EnsureUIAttached();
        if (HasUnlockedQuestSystem())
        {
            GenerateDailyQuestsIfNeed();
            UpdateMissionDayHUD();
        }
        else
        {
            if (missionDayPanel != null) missionDayPanel.SetActive(false);
            if (questPanel != null) questPanel.SetActive(false);
        }
    }

    // Tự động tạo và làm mới 3 nhiệm vụ Hàng Ngày (chỉ khi đã được NPC_Nvu giao nhiệm vụ)
    public void GenerateDailyQuestsIfNeed()
    {
        if (!HasUnlockedQuestSystem())
        {
            questList.RemoveAll(q => q.isDaily);
            return;
        }

        string lastDate = PlayerPrefs.GetString("Last_Daily_Quest_Date", "");
        string todayDate = System.DateTime.Now.ToString("yyyy-MM-dd");

        bool hasDaily = questList.Exists(q => q.isDaily && q.state != QuestState.Claimed);

        if (!hasDaily || lastDate != todayDate)
        {
            PlayerPrefs.SetString("Last_Daily_Quest_Date", todayDate);

            // Dọn dẹp các daily cũ đã hoàn thành
            questList.RemoveAll(q => q.isDaily && q.state == QuestState.Claimed);

            // Tạo mới bộ 3 nhiệm vụ hàng ngày nếu chưa có
            if (!questList.Exists(q => q.isDaily))
            {
                questList.Add(new Quest
                {
                    id = "Daily_Fish_" + todayDate,
                    title = "Câu Cá Hàng Ngày",
                    description = "Câu 3 con cá bất kỳ ở hồ nước gần nhất.",
                    targetItem = "Cá Bất Kỳ",
                    questType = QuestType.CatchFish,
                    isDaily = true,
                    currentAmount = 0,
                    targetAmount = 3,
                    goldReward = 300,
                    state = QuestState.InProgress
                });

                questList.Add(new Quest
                {
                    id = "Daily_Cook_" + todayDate,
                    title = "Bữa Ăn Dã Ngoại",
                    description = "Nướng chín 1 đĩa cá tại bếp dã ngoại bên bờ hồ.",
                    targetItem = "Cá Nướng",
                    questType = QuestType.CookFish,
                    isDaily = true,
                    currentAmount = 0,
                    targetAmount = 1,
                    goldReward = 300,
                    state = QuestState.InProgress
                });

                questList.Add(new Quest
                {
                    id = "Daily_Refuel_" + todayDate,
                    title = "Bảo Trì Xe Hàng Ngày",
                    description = "Nạp đầy bình xăng tại Trạm xăng hoặc nâng cấp lốp xe.",
                    targetItem = "Trạm Xăng / Gara",
                    questType = QuestType.Refuel,
                    isDaily = true,
                    currentAmount = 0,
                    targetAmount = 1,
                    goldReward = 400,
                    state = QuestState.InProgress
                });

                SaveQuestData();
            }
        }
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
        // Nhấn phím V để Bật / Tắt Panel Nhiệm vụ (Chỉ cho phép khi đã được NPC giao nhiệm vụ)
        bool vPressed = (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.V);
        if (vPressed)
        {
            if (HasUnlockedQuestSystem())
            {
                ToggleQuestPanel();
            }
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
        if (questPanel != null) questPanel.SetActive(false);
        if (HasUnlockedQuestSystem())
        {
            RenderQuestList();
        }
    }

    public void RegisterMissionDayHUD(GameObject panel, TextMeshProUGUI text)
    {
        missionDayPanel = panel;
        missionDayText = text;
        if (!HasUnlockedQuestSystem())
        {
            if (missionDayPanel != null) missionDayPanel.SetActive(false);
        }
        else
        {
            UpdateMissionDayHUD();
        }
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
        // Đánh dấu đã mở khóa bảng nhiệm vụ khi được NPC_Nvu giao nhiệm vụ
        PlayerPrefs.SetInt("QuestSystem_Unlocked", 1);
        PlayerPrefs.Save();

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
            q.questType = newQuest.questType;
            q.requiredGrade = newQuest.requiredGrade;
            q.requiredMinSize = newQuest.requiredMinSize;
        }

        GenerateDailyQuestsIfNeed();
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

        // Chỉ hiển thị khi đã được NPC_Nvu mở khóa / giao nhiệm vụ
        if (!HasUnlockedQuestSystem())
        {
            missionDayPanel.SetActive(false);
            return;
        }

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
            bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                                LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            List<Quest> storyQuests = activeQuests.FindAll(q => !q.isDaily);
            List<Quest> dailyQuests = activeQuests.FindAll(q => q.isDaily);

            // 1. Hiển thị Nhiệm Vụ Cốt Truyện
            for (int i = 0; i < storyQuests.Count; i++)
            {
                var q = storyQuests[i];
                if (sb.Length > 0) sb.Append("\n<color=#546E7A>──────────────</color>\n");

                string title = GetLocalizedText(q.title);
                string desc = GetLocalizedText(q.description);

                sb.Append($"<b><color=#FFD54F>📜 {title}</color></b>\n");
                if (!string.IsNullOrEmpty(desc))
                {
                    sb.Append($"<size=80%><color=#CFD8DC>{desc}</color></size>\n");
                }

                if (q.state == QuestState.CanClaim)
                {
                    sb.Append(isVietnamese 
                        ? "<color=#69F0AE><b>[Đã xong]</b> Gặp Cậu chủ làng nhận thưởng!</color>" 
                        : "<color=#69F0AE><b>[Ready]</b> Talk to Village Master to claim!</color>");
                }
                else
                {
                    string progressLabel = isVietnamese ? "Tiến độ:" : "Progress:";
                    sb.Append($"<color=#B0BEC5>{progressLabel}</color> <color=#FFEB3B><b>{q.currentAmount}/{q.targetAmount}</b></color>");
                }
            }

            // 2. Hiển thị Nhiệm Vụ Hàng Ngày
            if (dailyQuests.Count > 0)
            {
                if (sb.Length > 0) sb.Append("\n<color=#546E7A>──────────────</color>\n");
                sb.Append(isVietnamese 
                    ? "<b><color=#4DD0E1>⭐ NHIỆM VỤ HÀNG NGÀY</color></b>\n" 
                    : "<b><color=#4DD0E1>⭐ DAILY MISSIONS</color></b>\n");

                for (int i = 0; i < dailyQuests.Count; i++)
                {
                    var q = dailyQuests[i];
                    string title = GetLocalizedText(q.title);

                    if (q.state == QuestState.CanClaim)
                    {
                        sb.Append(isVietnamese 
                            ? $"• <color=#E0E0E0>{title}</color>: <color=#69F0AE><b>[Xong - Nhấn V nhận {q.goldReward}G]</b></color>\n" 
                            : $"• <color=#E0E0E0>{title}</color>: <color=#69F0AE><b>[Ready - Press V for {q.goldReward}G]</b></color>\n");
                    }
                    else
                    {
                        sb.Append($"• <color=#E0E0E0>{title}</color>: <color=#FFEB3B><b>{q.currentAmount}/{q.targetAmount}</b></color>\n");
                    }
                }
            }

            missionDayText.text = sb.ToString().TrimEnd();
        }
    }

    // ==========================================
    // CÁC HÀM HOOK CẬP NHẬT TIẾN ĐỘ TỰ ĐỘNG
    // ==========================================

    // 1. Hook khi câu được cá
    public void NotifyFishCaught(FishSO fish, float length, float weight, FishGrade grade)
    {
        if (fish == null) return;
        string fishName = string.IsNullOrEmpty(fish.itemName) ? fish.name : fish.itemName;
        string sceneName = SceneManager.GetActiveScene().name;
        bool isChanged = false;

        foreach (Quest q in questList)
        {
            if (q.state != QuestState.InProgress) continue;

            bool matched = false;

            switch (q.questType)
            {
                case QuestType.CatchFish:
                    if (string.IsNullOrEmpty(q.targetItem) || q.targetItem.Equals("Cá Bất Kỳ", System.StringComparison.OrdinalIgnoreCase) || q.targetItem.Equals("Cá Tươi", System.StringComparison.OrdinalIgnoreCase))
                    {
                        matched = true;
                    }
                    else if (q.targetItem.Contains("Đầm Lầy") && (sceneName.Contains("Map3") || sceneName.Contains("Swamp") || (fish.mapName != null && fish.mapName.Contains("Đầm Lầy"))))
                    {
                        matched = true;
                    }
                    else if (q.targetItem.Contains("Biển") && (sceneName.Contains("Map4") || sceneName.Contains("Ocean") || (fish.mapName != null && fish.mapName.Contains("Biển"))))
                    {
                        matched = true;
                    }
                    else if (q.targetItem.Equals(fishName, System.StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrEmpty(q.description) && q.description.Contains(fishName)))
                    {
                        matched = true;
                    }
                    break;

                case QuestType.CatchGrade:
                    if ((int)grade >= q.requiredGrade)
                    {
                        matched = true;
                    }
                    break;

                case QuestType.CatchRecordSize:
                    if (length >= q.requiredMinSize)
                    {
                        matched = true;
                    }
                    break;

                case QuestType.GenericItem:
                    if (!string.IsNullOrEmpty(q.targetItem) && q.targetItem.Equals(fishName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        matched = true;
                    }
                    break;
            }

            if (matched)
            {
                q.currentAmount += 1;
                if (q.currentAmount >= q.targetAmount)
                {
                    q.currentAmount = q.targetAmount;
                    q.state = QuestState.CanClaim;
                }
                isChanged = true;
            }
        }

        if (isChanged)
        {
            RenderQuestList();
            UpdateMissionDayHUD();
            SaveQuestData();
        }
    }

    // 2. Hook khi nướng chín cá
    public void NotifyCookingFinished(ItemShapeSO cookedItem)
    {
        bool isChanged = false;
        foreach (Quest q in questList)
        {
            if (q.state != QuestState.InProgress) continue;

            if (q.questType == QuestType.CookFish || (!string.IsNullOrEmpty(q.targetItem) && q.targetItem.Contains("Nướng")) || (!string.IsNullOrEmpty(q.description) && q.description.Contains("Nướng")))
            {
                q.currentAmount += 1;
                if (q.currentAmount >= q.targetAmount)
                {
                    q.currentAmount = q.targetAmount;
                    q.state = QuestState.CanClaim;
                }
                isChanged = true;
            }
        }

        if (isChanged)
        {
            RenderQuestList();
            UpdateMissionDayHUD();
            SaveQuestData();
        }
    }

    // 3. Hook khi nâng cấp xe / lốp
    public void NotifyVehicleUpgraded()
    {
        bool isChanged = false;
        foreach (Quest q in questList)
        {
            if (q.state != QuestState.InProgress) continue;

            if (q.questType == QuestType.UpgradeCar || q.questType == QuestType.Refuel)
            {
                q.currentAmount += 1;
                if (q.currentAmount >= q.targetAmount)
                {
                    q.currentAmount = q.targetAmount;
                    q.state = QuestState.CanClaim;
                }
                isChanged = true;
            }
        }

        if (isChanged)
        {
            RenderQuestList();
            UpdateMissionDayHUD();
            SaveQuestData();
        }
    }

    // 4. Hook khi đổ xăng
    public void NotifyFuelRefilled()
    {
        bool isChanged = false;
        foreach (Quest q in questList)
        {
            if (q.state != QuestState.InProgress) continue;

            if (q.questType == QuestType.Refuel || q.questType == QuestType.UpgradeCar)
            {
                q.currentAmount += 1;
                if (q.currentAmount >= q.targetAmount)
                {
                    q.currentAmount = q.targetAmount;
                    q.state = QuestState.CanClaim;
                }
                isChanged = true;
            }
        }

        if (isChanged)
        {
            RenderQuestList();
            UpdateMissionDayHUD();
            SaveQuestData();
        }
    }

    // 5. Hook khi nhặt/câu đồ (tương thích ngược)
    public void AddProgressByItem(string itemName, int amount = 1)
    {
        bool isChanged = false;
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
                    isChanged = true;
                }
            }
        }

        if (isChanged)
        {
            RenderQuestList();
            UpdateMissionDayHUD();
            SaveQuestData();
        }
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

    // Hàm tiện ích tra cứu từ điển (hỗ trợ cả Game Text và NPC Text với từ điển dự phòng)
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
        }
        catch { }

        bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        if (isVietnamese) return keyOrText;

        // Smart fallback dictionary for English translations
        switch (keyOrText)
        {
            // Daily Quests
            case "Câu Cá Hàng Ngày": return "Daily Fishing";
            case "Câu 3 con cá bất kỳ ở hồ nước gần nhất.": return "Catch 3 fish of any kind in the nearest lake.";
            case "Bữa Ăn Dã Ngoại": return "Campfire Meal";
            case "Nướng chín 1 đĩa cá tại bếp dã ngoại bên bờ hồ.": return "Grill 1 fish on the campfire cooking rack.";
            case "Bảo Trì Xe Hàng Ngày": return "Daily Vehicle Maintenance";
            case "Nạp đầy bình xăng tại Trạm xăng hoặc nâng cấp lốp xe.": return "Refuel at the Gas Station or upgrade tires at the Garage.";
            case "Cá Bất Kỳ": return "Any Fish";
            case "Cá Nướng": return "Grilled Fish";
            case "Trạm Xăng / Gara": return "Gas Station / Garage";

            // Story Quests
            case "Bữa Tiệc Hồ Thông": return "Pine Lake Banquet";
            case "Lái xe đến Hồ Thông (Map 2), câu 2 con cá tươi mang về cho làng.": return "Drive to Pine Lake (Map 2), catch 2 fresh fish for the village.";
            case "Hương Vị Cá Nướng": return "Grilled Fish Delicacy";
            case "Nướng chín 1 đĩa Cá Nướng dã ngoại bên bếp lửa trại mang về cho Cậu chủ làng.": return "Grill 1 fish at a campfire and bring it to the Village Master.";
            case "Bảo Dưỡng Chuyến Đi Xa": return "Long Trip Vehicle Prep";
            case "Đến gặp Bác thợ máy nâng cấp lốp xe Gai Off-road hoặc đổ đầy bình xăng tại Trạm xăng.": return "Visit the Mechanic to upgrade Off-Road tires or fully refuel at the Gas Station.";
            case "Đặc Sản Đầm Lầy": return "Swamp Delicacies";
            case "Lái xe vượt địa hình vào Đầm Lầy (Map 3) và câu 2 con cá đầm lầy.": return "Drive off-road into the Swamp (Map 3) and catch 2 swamp fish.";
            case "Cá Vàng May Mắn": return "Lucky Golden Fish";
            case "Dùng Mồi câu xịn bắt được ít nhất 1 con cá đạt phẩm chất Vàng Kim (Gold Grade).": return "Use premium bait to catch at least 1 Gold Grade fish.";
            case "Chinh Phục Đại Dương": return "Conquer the Ocean";
            case "Trang bị Cần câu 5 hoặc 6 cùng Mồi biển, câu 2 con cá biển lớn tại Bờ Biển (Map 4).": return "Equip Rod 5/6 with Ocean Bait, catch 2 ocean fish at Coast (Map 4).";

            default: return keyOrText;
        }
    }
}