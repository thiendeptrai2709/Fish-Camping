using UnityEngine;
using UnityEngine.UI;

public class Shop_Tab_Manager : MonoBehaviour
{
    public static Shop_Tab_Manager Instance { get; private set; }

    [Header("UI Canvas & Panels")]
    [SerializeField] private GameObject shopCanvas; // Kéo GameObject Canvas "Shop" chính vào đây

    [Header("Tab Panels (Khung nội dung từng tab)")]
    [SerializeField] private GameObject rodTabPanel;     // Khung danh sách Cần Cầu
    [SerializeField] private GameObject baitTabPanel;    // Khung danh sách Mồi Câu
    [SerializeField] private GameObject partsTabPanel;   // Khung danh sách Phụ Tùng Xe

    [Header("Tab Buttons (Các nút bấm chuyển Tab)")]
    [SerializeField] private Button rodTabButton;
    [SerializeField] private Button baitTabButton;
    [SerializeField] private Button partsTabButton;
    [SerializeField] private Button closeButton;        // Nút X để đóng Shop

    private System.Action _onCloseCallback;

    private void Awake()
    {
        // Khởi tạo Singleton để NPCBase có thể gọi trực tiếp
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Lắng nghe sự kiện click của các nút Tab
        if (rodTabButton != null) rodTabButton.onClick.AddListener(ShowRodTab);
        if (baitTabButton != null) baitTabButton.onClick.AddListener(ShowBaitTab);
        if (partsTabButton != null) partsTabButton.onClick.AddListener(ShowPartsTab);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
    }

    /// <summary>
    /// Hàm gọi từ NPCBase để bật UI Shop lên
    /// </summary>
    public void OpenShop(System.Action onClose)
    {
        _onCloseCallback = onClose;

        if (shopCanvas != null)
        {
            shopCanvas.SetActive(true);
        }

        // Mặc định khi bật shop sẽ hiển thị Tab Cần Cầu đầu tiên
        ShowRodTab();
    }

    /// <summary>
    /// Hàm đóng Shop, gán vào nút X hoặc gọi từ NPC
    /// </summary>
    public void CloseShop()
    {
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(false);
        }

        // Báo cho NPCBase biết để trả NPC về trạng thái Idle đứng im
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