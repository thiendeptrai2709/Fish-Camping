using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    private const string SAVE_MONEY_KEY = "PlayerMoney";
    private const string INIT_MONEY_KEY = "HasInitializedMoney";

    [Header("--- CÀI ĐẶT TIỀN CHO USER MỚI ---")]
    [Tooltip("Số tiền cố định cấp cho người chơi mới lần đầu vào game")]
    [SerializeField] private int soTienKhoiTaoChoUserMoi = 5000;

    [Header("--- SỐ TIỀN THẬT CỦA NGƯỜI CHƠI ---")]
    public int tongTien;

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

            KhoiTaoTienNguoiChoi();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void KhoiTaoTienNguoiChoi()
    {
        // Kiểm tra xem user này đã từng vào game và nhận tiền khởi tạo chưa
        if (!PlayerPrefs.HasKey(INIT_MONEY_KEY))
        {
            tongTien = soTienKhoiTaoChoUserMoi;
            PlayerPrefs.SetInt(SAVE_MONEY_KEY, tongTien);
            PlayerPrefs.SetInt(INIT_MONEY_KEY, 1);
            PlayerPrefs.Save();
            Debug.Log($"<color=cyan>[MoneyManager] Khởi tạo tài khoản mới: Cấp {tongTien} tiền cố định ban đầu.</color>");
        }
        else
        {
            tongTien = PlayerPrefs.GetInt(SAVE_MONEY_KEY, soTienKhoiTaoChoUserMoi);
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
#if UNITY_EDITOR
        // Bấm F9 để Reset tiền về mặc định (Chỉ trong Unity Editor)
        if (Input.GetKeyDown(phimResetTien))
        {
            ResetTien(soTienKhoiTaoChoUserMoi);
        }

        // Bấm F10 để buff nhanh +5000 tiền test mua đồ (Chỉ trong Unity Editor)
        if (Input.GetKeyDown(phimCongThemTien))
        {
            CongTien(5000);
            Debug.Log("<color=green>[Cheat] Đã cộng thêm 5000 Vàng!</color>");
        }
#endif
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        KhoiTaoTienNguoiChoi();
        CapNhatLaiDanhSachTextTien();
    }

    public void RefreshMoneyFromSave()
    {
        KhoiTaoTienNguoiChoi();
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

        PlayerPrefs.SetInt(SAVE_MONEY_KEY, tongTien);
        PlayerPrefs.Save();

        ChayHieuUngDemSo(tienTruoc, tongTien);
        return true;
    }

    public void CongTien(int soTien)
    {
        int tienTruoc = tongTien;
        tongTien += soTien;

        PlayerPrefs.SetInt(SAVE_MONEY_KEY, tongTien);
        PlayerPrefs.Save();

        ChayHieuUngDemSo(tienTruoc, tongTien);
    }

    // ==========================================
    // CÁC HÀM RESET TIỀN ĐỂ TEST GAME
    // ==========================================

    public void ResetTien(int soTienMacDinh)
    {
        int tienTruoc = tongTien;
        tongTien = soTienMacDinh;

        PlayerPrefs.SetInt(SAVE_MONEY_KEY, tongTien);
        PlayerPrefs.SetInt(INIT_MONEY_KEY, 1);
        PlayerPrefs.Save();

        ChayHieuUngDemSo(tienTruoc, tongTien);
        Debug.Log($"<color=yellow>[MoneyManager] Đã Reset tiền về: {soTienMacDinh} Vàng!</color>");
    }

    [ContextMenu("Xóa dữ liệu tiền (Reset PlayerPrefs)")]
    public void XoaLuuTruTien()
    {
        PlayerPrefs.DeleteKey(SAVE_MONEY_KEY);
        PlayerPrefs.DeleteKey(INIT_MONEY_KEY);
        PlayerPrefs.Save();

        tongTien = soTienKhoiTaoChoUserMoi;
        CapNhatGiaoDienTien(tongTien);
        Debug.Log("<color=cyan>[MoneyManager] Đã xóa PlayerPrefs tiền và đưa về trạng thái user mới thành công!</color>");
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