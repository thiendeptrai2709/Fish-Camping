using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerCursor))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Transform interactionPoint; // Điểm gốc "Point"
    [SerializeField] private float interactDistance = 3f; // Bán kính tương tác
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI References")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private Color highlightCrosshairColor = Color.yellow;
    [SerializeField] private InteractionPromptUI promptUI;

    private PlayerInputHandler inputHandler;
    private PlayerCursor playerCursor;
    private PlayerMovement playerMovement;
    private FishingController fishingController;

    private IInteractable currentInteractable;
    private bool wasUIOpen;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        playerCursor = GetComponent<PlayerCursor>();
        playerMovement = GetComponent<PlayerMovement>();
        fishingController = GetComponent<FishingController>();

        if (interactionPoint == null)
        {
            Transform pointChild = transform.Find("Point");
            interactionPoint = pointChild != null ? pointChild : transform;
        }
    }

    private void OnDisable()
    {
        ClearCurrentInteractable();
    }

    private void Update()
    {
        // Xử lý Input bấm nút E (Đặt trước kiểm tra IsUIOpen để luôn nhận phím tua thoại)
        HandleInteractInput();

        // Kiểm tra trạng thái UI Menu
        if (inputHandler != null && inputHandler.IsUIOpen != wasUIOpen)
        {
            wasUIOpen = inputHandler.IsUIOpen;

            if (crosshairImage != null) crosshairImage.enabled = !wasUIOpen;
            if (playerCursor != null) playerCursor.SetCursorState(!wasUIOpen);
            if (wasUIOpen) ClearCurrentInteractable();
        }

        if (inputHandler != null && inputHandler.IsUIOpen) return;

        // Nếu đang câu cá -> Tắt tương tác khác
        if (fishingController != null && fishingController.IsBusyFishing())
        {
            ClearCurrentInteractable();
            return;
        }

        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        Vector3 origin = interactionPoint != null ? interactionPoint.position : transform.position;

        Collider[] colliders = Physics.OverlapSphere(origin, interactDistance, interactableLayer);

        IInteractable closestInteractable = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            IInteractable interactable = col.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                if (TireRepairMinigame.ActiveTire != null)
                {
                    InteractableTire hitTire = col.GetComponentInParent<InteractableTire>();
                    if (hitTire == null || (hitTire.GetComponent<IInteractable>() != currentInteractable && hitTire.gameObject != TireRepairMinigame.ActiveTire.gameObject))
                    {
                        continue;
                    }
                }

                float distance = Vector3.Distance(origin, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        if (closestInteractable != null)
        {
            if (closestInteractable != currentInteractable)
            {
                if (currentInteractable != null) currentInteractable.OnLoseFocus();

                currentInteractable = closestInteractable;
                currentInteractable.OnFocus();
            }

            SetCrosshairState(true, currentInteractable.GetInteractPrompt());
        }
        else
        {
            ClearCurrentInteractable();
        }
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
        SetCrosshairState(false);
    }

    private void SetCrosshairState(bool isTargeting, string prompt = "")
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = isTargeting ? highlightCrosshairColor : defaultCrosshairColor;
        }

        if (promptUI != null)
        {
            promptUI.DisplayPrompt(isTargeting && !string.IsNullOrEmpty(prompt), prompt);
        }
    }

    private void HandleInteractInput()
    {
        if (inputHandler != null && inputHandler.InteractTriggered)
        {
            // 🌟 ĐIỂM SỬA QUAN TRỌNG: Ưu tiên tua thoại nếu Dialogue đang mở!
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.DisplayNextSentence();
                return;
            }

            // Nếu không mở thoại và đang nhắm vào vật thể -> Gọi Interact()
            if (currentInteractable != null)
            {
                MonoBehaviour targetObject = currentInteractable as MonoBehaviour;
                if (targetObject != null && playerMovement != null)
                {
                    playerMovement.FaceTarget(targetObject.transform.position);
                }

                currentInteractable.Interact();
            }
        }
    }

    public bool HasActiveInteractable() => currentInteractable != null;

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = interactionPoint != null ? interactionPoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, interactDistance);
    }
}