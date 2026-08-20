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

    private void CheckForInteractable()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            ClearCurrentInteractable();
            return;
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        // 1. Kiểm tra Raycast ngắm trực tiếp (Cửa xe, Cốp xe, Nắp Capo, Động cơ, Lốp xe, Đồ cắm trại,...)
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance, interactableLayer, QueryTriggerInteraction.Collide);
        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            IInteractable candidate = null;

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                // Bỏ qua các trigger zone nhiệm vụ / bản đồ
                if (hit.collider.GetComponent<TutorialTruckTriggerZone>() != null ||
                    hit.collider.GetComponent<TutorialShopArrivalTriggerZone>() != null ||
                    hit.collider.GetComponent<MapTriggerZone>() != null)
                {
                    continue;
                }

                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    interactable = hit.collider.GetComponentInParent<IInteractable>();
                }

                if (interactable != null)
                {
                    // Nếu là Động cơ mà nắp Capo đang đóng thì bỏ qua không nhận tương tác
                    if (interactable is InteractableEngine engine && !engine.CanInteract())
                    {
                        continue;
                    }

                    if (TireRepairMinigame.ActiveTire != null)
                    {
                        InteractableTire hitTire = hit.collider.GetComponent<InteractableTire>() ?? hit.collider.GetComponentInParent<InteractableTire>();
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

        // 2. Kiểm tra khoảng cách đứng gần NPC (Proximity)
        NPCBase[] allNpcs = Object.FindObjectsByType<NPCBase>(FindObjectsSortMode.None);
        NPCBase nearestNpc = null;
        float minDistance = interactDistance;

        foreach (var npc in allNpcs)
        {
            if (npc == null || !npc.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, npc.transform.position);
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

    private void HandleInteractInput()
    {
        if (inputHandler != null && inputHandler.InteractTriggered && currentInteractable != null)
        {
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