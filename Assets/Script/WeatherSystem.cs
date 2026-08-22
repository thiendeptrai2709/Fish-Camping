using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public enum RainIntensityType
{
    Light,      // Mưa nhỏ / Mưa phùn
    Moderate,   // Mưa vừa
    Heavy       // Mưa to / Mưa rào
}

public class WeatherSystem : MonoBehaviour
{
    [Header("Time Reference")]
    public DayNightSystem dayNightSystem;

    [Header("Fog Settings")]
    public float maxFogDensity = 0.05f;
    public float minFogDensity = 0.005f;

    [Header("Rain Settings")]
    [Tooltip("Kéo file Prefab Mưa từ cửa sổ Project vào đây")]
    public GameObject rainPrefab;
    public Transform playerTransform;
    public float rainCheckInterval = 60f;
    public float rainDuration = 240f;
    [Range(0f, 100f)]
    public float rainChance = 30f;

    [Header("--- Dynamic Rain Intensity (Mưa to / Mưa nhỏ) ---")]
    [Tooltip("Cho phép ngẫu nhiên cường độ mưa to / mưa nhỏ")]
    public bool enableRandomIntensity = true;

    [Tooltip("Tốc độ hạt khi mưa nhỏ (hạt/giây)")]
    public float lightRainRate = 200f;

    [Tooltip("Tốc độ hạt khi mưa vừa (hạt/giây)")]
    public float moderateRainRate = 500f;

    [Tooltip("Tốc độ hạt khi mưa to (hạt/giây)")]
    public float heavyRainRate = 850f;

    [Tooltip("Khoảng thời gian (giây) tự động chuyển đổi cường độ ngẫu nhiên trong cơn mưa")]
    public float intensityChangeInterval = 45f;

    [SerializeField] private RainIntensityType currentRainType = RainIntensityType.Moderate;

    [Header("--- Âm Thanh Mưa ---")]
    public AudioSource rainAudioSource;
    public float fadeDuration = 3f;
    [Range(0f, 1f)]
    public float maxRainVolume = 1f;

    public RainIntensityType CurrentRainType => currentRainType;
    public bool IsRaining => isRaining;

    private bool isRaining = false;
    private float nextRainCheckTime;
    private float rainEndTime;
    private float nextIntensityChangeTime;

    private float targetRainRate = 500f;
    private float currentRainRate = 500f;
    private float targetRainVolume = 0.7f;

    private GameObject currentRainInstance;
    private ParticleSystem currentRainParticle;
    private Coroutine audioFadeCoroutine;

    private float playerSearchTimer = 0f;
    private float playerSearchInterval = 1f;

