using UnityEngine;

public class NPCBase : MonoBehaviour, IInteractable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Bác Thợ Máy";
    [SerializeField] private string promptMessage = "Nâng cấp xe";

    [Header("Dialogues")]
    [SerializeField]
    [TextArea(2, 5)]
    private string[] introDialogues = new string[] {
        "Chào cậu, muốn nâng cấp gì cho chiếc xe tải cũ này à?"
    };

    [Header("System Links")]
    [SerializeField] private QuestGiver _questGiver;
    [SerializeField] private NPCOffroadUpgrade _tireUpgrader;

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

        if (_tireUpgrader == null)
        {
            _tireUpgrader = GetComponent<NPCOffroadUpgrade>();
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

        // Tự động xoay mặt NPC về hướng của Player
        RotateTowardsPlayer();

        // 1. Khi vừa bấm E nói chuyện: Chuyển sang trạng thái Nói chuyện (NPCState = 1)
        SetNPCAnimationState(1);

        // NHÁNH 1: Kiểm tra xem đây là NPC có nhiệm vụ cốt truyện
        if (_questGiver != null)
        {
            // Chạy logic thoại và quản lý trạng thái của QuestGiver
            _questGiver.HandleQuestInteraction(npcName, _animator, () => {
                ResetNPCState();
            });
        }
        // NHÁNH 2: Nếu là NPC nâng cấp lốp xe
        else if (_tireUpgrader != null)
        {
            // Chạy hội thoại chào hỏi, tư vấn trước
            _tireUpgrader.HandleUpgradeInteraction(npcName, () => {

                // Sau khi dứt lời thoại: Chuyển sang trạng thái Nâng cấp/Sửa xe (NPCState = 2)
                SetNPCAnimationState(2);

                // Mở bảng giao diện Gara lên cho người chơi thao tác
                GarageUIManager.Instance.OpenGarage(() => {
                    // Khi người chơi tắt UI Gara: Trả NPC về lại Đứng im (NPCState = 0)
                    ResetNPCState();
                });
            });
        }
        // NHÁNH 3: NPC thường, chỉ chạy thoại mặc định
        else
        {
            DialogueManager.Instance.StartDialogue(npcName, introDialogues, () => {
                ResetNPCState();
            });
        }
    }

    // Hàm trung gian quản lý việc cập nhật thông số Animator
    public void SetNPCAnimationState(int stateValue)
    {
        if (_animator != null)
        {
            _animator.SetInteger("NPCState", stateValue);
        }
    }

    private void ResetNPCState()
    {
        _isInteracting = false;
        SetNPCAnimationState(0); // Trả NPC về lại trạng thái Đứng im mặc định (NPCState = 0)
        Debug.Log("Hết tương tác! NPC quay về trạng thái đứng im bình thường.");
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

        // Tận dụng trạng thái di chuyển nếu sau này bạn làm AI di chuyển
        // (Tạm thời map theo logic cũ của bạn, ví dụ: 1 là đi bộ nếu cần, hoặc tùy biến sau)
        if (_animator != null)
        {
            _animator.SetInteger("NPCState", isWalking ? 1 : 0);
        }
    }
}