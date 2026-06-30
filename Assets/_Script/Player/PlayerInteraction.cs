using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerCursor))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactDistance = 3f; // Khoảng cách tương tác tối đa
    [SerializeField] private LayerMask interactableLayer; // Chỉ kiểm tra vật thể ở Layer này
    [SerializeField] private Image crosshairImage; // Tham chiếu đến UI Image đã tạo
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private Color highlightCrosshairColor = Color.yellow; // Đổi màu khi ngắm trúng

    private PlayerInputHandler inputHandler;
    private PlayerCursor playerCursor;
    private Transform cameraTransform;
    private IInteractable currentInteractable;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        playerCursor = GetComponent<PlayerCursor>();
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        CheckForInteractable();
        HandleInteractInput();
    }

    // Bắn tia Raycast từ giữa camera về phía trước
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
            // TODO: Bạn có thể code thêm UI để hiện câu lệnh prompt (ví dụ: [E] Mở hòm)
        }
    }

    private void HandleInteractInput()
    {
        if (inputHandler.InteractTriggered && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}