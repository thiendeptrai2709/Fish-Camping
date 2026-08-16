using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private float gravity = -9.81f;

    [Header("--- Âm thanh di chuyển (File dài) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip sprintSound;

    private CharacterController controller;
    private PlayerInputHandler inputHandler;
    private Transform cameraTransform;
    private FishingController fishingController;

    private float currentVelocity;
    private float verticalVelocity;
    public bool IsMovementLocked { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        cameraTransform = Camera.main.transform;
        fishingController = GetComponent<FishingController>();

        // Giữ nguyên thiết lập lặp lại file
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (controller == null || !controller.enabled)
        {
            StopAudio();
            return;
        }

        bool isDialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;

        if ((fishingController != null && fishingController.IsBusyFishing()) || IsMovementLocked || inputHandler.IsUIOpen || isDialogueActive)
        {
            StopAudio();
            HandleGravityOnly(); // Chỉ đứng im rớt xuống, không đi ngang
            return;
        }

        // Gọi 1 hàm duy nhất thay vì 2 hàm tách biệt
        HandleMovementAndGravity();
    }

    private void HandleMovementAndGravity()
    {
        Vector2 input = inputHandler.MoveInput;
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
        Vector3 horizontalMove = Vector3.zero;

        // 1. TÍNH LỰC ĐI NGANG (Chưa Move)
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = inputHandler.IsSprinting ? sprintSpeed : walkSpeed;

            horizontalMove = moveDirection.normalized * currentSpeed;
        }

        // 2. TÍNH LỰC HÚT TRÁI ĐẤT (Chưa Move)
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;

        // 3. GOM CHUNG THÀNH 1 LỆNH MOVE DUY NHẤT (Tuyệt chiêu trị lỗi isGrounded)
        Vector3 finalMove = horizontalMove + (Vector3.up * verticalVelocity);
        controller.Move(finalMove * Time.deltaTime);

        // 4. XỬ LÝ ÂM THANH (Lúc này isGrounded đã chạy ổn định 100%)
        if (direction.magnitude >= 0.1f)
        {
            if (controller.isGrounded)
            {
                PlayMovementAudio(inputHandler.IsSprinting);
            }
            else
            {
                StopAudio(); // Đang nhảy/rơi thì tắt
            }
        }
        else
        {
            StopAudio(); // Đứng im thì tắt
        }
    }

    // Hàm phụ trợ khi nhân vật bị khóa chân (chỉ áp dụng trọng lực)
    private void HandleGravityOnly()
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
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void PlayMovementAudio(bool isSprinting)
    {
        if (audioSource == null) return;

        AudioClip targetClip = isSprinting ? sprintSound : walkSound;

        if (audioSource.clip != targetClip || !audioSource.isPlaying)
        {
            audioSource.clip = targetClip;
            audioSource.Play();
        }
    }

    private void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}