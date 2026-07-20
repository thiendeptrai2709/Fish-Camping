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
        // 1. Chạy hội thoại chào hỏi trước
        DialogueManager.Instance.StartDialogue(npcName, welcomeDialogues, () => {

            // 2. Hội thoại kết thúc thì mở giao diện Gara
            GarageUIManager.Instance.OpenGarage(() => {

                // 3. Khi người chơi tắt giao diện Gara, hoàn tất tương tác đưa NPC về Idle
                onComplete?.Invoke();
            });
        });
    }
}