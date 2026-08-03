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
    [SerializeField] private NPCFishingShop _fishingShop;

    private Animator _animator;
    private bool _isInteracting = false;
    private NPCPatrol _npcPatrol;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _npcPatrol = GetComponent<NPCPatrol>();

        if (_questGiver == null) _questGiver = GetComponent<QuestGiver>();
        if (_tireUpgrader == null) _tireUpgrader = GetComponent<NPCOffroadUpgrade>();
        if (_fishingShop == null) _fishingShop = GetComponent<NPCFishingShop>();
    }

    // Thuộc tính từ Interface trả về chuỗi hiển thị UI
    public string InteractionPrompt => $"[{npcName}] \n Nhấn E để {promptMessage}";

    // Hàm gọi khi Player nhấn phím tương tác E
    public void Interact()
    {
        if (_isInteracting) return; // Chặn trùng lặp hội thoại
        _isInteracting = true;

        Debug.Log($"Đang tương tác với NPC: {npcName}");

        // 1. Dừng Agent tuần tra nếu NPC này có đi lại
        if (_npcPatrol != null) _npcPatrol.PausePatrol();

        // 2. Quay mặt về phía Player và chuyển Hoạt ảnh sang Nói chuyện (NPCState = 2)
        RotateTowardsPlayer();
        SetNPCAnimationState(2);

        // NHÁNH 1: Kiểm tra xem đây là NPC có nhiệm vụ cốt truyện
        if (_questGiver != null)
        {
            _questGiver.HandleQuestInteraction(npcName, _animator, () => {
                ResetNPCState();
            });
        }
        // NHÁNH 2: Nếu là NPC nâng cấp xe (Gara)
        else if (_tireUpgrader != null)
        {
            _tireUpgrader.HandleUpgradeInteraction(npcName, () => {
                // Nhận tín hiệu từ NPCOffroadUpgrade báo về là đã xong việc (đóng Garage).
                // Lập tức gọi hàm Reset để thả cờ _isInteracting = false.
                ResetNPCState();
            });
        }
        // NHÁNH 3: Nếu là NPC Thuyền trưởng bán đồ câu / thu mua cá
        else if (_fishingShop != null)
        {
            _fishingShop.HandleShopInteraction(npcName, () => {
                ResetNPCState();
            });
        }
        // NHÁNH 4: Dân làng bình thường thoại vu vơ
        else
        {
            DialogueManager.Instance.StartDialogue(npcName, introDialogues, () => {
                ResetNPCState();
            });
        }
    }

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
        SetNPCAnimationState(0); // Trả hoạt ảnh về Idle đứng im mặc định

        // Tiếp tục hành trình tuần tra nếu có
        if (_npcPatrol != null) _npcPatrol.ResumePatrol();

        Debug.Log($"Hết tương tác! NPC {npcName} quay về trạng thái bình thường.");
    }

    private void RotateTowardsPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0; // Giữ cân bằng trục Y không cho NPC bập bênh

            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    public void SetWalkingState(bool isWalking)
    {
        if (_isInteracting) return; // Đang nói chuyện thì không đổi trạng thái di chuyển

        if (_animator != null)
        {
            // Đi bộ tuần tra -> NPCState = 1, Đứng im nghỉ mệt -> NPCState = 0
            _animator.SetInteger("NPCState", isWalking ? 1 : 0);
        }
    }
    public string GetInteractPrompt()
    {
        return InteractionPrompt;
    }

    public void OnFocus()
    {
        // Logic khi Player nhìn vào NPC (ví dụ: hiện viền sáng)
    }

    public void OnLoseFocus()
    {
        // Logic khi Player quay đầu đi chỗ khác (ví dụ: tắt viền sáng)
    }
}
