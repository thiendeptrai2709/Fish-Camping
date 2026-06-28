using UnityEngine;

public class NPCBase : MonoBehaviour, IInteractable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Dân Làng";
    [SerializeField] private string promptMessage = "Nói chuyện";

    [Header("Dialogues")]
    [SerializeField]
    [TextArea(2, 5)]
    private string[] introDialogues = new string[] {
        "Chào cậu, hôm nay thời tiết ở Hồ Thông thật đẹp!",
        "Cậu có câu được con cá Vược nào lớn không?"
    };

    // Thực hiện thuộc tính từ Interface để trả về nội dung hiển thị trên UI
    public string InteractionPrompt => $"[{npcName}] \n Nhấn E để {promptMessage}";

    // Hàm này sẽ được gọi khi Player đứng gần và bấm nút E
    public void Interact()
    {
        Debug.Log($"Đang tương tác với NPC: {npcName}");

        // MVP Bước đầu: In hội thoại ra Console để kiểm tra logic trước khi làm UI vẽ chữ
        foreach (string line in introDialogues)
        {
            Debug.Log($"{npcName}: {line}");
        }

        // Kế hoạch tiếp theo: Ở đây sẽ gọi DialogueManager.Instance.StartDialogue(...)
    }
}