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

        CheckForInteractable();
        HandleInteractInput();
    }

    private void CheckForInteractable()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (TireRepairMinigame.ActiveTire != null)
                {
                    InteractableTire hitTire = hit.collider.GetComponent<InteractableTire>();
                    if (hitTire == null || (hitTire.GetComponent<IInteractable>() != currentInteractable && hitTire.gameObject != TireRepairMinigame.ActiveTire.gameObject))
                    {
                        ClearCurrentInteractable();
                        return;
                    }
                }

                if (interactable != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnLoseFocus();
                    }

                    currentInteractable = interactable;
                    currentInteractable.OnFocus();
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
        if (inputHandler.InteractTriggered && currentInteractable != null)
        {
            MonoBehaviour targetObject = currentInteractable as MonoBehaviour;
            if (targetObject != null)
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