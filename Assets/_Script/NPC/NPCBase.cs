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

    [Header("Quest System Link")]
    [SerializeField] private QuestGiver _questGiver;

    private Animator _animator;
    private bool _isInteracting = false;

    private void Awake()
    {
        // Tự động lấy linh kiện Animator gắn trên cùng GameObject của NPC
        _animator = GetComponent<Animator>();

        if (_questGiver == null)
        {
            _questGiver = GetComponent<QuestGiver>();
        }
    }

    // Thực hiện thuộc tính từ Interface để trả về nội dung hiển thị trên UI
    public string InteractionPrompt => $"[{npcName}] \n Nhấn E để {promptMessage}";

    // Hàm này sẽ được gọi khi Player đứng gần và bấm nút E
    public void Interact()
    {
        if (_isInteracting) return; // Nếu đang trong cuộc nói chuyện thì không chạy lại logic từ đầu
        _isInteracting = true;

        Debug.Log($"Đang tương tác với NPC: {npcName}");

        // 1. Tự động xoay mặt NPC về hướng của Player
        RotateTowardsPlayer();

        // 2. Chuyển Animator sang trạng thái Nghe nói chuyện (NPCState = 2)
        if (_animator != null)
        {
            _animator.SetInteger("NPCState", 2);
        }

        // 3. Kiểm tra xem đây là NPC thường hay NPC có nhiệm vụ cốt truyện
        if (_questGiver != null)
        {
            // Chạy logic thoại và quản lý trạng thái của QuestGiver
            _questGiver.HandleQuestInteraction(npcName, _animator, () => {
                ResetNPCState();
            });
        }
        else
        {
            // Nếu không có nhiệm vụ, chạy thoại mặc định của NPCBase hiển thị lên UI Canvas
            DialogueManager.Instance.StartDialogue(npcName, introDialogues, () => {
                ResetNPCState();
            });
        }
    }

    private void ResetNPCState()
    {
        _isInteracting = false;
        if (_animator != null)
        {
            _animator.SetInteger("NPCState", 0); // Trả NPC về lại trạng thái Đứng im mặc định (NPCState = 0)
        }
        Debug.Log("Hết hội thoại! NPC quay về trạng thái đứng im bình thường.");
    }

    // Hàm phụ xử lý xoay hướng nhìn về Player (chỉ xoay theo trục Y)
    private void RotateTowardsPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0; // Giữ cân bằng, không làm NPC bị ngửa lên hoặc chúi xuống

            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    // Hàm mở rộng: Dùng để bật trạng thái di chuyển/đứng im cho AI tuần tra sau này
    public void SetWalkingState(bool isWalking)
    {
        if (_isInteracting) return; // Nếu đang nói chuyện thì không cho phép đổi sang di chuyển

        if (_animator != null)
        {
            // Trạng thái Di chuyển (NPCState = 1), Đứng im (NPCState = 0)
            _animator.SetInteger("NPCState", isWalking ? 1 : 0);
        }
    }
}