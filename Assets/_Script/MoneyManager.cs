using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Nơi DUY NHẤT giữ số tiền thật của người chơi trong toàn bộ game.
// ShopManager (shop câu cá) và GarageZone (garage lốp xe) đều đọc/ghi tiền qua đây,
// nên dù đứng ở đâu, số tiền và mọi Text hiển thị đều luôn khớp nhau.
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("--- SỐ TIỀN THẬT CỦA NGƯỜI CHƠI ---")]
    public int tongTien = 5000;

    [Tooltip("Kéo TẤT CẢ các Text hiển thị tiền trong game vào đây (Shop câu cá, Garage lốp xe, HUD ngoài map...) - tất cả sẽ tự động chạy số cùng lúc")]
    public TextMeshProUGUI[] cacTextHienThiTien;

    [Header("--- CÀI ĐẶT HIỆU ỨNG CHẠY SỐ ---")]
    public float thoiGianDemSo = 0.5f;

    private Coroutine coroutineDemSo;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Giữ nguyên qua các Scene khác (nếu Shop và Garage nằm ở 2 Scene riêng).
            // Nếu 2 khu vực đang ở chung 1 Scene thì dòng này không gây hại gì cả.
            DontDestroyOnLoad(gameObject);

            // BẮT BUỘC: Vì object này sống xuyên Scene (DontDestroyOnLoad) nhưng các Text
            // hiển thị tiền lại là UI của TỪNG Scene (bị hủy khi đổi Scene), nên phải tự
            // đăng ký lắng nghe mỗi lần load Scene mới để "nối" lại đúng Text của Scene đó.
            // Nếu không làm cái này, sau khi đổi map quay lại, Text tiền sẽ bị kẹt ở giá trị
            // cũ/mặc định (nhìn như tiền bị reset về 0) dù tongTien bên trong vẫn đúng.
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        CapNhatLaiDanhSachTextTien();
    }

    // Được gọi tự động mỗi khi 1 Scene mới load xong (kể cả lúc quay lại 1 map đã đi qua)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CapNhatLaiDanhSachTextTien();
    }

    // Tự đi tìm lại TẤT CẢ Text đang gắn Tag "TextTien" trong Scene hiện tại rồi cập nhật số tiền.
    private void CapNhatLaiDanhSachTextTien()
    {
        try
        {
            // Phép thuật ở đây: Tìm TẤT CẢ các Text trong game, kể cả các Text đang bị ẨN
            TextMeshProUGUI[] tatCaText = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            List<TextMeshProUGUI> danhSachMoi = new List<TextMeshProUGUI>();

            foreach (TextMeshProUGUI txt in tatCaText)
            {
                // Lọc: Chỉ lấy những Text đang nằm trong Scene hiện tại (bỏ qua Prefab) và có Tag "TextTien"
                if (txt.gameObject.scene.isLoaded && txt.CompareTag("TextTien"))
                {
                    danhSachMoi.Add(txt);
                }
            }
            cacTextHienThiTien = danhSachMoi.ToArray();
        }
        catch (UnityException)
        {
            Debug.LogWarning("[MoneyManager] Chưa tạo Tag \"TextTien\" trong Project Settings...");
        }

        CapNhatGiaoDienTien(tongTien);
    }

    // Kiểm tra có đủ tiền không, dùng trước khi cho phép mua
    public bool CoDuTien(int soTien)
    {
        return tongTien >= soTien;
    }

    // Cố trừ tiền. Trả về false và KHÔNG trừ gì nếu không đủ tiền.
    public bool TruTien(int soTien)
    {
        if (tongTien < soTien) return false;

        int tienTruoc = tongTien;
        tongTien -= soTien;
        ChayHieuUngDemSo(tienTruoc, tongTien);
        return true;
    }

    // Cộng tiền (bán đồ, thưởng...)
    public void CongTien(int soTien)
    {
        int tienTruoc = tongTien;
        tongTien += soTien;
        ChayHieuUngDemSo(tienTruoc, tongTien);
    }

    private void ChayHieuUngDemSo(int tuSo, int denSo)
    {
        if (coroutineDemSo != null) StopCoroutine(coroutineDemSo);
        coroutineDemSo = StartCoroutine(HieuUngChaySo(tuSo, denSo));
    }

    private IEnumerator HieuUngChaySo(int soBatDau, int soKetThuc)
    {
        float thoiGianDaChay = 0f;
        while (thoiGianDaChay < thoiGianDemSo)
        {
            thoiGianDaChay += Time.deltaTime;
            float phanTram = thoiGianDaChay / thoiGianDemSo;
            int soHienTai = Mathf.RoundToInt(Mathf.Lerp(soBatDau, soKetThuc, phanTram));
            CapNhatGiaoDienTien(soHienTai);
            yield return null;
        }
        CapNhatGiaoDienTien(soKetThuc);
    }

    private void CapNhatGiaoDienTien(int soTien)
    {
        if (cacTextHienThiTien == null) return;

        string chuoiHienThi = soTien.ToString() + "K";
        foreach (TextMeshProUGUI txt in cacTextHienThiTien)
        {
            if (txt != null) txt.text = chuoiHienThi;
        }
    }
}