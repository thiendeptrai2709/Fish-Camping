using UnityEngine;

public class NPCBase : MonoBehaviour, INpcInteractable
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

    public string InteractionPrompt => $"[{npcName}] \n Nhấn E để {promptMessage}";

    public void Interact()
    {
        if (_isInteracting) return;
        _isInteracting = true;

        Debug.Log($"<color=green>[NPCBase] Bắt đầu tương tác với: {npcName}</color>");

        if (_npcPatrol != null) _npcPatrol.PausePatrol();

        RotateTowardsPlayer();
        SetNPCAnimationState(2);

        // 1. Ưu tiên Quest
        if (_questGiver != null)
        {
            _questGiver.HandleQuestInteraction(npcName, _animator, () => {
                ResetNPCState();
            });
        }
        // 2. Ưu tiên Nâng cấp lốp xe
        else if (_tireUpgrader != null)
        {
            _tireUpgrader.HandleUpgradeInteraction(npcName, () => {
                ResetNPCState(); // Chỉ cần ResetState, vì NPCOffroadUpgrade đã tự mở Garage rồi!
            });
        }
        // 3. Ưu tiên Shop câu cá
        else if (_fishingShop != null)
        {
            _fishingShop.HandleShopInteraction(npcName, () => {
                ResetNPCState(); // Chỉ cần ResetState, vì NPCFishingShop đã tự mở Shop rồi!
            });
        }
        // 4. Mặc định: Trò chuyện thông thường
        else
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(npcName, introDialogues, () => {
                    ResetNPCState();
                });
            }
            else
            {
                Debug.LogError("[NPCBase] Thiếu DialogueManager trong Scene!");
                ResetNPCState();
            }
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
        SetNPCAnimationState(0);

        if (_npcPatrol != null) _npcPatrol.ResumePatrol();

        Debug.Log($"[NPCBase] Hết tương tác với {npcName}.");
    }

    private void RotateTowardsPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    public void SetWalkingState(bool isWalking)
    {
        if (_isInteracting) return;

        if (_animator != null)
        {
            _animator.SetInteger("NPCState", isWalking ? 1 : 0);
        }
    }

    // INTERFACE INpcInteractable IMPLEMENTATION
    public string GetInteractPrompt() => InteractionPrompt;
    public void OnFocus() { }
    public void OnLoseFocus() { }
}