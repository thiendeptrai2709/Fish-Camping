using UnityEngine;

public class StreetLightController : MonoBehaviour
{
    [Header("Nguồn sáng")]
    [SerializeField] private Light streetLight;

    [Header("Mốc thời gian (Nếu không dùng IsNight)")]
    [Range(0f, 1f)]
    [SerializeField] private float turnOnHour = 0.75f;  // 18:00
    [Range(0f, 1f)]
    [SerializeField] private float turnOffHour = 0.25f; // 06:00

    [Header("Độ trễ kiểm tra")]
    [SerializeField] private float checkInterval = 0.3f;

    private float timer;

    private void Awake()
    {
        // Tự động tìm Light trên chính nó hoặc trong các object con
        if (streetLight == null)
        {
            streetLight = GetComponent<Light>();
            if (streetLight == null)
            {
                streetLight = GetComponentInChildren<Light>(true);
            }
        }
    }

    private void Start()
    {
        UpdateLightState();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            UpdateLightState();
        }
    }

    private void UpdateLightState()
    {
        if (streetLight == null) return;

        if (DayNightSystem.Instance == null)
        {
            // Nếu không tìm thấy DayNightSystem, ép bật luôn để test
            streetLight.enabled = true;
            return;
        }

        float time = DayNightSystem.Instance.currentTime;
        
        // Kiểm tra thời gian: Trời tối từ 18h (0.75) đến 6h sáng hôm sau (0.25)
        bool isNight = (time >= turnOnHour || time < turnOffHour);

        // Gán trực tiếp trạng thái đèn
        if (streetLight.enabled != isNight)
        {
            streetLight.enabled = isNight;
        }
    }
}