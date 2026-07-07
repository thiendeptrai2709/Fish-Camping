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
    [SerializeField] private NPCFishingShop _fishingShop; // Thêm liên kết tới Shop mua bán cá, đồ câu

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

        // Tự động tìm linh kiện Shop cá trên cùng GameObject NPC nếu có
        if (_fishingShop == null)
        {
            _fishingShop = GetComponent<NPCFishingShop>();
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

        // 1. Khi vừa bấm E nói chuyện: Chuyển sang trạng thái Nói chuyện (NPCState = 2 đối với con NPC mới này)
        SetNPCAnimationState(2);

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

                // Đối với ông thợ máy cũ, giữ nguyên trạng thái sửa xe (NPCState = 2) khi mở Gara
                SetNPCAnimationState(2);

                // Mở bảng giao diện Gara lên cho người chơi thao tác
                GarageUIManager.Instance.OpenGarage(() => {
                    // Khi người chơi tắt UI Gara: Trả NPC về lại Đứng im (NPCState = 0)
                    ResetNPCState();
                });
            });
        }
        // NHÁNH 3: Nếu là NPC Thuyền trưởng mua bán cá, đồ câu
        else if (_fishingShop != null)
        {
            // Gọi logic của hệ thống Shop cá
            _fishingShop.HandleShopInteraction(npcName, () => {
                ResetNPCState();
            });
        }
        // NHÁNH 4: NPC thường, chỉ chạy thoại mặc định
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

    // Hàm di chuyển/đứng im cho AI tuần tra (NPCState = 1 là đi bộ/chạy như sơ đồ Animator của bạn)
    public void SetWalkingState(bool isWalking)
    {
        if (_isInteracting) return; // Nếu đang nói chuyện thì không cho phép đổi sang di chuyển

        if (_animator != null)
        {
            // Đi bộ tuần tra -> NPCState = 1, Đứng im -> NPCState = 0
            _animator.SetInteger("NPCState", isWalking ? 1 : 0);
        }
    }
}