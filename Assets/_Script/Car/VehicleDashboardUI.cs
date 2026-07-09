using TMPro;
using UnityEngine;

public class VehicleDashboardUI : MonoBehaviour
{
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private GameObject dashboardPanel; // Panel chứa toàn bộ UI Đồng hồ xe

    private void Awake()
    {
        // Mặc định ban đầu vào game chưa lên xe thì phải tắt
        SetDashboardVisible(false);
    }

    private void Update()
    {
        if (vehicleController == null || speedText == null) return;

        // Nếu UI đang tắt thì không cần tốn hiệu năng tính toán text làm gì
        if (dashboardPanel != null && !dashboardPanel.activeSelf) return;

        int currentSpeed = Mathf.RoundToInt(vehicleController.GetCurrentSpeedKmh());
        speedText.text = $"{currentSpeed} KM/H";
    }

    public void SetDashboardVisible(bool isVisible)
    {
        if (dashboardPanel != null)
        {
            dashboardPanel.SetActive(isVisible);
        }
        else if (speedText != null)
        {
            // Dự phòng nếu mày không dùng Panel mà chỉ kéo mỗi cái Text vào
            speedText.gameObject.SetActive(isVisible);
        }
    }
}