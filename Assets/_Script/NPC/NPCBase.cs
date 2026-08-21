using UnityEngine;
using UnityEngine.Localization.Settings;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string text;
    public AudioClip voiceClip;
}

public class NPCBase : MonoBehaviour, IInteractable, INpcInteractable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Dân Làng";
    [SerializeField] private string promptMessage = "Trò chuyện";
    [SerializeField] private float proximityDistance = 3.5f;

    [Header("=== THOẠI LẦN ĐẦU GẶP MẶT ===")]
    [Tooltip("Chạy hết tất cả các câu này trong lần đầu người chơi tương tác")]
    [SerializeField] private DialogueLine[] introDialogues;

    [Header("=== THOẠI CÁC LẦN SAU QUAY LẠI (1 CÂU) ===")]
    [Tooltip("Từ lần 2 trở đi, NPC sẽ chọn ngẫu nhiên duy nhất 1 câu trong này")]
    [SerializeField] private DialogueLine[] returningDialogues = new DialogueLine[] {
        new DialogueLine { text = "NPCQUEST4" },
        new DialogueLine { text = "NPCQUEST5" },
        new DialogueLine { text = "NPCQUEST6" }
    };

    [Header("System Links")]
    [SerializeField] private QuestGiver _questGiver;
    [SerializeField] private NPCOffroadUpgrade _tireUpgrader;
    [SerializeField] private NPCFishingShop _fishingShop;

    private Animator _animator;
    private bool _isInteracting = false;
    private NPCPatrol _npcPatrol;
    private bool _hasMetPlayer = false; // Ghi nhớ đã nói chuyện lần đầu chưa

    public static readonly System.Collections.Generic.List<NPCBase> ActiveNpcs = new System.Collections.Generic.List<NPCBase>();

    public float ProximityDistance => proximityDistance;
    public bool IsFishingShop => _fishingShop != null || GetComponent<NPCFishingShop>() != null;
    public bool IsTireUpgrader => _tireUpgrader != null || GetComponent<NPCOffroadUpgrade>() != null;
    public bool IsQuestGiver => _questGiver != null || GetComponent<QuestGiver>() != null;

    private void OnEnable()
    {
        if (!ActiveNpcs.Contains(this)) ActiveNpcs.Add(this);
    }

    private void OnDisable()
    {
        ActiveNpcs.Remove(this);
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _npcPatrol = GetComponent<NPCPatrol>();

        if (_questGiver == null) _questGiver = GetComponent<QuestGiver>();
        if (_tireUpgrader == null) _tireUpgrader = GetComponent<NPCOffroadUpgrade>();
        if (_fishingShop == null) _fishingShop = GetComponent<NPCFishingShop>();

        // 1. Đảm bảo Layer là Interactable để hệ thống tương tác nhận diện
        int interactableLayer = LayerMask.NameToLayer("Interactable");
        if (interactableLayer != -1)
        {
            gameObject.layer = interactableLayer;
        }

        // 2. Tự động thêm Collider nếu NPC chưa có collider nào
        Collider existingCol = GetComponentInChildren<Collider>();
        if (existingCol == null)
        {
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 1f, 0);
            col.radius = 0.6f;
            col.height = 2f;
            col.isTrigger = false;
        }

        // 3. Nạp trạng thái đã gặp NPC từ PlayerPrefs
        _hasMetPlayer = PlayerPrefs.GetInt(GetNpcSaveKey(), 0) == 1;
    }

    private string GetNpcSaveKey()
    {
        return $"NPC_Met_{gameObject.scene.name}_{npcName}_{Mathf.RoundToInt(transform.position.x)}_{Mathf.RoundToInt(transform.position.z)}";
    }

    public string InteractionPrompt
    {
        get
        {
            string actionText = promptMessage;
            if (_fishingShop != null) actionText = "Mở Cửa Hàng Đồ Câu";
            else if (_tireUpgrader != null) actionText = "Nâng cấp xe";
            else if (_questGiver != null) actionText = "Nhận nhiệm vụ";

            string localizedName = LocalizationSettings.StringDatabase.GetLocalizedString("Game Text", npcName);
            if (string.IsNullOrEmpty(localizedName)) localizedName = npcName;

            bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                                LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

            if (isVietnamese)
            {
                return $"[{localizedName}] \n Click Chuột Trái để {actionText}";
            }
            else
            {
                return $"[{localizedName}] \n Left Click to {actionText}";
            }
        }
    }

    public void Interact()
    {
        if (_isInteracting) return;
        _isInteracting = true;

        if (_npcPatrol != null) _npcPatrol.PausePatrol();

        RotateTowardsPlayer();
        SetNPCAnimationState(2);

        if (_questGiver != null)
        {
            _questGiver.HandleQuestInteraction(npcName, _animator, ResetNPCState);
            ForcedTutorialManager.Instance?.NotifyQuestNPCTalked();
        }
        else if (_tireUpgrader != null)
        {
            _tireUpgrader.HandleUpgradeInteraction(npcName, ResetNPCState);
        }
        else if (_fishingShop != null)
        {
            _fishingShop.HandleShopInteraction(npcName, ResetNPCState);
        }
        else
        {
            // Nhánh NPC Dân Làng bình thường
            DialogueLine[] linesToPlay;

            if (!_hasMetPlayer)
            {
                // Lần đầu gặp: Nói toàn bộ danh sách intro
                _hasMetPlayer = true;
                PlayerPrefs.SetInt(GetNpcSaveKey(), 1);
                PlayerPrefs.Save();
                linesToPlay = introDialogues;
            }
            else
            {
                // Các lần sau: Bốc ngẫu nhiên đúng 1 câu chào ngắn
                linesToPlay = GetRandomReturningLine();
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogueWithVoice(npcName, linesToPlay, ResetNPCState);
            }
            else
            {
                ResetNPCState();
            }

            ForcedTutorialManager.Instance?.NotifyGasNPCTalked();
        }
    }

    private DialogueLine[] GetRandomReturningLine()
    {
        if (returningDialogues != null && returningDialogues.Length > 0)
        {
            int randomIndex = Random.Range(0, returningDialogues.Length);
            return new DialogueLine[] { returningDialogues[randomIndex] };
        }

        if (introDialogues != null && introDialogues.Length > 0)
        {
            return new DialogueLine[] { introDialogues[0] };
        }

        return new DialogueLine[] { new DialogueLine { text = "Chào cậu!" } };
    }

    public void SetNPCAnimationState(int stateValue)
    {
        if (_animator != null) _animator.SetInteger("NPCState", stateValue);
    }

    private void ResetNPCState()
    {
        _isInteracting = false;
        SetNPCAnimationState(0);
        if (_npcPatrol != null) _npcPatrol.ResumePatrol();
    }

    private void RotateTowardsPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    public void SetWalkingState(bool isWalking)
    {
        if (_isInteracting) return;
        if (_animator != null) _animator.SetInteger("NPCState", isWalking ? 1 : 0);
    }

    public string GetInteractPrompt() => InteractionPrompt;
    public void OnFocus() { }
    public void OnLoseFocus() { }
}