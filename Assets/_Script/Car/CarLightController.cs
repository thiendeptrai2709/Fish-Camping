using UnityEngine;
using UnityEngine.InputSystem;

public class CarLightController : MonoBehaviour
{
    [Header("Đèn Pha Trước (Headlights)")]
    [SerializeField] private Light[] headlights;
    public bool isHeadlightOn = false;

    [Header("Đèn Hậu / Đèn Phanh (Brake Lights)")]
    [SerializeField] private Light[] brakeLights;
    [SerializeField] private float brakeLightIntensity = 35f;

    private bool isBraking = false;
    private VehicleInput vehicleInput;

    private void Awake()
    {
        vehicleInput = GetComponent<VehicleInput>();
    }

    private void Start()
    {
        UpdateHeadlights();
        UpdateBrakeLights();
    }

    private void Update()
    {
        // Kiểm tra xem người chơi có đang ở trong xe hay không:
        // (Nếu VehicleInput bị disable khi người chơi rời xe, hoặc component không active thì chặn bấm phím)
        if (!IsPlayerInVehicle()) return;

        // Chỉ khi đang ngồi lái xe mới bấm G bật/tắt đèn pha được
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            isHeadlightOn = !isHeadlightOn;
            UpdateHeadlights();
        }
    }

    private bool IsPlayerInVehicle()
    {
        // Khi người chơi xuống xe, script VehicleInput/VehicleController thường sẽ bị tắt (enabled = false)
        if (vehicleInput != null)
        {
            return vehicleInput.enabled;
        }

        // Trường hợp dự phòng nếu bạn dùng cờ riêng
        return true;
    }

    public void SetBraking(bool braking)
    {
        if (isBraking != braking)
        {
            isBraking = braking;
            UpdateBrakeLights();
        }
    }

    private void UpdateHeadlights()
    {
        if (headlights == null) return;
        foreach (Light light in headlights)
        {
            if (light != null) light.enabled = isHeadlightOn;
        }
    }

    private void UpdateBrakeLights()
    {
        if (brakeLights == null) return;

        foreach (Light light in brakeLights)
        {
            if (light != null)
            {
                light.enabled = isBraking;
                light.intensity = brakeLightIntensity;
            }
        }
    }
}