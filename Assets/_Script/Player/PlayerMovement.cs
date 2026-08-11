using UnityEngine;
using UnityEngine.SceneManagement; // Thêm thư viện quản lý Scene

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour, ISaveable // Kế thừa ISaveable
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

        bool isDialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;

        if ((fishingController != null && fishingController.IsBusyFishing()) || IsMovementLocked || inputHandler.IsUIOpen || isDialogueActive)
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

    // ==================== TÍCH HỢP HỆ THỐNG LƯU / TẢI VỊ TRÍ & SCENE (ISAVEABLE) ====================

    public void SaveData(GameSaveData data)
    {
        // 1. Lưu tên Scene hiện tại
        data.currentSceneName = SceneManager.GetActiveScene().name;

        // 2. Lưu tọa độ X, Y, Z của Player
        data.playerPosX = transform.position.x;
        data.playerPosY = transform.position.y;
        data.playerPosZ = transform.position.z;

        Debug.Log($"[PlayerSave] Đã lưu vị trí Player: ({data.playerPosX}, {data.playerPosY}, {data.playerPosZ}) tại Scene: {data.currentSceneName}");
    }

    public void LoadData(GameSaveData data)
    {
        // Kiểm tra xem có dữ liệu vị trí hợp lệ không (tránh lỡ load về gốc 0,0,0)
        if (data.playerPosX == 0 && data.playerPosY == 0 && data.playerPosZ == 0) return;

        Vector3 savedPosition = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        // Phải tạm tắt CharacterController trước khi gán vị trí mới để Unity không bị xô lệch/chặn vị trí
        if (controller != null)
        {
            controller.enabled = false;
            transform.position = savedPosition;
            controller.enabled = true;
        }
        else
        {
            transform.position = savedPosition;
        }

        Debug.Log($"[PlayerSave] Đã khôi phục vị trí Player về: {savedPosition}");
    }
}