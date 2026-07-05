using UnityEngine;
using UnityEngine.UI;

public class BalanceMinigameUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform fishIcon;
    [SerializeField] private RectTransform catchZone;
    [SerializeField] private Slider progressBar;

    [SerializeField] private float fishMoveSpeed = 3f;
    [SerializeField] private float fishRandomTimeMin = 0.5f;
    [SerializeField] private float fishRandomTimeMax = 1.5f;

    [SerializeField] private float gravity = 400f;
    [SerializeField] private float liftPower = 600f;
    [SerializeField] private float maxSpeed = 300f;

    [SerializeField] private float progressGainSpeed = 0.25f;
    [SerializeField] private float progressLossSpeed = 0.2f;

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
            zoneVelocity = 0f;
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
            currentProgress += progressGainSpeed * Time.deltaTime;
        }
        else
        {
            currentProgress -= progressLossSpeed * Time.deltaTime;
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