using UnityEngine;

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

    private bool isRaining = false;
    private float nextRainCheckTime;
    private float rainEndTime;

    // Biến lưu trữ Instance mưa được sinh ra trong Scene
    private GameObject currentRainInstance;
    private ParticleSystem currentRainParticle;

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        nextRainCheckTime = Time.time + rainCheckInterval;
    }

    void Update()
    {
        UpdateFog();
        UpdateRainLogic();
        FollowPlayer();
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
        // Kiểm tra tỉ lệ mưa
        if (!isRaining && Time.time >= nextRainCheckTime)
        {
            nextRainCheckTime = Time.time + rainCheckInterval;

            if (Random.Range(0f, 100f) < rainChance)
            {
                StartRain();
            }
        }

        // Hết thời gian mưa
        if (isRaining && Time.time >= rainEndTime)
        {
            StopRain();
        }
    }

    void StartRain()
    {
        isRaining = true;
        rainEndTime = Time.time + rainDuration;

        // Sinh ra Prefab mưa và giữ nguyên 100% Transform (Position, Rotation, Scale) của Prefab
        if (rainPrefab != null && currentRainInstance == null)
        {
            currentRainInstance = Instantiate(rainPrefab);
            currentRainParticle = currentRainInstance.GetComponent<ParticleSystem>();

            // Cập nhật ngay vị trí ban đầu theo Player (giữ nguyên Rotation & Scale gốc của Prefab)
            if (playerTransform != null)
            {
                currentRainInstance.transform.position = GetRainPosition();
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
                // Tắt phát hạt mới và chờ hạt cũ rơi hết trước khi Destroy
                currentRainParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(rainToDestroy, 3f);
            }
            else
            {
                Destroy(rainToDestroy);
            }

            currentRainParticle = null;
        }
    }

    void FollowPlayer()
    {
        // Cho hệ thống mưa đi theo Player khi di chuyển
        if (isRaining && currentRainInstance != null && playerTransform != null)
        {
            currentRainInstance.transform.position = GetRainPosition();
        }
    }

    Vector3 GetRainPosition()
    {
        if (playerTransform != null)
        {
            Vector3 targetPos = playerTransform.position;
            targetPos.y += 20f; // Độ cao mưa so với Player
            return targetPos;
        }
        return transform.position;
    }
}