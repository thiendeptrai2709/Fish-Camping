using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private float gravity = -9.81f;

    [Header("--- Jump Settings ---")]
    [SerializeField] private float jumpHeight = 1.3f;

    [Header("--- Vitality & Agility Penalties (Độ Linh Hoạt) ---")]
    [Tooltip("Bật/tắt giảm độ linh hoạt khi đói, khát, mệt mỏi, buồn ngủ")]
    [SerializeField] private bool enableStatPenalties = true;
    [Tooltip("Tỷ lệ tốc độ đi bộ tối thiểu khi kiệt sức (0.55 = còn 55% tốc độ)")]
    [SerializeField] private float minWalkSpeedRatio = 0.55f;
    [Tooltip("Tỷ lệ tốc độ chạy tối thiểu khi kiệt sức (0.55 = còn 55% tốc độ)")]
    [SerializeField] private float minSprintSpeedRatio = 0.55f;
    [Tooltip("Tỷ lệ độ cao nhảy tối thiểu khi kiệt sức")]
    [SerializeField] private float minJumpHeightRatio = 0.40f;
    [Tooltip("Năng lượng (Energy) tiêu hao mỗi giây khi chạy nước rút")]
    [SerializeField] private float sprintEnergyDrainRate = 3.5f;
    [Tooltip("Năng lượng (Energy) hồi phục mỗi giây khi nghỉ ngơi / đứng yên")]
    [SerializeField] private float energyRegenRate = 2.0f;
    [Tooltip("Mức năng lượng tối thiểu để có thể kích hoạt chạy nước rút hoặc nhảy")]
    [SerializeField] private float criticalEnergyThreshold = 5f;

    [Header("--- Âm thanh di chuyển (File dài) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip sprintSound;

    private CharacterController controller;
    private PlayerInputHandler inputHandler;
    private PlayerAnimation playerAnimation;
    private CharacterStatsManager statsManager;
    private Transform cameraTransform;
    private FishingController fishingController;
    private DualCameraController dualCameraController;

    private float currentVelocity;
    private float verticalVelocity;
    private float currentVitalityFactor = 1f;
    private bool canSprint = true;

    public bool IsMovementLocked { get; set; }
    public bool CanSprint => canSprint;
    public float CurrentVitalityFactor => currentVitalityFactor;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        playerAnimation = GetComponent<PlayerAnimation>();
        statsManager = GetComponent<CharacterStatsManager>() ?? GetComponentInParent<CharacterStatsManager>();
        if (Camera.main != null) cameraTransform = Camera.main.transform;
        fishingController = GetComponent<FishingController>();
        dualCameraController = GetComponent<DualCameraController>() ?? GetComponentInParent<DualCameraController>();
        if (dualCameraController == null)
        {
            dualCameraController = gameObject.AddComponent<DualCameraController>();
        }

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

    private void UpdateVitalityAndAgility(bool isMoving, bool isTryingToSprint)
    {
        if (statsManager == null)
        {
            statsManager = GetComponent<CharacterStatsManager>() ?? CharacterStatsManager.Instance;
            if (statsManager == null) return;
        }

        if (!enableStatPenalties)
        {
            currentVitalityFactor = 1f;
            canSprint = true;
            return;
        }

        float energyRatio = statsManager.GetStatRatio(StatType.Energy, 1f);
        float hungerRatio = statsManager.GetStatRatio(StatType.Hunger, 1f);
        float thirstRatio = statsManager.GetStatRatio(StatType.Thirst, 1f);
        float sleepRatio = statsManager.GetStatRatio(StatType.Sleep, 1f);

        // Trọng số ảnh hưởng linh hoạt: Năng lượng (45%), Đói (20%), Khát (20%), Buồn ngủ (15%)
        currentVitalityFactor = Mathf.Clamp01((energyRatio * 0.45f) + (hungerRatio * 0.20f) + (thirstRatio * 0.20f) + (sleepRatio * 0.15f));

        float currentEnergy = statsManager.GetStatValue(StatType.Energy);
        canSprint = (currentEnergy > criticalEnergyThreshold) && (currentVitalityFactor > 0.15f);

        // Xử lý tiêu hao / hồi phục thể lực (Energy)
        if (isMoving && isTryingToSprint && canSprint)
        {
            // Tiêu hao năng lượng khi chạy nước rút
            statsManager.ModifyStat(StatType.Energy, -sprintEnergyDrainRate * Time.deltaTime);
        }
        else if (hungerRatio > 0.15f && thirstRatio > 0.15f)
        {
            // Hồi phục năng lượng khi đứng yên hoặc đi bộ nếu không bị đói/khát nghiêm trọng
            float regenFactor = isMoving ? 0.5f : 1.0f;
            statsManager.ModifyStat(StatType.Energy, energyRegenRate * regenFactor * Time.deltaTime);
        }
    }

    private void HandleMovementAndGravity()
    {
        Vector2 input = inputHandler.MoveInput;
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
        Vector3 horizontalMove = Vector3.zero;

        bool isMoving = direction.magnitude >= 0.1f;
        UpdateVitalityAndAgility(isMoving, inputHandler.IsSprinting);

        // Tính toán tốc độ và độ linh hoạt thực tế dựa trên mức độ suy kiệt thể lực
        float effectiveWalkSpeed = Mathf.Lerp(walkSpeed * minWalkSpeedRatio, walkSpeed, currentVitalityFactor);
        float effectiveSprintSpeed = Mathf.Lerp(sprintSpeed * minSprintSpeedRatio, sprintSpeed, currentVitalityFactor);
        float effectiveJumpHeight = Mathf.Lerp(jumpHeight * minJumpHeightRatio, jumpHeight, currentVitalityFactor);
        float effectiveRotationSmooth = Mathf.Lerp(rotationSmoothTime * 2.0f, rotationSmoothTime, currentVitalityFactor);

        bool isActuallySprinting = inputHandler.IsSprinting && canSprint;
        float currentSpeed = isActuallySprinting ? effectiveSprintSpeed : effectiveWalkSpeed;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        bool isFPP = (dualCameraController != null && dualCameraController.CurrentMode == PerspectiveMode.FirstPerson) ||
                     (DualCameraController.Instance != null && DualCameraController.Instance.CurrentMode == PerspectiveMode.FirstPerson);

        if (isFPP)
        {
            if (isMoving)
            {
                // FPP: Di chuyển strafe / tiến / lùi theo hướng mặt nhân vật
                Vector3 moveDirection = (transform.forward * input.y + transform.right * input.x).normalized;
                horizontalMove = moveDirection * currentSpeed;
            }
        }
        else
        {
            // TPP: Xoay tự do theo hướng di chuyển phím WASD
            if (isMoving)
            {
                float camYaw = cameraTransform != null ? cameraTransform.eulerAngles.y : 0f;
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camYaw;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, effectiveRotationSmooth);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                horizontalMove = moveDirection.normalized * currentSpeed;
            }
        }

        // 2. TÍNH LỰC NHẢY VÀ TRỌNG LỰC
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            if (inputHandler.JumpTriggered && !inputHandler.IsUIOpen && canSprint)
            {
                verticalVelocity = Mathf.Sqrt(effectiveJumpHeight * -2f * gravity);
                if (playerAnimation != null)
                {
                    playerAnimation.TriggerJump();
                }

                // Nhảy tiêu hao một lượng năng lượng nhỏ
                if (statsManager != null)
                {
                    statsManager.ModifyStat(StatType.Energy, -2f);
                }
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (playerAnimation != null)
        {
            playerAnimation.SetGrounded(controller.isGrounded);
        }

        // 3. GOM CHUNG THÀNH 1 LỆNH MOVE DUY NHẤT (Tuyệt chiêu trị lỗi isGrounded)
        Vector3 finalMove = horizontalMove + (Vector3.up * verticalVelocity);
        controller.Move(finalMove * Time.deltaTime);

        // 4. XỬ LÝ ÂM THANH (Lúc này isGrounded đã chạy ổn định 100%)
        if (isMoving)
        {
            if (controller.isGrounded)
            {
                PlayMovementAudio(isActuallySprinting);
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
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }

    private void PlayMovementAudio(bool isSprinting)
    {
        AudioClip targetClip = isSprinting ? sprintSound : walkSound;

        if (targetClip == null || audioSource == null) return;

        // Nếu đang phát sai clip (VD: từ đi bộ chuyển sang chạy nhanh) -> Đổi clip và phát tiếp
        if (audioSource.clip != targetClip)
        {
            audioSource.clip = targetClip;
            audioSource.Play();
        }
        // Nếu âm thanh đang bị ngắt quãng do vừa tiếp đất -> Bật lại
        else if (!audioSource.isPlaying)
        {
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