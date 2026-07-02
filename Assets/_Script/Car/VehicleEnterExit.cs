using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleEnterExit : MonoBehaviour
{
    [Header("Dữ liệu Người Chơi")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject playerCamera; // TPC_Camera của người chơi

    [Header("Dữ liệu Xe")]
    [SerializeField] private VehicleInput vehicleInput; // Script đọc Input của xe bạn đã viết
    [SerializeField] private VehicleController vehicleController; // Thêm tham chiếu đến Controller để lấy tốc độ
    [SerializeField] private GameObject carCamera; // Cinemachine Camera riêng của xe
    [SerializeField] private GameObject playerUI;

    private CarInputActions inputActions;
    private Transform currentExitPoint;
    private bool isInCar = false;

    private void Awake()
    {
        inputActions = new CarInputActions();

        // Lắng nghe sự kiện bấm phím E để xuống xe
        inputActions.Gameplay.ExitVehicle.performed += _ => TryExitVehicle();

        // Mặc định khi mới vào game: Người chưa lên xe thì TẮT điều khiển và TẮT camera xe
        if (vehicleInput != null) vehicleInput.enabled = false;
        if (carCamera != null) carCamera.SetActive(false);
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Hàm này được gọi từ cánh cửa khi bấm chuột trái
    public void EnterVehicle(Transform doorExitPoint)
    {
        if (isInCar) return;

        currentExitPoint = doorExitPoint;
        isInCar = true;

        playerObject.SetActive(false);
        playerCamera.SetActive(false);
        if (playerUI != null) playerUI.SetActive(false);

        carCamera.SetActive(true);
        vehicleInput.enabled = true;
    }

    // Hàm này chạy khi bấm phím E lúc đang ngồi trên xe
    private void TryExitVehicle()
    {
        if (!isInCar) return;

        // Kiểm tra xe đã dừng hẳn chưa (Tốc độ tuyệt đối < 1 km/h để tránh lỗi trôi vật lý nhẹ)
        if (vehicleController != null && Mathf.Abs(vehicleController.GetCurrentSpeedKmh()) > 1f)
        {
            Debug.Log("Xe phải dừng hẳn mới có thể xuống!");
            return;
        }

        isInCar = false;

        vehicleInput.enabled = false;
        carCamera.SetActive(false);

        playerObject.transform.position = currentExitPoint.position;
        playerObject.transform.rotation = currentExitPoint.rotation;

        playerObject.SetActive(true);
        playerCamera.SetActive(true);
        if (playerUI != null) playerUI.SetActive(true);
    }
}