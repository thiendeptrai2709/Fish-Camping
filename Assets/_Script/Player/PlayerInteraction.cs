using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerCursor))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private Color highlightCrosshairColor = Color.yellow;
    [SerializeField] private InteractionPromptUI promptUI;

    private PlayerInputHandler inputHandler;
    private PlayerCursor playerCursor;
    private PlayerMovement playerMovement;
    private Transform cameraTransform;
    private FishingController fishingController;
    private IInteractable currentInteractable;
    private bool wasUIOpen;
    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        playerCursor = GetComponent<PlayerCursor>();
        playerMovement = GetComponent<PlayerMovement>();
        cameraTransform = Camera.main.transform;
        fishingController = GetComponent<FishingController>();

        int interactableLayerIndex = LayerMask.NameToLayer("Interactable");
        int outlinedLayerIndex = LayerMask.NameToLayer("Outlined");

        if (interactableLayerIndex != -1)
            interactableLayer |= (1 << interactableLayerIndex);
        if (outlinedLayerIndex != -1)
            interactableLayer |= (1 << outlinedLayerIndex);
    }
    private void OnDisable()
    {
        ClearCurrentInteractable(); // Tự động xóa focus, tắt UI chữ và tắt tâm ngắm
    }
    private void Update()
    {
        if (inputHandler.IsUIOpen != wasUIOpen)
        {
            wasUIOpen = inputHandler.IsUIOpen;

            if (crosshairImage != null)
            {
                crosshairImage.enabled = !wasUIOpen;
            }

            if (playerCursor != null)
            {
                playerCursor.SetCursorState(!wasUIOpen);
            }

            if (wasUIOpen)
            {
                ClearCurrentInteractable();
            }
        }

        if (inputHandler.IsUIOpen) return;

        if (fishingController != null && fishingController.IsBusyFishing())
        {
            ClearCurrentInteractable();
            return;
        }
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            if (inputHandler.InteractTriggered)
            {
                DialogueManager.Instance.DisplayNextSentence();
            }
            return;
        }
        CheckForInteractable();
        HandleInteractInput();
    }

    private static readonly RaycastHit[] hitBuffer = new RaycastHit[16];

    private void CheckForInteractable()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            ClearCurrentInteractable();
            return;
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 1. Kiểm tra Raycast ngắm trực tiếp (Cửa xe, Cốp xe, Nắp Capo, Động cơ, Lốp xe, Đồ cắm trại,...)
        int hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, interactDistance, interactableLayer, QueryTriggerInteraction.Collide);
        if (hitCount > 0)
        {
            // Sắp xếp theo khoảng cách gần nhất (Non-Alloc In-Place Sort)
            for (int i = 0; i < hitCount - 1; i++)
            {
                int minIdx = i;
                for (int j = i + 1; j < hitCount; j++)
                {
                    if (hitBuffer[j].distance < hitBuffer[minIdx].distance) minIdx = j;
                }
                if (minIdx != i)
                {
                    RaycastHit temp = hitBuffer[i];
                    hitBuffer[i] = hitBuffer[minIdx];
                    hitBuffer[minIdx] = temp;
                }
            }

            IInteractable candidate = null;

            for (int i = 0; i < hitCount; i++)
            {
                var hitCol = hitBuffer[i].collider;
                if (hitCol == null) continue;

                // Bỏ qua các trigger zone nhiệm vụ / bản đồ
                if (hitCol.GetComponent<TutorialTruckTriggerZone>() != null ||
                    hitCol.GetComponent<TutorialShopArrivalTriggerZone>() != null ||
                    hitCol.GetComponent<MapTriggerZone>() != null)
                {
                    continue;
                }

                IInteractable interactable = hitCol.GetComponent<IInteractable>() ?? hitCol.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    // Nếu là Động cơ mà nắp Capo đang đóng thì bỏ qua không nhận tương tác
                    if (interactable is InteractableEngine engine && !engine.CanInteract())
                    {
                        continue;
                    }

                    if (TireRepairMinigame.ActiveTire != null)
                    {
                        InteractableTire hitTire = hitCol.GetComponent<InteractableTire>() ?? hitCol.GetComponentInParent<InteractableTire>();
                        if (hitTire == null || (hitTire.GetComponent<IInteractable>() != currentInteractable && hitTire.gameObject != TireRepairMinigame.ActiveTire.gameObject))
                        {
                            continue;
                        }
                    }

                    // Nếu gặp chức năng cụ thể (Cửa xe, Cốp, Capo, Động cơ, Lốp, NPC...), ưu tiên chọn ngay lập tức!
                    bool isGenericVehicleStats = (interactable is InteractableVehicleStats || interactable is VehicleBody);
                    if (!isGenericVehicleStats)
                    {
                        candidate = interactable;
                        break;
                    }
                    else if (candidate == null)
                    {
                        candidate = interactable;
                    }
                }
            }

            if (candidate != null)
            {
                if (candidate != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnLoseFocus();
                    }

                    currentInteractable = candidate;
                    currentInteractable.OnFocus();
                }

                SetCrosshairState(true, candidate.GetInteractPrompt());
                return;
            }
        }

        // 2. Kiểm tra khoảng cách đứng gần NPC (Proximity) qua ActiveNpcs Cache (Zero-Alloc)
        var allNpcs = NPCBase.ActiveNpcs;
        NPCBase nearestNpc = null;
        float minDistance = interactDistance;
        Vector3 playerPos = transform.position;

        for (int i = 0; i < allNpcs.Count; i++)
        {
            NPCBase npc = allNpcs[i];
            if (npc == null || !npc.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(playerPos, npc.transform.position);
            if (dist <= npc.ProximityDistance && dist < minDistance)
            {
                minDistance = dist;
                nearestNpc = npc;
            }
        }

        if (nearestNpc != null)
        {
            if (currentInteractable != nearestNpc)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.OnLoseFocus();
                }

                currentInteractable = nearestNpc;
                currentInteractable.OnFocus();
            }

            SetCrosshairState(true, nearestNpc.GetInteractPrompt());
            return;
        }

        ClearCurrentInteractable();
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

    private float lastInteractTime = 0f;
    private const float INTERACT_COOLDOWN = 0.2f;

    private void HandleInteractInput()
    {
        if (inputHandler != null && inputHandler.InteractTriggered && currentInteractable != null)
        {
            if (Time.unscaledTime - lastInteractTime < INTERACT_COOLDOWN)
            {
                return;
            }
            lastInteractTime = Time.unscaledTime;

            MonoBehaviour targetObject = currentInteractable as MonoBehaviour;
            if (targetObject != null && playerMovement != null)
            {
                playerMovement.FaceTarget(targetObject.transform.position);
            }

            currentInteractable.Interact();
        }
    }

    public bool HasActiveInteractable()
    {
        return currentInteractable != null;
    }
}