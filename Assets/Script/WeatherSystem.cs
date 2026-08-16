using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // BẮT BUỘC THÊM THƯ VIỆN NÀY

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

    [Header("--- Âm Thanh Mưa ---")]
    public AudioSource rainAudioSource;
    public float fadeDuration = 3f;
    [Range(0f, 1f)]
    public float maxRainVolume = 1f;

    private bool isRaining = false;
    private float nextRainCheckTime;
    private float rainEndTime;

    private GameObject currentRainInstance;
    private ParticleSystem currentRainParticle;
    private Coroutine audioFadeCoroutine;

    private float playerSearchTimer = 0f;
    private float playerSearchInterval = 1f;

    // --- BẮT SỰ KIỆN CHUYỂN SCENE ---
    private void OnEnable()
    {
        // Lắng nghe mỗi khi một Map mới được load xong
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Hủy lắng nghe và ép dừng khi script bị tắt
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ForceStopWeather();
    }

    // --- HÀM RESET CỨNG KHI ĐỔI MAP ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Ép tắt toàn bộ mưa, hạt mưa và âm thanh ngay lập tức
        ForceStopWeather();

        // 2. Reset lại bộ đếm thời gian để không bị mưa luôn ở Map mới
        nextRainCheckTime = Time.time + rainCheckInterval;

        // 3. Xóa transform của Player cũ đi để nó tự tìm Player ở Map mới
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
    }

    void Update()
    {
        UpdateFog();
        UpdateRainLogic();
        FollowPlayer();
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

        if (currentRainInstance != null)
        {
            Destroy(currentRainInstance);
            currentRainInstance = null;
            currentRainParticle = null;
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
            fogFactor = Mathf.Max(fogFactor, 0.6f);
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

    void StartRain()
    {
        isRaining = true;
        rainEndTime = Time.time + rainDuration;

        if (rainPrefab != null && currentRainInstance == null)
        {
            currentRainInstance = Instantiate(rainPrefab);
            currentRainParticle = currentRainInstance.GetComponent<ParticleSystem>();

            TryFindPlayer();

            if (playerTransform != null)
            {
                currentRainInstance.transform.position = GetRainPosition();
            }

            if (rainAudioSource != null)
            {
                if (audioFadeCoroutine != null) StopCoroutine(audioFadeCoroutine);
                audioFadeCoroutine = StartCoroutine(FadeAudio(rainAudioSource, maxRainVolume, fadeDuration));
            }
        }
    }

    void StopRain()
    {
        isRaining = false;
        nextRainCheckTime = Time.time + rainCheckInterval;

        if (currentRainInstance != null)
        {
            GameObject rainToDestroy = currentRainInstance;
            currentRainInstance = null;

            if (currentRainParticle != null)
            {
                currentRainParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(rainToDestroy, 3f);
            }
            else
            {
                Destroy(rainToDestroy);
            }

            currentRainParticle = null;

            if (rainAudioSource != null)
            {
                if (audioFadeCoroutine != null) StopCoroutine(audioFadeCoroutine);
                audioFadeCoroutine = StartCoroutine(FadeAudio(rainAudioSource, 0f, fadeDuration));
            }
        }
    }

    private IEnumerator FadeAudio(AudioSource audioSrc, float targetVolume, float duration)
    {
        if (audioSrc == null) yield break;

        if (!audioSrc.isPlaying && targetVolume > 0f)
        {
            audioSrc.volume = 0f;
            audioSrc.Play();
        }

        float startVolume = audioSrc.volume;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            if (audioSrc == null) yield break;

            audioSrc.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (audioSrc != null)
        {
            audioSrc.volume = targetVolume;
            if (targetVolume <= 0f)
            {
                audioSrc.Stop();
            }
        }
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