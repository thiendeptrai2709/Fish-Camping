using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    private System.Action _onShopClosed;

    [Header("--- BẬT/TẮT GIAO DIỆN SHOP ---")]
    public GameObject shopPanel;

    [Header("--- ẨN UI KHÁC KHI ĐANG MỞ SHOP ---")]
    [Tooltip("Kéo các UI cần ẩn tạm lúc mở Shop vào đây")]
    public GameObject[] cacUIAnKhiMoShop;

    [Header("--- DANH SÁCH DỮ LIỆU VẬT PHẨM (Tự động tải nếu để trống) ---")]
    public List<FishingRodSO> danhSachCanCau = new List<FishingRodSO>();
    public List<BaitSO> danhSachMoiCau = new List<BaitSO>();
    public List<ItemShapeSO> danhSachPhuTung = new List<ItemShapeSO>();

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (shopPanel == null)
        {
            shopPanel = gameObject;
        }
    }

    private void Start()
    {
        TuDongCapNhatGiaTien();

        if (Application.isPlaying && shopPanel != null && shopPanel != gameObject)
        {
            shopPanel.SetActive(false);
        }
    }

    private void OnValidate()
    {
        TuDongCapNhatGiaTien();
    }

    private void OnEnable()
    {
        TuDongCapNhatGiaTien();
    }

    private void Update()
    {
        // Bấm phím E hoặc Escape để đóng Shop
        if (shopPanel != null && shopPanel.activeInHierarchy)
        {
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                DongShop();
            }
        }
    }

    private void TaiDuLieuVatPham()
    {
        // 1. Tải Cần câu (Rod 1 đến Rod 6)
        if (danhSachCanCau == null || danhSachCanCau.Count == 0)
        {
            FishingRodSO[] rods = Resources.FindObjectsOfTypeAll<FishingRodSO>();
            danhSachCanCau = rods.OrderBy(r => r.rodTier > 0 ? r.rodTier : LaySoTrongTen(r.name)).ToList();
        }

        // 2. Tải Mồi câu (Bait 1 đến Bait 6)
        if (danhSachMoiCau == null || danhSachMoiCau.Count == 0)
        {
            BaitSO[] baits = Resources.FindObjectsOfTypeAll<BaitSO>();
            danhSachMoiCau = baits.OrderBy(b => LaySoTrongTen(b.name)).ToList();
        }

        // 3. Tải Phụ tùng xe / lốp xe
        if (danhSachPhuTung == null || danhSachPhuTung.Count == 0)
        {
            ItemShapeSO[] allShapes = Resources.FindObjectsOfTypeAll<ItemShapeSO>();
            danhSachPhuTung = allShapes.Where(s => s != null && (s.name.ToLower().Contains("lop") || s.name.ToLower().Contains("gara") || s.itemID.ToLower().Contains("tire"))).ToList();
        }
    }

    private int LaySoTrongTen(string name)
    {
        if (string.IsNullOrEmpty(name)) return 1;
        for (int i = 8; i >= 1; i--)
        {
            if (name.Contains(i.ToString())) return i;
        }
        return 1;
    }

    [ContextMenu("Cập Nhật Lại Giá Tiền (TuDongCapNhatGiaTien)")]
    /// <summary>
    /// Tự động cập nhật và hiển thị đúng GIÁ TIỀN lên giao diện Shop (Item_Price text và ShopItemButton.giaTien)
    /// CHỈ can thiệp vào Giá Tiền, hoàn toàn KHÔNG can thiệp vào dữ liệu món đồ hay sự kiện click của nút.
    /// </summary>
    public void TuDongCapNhatGiaTien()
    {
        Transform rootTransform = shopPanel != null ? shopPanel.transform : transform;

        // BẢNG GIÁ CHUẨN CÂN BẰNG NỀN KINH TẾ
        int[] giaCanCau = new int[] { 1500, 2000, 3500, 5000, 9500 };
        int[] giaMoiCau = new int[] { 30, 80, 150, 300, 500, 800 };
        int[] giaPhaoCau = new int[] { 100, 200, 350, 500, 800, 1200, 1800, 2500 };
        int[] giaPhuTung = new int[] { 300, 500, 800, 1200, 1600, 2000, 2500, 3000 };

        // 1. CẬP NHẬT GIÁ TAB CẦN CÂU (Scroll View_Cancau)
        Transform svCancau = TimObjectConTheoTen(rootTransform, "Scroll View_Cancau");
        if (svCancau == null) svCancau = TimObjectConTheoTen(rootTransform, "Scroll View_CanCau");
        if (svCancau != null)
        {
            List<Transform> itemTemplates = LayDanhSachItemTemplate(svCancau);
            for (int i = 0; i < itemTemplates.Count; i++)
            {
                int price = (i < giaCanCau.Length) ? giaCanCau[i] : 1000 * (i + 1);
                CapNhatGiaChoTemplate(itemTemplates[i], price);
            }
        }

        // 2. CẬP NHẬT GIÁ TAB MỒI CÂU (Scroll View_Moicau)
        Transform svMoicau = TimObjectConTheoTen(rootTransform, "Scroll View_Moicau");
        if (svMoicau == null) svMoicau = TimObjectConTheoTen(rootTransform, "Scroll View_MoiCau");
        if (svMoicau != null)
        {
            List<Transform> itemTemplates = LayDanhSachItemTemplate(svMoicau);
            for (int i = 0; i < itemTemplates.Count; i++)
            {
                int price = (i < giaMoiCau.Length) ? giaMoiCau[i] : 100 * (i + 1);
                CapNhatGiaChoTemplate(itemTemplates[i], price);
            }
        }

        // 3. CẬP NHẬT GIÁ TAB PHAO CÂU (Scroll View_Phao / Scroll View_Phaocau)
        Transform svPhao = TimObjectConTheoTen(rootTransform, "Scroll View_Phao");
        if (svPhao == null) svPhao = TimObjectConTheoTen(rootTransform, "Scroll View_Phaocau");
        if (svPhao == null) svPhao = TimObjectConTheoTen(rootTransform, "Scroll View_PhaoCau");
        if (svPhao != null)
        {
            List<Transform> itemTemplates = LayDanhSachItemTemplate(svPhao);
            for (int i = 0; i < itemTemplates.Count; i++)
            {
                int price = (i < giaPhaoCau.Length) ? giaPhaoCau[i] : 200 * (i + 1);
                CapNhatGiaChoTemplate(itemTemplates[i], price);
            }
        }

        // 4. CẬP NHẬT GIÁ TAB PHỤ TÙNG XE (Scroll View_Phutungxe)
        Transform svPhutung = TimObjectConTheoTen(rootTransform, "Scroll View_Phutungxe");
        if (svPhutung == null) svPhutung = TimObjectConTheoTen(rootTransform, "Scroll View_PhuTungXe");
        if (svPhutung != null)
        {
            List<Transform> itemTemplates = LayDanhSachItemTemplate(svPhutung);
            for (int i = 0; i < itemTemplates.Count; i++)
            {
                int price = (i < giaPhuTung.Length) ? giaPhuTung[i] : 500 * (i + 1);
                CapNhatGiaChoTemplate(itemTemplates[i], price);
            }
        }
    }

    private void CapNhatGiaChoTemplate(Transform template, int defaultPrice)
    {
        if (template == null) return;

        int finalPrice = defaultPrice;

        // 1. Nếu template có ShopItemButton, đồng bộ giá tiền
        ShopItemButton shopItemBtn = template.GetComponent<ShopItemButton>();
        if (shopItemBtn == null) shopItemBtn = template.GetComponentInChildren<ShopItemButton>(true);

        if (shopItemBtn != null)
        {
            // Nếu trên Inspector bạn đã tự điền giá (> 0) thì giữ nguyên giá của bạn
            if (shopItemBtn.giaTien > 0)
            {
                finalPrice = shopItemBtn.giaTien;
            }
            else
            {
                shopItemBtn.giaTien = defaultPrice;
            }
        }

        // 2. Cập nhật Text hiển thị giá tiền (con Text (TMP) của Item_Price)
        Transform itemPriceObj = TimObjectConTheoTen(template, "Item_Price");
        if (itemPriceObj != null)
        {
            // Tìm chính xác Text (TMP) là con trực tiếp hoặc con bên trong Item_Price
            Transform textTMPChild = itemPriceObj.Find("Text (TMP)");
            TextMeshProUGUI priceTextTMP = textTMPChild != null ? textTMPChild.GetComponent<TextMeshProUGUI>() : itemPriceObj.GetComponentInChildren<TextMeshProUGUI>(true);

            if (priceTextTMP != null)
            {
                priceTextTMP.text = finalPrice.ToString();
            }
            else
            {
                Text legacyText = itemPriceObj.GetComponentInChildren<Text>(true);
                if (legacyText != null)
                {
                    legacyText.text = finalPrice.ToString();
                }
            }
        }
    }

    private List<Transform> LayDanhSachItemTemplate(Transform scrollView)
    {
        List<Transform> result = new List<Transform>();
        Transform content = TimObjectConTheoTen(scrollView, "Content");
        if (content == null) content = scrollView;

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (child.name.ToLower().Contains("item_template") || child.name.ToLower().Contains("template"))
            {
                result.Add(child);
            }
        }
        return result;
    }

    private Transform TimObjectConTheoTen(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = TimObjectConTheoTen(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    public void MoShop(System.Action onClose = null)
    {
        _onShopClosed = onClose;

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        TuDongCapNhatGiaTien();
        AnHienCacUIKhac(false);

        PlayerInputHandler inputHandler = Object.FindFirstObjectByType<PlayerInputHandler>(FindObjectsInactive.Include);
        if (inputHandler != null) inputHandler.IsUIOpen = true;

        PlayerCursor playerCursor = Object.FindFirstObjectByType<PlayerCursor>(FindObjectsInactive.Include);
        if (playerCursor != null)
        {
            playerCursor.SetCursorState(false);
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        ForcedTutorialManager.Instance?.NotifyShopOpened();
    }

    public void DongShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        AnHienCacUIKhac(true);

        _onShopClosed?.Invoke();
        _onShopClosed = null;

        PlayerInputHandler inputHandler = Object.FindFirstObjectByType<PlayerInputHandler>(FindObjectsInactive.Include);
        if (inputHandler != null) inputHandler.IsUIOpen = false;

        PlayerCursor playerCursor = Object.FindFirstObjectByType<PlayerCursor>(FindObjectsInactive.Include);
        if (playerCursor != null)
        {
            playerCursor.SetCursorState(true);
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        ForcedTutorialManager.Instance?.NotifyShopClosed();
    }

    private void AnHienCacUIKhac(bool hienRa)
    {
        if (cacUIAnKhiMoShop == null) return;
        for (int i = 0; i < cacUIAnKhiMoShop.Length; i++)
        {
            if (cacUIAnKhiMoShop[i] == null)
            {
                GameObject uiBiMat = GameObject.Find("coin");
                if (uiBiMat != null) cacUIAnKhiMoShop[i] = uiBiMat;
            }

            if (cacUIAnKhiMoShop[i] != null)
            {
                cacUIAnKhiMoShop[i].SetActive(hienRa);
            }
        }
    }

    public void MuaVatPham(int giaTien, ItemShapeSO monDoDaMua)
    {
        FishingController fc = Object.FindFirstObjectByType<FishingController>();

        // 1. Kiểm tra tiền người chơi
        if (MoneyManager.Instance == null || !MoneyManager.Instance.CoDuTien(giaTien))
        {
            if (fc != null)
            {
                fc.ShowFishingFeedback($"Bạn không đủ tiền! Cần {giaTien} vàng.", new Color(1f, 0.4f, 0.4f));
            }
            Debug.LogWarning($"[Shop] Không đủ tiền để mua món này! Cần {giaTien} vàng.");
            return;
        }

        // 2. Thêm đồ vào Balo
        if (monDoDaMua != null && BackpackMinigameUI.Instance != null)
        {
            bool themThanhCong = BackpackMinigameUI.Instance.TryAutoAddItem(monDoDaMua);

            if (themThanhCong)
            {
                // Trừ tiền khi thêm đồ vào Balo thành công
                MoneyManager.Instance.TruTien(giaTien);
                GameDatabaseManager.Instance?.SaveAndSyncToCloud();

                string itemName = !string.IsNullOrEmpty(monDoDaMua.itemName) ? monDoDaMua.itemName : monDoDaMua.name;
                if (fc != null)
                {
                    fc.ShowFishingFeedback($"Đã mua thành công [{itemName}] (-{giaTien} vàng)!", new Color(0.4f, 1f, 0.6f));
                }
                Debug.Log($"<color=green>[Shop] Đã mua [{itemName}] thành công! Trừ {giaTien} vàng.</color>");
            }
            else
            {
                if (fc != null)
                {
                    fc.ShowFishingFeedback("Balo của bạn đã đầy! Hãy sắp xếp lại chỗ trống.", new Color(1f, 0.75f, 0.25f));
                }
                Debug.LogWarning("[Shop] Giao dịch thất bại: Balo của bạn đã đầy!");
            }
        }
        else
        {
            if (fc != null)
            {
                fc.ShowFishingFeedback("Chưa thể mua món đồ này vào lúc này!", Color.yellow);
            }
        }
    }

    public void BanVatPham(int giaTriVatPham)
    {
        if (MoneyManager.Instance == null) return;

        MoneyManager.Instance.CongTien(giaTriVatPham);
        GameDatabaseManager.Instance?.SaveAndSyncToCloud();

        FishingController fc = Object.FindFirstObjectByType<FishingController>();
        if (fc != null)
        {
            fc.ShowFishingFeedback($"Đã bán cá thành công! Nhận về +{giaTriVatPham} vàng.", new Color(0.4f, 1f, 0.6f));
        }

        Debug.Log("<color=green>[Shop] Đã bán thành công! Thu về " + giaTriVatPham + " vàng.</color>");
    }
}