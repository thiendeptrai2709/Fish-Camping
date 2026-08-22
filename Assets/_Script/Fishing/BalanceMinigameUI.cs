using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalanceMinigameUI : MonoBehaviour
{
    [Header("--- GIAO DIỆN CHÍNH ---")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform fishIcon;
    [SerializeField] private RectTransform catchZone;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI txtFishCombatStatus;

    [Header("--- THÔNG SỐ VẬT LÝ THANH BẮT (CATCH ZONE) ---")]
    [SerializeField] private float gravity = 800f;
    [SerializeField] private float liftPower = 1200f;
    [SerializeField] private float maxSpeed = 450f;
    [SerializeField] private float zoneDrag = 5f;

    [Header("--- THÔNG SỐ CƠ BẢN TIẾN ĐỘ ---")]
    [SerializeField] private float baseProgressGainSpeed = 0.32f;
    [SerializeField] private float baseProgressLossSpeed = 0.15f;

    private PlayerInputHandler inputHandler;
    private FishingController controller;
    private bool isActive = false;

    // Dữ liệu cá đang câu
    private FishSO currentHookedFish;
    private float fishMoveSpeed = 1.2f;
    private float fishRandomTimeMin = 1.0f;
    private float fishRandomTimeMax = 2.5f;
    private bool isDashing = false;
    private float dashTimer = 0f;

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

    private Vector2 originalBarAnchoredPos;
    private float shakeIntensity = 0f;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        if (barBackground != null)
        {
            originalBarAnchoredPos = barBackground.anchoredPosition;
        }
    }

    public void StartMinigame(PlayerInputHandler playerInput, FishingController fishingController, FishSO hookedFish = null)
    {
        if (barBackground == null || fishIcon == null || catchZone == null)
        {
            Debug.LogError("<color=red>[Balance Minigame] Lỗi: Chưa gán đủ các thành phần UI RectTransform trong Inspector!</color>");
            return;
        }

        inputHandler = playerInput;
        controller = fishingController;
        currentHookedFish = hookedFish;
        isActive = true;
        currentProgress = 0.32f;
        isDashing = false;
        dashTimer = 0f;
        shakeIntensity = 0f;

        if (panelRoot != null) panelRoot.SetActive(true);

        Canvas.ForceUpdateCanvases();
        barHeight = barBackground.rect.height;
        if (barHeight <= 0f) barHeight = 300f;

        if (defaultCatchZoneHeight <= 0f)
        {
            defaultCatchZoneHeight = catchZone.rect.height > 0f ? catchZone.rect.height : 70f;
        }

        // 1. TÍNH TOÁN ĐỘ KHÓ ĐỘNG DỰA THEO CÁ LỚN & ĐỘ HIẾM
        CalculateFishDifficulty(hookedFish);

        // 2. KẾT NỐI CHỈ SỐ CẦN CÂU & PHAO CÂU ĐỂ CÂN BẰNG
        FishingRodSO rod = controller != null ? controller.CurrentRod : null;
        BobberSO bobber = controller != null ? controller.CurrentBobber : null;

        // Cần câu xịn -> Tăng chiều cao thanh bắt cá (Catch Zone) & Tăng tốc độ kéo điểm
        float bonusZoneHeight = 0f;
        if (rod != null)
        {
            bonusZoneHeight = rod.fishingPower * 0.85f;
            effectiveProgressGainSpeed *= (1f + rod.fishingPower * 0.007f);
        }

        // Buff thức ăn kéo cước nhanh
        if (PlayerBuffManager.Instance != null && PlayerBuffManager.Instance.HasBuff(BuffType.ReelSpeed))
        {
            effectiveProgressGainSpeed *= PlayerBuffManager.Instance.GetBuffMultiplier(BuffType.ReelSpeed);
        }

        // Trọng lượng cá lớn làm co nhẹ thanh bắt cá cơ bản
        float fishWeightPenalty = 0f;
        if (hookedFish != null && hookedFish.maxWeight > 5f)
        {
            fishWeightPenalty = Mathf.Clamp((hookedFish.maxWeight - 5f) * 0.8f, 0f, 25f);
        }

        float finalZoneHeight = Mathf.Max(35f, defaultCatchZoneHeight + bonusZoneHeight - fishWeightPenalty);
        catchZone.sizeDelta = new Vector2(catchZone.sizeDelta.x, finalZoneHeight);

        // Phao câu xịn -> Giảm tốc độ tụt điểm khi cá trượt ra ngoài
        if (bobber != null && bobber.buoyancy > 0f)
        {
            effectiveProgressLossSpeed /= Mathf.Max(1f, bobber.buoyancy * 0.75f);
        }

        // Buff thức ăn cước bền
        if (PlayerBuffManager.Instance != null && PlayerBuffManager.Instance.HasBuff(BuffType.LineTension))
        {
            effectiveProgressLossSpeed *= 0.75f;
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

    private void CalculateFishDifficulty(FishSO fish)
    {
        if (fish == null)
        {
            fishMoveSpeed = 1.2f;
            fishRandomTimeMin = 1.0f;
            fishRandomTimeMax = 2.2f;
            effectiveProgressGainSpeed = baseProgressGainSpeed;
            effectiveProgressLossSpeed = baseProgressLossSpeed;
            if (txtFishCombatStatus != null) txtFishCombatStatus.text = "";
            return;
        }

        switch (fish.rarity)
        {
            case FishRarity.Common:
                fishMoveSpeed = 1.1f + (fish.difficulty * 0.1f);
                fishRandomTimeMin = 1.2f;
                fishRandomTimeMax = 2.4f;
                effectiveProgressGainSpeed = baseProgressGainSpeed * 1.1f;
                effectiveProgressLossSpeed = baseProgressLossSpeed * 0.9f;
                if (txtFishCombatStatus != null) txtFishCombatStatus.text = "Cá Nhỏ Điềm Tĩnh";
                break;

            case FishRarity.Uncommon:
                fishMoveSpeed = 1.7f + (fish.difficulty * 0.15f);
                fishRandomTimeMin = 0.8f;
                fishRandomTimeMax = 1.8f;
                effectiveProgressGainSpeed = baseProgressGainSpeed * 1.0f;
                effectiveProgressLossSpeed = baseProgressLossSpeed * 1.2f;
                if (txtFishCombatStatus != null) txtFishCombatStatus.text = "Cá Nhanh Nhẹn";
                break;

            case FishRarity.Rare:
                fishMoveSpeed = 2.4f + (fish.difficulty * 0.2f);
                fishRandomTimeMin = 0.5f;
                fishRandomTimeMax = 1.3f;
                effectiveProgressGainSpeed = baseProgressGainSpeed * 0.9f;
                effectiveProgressLossSpeed = baseProgressLossSpeed * 1.6f;
                if (txtFishCombatStatus != null) txtFishCombatStatus.text = "Cá Lớn Giãy Mạnh!";
                break;

            case FishRarity.Legendary:
                fishMoveSpeed = 3.6f + (fish.difficulty * 0.25f);
                fishRandomTimeMin = 0.25f;
                fishRandomTimeMax = 0.75f;
                effectiveProgressGainSpeed = baseProgressGainSpeed * 0.8f;
                effectiveProgressLossSpeed = baseProgressLossSpeed * 2.2f;
                if (txtFishCombatStatus != null) txtFishCombatStatus.text = "THỦY QUÁI HUYỀN THOẠI!";
                break;
        }

        if (fish.maxWeight > 10f)
        {
            fishMoveSpeed += Mathf.Min(1.5f, (fish.maxWeight - 10f) * 0.05f);
        }
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

            if (currentHookedFish != null && (currentHookedFish.rarity == FishRarity.Rare || currentHookedFish.rarity == FishRarity.Legendary))
            {
                float dashChance = currentHookedFish.rarity == FishRarity.Legendary ? 0.65f : 0.4f;
                if (Random.value < dashChance)
                {
                    isDashing = true;
                    dashTimer = Random.Range(0.35f, 0.7f);
                    shakeIntensity = currentHookedFish.rarity == FishRarity.Legendary ? 7f : 4f;
                }
            }
        }

        float currentSpeed = fishMoveSpeed;
        if (isDashing)
        {
            currentSpeed *= 1.8f;
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }

        fishPosition = Mathf.MoveTowards(fishPosition, fishTargetPosition, currentSpeed * barBackground.rect.height * Time.deltaTime);
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
            zoneVelocity = -zoneVelocity * 0.3f;
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

            if (currentHookedFish != null && currentHookedFish.rarity >= FishRarity.Rare)
            {
                shakeIntensity = Mathf.Max(shakeIntensity, 3f);
            }
        }

        currentProgress = Mathf.Clamp01(currentProgress);
    }

    private void UpdateVisuals()
    {
        if (fishIcon != null)
        {
            fishIcon.anchoredPosition = new Vector2(fishIcon.anchoredPosition.x, fishPosition);
        }

        if (catchZone != null)
        {
            catchZone.anchoredPosition = new Vector2(catchZone.anchoredPosition.x, zonePosition);
        }

        if (progressBar != null)
        {
            progressBar.value = currentProgress;
        }

        // Rung lắc thanh minigame khi cá lớn giãy mạnh
        if (barBackground != null)
        {
            if (shakeIntensity > 0.05f)
            {
                float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
                float offsetY = Random.Range(-shakeIntensity, shakeIntensity) * 0.5f;
                barBackground.anchoredPosition = originalBarAnchoredPos + new Vector2(offsetX, offsetY);
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, Time.deltaTime * 8f);
            }
            else
            {
                barBackground.anchoredPosition = originalBarAnchoredPos;
            }
        }
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
        if (barBackground != null) barBackground.anchoredPosition = originalBarAnchoredPos;
    }

    private void EndMinigame(bool isSuccess)
    {
        isActive = false;
        if (panelRoot != null) panelRoot.SetActive(false);
        if (barBackground != null) barBackground.anchoredPosition = originalBarAnchoredPos;

        if (controller != null)
        {
            controller.OnMinigameEnd(isSuccess);
        }
    }
}