using UnityEngine;
using TMPro;
using System;

public enum TimePeriod
{
    Morning,    // 05:00 - 09:00 (Sáng sớm - Cá ăn mồi nhanh)
    Day,        // 09:00 - 17:00 (Ban ngày - Tiêu chuẩn)
    Sunset,     // 17:00 - 20:00 (Hoàng hôn - Cá cắn câu mạnh)
    Night       // 20:00 - 05:00 (Đêm khuya - Tăng tỉ lệ cá hiếm & quái vật)
}

public class FishEcologyManager : MonoBehaviour
{
    public static FishEcologyManager Instance { get; private set; }

    [Header("--- THIẾT LẬP THỜI GIAN GAME ---")]
    [Tooltip("Thời gian 1 ngày trong game (tính bằng giây thực tế)")]
    [SerializeField] private float realSecondsPerGameDay = 720f; // 12 phút thực = 1 ngày game
    [SerializeField] private float startHour = 8f; // Bắt đầu lúc 8h sáng

    private float currentGameHour;
    private TimePeriod currentPeriod = TimePeriod.Day;
    private TimePeriod lastAnnouncedPeriod = TimePeriod.Day;

    [Header("--- CLOCK UI REFERENCE ---")]
    [SerializeField] private TextMeshProUGUI timeDayText;
    private float findUITimer = 0f;

    public event Action<TimePeriod> OnTimePeriodChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("FishEcologyManager");
            obj.AddComponent<FishEcologyManager>();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentGameHour = startHour;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindTimeDayTextUI();
    }

    private void Update()
    {
        // Cập nhật giờ trong game
        float hoursPerSecond = 24f / Mathf.Max(60f, realSecondsPerGameDay);
        currentGameHour = (currentGameHour + Time.deltaTime * hoursPerSecond) % 24f;

        TimePeriod newPeriod = GetPeriodFromHour(currentGameHour);
        if (newPeriod != currentPeriod)
        {
            currentPeriod = newPeriod;
            OnTimePeriodChanged?.Invoke(currentPeriod);

            if (currentPeriod != lastAnnouncedPeriod)
            {
                lastAnnouncedPeriod = currentPeriod;
                AnnounceEcologyState();
            }
        }

        UpdateClockUI();
    }

    private void FindTimeDayTextUI()
    {
        if (timeDayText != null) return;

        // Quét tìm TextMeshProUGUI tên là "TImeDay" hoặc "TimeDay" trong Scene
        TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in tmps)
        {
            if (t == null) continue;
            string tName = t.gameObject.name.ToLower();
            if (tName.Contains("timeday") || tName.Equals("timeday") || tName.Equals("timedate"))
            {
                timeDayText = t;
                break;
            }
        }
    }

    private void UpdateClockUI()
    {
        if (timeDayText == null)
        {
            findUITimer += Time.deltaTime;
            if (findUITimer >= 1f)
            {
                findUITimer = 0f;
                FindTimeDayTextUI();
            }
            return;
        }

        int hour = Mathf.FloorToInt(currentGameHour);
        int minute = Mathf.FloorToInt((currentGameHour - hour) * 60f);

        string periodName = "Ngày";
        Color periodColor = Color.white;

        switch (currentPeriod)
        {
            case TimePeriod.Morning:
                periodName = "Sáng";
                periodColor = new Color(1f, 0.92f, 0.6f);
                break;
            case TimePeriod.Day:
                periodName = "Ngày";
                periodColor = Color.white;
                break;
            case TimePeriod.Sunset:
                periodName = "Hoàng hôn";
                periodColor = new Color(1f, 0.68f, 0.35f);
                break;
            case TimePeriod.Night:
                periodName = "Đêm";
                periodColor = new Color(0.65f, 0.85f, 1f);
                break;
        }

        timeDayText.text = $"<b>{hour:D2}:{minute:D2}</b>  <size=75%>{periodName}</size>";
        timeDayText.color = periodColor;
    }

    private TimePeriod GetPeriodFromHour(float hour)
    {
        if (hour >= 5f && hour < 9f) return TimePeriod.Morning;
        if (hour >= 9f && hour < 17f) return TimePeriod.Day;
        if (hour >= 17f && hour < 20f) return TimePeriod.Sunset;
        return TimePeriod.Night;
    }

    public TimePeriod CurrentPeriod => currentPeriod;
    public float CurrentHour => currentGameHour;

    public string GetFormattedTime()
    {
        int hour = Mathf.FloorToInt(currentGameHour);
        int minute = Mathf.FloorToInt((currentGameHour - hour) * 60f);
        return $"{hour:D2}:{minute:D2}";
    }

    /// <summary>
    /// Điểm thưởng Rarity (tăng cơ hội bắt cá Rare/Legendary) dựa theo thời gian và môi trường
    /// </summary>
    public int GetEcologyRarityBonus()
    {
        int bonus = 0;
        switch (currentPeriod)
        {
            case TimePeriod.Night:
                bonus += 2; // Đêm khuya: Tăng mạnh tỷ lệ cá hiếm
                break;
            case TimePeriod.Sunset:
                bonus += 1; // Hoàng hôn: Tăng nhẹ cá hiếm
                break;
            case TimePeriod.Morning:
                bonus += 1; // Sáng sớm: Tăng cá giá trị
                break;
        }

        // Tích hợp thêm từ Buff thức ăn của người chơi
        if (PlayerBuffManager.Instance != null && PlayerBuffManager.Instance.HasBuff(BuffType.AnglerLuck))
        {
            bonus += Mathf.RoundToInt(PlayerBuffManager.Instance.GetBuffMultiplier(BuffType.AnglerLuck) * 2f);
        }

        return bonus;
    }

    /// <summary>
    /// Hệ số giảm thời gian chờ cá cắn câu
    /// </summary>
    public float GetBiteWaitMultiplier()
    {
        switch (currentPeriod)
        {
            case TimePeriod.Morning:
                return 0.7f; // Sáng sớm: Cắn câu nhanh hơn 30%
            case TimePeriod.Sunset:
                return 0.8f; // Hoàng hôn: Cắn câu nhanh hơn 20%
            case TimePeriod.Night:
                return 0.9f;
            default:
                return 1.0f;
        }
    }

    private void AnnounceEcologyState()
    {
        FishingController fc = FindFirstObjectByType<FishingController>();
        if (fc == null) return;

        switch (currentPeriod)
        {
            case TimePeriod.Morning:
                fc.ShowFishingFeedback("Sáng Sớm: Không khí trong lành, cá đi ăn mồi rất nhanh!", new Color(1f, 0.9f, 0.5f));
                break;
            case TimePeriod.Day:
                fc.ShowFishingFeedback("Ban Ngày: Ánh nắng ấm áp trên mặt hồ.", new Color(0.9f, 0.9f, 0.9f));
                break;
            case TimePeriod.Sunset:
                fc.ShowFishingFeedback("Hoàng Hôn: Thời điểm vàng để săn cá lớn!", new Color(1f, 0.6f, 0.3f));
                break;
            case TimePeriod.Night:
                fc.ShowFishingFeedback("Đêm Khuya: Cá săn mồi và cá Huyền Thoại bắt đầu xuất hiện!", new Color(0.6f, 0.8f, 1f));
                break;
        }
    }
}
