using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    [Header("Time Reference")]
    public DayNightSystem dayNightSystem;

    [Header("Fog Settings")]
    public float maxFogDensity = 0.05f;
    public float minFogDensity = 0.005f;

    [Header("Rain Settings")]
    public ParticleSystem rainParticleSystem;
    public Transform playerTransform;
    public float rainCheckInterval = 30f;
    [Range(0f, 100f)]
    public float rainChance = 30f;

    private bool isRaining = false;
    private float nextRainCheckTime;
    private bool positionInitialized = false;

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        nextRainCheckTime = Time.time + rainCheckInterval;

        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        UpdateFog();
        UpdateRainLogic();
    }

    void UpdateFog()
    {
        if (dayNightSystem == null) return;

        float time = dayNightSystem.currentTime;
        float fogFactor = 0f;

        if (time >= 0f && time < 0.25f)
        {
            fogFactor = Mathf.InverseLerp(0f, 0.2f, time);
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
        if (Time.time >= nextRainCheckTime)
        {
            nextRainCheckTime = Time.time + rainCheckInterval;

            bool oldRainState = isRaining;
            isRaining = Random.Range(0f, 100f) < rainChance;

            if (isRaining && !oldRainState)
            {
                positionInitialized = false;
            }
        }

        if (rainParticleSystem != null)
        {
            if (isRaining)
            {
                if (!positionInitialized)
                {
                    CenterRainOnPlayer();
                    positionInitialized = true;
                }

                if (!rainParticleSystem.isPlaying)
                {
                    rainParticleSystem.Play(true);
                }
            }
            else
            {
                if (rainParticleSystem.isPlaying)
                {
                    rainParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
    }

    void CenterRainOnPlayer()
    {
        if (playerTransform != null && rainParticleSystem != null)
        {
            Vector3 spawnPosition = playerTransform.position;
            spawnPosition.y += 15f;
            rainParticleSystem.transform.position = spawnPosition;
        }
    }
}