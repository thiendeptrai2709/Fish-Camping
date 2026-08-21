using UnityEngine;
using UnityEngine.UI;

public class BalanceMinigameUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform fishIcon;
    [SerializeField] private RectTransform catchZone;
    [SerializeField] private Slider progressBar;

    [SerializeField] private float fishMoveSpeed = 1.2f; // Giảm tốc độ cá bơi xuống cho êm hơn
    [SerializeField] private float fishRandomTimeMin = 1.0f; // Cá đứng yên lâu hơn một chút
    [SerializeField] private float fishRandomTimeMax = 2.5f;

    [SerializeField] private float gravity = 800f; // Tăng trọng lực để rơi đầm tay hơn
    [SerializeField] private float liftPower = 1200f; // Tăng lực nâng để nháy chuột nhạy hơn
    [SerializeField] private float maxSpeed = 450f;
    [SerializeField] private float zoneDrag = 5f; // Lực cản giúp thanh không bị trơn tuột

    [SerializeField] private float progressGainSpeed = 0.35f; // Tăng tốc độ lên điểm
    [SerializeField] private float progressLossSpeed = 0.15f; // Giảm tốc độ tụt điểm khi trượt

    private PlayerInputHandler inputHandler;
    private FishingController controller;
    private bool isActive = false;

    private float fishPosition;
    private float fishTargetPosition;
    private float fishTimer;

    private float zonePosition;
    private float zoneVelocity;

    private float currentProgress;
    private float barHeight;

    private float defaultCatchZoneHeight = -1f;
    private float effectiveProgressGainSpeed;
    private float effectiveProgressLossSpeed;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void StartMinigame(PlayerInputHandler playerInput, FishingController fishingController)
    {
        if (barBackground == null || fishIcon == null || catchZone == null)
        {
            Debug.LogError("<color=red>[Balance Minigame] Lỗi: Chưa gán đủ các thành phần UI RectTransform trong Inspector!</color>");
            return;
        }

        inputHandler = playerInput;
        controller = fishingController;
        isActive = true;
        currentProgress = 0.3f;

        if (panelRoot != null) panelRoot.SetActive(true);

        Canvas.ForceUpdateCanvases();
        barHeight = barBackground.rect.height;
        if (barHeight <= 0f) barHeight = 300f;

        if (defaultCatchZoneHeight <= 0f)
        {
            defaultCatchZoneHeight = catchZone.rect.height > 0f ? catchZone.rect.height : 70f;
        }

        // ========================================================
        // KẾT NỐI CHỈ SỐ CẦN CÂU & PHAO CÂU VÀO MINIGAME
        // ========================================================
        FishingRodSO rod = controller != null ? controller.CurrentRod : null;
        BobberSO bobber = controller != null ? controller.CurrentBobber : null;

        // 1. Cần câu xịn -> Tăng chiều cao thanh bắt cá (Catch Zone) & Tăng tốc độ kéo điểm
        float bonusZoneHeight = 0f;
        effectiveProgressGainSpeed = progressGainSpeed;
        if (rod != null)
        {
            bonusZoneHeight = rod.fishingPower * 0.7f; // Power 15 -> +10.5px; Power 85 -> +60px
            effectiveProgressGainSpeed = progressGainSpeed * (1f + rod.fishingPower * 0.006f);
        }
        catchZone.sizeDelta = new Vector2(catchZone.sizeDelta.x, defaultCatchZoneHeight + bonusZoneHeight);

        // 2. Phao câu xịn -> Tăng độ ổn định, giảm tốc độ tụt điểm khi cá trượt
        effectiveProgressLossSpeed = progressLossSpeed;
        if (bobber != null && bobber.buoyancy > 0f)
        {
            effectiveProgressLossSpeed = progressLossSpeed / Mathf.Max(1f, bobber.buoyancy * 0.8f);
        }

        fishIcon.pivot = new Vector2(0.5f, 0f);
        fishIcon.anchorMin = new Vector2(0.5f, 0f);
        fishIcon.anchorMax = new Vector2(0.5f, 0f);

        catchZone.pivot = new Vector2(0.5f, 0f);
        catchZone.anchorMin = new Vector2(0.5f, 0f);
        catchZone.anchorMax = new Vector2(0.5f, 0f);

        fishPosition = 0f;
        fishTargetPosition = 0f;
        zonePosition = 0f;
        zoneVelocity = 0f;

        UpdateVisuals();
    }

    private void Update()
    {
        if (!isActive) return;

        HandleFishMovement();
        HandleZonePhysics();
        CalculateProgress();
        UpdateVisuals();
        CheckGameEnd();
    }

    private void HandleFishMovement()
    {
        float maxFishPos = Mathf.Max(0f, barBackground.rect.height - fishIcon.rect.height);

        fishTimer -= Time.deltaTime;
        if (fishTimer <= 0f)
        {
            fishTargetPosition = Random.Range(0f, maxFishPos);
            fishTimer = Random.Range(fishRandomTimeMin, fishRandomTimeMax);
        }

        fishPosition = Mathf.MoveTowards(fishPosition, fishTargetPosition, fishMoveSpeed * barBackground.rect.height * Time.deltaTime);
        fishPosition = Mathf.Clamp(fishPosition, 0f, maxFishPos);
    }

    private void HandleZonePhysics()
    {
        bool isHolding = inputHandler != null && inputHandler.IsInteractHeld;

        if (isHolding)
        {
            zoneVelocity += liftPower * Time.deltaTime;
        }
        else
        {
            zoneVelocity -= gravity * Time.deltaTime;
        }

        // Áp dụng lực cản (Drag) để giảm quán tính, giúp thanh dừng lại mượt mà khi nhấp nhả chuột
        zoneVelocity -= zoneVelocity * zoneDrag * Time.deltaTime;

        zoneVelocity = Mathf.Clamp(zoneVelocity, -maxSpeed, maxSpeed);
        zonePosition += zoneVelocity * Time.deltaTime;


        float maxZonePos = Mathf.Max(0f, barBackground.rect.height - catchZone.rect.height);
        if (zonePosition < 0f)
        {
            zonePosition = 0f;
            zoneVelocity = 0f;
        }
        else if (zonePosition > maxZonePos)
        {
            zonePosition = maxZonePos;
            zoneVelocity = -zoneVelocity * 0.3f; // Tạo độ nảy nhẹ (bounce) khi va vào đỉnh thay vì khựng lại
        }
    }

    private void CalculateProgress()
    {
        float fishMin = fishPosition;
        float fishMax = fishPosition + fishIcon.rect.height;
        float zoneMin = zonePosition;
        float zoneMax = zonePosition + catchZone.rect.height;

        bool isOverlapping = (fishMin < zoneMax && fishMax > zoneMin);

        if (isOverlapping)
        {
            currentProgress += effectiveProgressGainSpeed * Time.deltaTime;
        }
        else
        {
            currentProgress -= effectiveProgressLossSpeed * Time.deltaTime;
        }

        currentProgress = Mathf.Clamp01(currentProgress);
    }

    private void UpdateVisuals()
    {
        if (fishIcon != null) fishIcon.anchoredPosition = new Vector2(fishIcon.anchoredPosition.x, fishPosition);
        if (catchZone != null) catchZone.anchoredPosition = new Vector2(catchZone.anchoredPosition.x, zonePosition);
        if (progressBar != null) progressBar.value = currentProgress;
    }

    private void CheckGameEnd()
    {
        if (currentProgress >= 1f)
        {
            EndMinigame(true);
        }
        else if (currentProgress <= 0f)
        {
            EndMinigame(false);
        }
    }

    public void ForceStopMinigame()
    {
        isActive = false;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void EndMinigame(bool isSuccess)
    {
        isActive = false;
        if (panelRoot != null) panelRoot.SetActive(false);

        if (controller != null)
        {
            controller.OnMinigameEnd(isSuccess);
        }
    }
}