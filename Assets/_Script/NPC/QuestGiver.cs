using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestData
{
    [Header("--- THÔNG TIN NHIỆM VỤ ---")]
    public string questId = "Quest1";              // ID riêng (Quest1, Quest2...)
    public string questName = "NV1";                // Tên hiển thị (NV1, NV2...)
    [TextArea(2, 3)]
    public string questDescription = "Mô tả nhiệm vụ...";
    public string targetItem = "Cá Hồ Cam Đốm";     // Tên cá / vật phẩm
    public int targetAmount = 1;                    // Số lượng
    public int rewardGold = 1500;                   // Tiền thưởng

    [Header("--- HỘI THOẠI RIÊNG CHO NHIỆM VỤ NÀY ---")]
    [TextArea(2, 4)] public string[] offerDialogues;    // Thoại khi NPC bắt đầu giao NV này
    [TextArea(2, 4)] public string[] progressDialogues; // Thoại khi người chơi đang làm NV này
    [TextArea(2, 4)] public string[] completeDialogues; // Thoại khi hoàn thành / trả thưởng NV này
}

public class QuestGiver : MonoBehaviour
{
    [Header("Danh Sách Nhiệm Vụ Nối Tiếp (NV1 -> NV2 -> ...)")]
    public List<QuestData> questList = new List<QuestData>();

    [Header("Thoại khi ĐÃ HOÀN THÀNH HẾT TẤT CẢ Nhiệm vụ")]
    [SerializeField]
    [TextArea(2, 5)]
    private string[] finalDialogues = new string[] {
        "Cảm ơn cậu nhé! Cậu đã giúp ta làm xong tất cả công việc rồi."
    };

    public void HandleQuestInteraction(string npcName, Animator animator, System.Action onComplete)
    {
        // 1. SỬA LỖI UI: Gọi hàm ClosePanel() để tự động tìm lại UI và ẩn chắc chắn 100%
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ClosePanel();
        }

        if (questList == null || questList.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // 2. TÌM NHIỆM VỤ ĐẦU TIÊN CHƯA HOÀN THÀNH (Chưa Claimed)
        QuestData currentQuestData = null;
        QuestState currentState = QuestState.NotStarted;

        foreach (var qData in questList)
        {
            QuestState state = QuestManager.Instance != null
                ? QuestManager.Instance.GetQuestState(qData.questId)
                : QuestState.NotStarted;

            if (state != QuestState.Claimed)
            {
                currentQuestData = qData;
                currentState = state;
                break; // Dừng lại ở nhiệm vụ chưa xong đầu tiên
            }
        }

        // 3. Nếu tất cả NV trong danh sách đều đã làm xong (Claimed)
        if (currentQuestData == null)
        {
            DialogueManager.Instance.StartDialogue(npcName, finalDialogues, onComplete);
            return;
        }

        // 4. XỬ LÝ TRẠNG THÁI NHIỆM VỤ HIỆN TẠI
        switch (currentState)
        {
            case QuestState.NotStarted:
                // Chưa nhận -> Nói chuyện nhận NV hiện tại
                string[] offerLines = (currentQuestData.offerDialogues != null && currentQuestData.offerDialogues.Length > 0)
                    ? currentQuestData.offerDialogues
                    : new string[] { $"Ta có việc nhờ cậu: {currentQuestData.questDescription}" };

                DialogueManager.Instance.StartDialogue(npcName, offerLines, () => {
                    if (QuestManager.Instance != null)
                    {
                        Quest newQuest = new Quest
                        {
                            id = currentQuestData.questId,
                            title = currentQuestData.questName,
                            description = currentQuestData.questDescription,
                            targetItem = currentQuestData.targetItem,
                            currentAmount = 0,
                            targetAmount = currentQuestData.targetAmount,
                            goldReward = currentQuestData.rewardGold,
                            state = QuestState.InProgress
                        };
                        QuestManager.Instance.AddQuestToList(newQuest);
                    }
                    onComplete?.Invoke();
                });
                break;

            case QuestState.InProgress:
                // Đang làm -> Nhắc nhở tiến độ
                string[] progressLines = (currentQuestData.progressDialogues != null && currentQuestData.progressDialogues.Length > 0)
                    ? currentQuestData.progressDialogues
                    : new string[] { "Cậu vẫn đang làm nhiệm vụ đúng không? Cố gắng lên nhé!" };

                DialogueManager.Instance.StartDialogue(npcName, progressLines, onComplete);
                break;

            case QuestState.CanClaim:
                // Đã đủ điều kiện -> Trả thưởng & chuyển sang Claimed (Mở khóa NV tiếp theo)
                string[] completeLines = (currentQuestData.completeDialogues != null && currentQuestData.completeDialogues.Length > 0)
                    ? currentQuestData.completeDialogues
                    : new string[] { "Tuyệt vời! Cảm ơn cậu đã hoàn thành công việc!" };

                DialogueManager.Instance.StartDialogue(npcName, completeLines, () => {
                    if (QuestManager.Instance != null)
                    {
                        QuestManager.Instance.ClaimReward(currentQuestData.questId);
                    }
                    onComplete?.Invoke();
                });
                break;
        }
    }
}