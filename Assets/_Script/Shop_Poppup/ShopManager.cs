using UnityEngine;
using TMPro;
using System.Collections; // BẮT BUỘC: Thêm thư viện này để dùng được Coroutine

public class ShopManager : MonoBehaviour
{
    [Header("--- HỆ THỐNG TIỀN ---")]
    public int tongTien = 5000;         // Tiền thật trong túi (Logic)
    private int soTienHienThi;          // Tiền ảo đang chạy trên màn hình (Giao diện)
    public TextMeshProUGUI txtHienThiTien;

    [Header("--- BẬT/TẮT GIAO DIỆN SHOP ---")]
    public GameObject shopPanel;

    [Header("--- CÀI ĐẶT HIỆU ỨNG ---")]
    public float thoiGianDemSo = 0.5f;  // Chỉnh thời gian đếm số nhanh/chậm (giây)

    void Start()
    {
        // Vừa vào game, gán số hiển thị bằng tổng tiền và cập nhật luôn
        soTienHienThi = tongTien;
        CapNhatGiaoDienTien(soTienHienThi);
        DongShop();
    }

    public void MoShop()
    {
        shopPanel.SetActive(true);
    }

    public void DongShop()
    {
        shopPanel.SetActive(false);
    }

    public void MuaVatPham(int giaTien)
    {
        if (tongTien >= giaTien)
        {
            int tienTruocKhiMua = tongTien; // Nhớ lại lúc chưa trừ đang có bao nhiêu tiền

            // TRỪ TIỀN GỐC NGAY LẬP TỨC (Rất quan trọng: Chống lỗi người chơi bấm mua nhanh 2 lần)
            tongTien -= giaTien;

            // Dừng hiệu ứng cũ (nếu đang chạy dở do bấm mua liên tục) và bắt đầu đếm số mới
            StopAllCoroutines();
            StartCoroutine(HieuUngChaySo(tienTruocKhiMua, tongTien));

            Debug.Log("Đã mua thành công! Trừ " + giaTien + " vàng.");
        }
        else
        {
            Debug.Log("Không đủ tiền để mua món này!");
        }
    }

    // --- CỤC PHÉP THUẬT XỬ LÝ VIỆC ĐẾM SỐ TỪ TỪ ---
    IEnumerator HieuUngChaySo(int soBatDau, int soKetThuc)
    {
        float thoiGianDaChay = 0f;

        // Vòng lặp này sẽ chạy liên tục cho đến khi hết thời gian (thoiGianDemSo)
        while (thoiGianDaChay < thoiGianDemSo)
        {
            thoiGianDaChay += Time.deltaTime;

            // Hàm Lerp sẽ tự động tính toán ra con số ở giữa cực kỳ mượt
            float phanTram = thoiGianDaChay / thoiGianDemSo;
            soTienHienThi = Mathf.RoundToInt(Mathf.Lerp(soBatDau, soKetThuc, phanTram));

            // Cập nhật số đang chạy lên màn hình
            CapNhatGiaoDienTien(soTienHienThi);

            yield return null; // Lệnh chờ tới khung hình (frame) tiếp theo rồi mới vòng lại
        }

        // Chốt hạ: Đảm bảo khi hết giờ thì số hiển thị chính xác 100% bằng số tiền thật
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