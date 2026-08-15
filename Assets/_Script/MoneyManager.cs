using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("--- SỐ TIỀN THẬT CỦA NGƯỜI CHƠI ---")]
    public int tongTien = 5000;

    [Tooltip("Kéo TẤT CẢ các Text hiển thị tiền trong game vào đây (Shop câu cá, Garage lốp xe, HUD ngoài map...)")]
    public TextMeshProUGUI[] cacTextHienThiTien;

    [Header("--- CÀI ĐẶT HIỆU ỨNG CHẠY SỐ ---")]
    public float thoiGianDemSo = 0.5f;

    [Header("--- PHÍM TẮT TEST DEBUG (TRONG LÚC CHƠI) ---")]
    [Tooltip("Bấm phím này để reset tiền về mặc định")]
    public KeyCode phimResetTien = KeyCode.F9;
    [Tooltip("Bấm phím này để cộng thêm 5000 tiền test nhanh")]
    public KeyCode phimCongThemTien = KeyCode.F10;

    private Coroutine coroutineDemSo;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            tongTien = PlayerPrefs.GetInt("PlayerMoney", 5000);
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

    private void Update()
    {
        // Bấm F9 để Reset tiền về 5000 (hoặc xóa sạch dữ liệu tiền)
        if (Input.GetKeyDown(phimResetTien))
        {
            ResetTien(5000);
        }

        // Bấm F10 để buff nhanh +5000 tiền test mua đồ
        if (Input.GetKeyDown(phimCongThemTien))
        {
            CongTien(5000);
            Debug.Log("<color=green>[Cheat] Đã cộng thêm 5000 Vàng!</color>");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CapNhatLaiDanhSachTextTien();
    }

    private void CapNhatLaiDanhSachTextTien()
    {
        try
        {
            TextMeshProUGUI[] tatCaText = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            List<TextMeshProUGUI> danhSachMoi = new List<TextMeshProUGUI>();

            foreach (TextMeshProUGUI txt in tatCaText)
            {
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

    public bool CoDuTien(int soTien)
    {
        return tongTien >= soTien;
    }

    public bool TruTien(int soTien)
    {
        if (tongTien < soTien) return false;

        int tienTruoc = tongTien;
        tongTien -= soTien;

        PlayerPrefs.SetInt("PlayerMoney", tongTien);
        PlayerPrefs.Save();

        ChayHieuUngDemSo(tienTruoc, tongTien);
        return true;
    }

    public void CongTien(int soTien)
    {
        int tienTruoc = tongTien;
        tongTien += soTien;

        PlayerPrefs.SetInt("PlayerMoney", tongTien);
        PlayerPrefs.Save();

        ChayHieuUngDemSo(tienTruoc, tongTien);
    }

    // ==========================================
    // CÁC HÀM RESET TIỀN ĐỂ TEST GAME
    // ==========================================

    // Gọi bằng code hoặc phím F9
    public void ResetTien(int soTienMacDinh = 5000)
    {
        int tienTruoc = tongTien;
        tongTien = soTienMacDinh;

        PlayerPrefs.SetInt("PlayerMoney", tongTien);
        PlayerPrefs.Save();

        ChayHieuUngDemSo(tienTruoc, tongTien);
        Debug.Log($"<color=yellow>[MoneyManager] Đã Reset tiền về: {soTienMacDinh} Vàng!</color>");
    }

    // Nút bấm trên thanh Menu Unity (ngay cả khi chưa Play game)
    [ContextMenu("Xóa dữ liệu tiền (Reset PlayerPrefs)")]
    public void XoaLuuTruTien()
    {
        PlayerPrefs.DeleteKey("PlayerMoney");
        PlayerPrefs.Save();
        tongTien = 5000;
        CapNhatGiaoDienTien(tongTien);
        Debug.Log("<color=cyan>[MoneyManager] Đã xóa PlayerPrefs tiền thành công!</color>");
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