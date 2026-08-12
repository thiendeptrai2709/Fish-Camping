using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text progressText;
    public Button actionButton;
    public TMP_Text buttonText;

    private Quest currentQuest;

    public void Setup(Quest quest)
    {
        currentQuest = quest;

        if (titleText != null) titleText.text = quest.title;
        if (descriptionText != null) descriptionText.text = quest.description;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (progressText != null)
        {
            progressText.text = $"{currentQuest.currentAmount}/{currentQuest.targetAmount}";
        }

        if (actionButton == null) return;

        actionButton.onClick.RemoveAllListeners();

        switch (currentQuest.state)
        {
            case QuestState.NotStarted:
                if (buttonText != null) buttonText.text = "Nhận";
                actionButton.interactable = true;
                actionButton.onClick.AddListener(OnAcceptClick);
                break;

            case QuestState.InProgress:
                if (buttonText != null) buttonText.text = "Đang làm";
                actionButton.interactable = false;
                break;

            case QuestState.CanClaim:
                if (buttonText != null) buttonText.text = "Nhận thưởng";
                actionButton.interactable = true;
                actionButton.onClick.AddListener(OnClaimClick);
                break;

            case QuestState.Claimed:
                if (buttonText != null) buttonText.text = "Đã xong";
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
}