using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ SỬA LỖI KEYBOARD

public class ShopManager : MonoBehaviour
{
    // BẮT BUỘC CÓ: Để NPC có thể tìm thấy ShopManager trong Scene
    public static ShopManager Instance;

    // BẮT BUỘC CÓ: Biến lưu trữ lệnh để gọi NPC thả trạng thái kẹt
    private System.Action _onShopClosed;

    [Header("--- HỆ THỐNG TIỀN ---")]
    public int tongTien = 5000;
    private int soTienHienThi;
    public TextMeshProUGUI txtHienThiTien;

    [Header("--- BẬT/TẮT GIAO DIỆN SHOP ---")]
    public GameObject shopPanel;

    [Header("--- CÀI ĐẶT HIỆU ỨNG ---")]
    public float thoiGianDemSo = 0.5f;

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        soTienHienThi = tongTien;
        CapNhatGiaoDienTien(soTienHienThi);
        shopPanel.SetActive(false); // Đảm bảo lúc vào game là Shop ẩn
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

        // HIỆN VÀ MỞ KHÓA CHUỘT
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void DongShop()
    {
        shopPanel.SetActive(false);

        _onShopClosed?.Invoke();
        _onShopClosed = null;

        // ẨN VÀ KHÓA CHUỘT LẠI KHI ĐÓNG SHOP
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Cập nhật tham số: Dùng ItemShapeSO cho đồng bộ với code Balo
    public void MuaVatPham(int giaTien, ItemShapeSO monDoDaMua)
    {
        if (tongTien >= giaTien)
        {
            if (monDoDaMua != null && BackpackMinigameUI.Instance != null)
            {
                // Gọi code Balo của bạn bồ để thử nhét đồ vào
                bool themThanhCong = BackpackMinigameUI.Instance.TryAutoAddItem(monDoDaMua);

                if (themThanhCong)
                {
                    // KHI NHÉT BALO THÀNH CÔNG THÌ MỚI TRỪ TIỀN
                    int tienTruocKhiMua = tongTien;
                    tongTien -= giaTien;

                    StopAllCoroutines();
                    StartCoroutine(HieuUngChaySo(tienTruocKhiMua, tongTien));

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
        int tienTruocKhiBan = tongTien;

        // CỘNG TIỀN VÀO TÚI
        tongTien += giaTriVatPham;

        // Chạy lại hiệu ứng đếm số cho mượt
        StopAllCoroutines();
        StartCoroutine(HieuUngChaySo(tienTruocKhiBan, tongTien));

        Debug.Log("Đã bán thành công! Thu về " + giaTriVatPham + " vàng.");
    }

    IEnumerator HieuUngChaySo(int soBatDau, int soKetThuc)
    {
        float thoiGianDaChay = 0f;
        while (thoiGianDaChay < thoiGianDemSo)
        {
            thoiGianDaChay += Time.deltaTime;
            float phanTram = thoiGianDaChay / thoiGianDemSo;
            soTienHienThi = Mathf.RoundToInt(Mathf.Lerp(soBatDau, soKetThuc, phanTram));
            CapNhatGiaoDienTien(soTienHienThi);
            yield return null;
        }
        soTienHienThi = soKetThuc;
        CapNhatGiaoDienTien(soTienHienThi);
    }

    private void CapNhatGiaoDienTien(int soTien)
    {
        if (txtHienThiTien != null)
        {
            txtHienThiTien.text = soTien.ToString() + "K";
        }
    }
}