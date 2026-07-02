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
    private PlayerMovement playerMovement;
    private Transform cameraTransform;
    private IInteractable currentInteractable;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        playerCursor = GetComponent<PlayerCursor>();
        playerMovement = GetComponent<PlayerMovement>();
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
                if (TireRepairMinigame.ActiveTire != null)
                {
                    InteractableTire hitTire = hit.collider.GetComponent<InteractableTire>();
                    if (hitTire == null || hitTire.GetComponent<IInteractable>() != currentInteractable && hitTire.gameObject != TireRepairMinigame.ActiveTire.gameObject)
                    {
                        // Từ chối tương tác với mọi vật thể ngoại đạo
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

    // Tách đoạn xóa trạng thái ra thành hàm nhỏ cho sạch code
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
            // TODO: Bạn có thể code thêm UI để hiện câu lệnh prompt (ví dụ: [E] Mở hòm)
        }
    }

    private void HandleInteractInput()
    {
        if (inputHandler.InteractTriggered && currentInteractable != null)
        {
            // Lấy Transform của vật thể đang được ngắm trúng để ép nhân vật quay ra nhìn
            MonoBehaviour targetObject = currentInteractable as MonoBehaviour;
            if (targetObject != null)
            {
                playerMovement.FaceTarget(targetObject.transform.position);
            }

            currentInteractable.Interact();
        }
    }
}