using UnityEngine;
using UnityEngine.InputSystem; // BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ SỬA LỖI KEYBOARD

public class ShopManager : MonoBehaviour
{
    // BẮT BUỘC CÓ: Để NPC có thể tìm thấy ShopManager trong Scene
    public static ShopManager Instance;

    // BẮT BUỘC CÓ: Biến lưu trữ lệnh để gọi NPC thả trạng thái kẹt
    private System.Action _onShopClosed;

    [Header("--- BẬT/TẮT GIAO DIỆN SHOP ---")]
    public GameObject shopPanel;

    [Header("--- ẨN UI KHÁC KHI ĐANG MỞ SHOP ---")]
    [Tooltip("Kéo các UI cần ẩn tạm lúc mở Shop vào đây (VD: cái HUD tiền 'coin' đang đè lên góc màn hình). Đóng Shop sẽ tự hiện lại.")]
    public GameObject[] cacUIAnKhiMoShop;

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        shopPanel.SetActive(false); // Đảm bảo lúc vào game là Shop ẩn
    }

    private void OnDisable()
    {
        if (shopPanel != null && shopPanel.activeInHierarchy)
        {
            DongShop();
        }
    }

    void Update()
    {
        // Bấm phím E để tắt Shop khi bảng shop đang mở
        if (shopPanel.activeInHierarchy && (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame))
        {
            DongShop();
        }
    }

    // Thêm (System.Action onClose) để nhận Giao ước từ NPC
    public void MoShop(System.Action onClose = null)
    {
        _onShopClosed = onClose;
        shopPanel.SetActive(true);
        AnHienCacUIKhac(false); // Ẩn HUD/UI khác đi trong lúc Shop đang mở

        PlayerInputHandler inputHandler = Object.FindFirstObjectByType<PlayerInputHandler>(FindObjectsInactive.Include);
        if (inputHandler != null) inputHandler.IsUIOpen = true;

        PlayerCursor playerCursor = Object.FindFirstObjectByType<PlayerCursor>(FindObjectsInactive.Include);
        if (playerCursor != null)
        {
            playerCursor.SetCursorState(false);
        }
        else
        {
            // HIỆN VÀ MỞ KHÓA CHUỘT
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        ForcedTutorialManager.Instance?.NotifyShopOpened();
    }

    public void DongShop()
    {
        shopPanel.SetActive(false);
        AnHienCacUIKhac(true); // Hiện lại UI đã ẩn lúc mở Shop

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
            // ẨN VÀ KHÓA CHUỘT LẠI KHI ĐÓNG SHOP
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
            // Nếu phát hiện ô nào bị đứt dây do chuyển Map (bị null)
            if (cacUIAnKhiMoShop[i] == null)
            {
                // Tự động radar quét tìm lại UI tên là "coin"
                GameObject uiBiMat = GameObject.Find("coin");
                if (uiBiMat != null)
                {
                    cacUIAnKhiMoShop[i] = uiBiMat;
                }
            }

            // Tiến hành ẩn/hiện bình thường
            if (cacUIAnKhiMoShop[i] != null)
            {
                cacUIAnKhiMoShop[i].SetActive(hienRa);
            }
        }
    }

    // Cập nhật tham số: Dùng ItemShapeSO cho đồng bộ với code Balo
    public void MuaVatPham(int giaTien, ItemShapeSO monDoDaMua)
    {
        if (MoneyManager.Instance != null && MoneyManager.Instance.CoDuTien(giaTien))
        {
            if (monDoDaMua != null && BackpackMinigameUI.Instance != null)
            {
                // Gọi code Balo của bạn bồ để thử nhét đồ vào
                bool themThanhCong = BackpackMinigameUI.Instance.TryAutoAddItem(monDoDaMua);

                if (themThanhCong)
                {
                    // KHI NHÉT BALO THÀNH CÔNG THÌ MỚI TRỪ TIỀN
                    MoneyManager.Instance.TruTien(giaTien);

                    Debug.Log($"Đã ném [{monDoDaMua.itemName}] vào balo! Trừ {giaTien} vàng.");
                }
                else
                {
                    Debug.Log("Giao dịch thất bại: Balo của bạn đã đầy!");
                }
            }
        }
        else
        {
            Debug.Log("Không đủ tiền để mua món này!");
        }
    }
    public void BanVatPham(int giaTriVatPham)
    {
        if (MoneyManager.Instance == null) return;

        // CỘNG TIỀN VÀO TÚI
        MoneyManager.Instance.CongTien(giaTriVatPham);

        Debug.Log("Đã bán thành công! Thu về " + giaTriVatPham + " vàng.");
    }
}