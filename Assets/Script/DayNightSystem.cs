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
    public Material dayNightSkyboxMaterial;

    private float sunXRotation;

    void Update()
    {
        UpdateIntensityAndSkybox();
        UpdateTime();
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
        sunXRotation = (currentTime * 360f) - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunXRotation, 170f, 0f);

        if (moonLight != null)
        {
            moonLight.transform.localRotation = Quaternion.Euler(sunXRotation + 180f, 170f, 0f);
        }

        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        float sunFactor = Mathf.Clamp01(dotProduct);

        sunLight.intensity = Mathf.SmoothStep(0f, maxSunIntensity, sunFactor);

        if (moonLight != null)
        {
            moonLight.intensity = Mathf.SmoothStep(maxMoonIntensity, 0f, sunFactor);
        }

        if (dayNightSkyboxMaterial != null)
        {
            float blendValue = 1f - sunFactor;
            dayNightSkyboxMaterial.SetFloat("_Blend", blendValue);
        }
    }
}