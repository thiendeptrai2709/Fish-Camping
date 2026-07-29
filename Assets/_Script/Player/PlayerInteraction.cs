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

    // ĐỔI 1: Chuyển biến lưu sang INpcInteractable
    private INpcInteractable currentInteractable;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        playerCursor = GetComponent<PlayerCursor>();
        playerMovement = GetComponent<PlayerMovement>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        fishingController = GetComponent<FishingController>();
    }

    private void Update()
    {
        if (fishingController != null && fishingController.IsBusyFishing())
        {
            ClearCurrentInteractable();
            return;
        }

        CheckForInteractable();
        HandleInteractInput();
    }

    private void CheckForInteractable()
    {
        if (cameraTransform == null)
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
            else return;
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            // ĐỔI 2: Tìm INpcInteractable ở chính nó hoặc ở cha (NPCBase)
            INpcInteractable interactable = hit.collider.GetComponentInParent<INpcInteractable>();

            if (interactable != null)
            {
                if (interactable != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnLoseFocus();
                    }

                    currentInteractable = interactable;
                    currentInteractable.OnFocus();
                    Debug.Log("<color=green>[PLAYER INTERACTION] Nhắm trúng NPC: </color>" + hit.collider.name);
                }

                SetCrosshairState(true, interactable.GetInteractPrompt());
                return;
            }
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
        if (inputHandler != null && inputHandler.InteractTriggered)
        {
            if (currentInteractable != null)
            {
                MonoBehaviour targetObject = currentInteractable as MonoBehaviour;
                if (targetObject != null && playerMovement != null)
                {
                    playerMovement.FaceTarget(targetObject.transform.position);
                }

                Debug.Log("<color=cyan>[PLAYER INTERACTION] Gọi Interact() trên NPC thành công!</color>");
                currentInteractable.Interact();
            }
        }
    }

    public bool HasActiveInteractable()
    {
        return currentInteractable != null;
    }
}