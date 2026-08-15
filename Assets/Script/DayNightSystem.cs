using UnityEngine;
using UnityEngine.SceneManagement;

public class DayNightSystem : MonoBehaviour
{
    public static DayNightSystem Instance { get; private set; }

    [Header("Cấu hình thời gian")]
    public float dayLengthInSeconds = 720f;
    [Range(0f, 1f)]
    public float currentTime = 0.25f;

    [Header("Nguồn sáng")]
    public Light sunLight;
    public Light moonLight;

    [Header("Cường độ sáng")]
    public float maxSunIntensity = 1.5f;
    public float maxMoonIntensity = 0.05f;

    [Header("Skybox Hòa Trộn (Blended Skybox)")]
    [Tooltip("Kéo Material 'Mat_SkyboxBlended' vào đây")]
    public Material blendedSkyboxMaterial;

    [Header("Dải màu chuyển tiếp")]
    public Gradient sunColorGradient;
    public Gradient ambientColorGradient;

    private float sunXRotation;
    public bool IsNight { get; private set; }

    private void Awake()
    {
        // Giữ hệ thống thời gian không bị hủy khi đổi Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupDefaultGradients();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Khi qua Map mới hoặc quay về Map cũ: Tự tìm lại đèn và gán lại Skybox
        ApplySkyboxMaterial();
        FindSceneLights();
        UpdateLightingAndSkybox();
    }

    private void Start()
    {
        ApplySkyboxMaterial();
        FindSceneLights();
    }

    private void ApplySkyboxMaterial()
    {
        if (blendedSkyboxMaterial != null)
        {
            RenderSettings.skybox = blendedSkyboxMaterial;
        }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    }

    private void FindSceneLights()
    {
        if (sunLight == null)
        {
            GameObject sun = GameObject.FindGameObjectWithTag("Sun");
            if (sun == null) sun = GameObject.Find("Sun Light");
            if (sun != null) sunLight = sun.GetComponent<Light>();
        }

        if (moonLight == null)
        {
            GameObject moon = GameObject.FindGameObjectWithTag("Moon");
            if (moon == null) moon = GameObject.Find("Moon Light");
            if (moon != null) moonLight = moon.GetComponent<Light>();
        }
    }

    private void Update()
    {
        FindSceneLights();
        UpdateTime();
        UpdateLightingAndSkybox();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime / dayLengthInSeconds;
        if (currentTime >= 1f) currentTime = 0f;
    }

    private void UpdateLightingAndSkybox()
    {
        // 1. Góc quay mặt trời và mặt trăng
        sunXRotation = (currentTime * 360f) - 90f;

        if (sunLight != null)
        {
            sunLight.transform.localRotation = Quaternion.Euler(sunXRotation, 170f, 0f);
        }

        if (moonLight != null)
        {
            moonLight.transform.localRotation = Quaternion.Euler(sunXRotation + 180f, 170f, 0f);
        }

        // 2. Tính hệ số chuyển giao mềm
        float dotProduct = sunLight != null ? Vector3.Dot(sunLight.transform.forward, Vector3.down) : Mathf.Sin(currentTime * Mathf.PI * 2f);
        float rawSunFactor = Mathf.InverseLerp(-0.25f, 0.25f, dotProduct);
        float smoothSunFactor = Mathf.SmoothStep(0f, 1f, rawSunFactor);

        IsNight = smoothSunFactor < 0.15f;
        // 3. Cập nhật cường độ đèn
        if (sunLight != null)
        {
            sunLight.intensity = smoothSunFactor * maxSunIntensity;
            sunLight.color = sunColorGradient.Evaluate(currentTime);
        }

        if (moonLight != null)
        {
            float moonFactor = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, -0.2f, dotProduct));
            moonLight.intensity = moonFactor * maxMoonIntensity;
        }

        // 4. Ánh sáng môi trường
        RenderSettings.ambientLight = ambientColorGradient.Evaluate(currentTime);

        // 5. Cập nhật Blend Factor trên Skybox
        if (blendedSkyboxMaterial != null)
        {
            float nightBlend = 1f - smoothSunFactor;
            blendedSkyboxMaterial.SetFloat("_Blend", nightBlend);
        }
    }

    private void SetupDefaultGradients()
    {
        if (sunColorGradient == null || sunColorGradient.colorKeys.Length == 0)
        {
            sunColorGradient = new Gradient();
            GradientColorKey[] gck = new GradientColorKey[5];
            gck[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.25f), 0.0f);
            gck[1] = new GradientColorKey(new Color(1.0f, 0.5f, 0.2f), 0.22f);
            gck[2] = new GradientColorKey(new Color(1.0f, 0.95f, 0.85f), 0.5f);
            gck[3] = new GradientColorKey(new Color(1.0f, 0.45f, 0.2f), 0.75f);
            gck[4] = new GradientColorKey(new Color(0.1f, 0.1f, 0.25f), 0.85f);
            GradientAlphaKey[] gak = new GradientAlphaKey[2] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            sunColorGradient.SetKeys(gck, gak);
        }

        if (ambientColorGradient == null || ambientColorGradient.colorKeys.Length == 0)
        {
            ambientColorGradient = new Gradient();
            GradientColorKey[] gck = new GradientColorKey[5];
            gck[0] = new GradientColorKey(new Color(0.02f, 0.02f, 0.05f), 0.0f);
            gck[1] = new GradientColorKey(new Color(0.4f, 0.25f, 0.2f), 0.22f);
            gck[2] = new GradientColorKey(new Color(0.6f, 0.65f, 0.7f), 0.5f);
            gck[3] = new GradientColorKey(new Color(0.45f, 0.2f, 0.15f), 0.75f);
            gck[4] = new GradientColorKey(new Color(0.02f, 0.02f, 0.05f), 0.85f);
            GradientAlphaKey[] gak = new GradientAlphaKey[2] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            ambientColorGradient.SetKeys(gck, gak);
        }
    }
    // Hàm tua đến một mốc giờ cố định trong ngày (0.0 -> 1.0)
    public void SetTime(float targetTime)
    {
        currentTime = Mathf.Repeat(targetTime, 1f);
        ForceUpdateLighting();
    }

    // Hàm cộng thêm số tiếng ngủ (ví dụ: ngủ 4 tiếng -> sleepHours = 4f)
    public void AdvanceTime(float sleepHours)
    {
        float timeToAdd = sleepHours / 24f;
        currentTime = Mathf.Repeat(currentTime + timeToAdd, 1f);
        ForceUpdateLighting();
    }

    // Ép cập nhật góc đèn, cường độ và Skybox ngay lập tức trong 1 frame
    public void ForceUpdateLighting()
    {
        FindSceneLights();
        UpdateLightingAndSkybox();
    }
}