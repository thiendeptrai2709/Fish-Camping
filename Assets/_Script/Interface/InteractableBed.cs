using System.Collections;
using UnityEngine;

public class InteractableBed : MonoBehaviour, IInteractable
{
    [Header("Sleeping Settings")]
    [SerializeField] private string interactPrompt = "Click [Chuột Trái] để Ngủ";
    [SerializeField] private float sleepRestoreAmount = 100f;
    [SerializeField] private float energyRestoreAmount = 100f;

    [Header("Fade & Time Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float sleepHoldDuration = 1.0f;

    [Header("Chế độ ngủ")]
    [Tooltip("Tích chọn nếu muốn ngủ theo số tiếng (ví dụ ngủ 4 tiếng). Bỏ tích nếu muốn thức dậy vào một giờ cố định.")]
    [SerializeField] private bool sleepByDuration = true;
    [Tooltip("Số tiếng ngủ cộng dồn (VD: 4 = 4 tiếng, 8 = 8 tiếng)")]
    [SerializeField] private float sleepHours = 4f; 
    
    [Tooltip("Giờ thức dậy cố định nếu không chọn sleepByDuration (0.25 = 6:00 sáng, 0.875 = 21:00 tối)")]
    [Range(0f, 1f)]
    [SerializeField] private float fixedWakeUpTime = 0.25f;

    private PlayerInteraction playerInteraction;
    private int normalLayer;
    private int outlineLayer;
    private bool isSleeping = false;

    private void Awake()
    {
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");
        gameObject.layer = normalLayer;
    }

    public void Interact()
    {
        if (isSleeping) return;

        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        isSleeping = true;

        // 1. Màn hình tối dần sang đen
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(fadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        // 2. ĐANG TRONG MÀN ĐEN: Tua thời gian và cập nhật toàn bộ ánh sáng
        if (DayNightSystem.Instance != null)
        {
            if (sleepByDuration)
            {
                // Ngủ cộng thêm số tiếng (5h chiều ngủ 4 tiếng -> 9h tối thức dậy)
                DayNightSystem.Instance.AdvanceTime(sleepHours);
            }
            else
            {
                // Nhảy đến giờ cố định
                DayNightSystem.Instance.SetTime(fixedWakeUpTime);
            }
        }

        // Chờ 1 chút trong bóng tối + hồi máu/thể lực
        yield return new WaitForSeconds(sleepHoldDuration);

        if (playerInteraction != null)
        {
            ForcedSleepController forcedSleep = playerInteraction.GetComponent<ForcedSleepController>();
            if (forcedSleep != null) forcedSleep.ResetAllPenalties();

            SleepStatController sleepController = playerInteraction.GetComponent<SleepStatController>();
            CharacterStatsManager statsManager = playerInteraction.GetComponent<CharacterStatsManager>();

            if (sleepController != null) sleepController.Sleep(sleepRestoreAmount);
            if (statsManager != null) statsManager.ModifyStat(StatType.Energy, energyRestoreAmount);

            Debug.Log("Nhân vật đã ngủ và hồi phục sức khỏe!");
        }

        // 3. Màn hình sáng dần trở lại (Lúc này trời, đèn và Skybox đã chuyển cảnh hoàn tất)
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeIn(fadeDuration);
        }

        isSleeping = false;
    }

    public string GetInteractPrompt() => interactPrompt;

    public void OnFocus()
    {
        if (!isSleeping) SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
} 