using UnityEngine;
using UnityEngine.UI; // Cần thiết để dùng UI Slider

public class CarFuel : MonoBehaviour
{
    [Header("Thông số Xăng / Năng lượng")]
    public float maxFuel = 100f;
    public float currentFuel = 100f;
    public float consumptionRate = 2f;

    [Header("Liên kết UI")]
    public GameObject fuelBarUI;            // (MỚI) Kéo cả cụm GameObject "FuelBar" vào đây để Tắt/Bật
    public Slider fuelSlider;

    [HideInInspector]
    public bool isEngineOn = false;         // (MỚI) Đánh dấu xem nhân vật có đang trên xe không

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentFuel = maxFuel;

        // Cấu hình giá trị Min/Max ban đầu cho Slider
        if (fuelSlider != null)
        {
            fuelSlider.minValue = 0f;
            fuelSlider.maxValue = maxFuel;
            fuelSlider.value = currentFuel;
        }

        // Ẩn thanh xăng lúc mới vào game (vì người chơi đang đi bộ)
        if (fuelBarUI != null) fuelBarUI.SetActive(false);
    }

    private void Update()
    {
        // Chỉ trừ xăng khi ĐÃ LÊN XE (isEngineOn) VÀ xe đang lăn bánh
        if (isEngineOn && rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            ConsumeFuel();
        }

        // Cập nhật giá trị hiển thị trên thanh UI Slider
        UpdateUI();
    }

    private void ConsumeFuel()
    {
        if (currentFuel > 0)
        {
            currentFuel -= consumptionRate * Time.deltaTime;
            currentFuel = Mathf.Max(currentFuel, 0f);
        }
    }

    // Hàm gọi khi đứng ở Cây xăng (hoặc dùng Menu trong Cốp) bơm xăng
    public void AddFuel(float amount)
    {
        currentFuel += amount;
        currentFuel = Mathf.Min(currentFuel, maxFuel);
        UpdateUI(); // Cập nhật lại thanh UI ngay lập tức
    }

    private void UpdateUI()
    {
        if (fuelSlider != null)
        {
            fuelSlider.value = currentFuel;
        }
    }

    // ==========================================
    // CÁC HÀM MỚI ĐỂ LIÊN KẾT VỚI HỆ THỐNG KHÁC
    // ==========================================

    // Gọi hàm này khi nhân vật bấm F lên xe
    public void PlayerEnterCar()
    {
        isEngineOn = true;
        if (fuelBarUI != null) fuelBarUI.SetActive(true); // Bật thanh UI
    }

    // Gọi hàm này khi nhân vật bấm F xuống xe
    public void PlayerExitCar()
    {
        isEngineOn = false;
        if (fuelBarUI != null) fuelBarUI.SetActive(false); // Tắt thanh UI
    }

    // Script điều khiển xe sẽ gọi hàm này để xem có cho phép lăn bánh không
    public bool HasFuel()
    {
        return currentFuel > 0;
    }
}