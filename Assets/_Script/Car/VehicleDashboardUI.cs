using UnityEngine;
using TMPro;

public class VehicleDashboardUI : MonoBehaviour
{
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private TextMeshProUGUI speedText;

    private void Update()
    {
        if (vehicleController == null || speedText == null) return;

        int currentSpeed = Mathf.RoundToInt(vehicleController.GetCurrentSpeedKmh());
        speedText.text = $"{currentSpeed} KM/H";
    }
}