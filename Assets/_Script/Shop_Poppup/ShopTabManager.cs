using UnityEngine;
using UnityEngine.UI;

public class Shop_Tab_Manager : MonoBehaviour
{
    public static Shop_Tab_Manager Instance { get; private set; }

    [Header("UI Canvas & Panels")]
    [SerializeField] private GameObject shopCanvas; // Kéo GameObject Canvas/Popup "Shop" vào đây

    [Header("Tab Panels (Khung nội dung từng tab)")]
    [SerializeField] private GameObject rodTabPanel;     // Khung Cần Câu
    [SerializeField] private GameObject baitTabPanel;    // Khung Mồi Câu
    [SerializeField] private GameObject partsTabPanel;   // Khung Phụ Tùng Xe

    [Header("Tab Buttons (Các nút bấm chuyển Tab)")]
    [SerializeField] private Button rodTabButton;
    [SerializeField] private Button baitTabButton;
    [SerializeField] private Button partsTabButton;
    [SerializeField] private Button closeButton;        // Nút X để đóng Shop

    private System.Action _onCloseCallback;

    private void Awake()
    {
        // Khởi tạo Singleton ngay khi Game bắt đầu
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Lắng nghe sự kiện click của các nút
        if (rodTabButton != null) rodTabButton.onClick.AddListener(ShowRodTab);
        if (baitTabButton != null) baitTabButton.onClick.AddListener(ShowBaitTab);
        if (partsTabButton != null) partsTabButton.onClick.AddListener(ShowPartsTab);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
    }

    private void Start()
    {
        // Mới vào game tự động ẩn Shop đi, đảm bảo Awake vẫn chạy để đăng ký Instance!
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(false);
        }
    }

    /// <summary>
    /// Hàm gọi từ NPC để bật UI Shop
    /// </summary>
    public void OpenShop(System.Action onClose)
    {
        _onCloseCallback = onClose;

        if (shopCanvas != null)
        {
            shopCanvas.SetActive(true);
        }

        // Mặc định hiển thị Tab Cần Cầu đầu tiên
        ShowRodTab();
    }

    /// <summary>
    /// Hàm đóng Shop
    /// </summary>
    public void CloseShop()
    {
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(false);
        }

        // Báo cho NPC biết để trả về trạng thái Idle
        _onCloseCallback?.Invoke();
        _onCloseCallback = null;
    }

    // --- LOGIC CHUYỂN TAB ---

    public void ShowRodTab()
    {
        SetTabActive(rodTabPanel);
    }

    public void ShowBaitTab()
    {
        SetTabActive(baitTabPanel);
    }

    public void ShowPartsTab()
    {
        SetTabActive(partsTabPanel);
    }

    private void SetTabActive(GameObject activePanel)
    {
        // Ẩn tất cả các Panel
        if (rodTabPanel != null) rodTabPanel.SetActive(false);
        if (baitTabPanel != null) baitTabPanel.SetActive(false);
        if (partsTabPanel != null) partsTabPanel.SetActive(false);

        // Bật duy nhất Panel được chọn
        if (activePanel != null) activePanel.SetActive(true);
    }
}