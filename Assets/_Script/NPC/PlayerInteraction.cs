using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerNpcInteraction : MonoBehaviour
{
    [Header("Point Interaction Settings")]
    [SerializeField] private Transform interactionPoint; // Kéo GameObject "Point" vào đây
    [SerializeField] private float interactDistance = 2.5f; // Bán kính tương tác xung quanh Point
    [SerializeField] private LayerMask npcLayer; // Layer "Interactable" hoặc "NPC"

    [Header("UI Feedback")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color highlightCrosshairColor = Color.yellow;
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private InteractionPromptUI promptUI;

    private PlayerInputHandler inputHandler;
    private PlayerMovement playerMovement;
    private INpcInteractable currentNpcInteractable;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        playerMovement = GetComponent<PlayerMovement>();

        // Tự động tìm GameObject con tên "Point" nếu quên kéo ngoài Inspector
        if (interactionPoint == null)
        {
            Transform pointChild = transform.Find("Point");
            if (pointChild != null)
            {
                interactionPoint = pointChild;
            }
            else
            {
                interactionPoint = transform; // Tạm dùng vị trí Player nếu không thấy Point
            }
        }
    }

    private void Update()
    {
        CheckForNpc();
        HandleNpcInput();
    }

    private void CheckForNpc()
    {
        Vector3 origin = interactionPoint != null ? interactionPoint.position : transform.position;

        // Quét tất cả các Collider nằm trong bán kính xung quanh Point
        Collider[] colliders = Physics.OverlapSphere(origin, interactDistance, npcLayer);

        INpcInteractable foundNpc = null;

        if (colliders.Length > 0)
        {
            // Tìm đối tượng đầu tiên chứa INpcInteractable (ở chính nó hoặc ở cha)
            foreach (var col in colliders)
            {
                INpcInteractable interactable = col.GetComponentInParent<INpcInteractable>();
                if (interactable != null)
                {
                    foundNpc = interactable;
                    break;
                }
            }
        }

        // Xử lý đổi đối tượng NPC an toàn
        if (foundNpc != null)
        {
            if (foundNpc != currentNpcInteractable)
            {
                if (currentNpcInteractable != null)
                {
                    currentNpcInteractable.OnLoseFocus();
                }

                currentNpcInteractable = foundNpc;
                currentNpcInteractable.OnFocus();
                Debug.Log("<color=green>[NPC INTERACTION] Đã chạm tầm tương tác NPC: </color>" + currentNpcInteractable.GetInteractPrompt());
            }

            // Bật UI Prompt & Highlight Crosshair
            if (crosshairImage != null) crosshairImage.color = highlightCrosshairColor;
            if (promptUI != null) promptUI.DisplayPrompt(true, currentNpcInteractable.GetInteractPrompt());
        }
        else
        {
            // Nếu đi ra khỏi tầm hoặc không tìm thấy NPC -> Xóa focus gọn gàng
            ClearCurrentNpc();
        }
    }

    private void ClearCurrentNpc()
    {
        if (currentNpcInteractable != null)
        {
            currentNpcInteractable.OnLoseFocus();
            currentNpcInteractable = null;
            if (crosshairImage != null) crosshairImage.color = defaultCrosshairColor;
            if (promptUI != null) promptUI.DisplayPrompt(false, "");
        }
    }

    private void HandleNpcInput()
    {
        if (inputHandler != null && inputHandler.InteractTriggered)
        {
            // Kiểm tra trạng thái Dialogue an toàn qua Singleton
            bool isDialogueActive = CheckDialogueActive();

            if (isDialogueActive)
            {
                // Nếu thoại đang mở -> Bấm E để tua câu tiếp theo
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.DisplayNextSentence();
                }
            }
            else if (currentNpcInteractable != null)
            {
                // Xoay nhân vật về phía NPC
                MonoBehaviour targetNpc = currentNpcInteractable as MonoBehaviour;
                if (targetNpc != null && playerMovement != null)
                {
                    playerMovement.FaceTarget(targetNpc.transform.position);
                }

                Debug.Log("<color=cyan>[NPC INTERACTION] Bấm E mở tương tác NPC thành công!</color>");
                currentNpcInteractable.Interact();
            }
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái thoại an toàn, ưu tiên Singleton tránh lỗi GameObject.Find
    /// </summary>
    private bool CheckDialogueActive()
    {
        if (DialogueManager.Instance != null)
        {
            return DialogueManager.Instance.IsDialogueActive;
        }
        return false;
    }

    public bool HasActiveNpc()
    {
        return currentNpcInteractable != null;
    }

    // Vẽ hình cầu màu vàng trong Scene view để căn tầm tương tác
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = interactionPoint != null ? interactionPoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, interactDistance);
    }
}