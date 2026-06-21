using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalanceMinigame : MonoBehaviour
{
    [SerializeField] private VehicleInput vehicleInput;
    [Header("UI Elements")]
    [SerializeField] private GameObject minigameCanvas;
    [SerializeField] private RectTransform needle;
    [SerializeField] private RectTransform safeZone;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Settings")]
    [SerializeField] private float gravity = 250f;
    [SerializeField] private float pushForce = 400f;
    [SerializeField] private float winTimeRequired = 3f;
    [SerializeField] private float barHeight = 400f;
    [SerializeField] private float safeZoneMoveSpeed = 80f;
    [SerializeField] private float safeZoneChangeInterval = 1.5f;


    private System.Action onWinCallback;
    private bool isPlaying = false;
    private float currentNeedlePos = 0f;
    private float currentProgress = 0f;
    private float safeZoneTargetY = 0f;
    private float safeZoneTimer = 0f;

    public bool IsPlaying => isPlaying;

    public void BeginMinigame(string title, System.Action onWinAction)
    {
        onWinCallback = onWinAction;
        titleText.text = $"{title}\n<size=24>(Giữ SPACE để giữ kim trong vùng xanh)</size>";
        currentNeedlePos = -barHeight / 2f;
        currentProgress = 0f;
        progressSlider.value = 0f;

        RandomizeSafeZone();
        safeZoneTimer = safeZoneChangeInterval;
        minigameCanvas.SetActive(true);
        isPlaying = true;
    }

    private void RandomizeSafeZone()
    {
        float limit = (barHeight / 2f) - (safeZone.rect.height / 2f);
        safeZoneTargetY = Random.Range(-limit, limit);
        safeZone.anchoredPosition = new Vector2(0f, safeZoneTargetY);
    }

    private void Update()
    {
        if (!isPlaying) return;

        MoveSafeZoneRandomly();
        HandleNeedleMovement();
        CheckWinCondition();
    }
    private void MoveSafeZoneRandomly()
    {
        safeZoneTimer -= Time.deltaTime;
        if (safeZoneTimer <= 0f)
        {
            safeZoneTimer = safeZoneChangeInterval;
            float limit = (barHeight / 2f) - (safeZone.rect.height / 2f);
            safeZoneTargetY = Random.Range(-limit, limit);
        }

        float currentY = safeZone.anchoredPosition.y;
        float newY = Mathf.MoveTowards(currentY, safeZoneTargetY, safeZoneMoveSpeed * Time.deltaTime);
        safeZone.anchoredPosition = new Vector2(0f, newY);
    }
    private void HandleNeedleMovement()
    {
        if (vehicleInput != null && vehicleInput.IsPushing)
        {
            currentNeedlePos += pushForce * Time.deltaTime;
        }
        else
        {
            currentNeedlePos -= gravity * Time.deltaTime;
        }

        currentNeedlePos = Mathf.Clamp(currentNeedlePos, -barHeight / 2f, barHeight / 2f);
        needle.anchoredPosition = new Vector2(0f, currentNeedlePos);
    }

    private void CheckWinCondition()
    {
        float zoneMinY = safeZone.anchoredPosition.y - (safeZone.rect.height / 2f);
        float zoneMaxY = safeZone.anchoredPosition.y + (safeZone.rect.height / 2f);

        if (currentNeedlePos >= zoneMinY && currentNeedlePos <= zoneMaxY)
        {
            currentProgress += Time.deltaTime;
            progressSlider.value = currentProgress / winTimeRequired;

            if (currentProgress >= winTimeRequired)
            {
                isPlaying = false;
                minigameCanvas.SetActive(false);
                onWinCallback?.Invoke();
            }
        }
        else
        {
            currentProgress = Mathf.Max(0f, currentProgress - Time.deltaTime);
            progressSlider.value = currentProgress / winTimeRequired;
        }
    }

    public void ForceAbort()
    {
        if (!isPlaying) return;
        isPlaying = false;
        minigameCanvas.SetActive(false);
    }
}