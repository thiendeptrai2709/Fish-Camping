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

    // ĐÃ SỬA: Thêm (System.Action onClose) để nhận Giao ước từ NPC
    public void MoShop(System.Action onClose = null)
    {
        _onShopClosed = onClose; // Ghi nhớ lại NPC nào vừa gọi
        shopPanel.SetActive(true);
    }

    // ĐÃ SỬA: Báo cho NPC biết khi tắt
    public void DongShop()
    {
        shopPanel.SetActive(false);

        // CỰC KỲ QUAN TRỌNG: Gọi ngược lại NPC để nhả trạng thái "Đang tương tác"
        _onShopClosed?.Invoke();
        _onShopClosed = null; // Xóa trí nhớ
    }

    public void MuaVatPham(int giaTien)
    {
        if (tongTien >= giaTien)
        {
            int tienTruocKhiMua = tongTien;
            tongTien -= giaTien;

            StopAllCoroutines();
            StartCoroutine(HieuUngChaySo(tienTruocKhiMua, tongTien));
            Debug.Log("Đã mua thành công! Trừ " + giaTien + " vàng.");
        }
        else
        {
            Debug.Log("Không đủ tiền để mua món này!");
        }
    }
    // TÍNH NĂNG MỚI: Dành cho việc bán cá hoặc bán đồ
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