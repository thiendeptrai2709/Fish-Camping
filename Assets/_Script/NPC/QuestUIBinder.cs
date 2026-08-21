using UnityEngine;
using TMPro;

public class QuestUIBinder : MonoBehaviour
{
    [Header("Kéo UI của Scene này vào đây")]
    public GameObject questPanel;
    public Transform questContentParent;

    [Header("Mission Day HUD (Tùy chọn)")]
    public GameObject missionDayPanel;
    public TextMeshProUGUI missionDayText;

    private void Start()
    {
        // Vì QuestCanvas luôn BẬT nên hàm Start() chắc chắn sẽ chạy ngay khi Load Scene mới!
        RegisterToManager();
    }

    public void RegisterToManager()
    {
        if (QuestManager.Instance != null)
        {
            if (questPanel != null && questContentParent != null)
            {
                QuestManager.Instance.RegisterUI(questPanel, questContentParent);
            }
            if (missionDayPanel != null || missionDayText != null)
            {
                QuestManager.Instance.RegisterMissionDayHUD(missionDayPanel, missionDayText);
            }
        }
    }
}