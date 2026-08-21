using UnityEngine;

public class LanternController : MonoBehaviour, IInteractable
{
    [Header("Cấu hình Đèn")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private bool turnOnByDefault = true;
    [SerializeField] private string promptTurnOff = "Click [Chuột Trái] để Tắt đèn";
    [SerializeField] private string promptTurnOn = "Click [Chuột Trái] để Bật đèn";

    [Header("Hiệu ứng Lập Lòe (Flicker)")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float baseIntensity = 3.5f;
    [SerializeField] private float flickerRange = 0.5f;
    [SerializeField] private float flickerSpeed = 8f;

    private bool isOn;
    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");
        gameObject.layer = normalLayer;

        if (lanternLight == null)
        {
            lanternLight = GetComponentInChildren<Light>();
        }

        isOn = turnOnByDefault;
        ApplyLightState();
    }

    private void Update()
    {
        // Hiệu ứng ánh lửa bập bùng nhẹ ban đêm khi đèn đang bật
        if (isOn && enableFlicker && lanternLight != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            lanternLight.intensity = baseIntensity + (noise - 0.5f) * flickerRange;
        }
    }

    // Hàm gọi khi Click Chuột Trái vào đèn
    public void Interact()
    {
        isOn = !isOn;
        ApplyLightState();

        // Cập nhật lại UI gợi ý ngay lập tức khi trạng thái thay đổi
        InteractionPromptUI promptUI = FindFirstObjectByType<InteractionPromptUI>();
        if (promptUI != null)
        {
            promptUI.DisplayPrompt(true, GetInteractPrompt());
        }
    }

    private void ApplyLightState()
    {
        if (lanternLight != null)
        {
            lanternLight.enabled = isOn;
            if (isOn) lanternLight.intensity = baseIntensity;
        }
    }

    public string GetInteractPrompt()
    {
        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        if (isOn)
        {
            return isVietnamese ? (string.IsNullOrEmpty(promptTurnOff) ? "[Chuột Trái] Tắt đèn" : promptTurnOff) : "[Left Click] Turn Off Lamp";
        }
        else
        {
            return isVietnamese ? (string.IsNullOrEmpty(promptTurnOn) ? "[Chuột Trái] Bật đèn" : promptTurnOn) : "[Left Click] Turn On Lamp";
        }
    }

    public void OnFocus()
    {
        SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}