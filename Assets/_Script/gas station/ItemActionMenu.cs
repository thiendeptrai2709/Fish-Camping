using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemActionMenu : MonoBehaviour
{
    public static ItemActionMenu Instance { get; private set; }

    [Header("=== GIAO DIỆN MENU ===")]
    public GameObject menuPanel;
    public Button btnUse;
    public Button btnClose;
    public TextMeshProUGUI btnUseText;
    public TextMeshProUGUI notificationText; // (Tùy chọn) Chữ báo lỗi xe đầy xăng

    private InventoryItemUI currentItem;

     
    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    private void Start()
    {
        if (btnUse != null) btnUse.onClick.AddListener(OnUseClicked);
        if (btnClose != null) btnClose.onClick.AddListener(CloseMenu);
    }

    public void ShowMenu(InventoryItemUI item)
    {
        currentItem = item;
        bool hasAction = false; // Mặc định là món đồ chưa có chức năng gì

        // Đọc ID để kiểm tra chức năng
        if (item.GetItemShape().itemID == "fuel_can_01")
        {
            btnUseText.text = "Đổ Xăng";
            btnUse.gameObject.SetActive(true);
            hasAction = true; // Đánh dấu là món này CÓ chức năng
        }
        else
        {
            // Các đồ khác (cá, cần câu, mồi...) tạm thời chưa có chức năng
            btnUse.gameObject.SetActive(false);
        }

        // --- BƯỚC CHỐT: CHỈ HIỆN BẢNG KHI CÓ CHỨC NĂNG ---
        if (hasAction)
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);

                // Ép bảng ra chính giữa màn hình như bồ muốn
                RectTransform rect = menuPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }
        else
        {
            // Đảm bảo bảng không thò mặt ra nếu click vào đồ vô dụng
            if (menuPanel != null) menuPanel.SetActive(false);
        }
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        currentItem = null;
    }

    private void OnUseClicked()
    {
        if (currentItem == null) return;

        if (currentItem.GetItemShape().itemID == "fuel_can_01")
        {
            // Tìm component CarFuel của xe trong Map
            CarFuel carFuel = Object.FindFirstObjectByType<CarFuel>();
            if (carFuel != null)
            {
                // Kiểm tra xăng đầy
                if (carFuel.currentFuel >= carFuel.maxFuel)
                {
                    ShowNotif("Xe đã đầy xăng, không thể đổ thêm!");
                    return; // Chặn lại, không trừ item
                }

                // Nếu chưa đầy -> Bơm 30 Lít
                carFuel.AddFuel(30f);
                ShowNotif("Đã bơm 30 Lít xăng!");

                // XÓA DATA CAN XĂNG KHỎI LƯỚI CỐP/BALO
                if (currentItem.currentOwner == InventoryItemUI.GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                {
                    TrunkMinigameUI.Instance.GetGridData().ClearCells(currentItem.GetGridX(), currentItem.GetGridY(), currentItem.GetItemShape(), currentItem.IsRotated());
                }
                else if (BackpackMinigameUI.Instance != null)
                {
                    BackpackMinigameUI.Instance.GetGridData().ClearCells(currentItem.GetGridX(), currentItem.GetGridY(), currentItem.GetItemShape(), currentItem.IsRotated());
                }

                // Hủy luôn hình ảnh UI của can xăng rồi đóng Menu
                Destroy(currentItem.gameObject);
                CloseMenu();
            }
        }
    }

    private void ShowNotif(string msg)
    {
        if (notificationText != null)
        {
            StopAllCoroutines();
            StartCoroutine(NotifRoutine(msg));
        }
        else Debug.Log(msg);
    }

    private System.Collections.IEnumerator NotifRoutine(string msg)
    {
        notificationText.text = msg;
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        notificationText.gameObject.SetActive(false);
    }
}