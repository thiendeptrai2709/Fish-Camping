using UnityEngine;

public enum QuestState
{
    NotStarted,  // Chưa nhận
    InProgress,  // Đang làm
    Completed    // Đã xong hoàn toàn
}

public class QuestGiver : MonoBehaviour
{
    [Header("Quest Info")]
    public string questName = "Săn Tìm Cá Vược Hồ Thông";
    public string targetItem = "Largemouth Bass"; // Tên loài cá cần kiểm tra trong hòm đồ
    public int rewardGold = 150;                  // Phần thưởng xu theo GDD

    [Header("Quest Dialogues")]
    [SerializeField]
    [TextArea(2, 5)]
    private string[] offerDialogues = new string[] {
        "Chào cậu lữ khách! Ta nghe nói ông của cậu để lại một cuốn nhật ký cá rất giá trị.",
        "Ta đang rất thèm một bữa cá Vược nướng bên Hồ Thông, cậu có thể giúp ta câu một con [Largemouth Bass] không?",
        "Ta sẽ thưởng cho cậu 150G để nâng cấp chiếc xe tải cũ đó!"
    };

    [SerializeField]
    [TextArea(2, 5)]
    private string[] progressDialogues = new string[] {
        "Cậu vẫn chưa câu được [Largemouth Bass] sao?",
        "Hãy kiểm tra lại mồi bột hoặc thử câu vào lúc Bình minh/Hoàng hôn xem nhé!"
    };

    [SerializeField]
    [TextArea(2, 5)]
    private string[] completeDialogues = new string[] {
        "Ôi! Con cá Vược này tươi ngon quá! Đúng là tay nghề dòng họ cậu.",
        "Cầm lấy 150G này như đã hứa. Hãy tiếp tục hành trình điền kín cuốn nhật ký nhé!"
    };

    [SerializeField]
    [TextArea(2, 5)]
    private string[] finalDialogues = new string[] {
        "Cảm ơn cậu vì con cá nhé! Chúc cậu một ngày đi câu vui vẻ."
    };

    // Trạng thái hiện tại của nhiệm vụ đối với NPC này
    private QuestState _currentState = QuestState.NotStarted;

    public void HandleQuestInteraction(string npcName, Animator animator, System.Action onComplete)
    {
        string[] selectedDialogues;

        switch (_currentState)
        {
            case QuestState.NotStarted:
                selectedDialogues = offerDialogues;
                // Nhấp nhận nói chuyện xong thì chuyển sang trạng thái Đang làm luôn
                DialogueManager.Instance.StartDialogue(npcName, selectedDialogues, () => {
                    _currentState = QuestState.InProgress;
                    Debug.Log($"Đã nhận nhiệm vụ: {questName}");
                    onComplete?.Invoke();
                });
                break;

            case QuestState.InProgress:
                // Giả lập check hòm đồ: ở đây tạm thời dùng phím check giả lập, 
                // hoặc bạn có hệ thống Inventory thì thay bằng Inventory.Contains(targetItem)
                bool hasFish = CheckPlayerInventoryMock();

                if (hasFish)
                {
                    selectedDialogues = completeDialogues;
                    DialogueManager.Instance.StartDialogue(npcName, selectedDialogues, () => {
                        _currentState = QuestState.Completed;
                        GiveReward();
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    selectedDialogues = progressDialogues;
                    DialogueManager.Instance.StartDialogue(npcName, selectedDialogues, onComplete);
                }
                break;

            case QuestState.Completed:
                selectedDialogues = finalDialogues;
                DialogueManager.Instance.StartDialogue(npcName, selectedDialogues, onComplete);
                break;
        }
    }

    private bool CheckPlayerInventoryMock()
    {
        // TẠM THỜI: Để test nhanh, nếu người chơi nhấn phím I trước khi nói chuyện thì coi như có cá
        // Sau này bạn thay bằng logic kết nối với Inventory thực tế của Player nhé
        return Input.GetKey(KeyCode.I);
    }

    private void GiveReward()
    {
        Debug.Log($"Nhiệm vụ hoàn thành! Cộng {rewardGold}G cho người chơi.");
        // Sau này gọi thêm: PlayerWallet.AddGold(rewardGold);
    }
}