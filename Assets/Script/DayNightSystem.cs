using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayLengthInSeconds = 720f;
    [Range(0f, 1f)]
    public float currentTime = 0.25f;

    [Header("Lights & Environment")]
    public Light sunLight;
    public Light moonLight;

    [Header("Intensity Settings")]
    public float maxSunIntensity = 1.5f;
    [Tooltip("Cường độ sáng tối đa của mặt trăng khi ban đêm đạt đỉnh")]
    public float maxMoonIntensity = 0.1f;

    [Header("Skybox Settings")]
    public Material daySkybox;
    public Material nightSkybox;

    private float sunXRotation;

    void Update()
    {
        UpdateTime();
        UpdateIntensityAndSkybox();
    }

    void UpdateTime()
    {
        currentTime += Time.deltaTime / dayLengthInSeconds;

        if (currentTime >= 1f)
        {
            currentTime = 0f;
        }
    }

    void UpdateIntensityAndSkybox()
    {
        // Tính góc xoay của Mặt Trời
        sunXRotation = (currentTime * 360f) - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunXRotation, 170f, 0f);

        if (moonLight != null)
        {
            moonLight.transform.localRotation = Quaternion.Euler(sunXRotation + 180f, 170f, 0f);
        }

        // Tính toán độ chiếu sáng của mặt trời xuống mặt đất (độ cao mặt trời)
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        float sunFactor = Mathf.Clamp01(dotProduct);

        // Cập nhật cường độ sáng
        sunLight.intensity = Mathf.SmoothStep(0f, maxSunIntensity, sunFactor);

        if (moonLight != null)
        {
            moonLight.intensity = Mathf.SmoothStep(maxMoonIntensity, 0f, sunFactor);
        }


        if (sunFactor > 0f)
        {
            if (daySkybox != null && RenderSettings.skybox != daySkybox)
            {
                RenderSettings.skybox = daySkybox;
            }
        }
        else
        {
            if (nightSkybox != null && RenderSettings.skybox != nightSkybox)
            {
                RenderSettings.skybox = nightSkybox;
            }
        }
    }
}