using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private PlayerInputHandler inputHandler;
    private Transform cameraTransform;
    private FishingController fishingController;

    private float currentVelocity;
    private float verticalVelocity;
    public bool IsMovementLocked { get; set; } // Thêm biến khóa di chuyển
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        cameraTransform = Camera.main.transform;
        fishingController = GetComponent<FishingController>();
    }

    private void Update()
    {
        // Chốt an toàn: Nếu CharacterController bị tắt (ví dụ lúc đang ngồi trên xe) thì dừng toàn bộ tính toán di chuyển/trọng lực
        if (controller == null || !controller.enabled) return;

        if ((fishingController != null && fishingController.IsBusyFishing()) || IsMovementLocked)
        {
            HandleGravity();
            return;
        }
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector2 input = inputHandler.MoveInput;
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = inputHandler.IsSprinting ? sprintSpeed : walkSpeed;

            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }
    public void FaceTarget(Vector3 targetPosition)
    {
        Vector3 lookDirection = targetPosition - transform.position;
        lookDirection.y = 0f; // Triệt tiêu trục Y để nhân vật không bị ngửa mặt hay cúi đầu

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}