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
    public GameObject questPanel;       // Kéo QuestPanel từ Hierarchy vào đây

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ QuestManager không bị xóa khi đổi Scene / Map
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Nhấn phím Z để Bật / Tắt Panel Nhiệm vụ
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleQuestPanel();
        }
    }

    // Hàm ẩn/hiện Panel UI
    public void ToggleQuestPanel()
    {
        if (questPanel != null)
        {
            bool isActive = questPanel.activeSelf;
            questPanel.SetActive(!isActive);

            // Mở Panel lên thì cập nhật lại danh sách nhiệm vụ mới nhất
            if (!isActive)
            {
                RenderQuestList();
            }
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

    // Chuyển trạng thái khi xong nhiệm vụ
    public void CompleteQuest(string questId)
    {
        Quest q = questList.Find(x => x.id == questId);
        if (q != null)
        {
            q.state = QuestState.Claimed;
            RenderQuestList();
        }
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
    public void RegisterUI(GameObject panel, Transform contentParent)
    {
        questPanel = panel;
        questContentParent = contentParent;
        RenderQuestList(); // Tự động vẽ lại danh sách nhiệm vụ ngay khi kết nối UI mới
    }

    // Cập nhật tiến độ khi cất cá/vật phẩm vào Balo
    public void AddProgressByItem(string itemName, int amount = 1)
    {
        foreach (Quest q in questList)
        {
            if (q.state == QuestState.InProgress)
            {
                // Kiểm tra khớp tên targetItem hoặc khớp từ khóa trong description
                bool isMatch = (!string.IsNullOrEmpty(q.targetItem) && q.targetItem.Equals(itemName, System.StringComparison.OrdinalIgnoreCase))
                            || (!string.IsNullOrEmpty(q.description) && q.description.Contains(itemName));

                if (isMatch)
                {
                    q.currentAmount += amount;
                    if (q.currentAmount >= q.targetAmount)
                    {
                        q.currentAmount = q.targetAmount;
                        q.state = QuestState.CanClaim; // Tự động chuyển nút thành "Nhận thưởng"
                    }
                }
            }
        }
        RenderQuestList();
    }

    // Bấm nút Nhận thưởng -> Lập tức cộng tiền qua MoneyManager
    public void ClaimReward(string questId)
    {
        Quest q = questList.Find(x => x.id == questId);
        if (q != null && q.state == QuestState.CanClaim)
        {
            q.state = QuestState.Claimed;

            // Gọi MoneyManager chạy hiệu ứng cộng tiền lên UI góc phải
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.CongTien(q.goldReward);
            }

            RenderQuestList();
        }
    }
}