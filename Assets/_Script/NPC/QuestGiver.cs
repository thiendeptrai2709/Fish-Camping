using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings; // Thêm thư viện Localization

[System.Serializable]
public class QuestData
{
    [Header("--- THÔNG TIN NHIỆM VỤ ---")]
    public string questId = "Quest1";               // ID riêng (Quest1, Quest2...)
    public string questName = "NV1";                // Mã Key hoặc Tên hiển thị
    [TextArea(2, 3)]
    public string questDescription = "QP1";         // Mã Key hoặc Nội dung mô tả
    public string targetItem = "Cá Hồ Cam Đốm";     // Mã Key hoặc Tên cá/vật phẩm
    public int targetAmount = 1;                    // Số lượng
    public int rewardGold = 1500;                   // Tiền thưởng

    [Header("--- HỘI THOẠI & VOICE AI RIÊNG CHO NHIỆM VỤ NÀY ---")]
    [Tooltip("Thoại lúc đầu giao nhiệm vụ (chạy hết tất cả các câu)")]
    public DialogueLine[] offerDialogues;

    [Tooltip("Thoại nhắc nhở khi đang làm (chọn ngẫu nhiên 1 câu mỗi khi quay lại)")]
    public DialogueLine[] progressDialogues = new DialogueLine[] {
        new DialogueLine { text = "Cậu vẫn đang làm nhiệm vụ đúng không? Cố lên nhé!" },
        new DialogueLine { text = "Tiến độ tới đâu rồi? Nhớ mang đủ đồ về cho ta nhé!" }
    };

    [Tooltip("Thoại khen thưởng khi hoàn thành (chạy hết các câu trả thưởng)")]
    public DialogueLine[] completeDialogues;
}

public class QuestGiver : MonoBehaviour
{
    [Header("Danh Sách Nhiệm Vụ Nối Tiếp (NV1 -> NV2 -> ...)")]
    public List<QuestData> questList = new List<QuestData>();

    [Header("Thoại khi ĐÃ HOÀN THÀNH HẾT TẤT CẢ Nhiệm vụ (1 câu ngẫu nhiên)")]
    [SerializeField]
    private DialogueLine[] finalReturningDialogues = new DialogueLine[] {
        new DialogueLine { text = "Cảm ơn cậu nhé! Nhờ có cậu mà mọi việc êm xuôi rồi." },
        new DialogueLine { text = "Hôm nay thời tiết đẹp thật đấy, nghỉ ngơi chút đi cậu!" },
        new DialogueLine { text = "Dạo này khỏe chứ? Ta vẫn nhớ công sức cậu giúp ta đấy!" }
    };

    public void HandleQuestInteraction(string npcName, Animator animator, System.Action onComplete)
    {
        // 1. Tự động đóng Panel Quest nếu đang bật
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ClosePanel();
        }

        if (questList == null || questList.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // 2. Tìm nhiệm vụ đầu tiên chưa xong (chưa Claimed)
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
                break;
            }
        }

        // 3. Nếu ĐÃ HOÀN THÀNH TẤT CẢ nhiệm vụ -> Chọn ngẫu nhiên DUY NHẤT 1 câu chào ngắn
        if (currentQuestData == null)
        {
            DialogueLine[] singleFinalLine = GetRandomDialogueLine(finalReturningDialogues, "Cảm ơn cậu đã giúp đỡ ta!");
            DialogueManager.Instance.StartDialogueWithVoice(npcName, singleFinalLine, onComplete);
            return;
        }

        // 4. Xử lý các trạng thái của nhiệm vụ hiện tại
        switch (currentState)
        {
            case QuestState.NotStarted:
                // LẦN ĐẦU NHẬN NV: Chạy toàn bộ câu thoại giao việc
                DialogueLine[] offerLines = (currentQuestData.offerDialogues != null && currentQuestData.offerDialogues.Length > 0)
                    ? currentQuestData.offerDialogues
                    : new DialogueLine[] { new DialogueLine { text = $"Ta có việc nhờ cậu: {GetLocalizedText(currentQuestData.questDescription)}" } };

                DialogueManager.Instance.StartDialogueWithVoice(npcName, offerLines, () => {
                    if (QuestManager.Instance != null)
                    {
                        // Tự động dịch Tên, Mô tả và Tên cá trước khi đưa vào hệ thống Quest
                        Quest newQuest = new Quest
                        {
                            id = currentQuestData.questId,
                            title = GetLocalizedText(currentQuestData.questName),
                            description = GetLocalizedText(currentQuestData.questDescription),
                            targetItem = GetLocalizedText(currentQuestData.targetItem),
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
                // ĐANG LÀM DỞ QUAY LẠI: Chọn ngẫu nhiên DUY NHẤT 1 câu nhắc nhở ngắn
                DialogueLine[] singleProgressLine = GetRandomDialogueLine(currentQuestData.progressDialogues, "Cố gắng hoàn thành nhiệm vụ nhé!");
                DialogueManager.Instance.StartDialogueWithVoice(npcName, singleProgressLine, onComplete);
                break;

            case QuestState.CanClaim:
                // HOÀN THÀNH: Chạy toàn bộ câu trả thưởng và mở khóa nhiệm vụ tiếp theo
                DialogueLine[] completeLines = (currentQuestData.completeDialogues != null && currentQuestData.completeDialogues.Length > 0)
                    ? currentQuestData.completeDialogues
                    : new DialogueLine[] { new DialogueLine { text = "Tuyệt vời! Cảm ơn cậu đã hoàn thành công việc!" } };

                DialogueManager.Instance.StartDialogueWithVoice(npcName, completeLines, () => {
                    if (QuestManager.Instance != null)
                    {
                        QuestManager.Instance.ClaimReward(currentQuestData.questId);
                    }
                    onComplete?.Invoke();
                });
                break;
        }
    }

    // Hàm tiện ích bốc ngẫu nhiên 1 câu DialogueLine
    private DialogueLine[] GetRandomDialogueLine(DialogueLine[] sourceList, string defaultText)
    {
        if (sourceList != null && sourceList.Length > 0)
        {
            int randomIndex = Random.Range(0, sourceList.Length);
            return new DialogueLine[] { sourceList[randomIndex] };
        }
        return new DialogueLine[] { new DialogueLine { text = defaultText } };
    }

    // Hàm phụ trợ tra từ điển: Dò cả 2 bảng "Game Text" và "NPC Text"
    private string GetLocalizedText(string keyOrText)
    {
        if (string.IsNullOrEmpty(keyOrText)) return "";

        try
        {
            // 1. Tìm trong bảng "Game Text" trước
            var gameTable = LocalizationSettings.StringDatabase.GetTable("Game Text");
            if (gameTable != null)
            {
                var entry = gameTable.GetEntry(keyOrText);
                if (entry != null) return entry.GetLocalizedString();
            }

            // 2. Nếu không có, tìm tiếp trong bảng "NPC Text"
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