    // --- BẮT SỰ KIỆN CHUYỂN SCENE ---
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ForceStopWeather();
    }

    // --- HÀM RESET CỨNG KHI ĐỔI MAP ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ForceStopWeather();
        nextRainCheckTime = Time.time + rainCheckInterval;
        playerTransform = null;
    }

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        nextRainCheckTime = Time.time + rainCheckInterval;

        if (rainAudioSource != null && !isRaining)
        {
            rainAudioSource.volume = 0f;
        }

        EnsureRainInstance();
    }

    void Update()
    {
        UpdateFog();
        UpdateRainLogic();
        UpdateRainIntensity();
        FollowPlayer();
    }

    private void EnsureRainInstance()
    {
        if (rainPrefab != null && currentRainInstance == null)
        {
            currentRainInstance = Instantiate(rainPrefab);
            currentRainParticle = currentRainInstance.GetComponent<ParticleSystem>();
            if (currentRainParticle != null)
            {
                currentRainParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    // --- HÀM DỌN DẸP SẠCH SẼ MỌI THỨ ---
    private void ForceStopWeather()
    {
        isRaining = false;

        if (audioFadeCoroutine != null)
        {
            StopCoroutine(audioFadeCoroutine);
            audioFadeCoroutine = null;
        }

        if (rainAudioSource != null)
        {
            rainAudioSource.Stop();
            rainAudioSource.volume = 0f;
        }

        if (currentRainParticle != null)
        {
            currentRainParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void UpdateFog()
    {
        if (dayNightSystem == null) return;

        float time = dayNightSystem.currentTime;
        float fogFactor = 0f;

        if (time >= 0.20f && time < 0.25f)
        {
            fogFactor = Mathf.InverseLerp(0.20f, 0.25f, time);
        }
        else if (time >= 0.25f && time < 0.35f)
        {
            fogFactor = Mathf.InverseLerp(0.35f, 0.25f, time);
        }

        if (isRaining)
        {
            // Sương mù dày hơn tương ứng với cấp độ mưa (Mưa to -> Sương mù dày hơn)
            float rainFogMultiplier = (currentRainType == RainIntensityType.Light) ? 0.45f :
                                      (currentRainType == RainIntensityType.Moderate) ? 0.65f : 0.88f;
            fogFactor = Mathf.Max(fogFactor, rainFogMultiplier);
        }

        RenderSettings.fogDensity = Mathf.Lerp(minFogDensity, maxFogDensity, fogFactor);
    }

    void UpdateRainLogic()
    {
        if (!isRaining && Time.time >= nextRainCheckTime)
        {
            nextRainCheckTime = Time.time + rainCheckInterval;

            if (Random.Range(0f, 100f) < rainChance)
            {
                StartRain();
            }
        }

        if (isRaining && Time.time >= rainEndTime)
        {
            StopRain();
        }
    }

    private void UpdateRainIntensity()
    {
        if (!isRaining || currentRainParticle == null) return;

        // Đổi ngẫu nhiên cường độ mưa theo chu kỳ trong lúc đang mưa
        if (enableRandomIntensity && Time.time >= nextIntensityChangeTime)
        {
            nextIntensityChangeTime = Time.time + intensityChangeInterval;
            RollRandomRainIntensity();
        }

        // Chuyển đổi mượt mà số lượng hạt mưa
        currentRainRate = Mathf.MoveTowards(currentRainRate, targetRainRate, Time.deltaTime * 150f);
        var emission = currentRainParticle.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(currentRainRate);

        // Chuyển đổi mượt mà âm lượng tiếng mưa theo cường độ
        if (rainAudioSource != null && audioFadeCoroutine == null)
        {
            rainAudioSource.volume = Mathf.MoveTowards(rainAudioSource.volume, targetRainVolume, Time.deltaTime * 0.4f);
        }
    }

    private void RollRandomRainIntensity()
    {
        float roll = Random.Range(0f, 100f);
        if (roll < 40f)
        {
            SetRainIntensity(RainIntensityType.Light);
        }
        else if (roll < 75f)
        {
            SetRainIntensity(RainIntensityType.Moderate);
        }
        else
        {
            SetRainIntensity(RainIntensityType.Heavy);
        }
    }

    public void SetRainIntensity(RainIntensityType type)
    {
        currentRainType = type;
        switch (type)
        {
            case RainIntensityType.Light:
                targetRainRate = lightRainRate;
                targetRainVolume = maxRainVolume * 0.35f;
                break;
            case RainIntensityType.Moderate:
                targetRainRate = moderateRainRate;
                targetRainVolume = maxRainVolume * 0.70f;
                break;
            case RainIntensityType.Heavy:
                targetRainRate = heavyRainRate;
                targetRainVolume = maxRainVolume * 1.00f;
                break;
        }
    }

    void StartRain()
    {
        isRaining = true;
        rainEndTime = Time.time + rainDuration;
        nextIntensityChangeTime = Time.time + intensityChangeInterval;

        RollRandomRainIntensity();
        currentRainRate = targetRainRate;

        EnsureRainInstance();

        if (currentRainInstance != null)
        {
            TryFindPlayer();

            if (playerTransform != null)
            {
                currentRainInstance.transform.position = GetRainPosition();
            }

            if (currentRainParticle != null)
            {
                var emission = currentRainParticle.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(currentRainRate);
                currentRainParticle.Play(true);
            }

            if (rainAudioSource != null)
            {
                if (audioFadeCoroutine != null) StopCoroutine(audioFadeCoroutine);
                audioFadeCoroutine = StartCoroutine(FadeAudio(rainAudioSource, targetRainVolume, fadeDuration));
            }
        }
    }

    void StopRain()
    {
        isRaining = false;
        nextRainCheckTime = Time.time + rainCheckInterval;

        if (currentRainParticle != null)
        {
            currentRainParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (rainAudioSource != null)
        {
            if (audioFadeCoroutine != null) StopCoroutine(audioFadeCoroutine);
            audioFadeCoroutine = StartCoroutine(FadeAudio(rainAudioSource, 0f, fadeDuration));
        }
    }

    private IEnumerator FadeAudio(AudioSource audioSrc, float targetVol, float duration)
    {
        if (audioSrc == null) yield break;

        if (!audioSrc.isPlaying && targetVol > 0f)
        {
            audioSrc.volume = 0f;
            audioSrc.Play();
        }

        float startVolume = audioSrc.volume;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            if (audioSrc == null) yield break;

            audioSrc.volume = Mathf.Lerp(startVolume, targetVol, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (audioSrc != null)
        {
            audioSrc.volume = targetVol;
            if (targetVol <= 0f)
            {
                audioSrc.Stop();
            }
        }

        audioFadeCoroutine = null;
    }

    void FollowPlayer()
    {
        if (playerTransform == null)
        {
            TryFindPlayer();
        }

        if (isRaining && currentRainInstance != null && playerTransform != null)
        {
            currentRainInstance.transform.position = GetRainPosition();
        }
    }

    private void TryFindPlayer()
    {
        playerSearchTimer -= Time.deltaTime;
        if (playerSearchTimer <= 0f)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            playerSearchTimer = playerSearchInterval;
        }
    }

    Vector3 GetRainPosition()
    {
        if (playerTransform != null)
        {
            Vector3 targetPos = playerTransform.position;
            targetPos.y += 20f;
            return targetPos;
        }
        return transform.position;
    }
}