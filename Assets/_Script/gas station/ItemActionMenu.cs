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
    public TextMeshProUGUI notificationText;

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
        bool hasAction = false;

        // 1. Kiểm tra Can Xăng
        if (item.GetItemShape().itemID == "fuel_can_01")
        {
            btnUseText.text = "Đổ Xăng";
            btnUse.gameObject.SetActive(true);
            hasAction = true;
        }
        // 2. Kiểm tra Đồ Ăn (MỚI THÊM VÀO ĐÂY)
        else if (item.GetItemShape() is FoodSO)
        {
            btnUseText.text = "Ăn";
            btnUse.gameObject.SetActive(true);
            hasAction = true;
        }
        else
        {
            // Các đồ khác tạm thời chưa có chức năng
            btnUse.gameObject.SetActive(false);
        }

        // Hiện bảng nếu item có chức năng
        if (hasAction)
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);

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

        // Xử lý chức năng đổ xăng
        if (currentItem.GetItemShape().itemID == "fuel_can_01")
        {
            CarFuel carFuel = Object.FindFirstObjectByType<CarFuel>();
            if (carFuel != null)
            {
                if (carFuel.currentFuel >= carFuel.maxFuel)
                {
                    ShowNotif("Xe đã đầy xăng, không thể đổ thêm!");
                    return;
                }

                carFuel.AddFuel(30f);
                ShowNotif("Đã bơm 30 Lít xăng!");

                if (currentItem.currentOwner == InventoryItemUI.GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                {
                    TrunkMinigameUI.Instance.GetGridData().ClearCells(currentItem.GetGridX(), currentItem.GetGridY(), currentItem.GetItemShape(), currentItem.IsRotated());
                }
                else if (BackpackMinigameUI.Instance != null)
                {
                    BackpackMinigameUI.Instance.GetGridData().ClearCells(currentItem.GetGridX(), currentItem.GetGridY(), currentItem.GetItemShape(), currentItem.IsRotated());
                }

                Destroy(currentItem.gameObject);
                CloseMenu();
            }
        }
        // Xử lý chức năng Ăn uống (MỚI THÊM VÀO ĐÂY)
        else if (currentItem.GetItemShape() is FoodSO)
        {
            // Gọi lệnh ăn từ InventoryItemUI (Nó sẽ tự động hồi máu, xóa hình ảnh cá, dọn ô Balo)
            currentItem.ConsumeItem();

            // Hiện thông báo lên màn hình cho xịn (Tùy chọn)
            ShowNotif("Đã ăn xong, bụng no căng!");

            CloseMenu();
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