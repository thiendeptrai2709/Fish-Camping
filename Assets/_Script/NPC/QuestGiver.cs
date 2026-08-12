using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestData
{
    [Header("Quest Identifier")]
    public string questId = "QUEST_01"; // ID riêng cho từng nhiệm vụ (không được trùng)

    [Header("Quest Info")]
    public string questName = "Tên Nhiệm Vụ";
    public string questDescription = "Mô tả nhiệm vụ...";
    public string targetItem = "Largemouth Bass";
    public int targetAmount = 1;
    public int rewardGold = 150;
}

public class QuestGiver : MonoBehaviour
{
    [Header("Danh Sách Tất Cả Nhiệm Vụ NPC Sẽ Giao")]
    public List<QuestData> questList = new List<QuestData>();

    [Header("Quest Dialogues")]
    [SerializeField]
    [TextArea(2, 5)]
    private string[] offerDialogues = new string[] {
        "Chào cậu! Ta có một vài việc cần cậu giúp đỡ đây.",
        "Hãy kiểm tra bảng nhiệm vụ (phím J) để xem chi tiết danh sách nhé!"
    };

    [SerializeField]
    [TextArea(2, 5)]
    private string[] progressDialogues = new string[] {
        "Cậu vẫn đang làm các nhiệm vụ ta giao đúng không?",
        "Cố gắng hoàn thành sớm nhé!"
    };

    [SerializeField]
    [TextArea(2, 5)]
    private string[] completeDialogues = new string[] {
        "Tuyệt vời! Cảm ơn cậu đã hoàn thành các công việc!"
    };

    [SerializeField]
    [TextArea(2, 5)]
    private string[] finalDialogues = new string[] {
        "Cảm ơn cậu nhé! Chúc cậu một ngày vui vẻ."
    };

    public void HandleQuestInteraction(string npcName, Animator animator, System.Action onComplete)
    {
        if (questList == null || questList.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Kiểm tra trạng thái chung của tất cả nhiệm vụ trong danh sách
        bool hasNotStarted = false;
        bool hasCanClaim = false;
        bool allClaimed = true;

        foreach (var qData in questList)
        {
            QuestState state = QuestManager.Instance != null
                ? QuestManager.Instance.GetQuestState(qData.questId)
                : QuestState.NotStarted;

            if (state == QuestState.NotStarted) hasNotStarted = true;
            if (state == QuestState.CanClaim) hasCanClaim = true;
            if (state != QuestState.Claimed) allClaimed = false;
        }

        // 1. Nếu còn nhiệm vụ chưa nhận -> Nói chuyện xong sẽ NHẬN HẾT TẤT CẢ NV cùng lúc
        if (hasNotStarted)
        {
            DialogueManager.Instance.StartDialogue(npcName, offerDialogues, () => {
                if (QuestManager.Instance != null)
                {
                    foreach (var qData in questList)
                    {
                        if (QuestManager.Instance.GetQuestState(qData.questId) == QuestState.NotStarted)
                        {
                            Quest newQuest = new Quest
                            {
                                id = qData.questId,
                                title = qData.questName,
                                description = qData.questDescription,
                                targetItem = qData.targetItem,
                                currentAmount = 0,
                                targetAmount = qData.targetAmount,
                                goldReward = qData.rewardGold,
                                state = QuestState.InProgress
                            };
                            QuestManager.Instance.AddQuestToList(newQuest);
                        }
                    }
                }
                onComplete?.Invoke();
            });
        }
        // 2. Nếu đã hoàn thành và nhận thưởng hết tất cả nhiệm vụ
        else if (allClaimed)
        {
            DialogueManager.Instance.StartDialogue(npcName, finalDialogues, onComplete);
        }
        // 3. Nếu có nhiệm vụ đã đủ điều kiện trả thưởng
        else if (hasCanClaim)
        {
            DialogueManager.Instance.StartDialogue(npcName, completeDialogues, () => {
                if (QuestManager.Instance != null)
                {
                    foreach (var qData in questList)
                    {
                        if (QuestManager.Instance.GetQuestState(qData.questId) == QuestState.CanClaim)
                        {
                            QuestManager.Instance.ClaimReward(qData.questId);
                        }
                    }
                }
                onComplete?.Invoke();
            });
        }
        // 4. Đang trong quá trình làm
        else
        {
            DialogueManager.Instance.StartDialogue(npcName, progressDialogues, onComplete);
        }
    }
}