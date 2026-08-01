using UnityEngine;

public class NPCOffroadUpgrade : MonoBehaviour
{
    [Header("Dialogue Config")]
    [SerializeField]
    [TextArea(2, 5)]
    private string[] welcomeDialogues = new string[] {
        "Chào cậu! Chiếc xe tải này nhìn rỉ sét quá nhỉ?",
        "Nếu cậu muốn đi vào khu vực Đầm Lầy Sương Mù, lốp thường sẽ bị trơn trượt đấy.",
        "Để ta xem xem có thể nâng cấp bộ lốp gai off-road nào cho cậu không!"
    };

    public void HandleUpgradeInteraction(string npcName, System.Action onComplete)
    {
        if (DialogueManager.Instance != null)
        {
            // 1. Chạy thoại chào hỏi
            DialogueManager.Instance.StartDialogue(npcName, welcomeDialogues, () => {

                // 2. Thoại xong mới mở Garage UI
                if (GarageUIManager.Instance != null)
                {
                    GarageUIManager.Instance.OpenGarage(() => {
                        onComplete?.Invoke(); // Tắt Garage -> Báo cho NPCBase trả về Idle
                    });
                }
                else
                {
                    Debug.LogError("[NPCOffroadUpgrade] Thiếu GarageUIManager.Instance trong Scene!");
                    onComplete?.Invoke();
                }
            });
        }
        else
        {
            Debug.LogError("[NPCOffroadUpgrade] Thiếu DialogueManager.Instance trong Scene!");
            onComplete?.Invoke();
        }
    }
}