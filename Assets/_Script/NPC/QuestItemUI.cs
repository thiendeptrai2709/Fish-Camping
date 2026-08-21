using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings; // Thêm thư viện Localization

public class QuestItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text progressText;
    public TMP_Text rewardText;
    public Button actionButton;
    public TMP_Text buttonText;

    private Quest currentQuest;

    public void Setup(Quest quest)
    {
        currentQuest = quest;

        // 1. Tự động tra từ điển cho Tên và Mô tả nhiệm vụ
        if (titleText != null)
            titleText.text = GetLocalizedText(quest.title);

        if (descriptionText != null)
            descriptionText.text = GetLocalizedText(quest.description);

        if (rewardText != null)
            rewardText.text = $"+{quest.goldReward}G";

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (currentQuest == null) return;

        if (progressText != null)
        {
            progressText.text = $"{currentQuest.currentAmount}/{currentQuest.targetAmount}";
        }

        if (actionButton == null) return;

        actionButton.onClick.RemoveAllListeners();

        // 2. Kiểm tra ngôn ngữ hiện tại để dịch chữ trên Nút Bấm
        bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        switch (currentQuest.state)
        {
            case QuestState.NotStarted:
                if (buttonText != null)
                    buttonText.text = isVietnamese ? "Nhận" : "Accept";
                actionButton.interactable = true;
                actionButton.onClick.AddListener(OnAcceptClick);
                break;

            case QuestState.InProgress:
                if (buttonText != null)
                    buttonText.text = isVietnamese ? "Đang làm" : "In Progress";
                actionButton.interactable = false;
                break;

            case QuestState.CanClaim:
                if (buttonText != null)
                    buttonText.text = isVietnamese ? "Gặp NPC" : "Return to NPC";
                actionButton.interactable = false; // Bắt buộc phải quay lại gặp NPC giao việc để nhận thưởng
                break;

            case QuestState.Claimed:
                if (buttonText != null)
                    buttonText.text = isVietnamese ? "Đã xong" : "Completed";
                actionButton.interactable = false;
                break;
        }
    }

    private void OnAcceptClick()
    {
        QuestManager.Instance.AddQuestToList(currentQuest);
        RefreshUI();
    }

    private void OnClaimClick()
    {
        QuestManager.Instance.ClaimReward(currentQuest.id);
        RefreshUI();
    }

    // Hàm phụ trợ tra từ điển: Dùng trực tiếp hàm tra của QuestManager
    private string GetLocalizedText(string keyOrText)
    {
        if (QuestManager.Instance != null)
        {
            return QuestManager.Instance.GetLocalizedText(keyOrText);
        }

        if (string.IsNullOrEmpty(keyOrText)) return "";

        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable("Game Text");
            if (table != null)
            {
                var entry = table.GetEntry(keyOrText);
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