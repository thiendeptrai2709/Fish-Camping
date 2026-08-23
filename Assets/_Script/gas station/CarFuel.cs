using System.Collections;
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

    [Header("Cảnh báo Xăng Thấp (<20%)")]
    [Tooltip("Ngưỡng kích hoạt cảnh báo (0.2 = 20%)")]
    [SerializeField] private float lowFuelThreshold = 0.20f;
    [SerializeField] private Color lowFuelFlashColor = new Color(1f, 0.15f, 0.15f, 1f); // Đỏ tươi cảnh báo
    [SerializeField] private int flashCount = 3; // Nháy đỏ 3 lần
    [SerializeField] private float flashInterval = 0.25f; // Thời gian mỗi nhịp nháy

    [HideInInspector]
    public bool isEngineOn = false;         // (MỚI) Đánh dấu xem nhân vật có đang trên xe không

    private Rigidbody rb;
    private bool hasWarnedLowFuel = false;
    private Coroutine flashCoroutine;
    private Color originalFillColor = Color.white;
    private Image fillImage;

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

            if (fuelSlider.fillRect != null)
            {
                fillImage = fuelSlider.fillRect.GetComponent<Image>();
                if (fillImage != null) originalFillColor = fillImage.color;
            }
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

        // Kiểm tra ngưỡng cảnh báo xăng <= 20%
        CheckLowFuelWarning();

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

    private void CheckLowFuelWarning()
    {
        if (!isEngineOn) return;

        float fuelRatio = maxFuel > 0f ? (currentFuel / maxFuel) : 0f;
        if (fuelRatio <= lowFuelThreshold)
        {
            if (!hasWarnedLowFuel)
            {
                hasWarnedLowFuel = true;
                TriggerLowFuelWarning();
            }
        }
        else if (fuelRatio > lowFuelThreshold + 0.05f)
        {
            // Đã đổ thêm xăng qua mức 25% -> reset cờ cảnh báo
            hasWarnedLowFuel = false;
        }
    }

    public void TriggerLowFuelWarning()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashLowFuelRoutine());
    }

    private IEnumerator FlashLowFuelRoutine()
    {
        if (fillImage == null && fuelSlider != null && fuelSlider.fillRect != null)
        {
            fillImage = fuelSlider.fillRect.GetComponent<Image>();
            if (fillImage != null) originalFillColor = fillImage.color;
        }

        Image[] allBarImages = fuelBarUI != null ? fuelBarUI.GetComponentsInChildren<Image>(true) : null;
        Color[] origColors = null;
        if (allBarImages != null && allBarImages.Length > 0)
        {
            origColors = new Color[allBarImages.Length];
            for (int k = 0; k < allBarImages.Length; k++)
            {
                origColors[k] = allBarImages[k].color;
            }
        }

        // Nháy đỏ đúng 3 lần
        for (int i = 0; i < flashCount; i++)
        {
            // Chuyển sang màu đỏ cảnh báo
            if (fillImage != null) fillImage.color = lowFuelFlashColor;
            if (allBarImages != null)
            {
                foreach (var img in allBarImages)
                {
                    if (img != null) img.color = lowFuelFlashColor;
                }
            }
            yield return new WaitForSeconds(flashInterval);

            // Chuyển về màu gốc
            if (fillImage != null) fillImage.color = originalFillColor;
            if (allBarImages != null && origColors != null)
            {
                for (int k = 0; k < allBarImages.Length; k++)
                {
                    if (allBarImages[k] != null) allBarImages[k].color = origColors[k];
                }
            }
            yield return new WaitForSeconds(flashInterval);
        }

        flashCoroutine = null;
    }

    // Hàm gọi khi đứng ở Cây xăng (hoặc dùng Menu trong Cốp) bơm xăng
    public void AddFuel(float amount)
    {
        currentFuel += amount;
        currentFuel = Mathf.Min(currentFuel, maxFuel);
        if (currentFuel / maxFuel > lowFuelThreshold + 0.05f)
        {
            hasWarnedLowFuel = false; // Reset cờ cảnh báo khi nạp đủ xăng
        }
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

        // Nếu vừa lên xe mà xăng đã dưới 20% -> nháy cảnh báo ngay
        if (maxFuel > 0f && (currentFuel / maxFuel) <= lowFuelThreshold && !hasWarnedLowFuel)
        {
            hasWarnedLowFuel = true;
            TriggerLowFuelWarning();
        }
    }

    // Gọi hàm này khi nhân vật bấm F xuống xe
    public void PlayerExitCar()
    {
        isEngineOn = false;
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Khôi phục màu gốc
        if (fillImage != null) fillImage.color = originalFillColor;

        if (fuelBarUI != null) fuelBarUI.SetActive(false); // Tắt thanh UI
    }

    // Script điều khiển xe sẽ gọi hàm này để xem có cho phép lăn bánh không
    public bool HasFuel()
    {
        return currentFuel > 0;
    }
}