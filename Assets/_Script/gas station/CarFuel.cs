using UnityEngine;
using UnityEngine.UI; // Cần thiết để dùng UI Slider

public class CarFuel : MonoBehaviour
{
    [Header("Thông số Xăng / Năng lượng")]
    public float maxFuel = 100f;            // Năng lượng tối đa
    public float currentFuel = 100f;        // Năng lượng hiện tại
    public float consumptionRate = 2f;      // Tốc độ tiêu hao (năng lượng/giây)

    [Header("Liên kết UI")]
    public Slider fuelSlider;               // Kéo thanh FuelBar vào đây

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
    }

    private void Update()
    {
        // Khi xe chạy (vận tốc > 0.1) thì trừ xăng/năng lượng
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
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

    // Hàm gọi khi đứng ở Cây xăng bấm F
    public void AddFuel(float amount)
    {
        currentFuel += amount;
        currentFuel = Mathf.Min(currentFuel, maxFuel);
    }

    private void UpdateUI()
    {
        if (fuelSlider != null)
        {
            // Gán trực tiếp giá trị currentFuel vào slider
            fuelSlider.value = currentFuel;
        }
    }
}