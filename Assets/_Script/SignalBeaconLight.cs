using UnityEngine;

public class SignalBeaconLight : MonoBehaviour
{
    public enum BlinkMode
    {
        Flash, // Nhấp nháy bật/tắt dứt khoát như đèn tháp/cột cờ
        SmoothPulse // Sáng mờ dần mềm mại như nhịp thở
    }

    [Header("Cấu Hình Chế Độ")]
    [SerializeField] private BlinkMode mode = BlinkMode.Flash;

    [Header("Cường Độ Sáng")]
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 3f;

    [Header("Thời Gian (Dành cho chế độ Flash)")]
    [Tooltip("Thời gian sáng (giây)")]
    [SerializeField] private float onDuration = 0.5f;

    [Tooltip("Thời gian tắt (giây)")]
    [SerializeField] private float offDuration = 0.5f;

    [Header("Tốc Độ (Dành cho chế độ SmoothPulse)")]
    [Tooltip("Tốc độ nhịp đập sáng")]
    [SerializeField] private float pulseSpeed = 3f;

    private Light targetLight;
    private float timer = 0f;
    private bool isLightOn = true;

    private void Awake()
    {
        targetLight = GetComponent<Light>();
    }

    private void Update()
    {
        if (targetLight == null) return;

        if (mode == BlinkMode.Flash)
        {
            timer += Time.deltaTime;

            if (isLightOn && timer >= onDuration)
            {
                isLightOn = false;
                timer = 0f;
                targetLight.intensity = minIntensity;
            }
            else if (!isLightOn && timer >= offDuration)
            {
                isLightOn = true;
                timer = 0f;
                targetLight.intensity = maxIntensity;
            }
        }
        else if (mode == BlinkMode.SmoothPulse)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        }
    }
}