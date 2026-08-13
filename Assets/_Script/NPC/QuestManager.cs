using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Data")]
    public List<Quest> questList = new List<Quest>();

    [Header("UI References")]
    public Transform questContentParent; // Kéo ô Content trong ScrollView vào đây
    public GameObject questItemPrefab;   // Kéo Prefab ô Nhiệm vụ vào đây
    public GameObject questPanel;        // Kéo QuestPanel từ Hierarchy vào đây

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ QuestManager không bị xóa khi đổi Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Nhấn phím V để Bật / Tắt Panel Nhiệm vụ
        if (Input.GetKeyDown(KeyCode.V))
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

        if (questPanel == null) return;

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

            // --- BẬT CON TRỎ CHUỘT ĐỂ CLICK NHẬN THƯỞNG ---
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // TẮT BẢNG NHIỆM VỤ
            questPanel.SetActive(false);

            // --- KHÓA LẠI CON TRỎ CHUỘT ĐỂ ĐIỀU KHIỂN NHÂN VẬT ---
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Kiểm tra chính xác xem khung thoại NPC có đang hiện trên màn hình không
    private bool IsDialogueShowing()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.dialogueCanvas != null)
        {
            // Chỉ chặn phím V khi cái Khung Thoại (dialogueCanvas) thực sự đang BẬT
            return DialogueManager.Instance.dialogueCanvas.activeInHierarchy;
        }
        return false;
    }


    // Tự động nối lại UI khi chuyển Scene
    private void EnsureUIAttached()
    {
        if (questPanel == null)
        {
            QuestUIBinder binder = FindFirstObjectByType<QuestUIBinder>();
            if (binder != null)
            {
                binder.RegisterToManager();
            }
        }
    }

    // Đăng ký UI từ QuestUIBinder
    public void RegisterUI(GameObject panel, Transform contentParent)
    {
        questPanel = panel;
        questContentParent = contentParent;
        RenderQuestList();
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
        }
        RenderQuestList();
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
        }
    }
}