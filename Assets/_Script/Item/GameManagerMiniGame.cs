using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class GameManagerMiniGame : MonoBehaviour
{
    #region PHẦN 0: QUẢN LÝ ÂM THANH (AUDIO MANAGER)
    [Header("---- QUẢN LÝ ÂM THANH (AUDIO) ----")]
    public AudioSource bgmAudioSource;      // AudioSource phát nhạc nền
    public AudioSource sfxAudioSource;      // AudioSource phát hiệu ứng sound

    [Header("---- BẢN NHẠC NỀN (BGM) ----")]
    public AudioClip bgmGameNemDa;          // Nhạc nền Game Ném Đá
    public AudioClip bgmGameLatThe;         // Nhạc nền Game Lật Thẻ

    [Header("---- ÂM THANH HIỆU ỨNG (SFX) ----")]
    public AudioClip sfxLatTheDung;         // SFX lật đúng
    public AudioClip sfxLatTheSai;          // SFX lật sai
    public AudioClip sfxNemDaTrung;         // SFX ném trúng bia

    [Header("---- UI NÚT TẮT/BẬT NHẠC ----")]
    public Button btnToggleMusicLatThe;
    public Button btnToggleMusicNemDa;
    public Sprite iconMusicOn;
    public Sprite iconMusicOff;
    private bool isMusicMuted = false;

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        if (bgmAudioSource != null) bgmAudioSource.mute = isMusicMuted;

        CapNhatSpriteNut(btnToggleMusicLatThe);
        CapNhatSpriteNut(btnToggleMusicNemDa);
    }

    private void CapNhatSpriteNut(Button btn)
    {
        if (btn != null)
        {
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null && iconMusicOn != null && iconMusicOff != null)
            {
                btnImg.sprite = isMusicMuted ? iconMusicOff : iconMusicOn;
            }
        }
    }

    private void PlayBGM(AudioClip clip)
    {
        if (bgmAudioSource == null || clip == null) return;
        bgmAudioSource.clip = clip;
        bgmAudioSource.mute = isMusicMuted;
        bgmAudioSource.Play();
    }

    private void StopBGM()
    {
        if (bgmAudioSource != null) bgmAudioSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }
    #endregion

    #region PHẦN 1: QUẢN LÝ MENU SẢNH & TỰ ĐỘNG QUẢN LÝ CANVAS
    [Header("---- KHAI BÁO CANVAS MINIGAME ----")]
    public Canvas mainCanvasMiniGame;

    [Header("---- QUẢN LÝ LUỒNG UI MENU ----")]
    public GameObject panelMenuMiniGame;
    public Image imgNenMinhHoa;
    public Sprite[] danhSachAnhNenMenu;
    public Button btnThamGia;

    private int idGameDangChon = 0;

    // Lưu danh sách các CanvasGroup thực sự ĐANG BẬT trước khi ẩn
    private List<CanvasGroup> listCanvasGroupDaAn = new List<CanvasGroup>();

    private void Start()
    {
        if (mainCanvasMiniGame == null)
        {
            mainCanvasMiniGame = GetComponentInParent<Canvas>();
            if (mainCanvasMiniGame == null) mainCanvasMiniGame = GetComponent<Canvas>();
        }

        AnToanBoPanelMiniGame();
    }

    private void AnToanBoPanelMiniGame()
    {
        if (panelMenuMiniGame != null) panelMenuMiniGame.SetActive(false);
        if (panelMiniGame != null) panelMiniGame.SetActive(false);
        if (panelGameNemDa != null) panelGameNemDa.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelGameWin != null) panelGameWin.SetActive(false);
        if (panelGameOverNemDa != null) panelGameOverNemDa.SetActive(false);
        if (panelGameWinNemDa != null) panelGameWinNemDa.SetActive(false);
    }

    public void MoMenuMiniGame()
    {
        // 1. HIỆN VÀ MỞ KHÓA CON CHUỘT
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 2. Ẩn an toàn các Canvas khác đang hiển thị
        AnTatCaCanvasKhac();

        // 3. Hiển thị Panel Menu
        if (panelMenuMiniGame != null) panelMenuMiniGame.SetActive(true);
        if (panelMiniGame != null) panelMiniGame.SetActive(false);
        if (panelGameNemDa != null) panelGameNemDa.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelGameWin != null) panelGameWin.SetActive(false);
        if (panelGameOverNemDa != null) panelGameOverNemDa.SetActive(false);
        if (panelGameWinNemDa != null) panelGameWinNemDa.SetActive(false);

        AnTatCaNutMusic();
        StopBGM();
        ChonGameOMenu(0);
    }

    public void DongMenuHoanToan()
    {
        StopBGM();
        AnTatCaNutMusic();

        AnToanBoPanelMiniGame();

        // 1. Khôi phục lại trạng thái Canvas ban đầu
        KhoiPhucCanvasKhac();

        // 2. ẨN VÀ KHÓA CHUỘT TRỞ LẠI GAME CHÍNH
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void AnTatCaCanvasKhac()
    {
        listCanvasGroupDaAn.Clear();

        // Chỉ tìm các Canvas ĐANG BẬT (Exclude inactive)
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Canvas c in allCanvases)
        {
            if (mainCanvasMiniGame != null && c == mainCanvasMiniGame) continue;
            if (c.gameObject == this.gameObject) continue;

            CanvasGroup cg = c.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = c.gameObject.AddComponent<CanvasGroup>();
            }

            // Chỉ lưu và ẩn nếu CanvasGroup đang thực sự hiển thị
            if (cg.alpha > 0f)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;

                listCanvasGroupDaAn.Add(cg);
            }
        }
    }

    private void KhoiPhucCanvasKhac()
    {
        foreach (CanvasGroup cg in listCanvasGroupDaAn)
        {
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
        listCanvasGroupDaAn.Clear();
    }

    public void ChonGameOMenu(int idGame)
    {
        idGameDangChon = idGame;
        if (btnThamGia != null) btnThamGia.interactable = true;
        if (danhSachAnhNenMenu != null && danhSachAnhNenMenu.Length > idGame && danhSachAnhNenMenu[idGame] != null)
        {
            imgNenMinhHoa.sprite = danhSachAnhNenMenu[idGame];
        }
    }

    public void NhanThamGia()
    {
        if (panelMenuMiniGame != null) panelMenuMiniGame.SetActive(false);

        if (idGameDangChon == 0) // Ném Đá
        {
            if (panelGameNemDa != null) panelGameNemDa.SetActive(true);

            if (btnToggleMusicNemDa != null) btnToggleMusicNemDa.gameObject.SetActive(true);
            if (btnToggleMusicLatThe != null) btnToggleMusicLatThe.gameObject.SetActive(false);

            PlayBGM(bgmGameNemDa);
            KhoiTaoGameNemDa();
        }
        else if (idGameDangChon == 1) // Lật Thẻ
        {
            if (panelMiniGame != null) panelMiniGame.SetActive(true);

            if (btnToggleMusicLatThe != null) btnToggleMusicLatThe.gameObject.SetActive(true);
            if (btnToggleMusicNemDa != null) btnToggleMusicNemDa.gameObject.SetActive(false);

            PlayBGM(bgmGameLatThe);
            KhoiTaoGameMoi();
        }
    }

    public void ThoatVeMenu()
    {
        AnToanBoPanelMiniGame();
        AnTatCaNutMusic();
        StopBGM();
        MoMenuMiniGame();
    }

    private void AnTatCaNutMusic()
    {
        if (btnToggleMusicLatThe != null) btnToggleMusicLatThe.gameObject.SetActive(false);
        if (btnToggleMusicNemDa != null) btnToggleMusicNemDa.gameObject.SetActive(false);
    }
    #endregion

    #region PHẦN 2: LOGIC GAME LẬT THẺ
    [Header("---- GIAO DIỆN & ĐỐI TƯỢNG (Lật Thẻ) ----")]
    public GameObject panelMiniGame;
    public Transform gridChuaThe;
    public GameObject mauThePrefab;
    public TextMeshProUGUI txtSoLanSai;
    public GameObject panelGameOver;
    public GameObject panelGameWin;

    [Header("---- HÌNH ẢNH (Lật Thẻ) ----")]
    public Sprite hinhMatSau;
    public Sprite[] danhSachHinhMatTruoc;

    [Header("---- CÀI ĐẶT GAME (Lật Thẻ) ----")]
    public int tongSoThe = 60;
    public float thoiGianGhiNho = 10f;
    public int soLanSaiToiDa = 10;

    [HideInInspector] public bool choPhepClick = false;

    private List<CardScript> danhSachThe = new List<CardScript>();
    private CardScript theThuNhat;
    private CardScript theThuHai;
    private int soCapDaTimThay = 0;
    private int soLanSaiHienTai = 0;

    public void ChoiLai() => KhoiTaoGameMoi();

    private void KhoiTaoGameMoi()
    {
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelGameWin != null) panelGameWin.SetActive(false);

        choPhepClick = false;
        soCapDaTimThay = 0;
        soLanSaiHienTai = 0;
        theThuNhat = null;
        theThuHai = null;

        CapNhatUITextSai();

        foreach (Transform child in gridChuaThe) Destroy(child.gameObject);
        danhSachThe.Clear();

        if (tongSoThe % 2 != 0) tongSoThe += 1;
        int soCapCanTao = tongSoThe / 2;
        List<int> danhSachID = new List<int>();

        for (int i = 0; i < soCapCanTao; i++)
        {
            int idNgauNhien = Random.Range(0, danhSachHinhMatTruoc.Length);
            danhSachID.Add(idNgauNhien);
            danhSachID.Add(idNgauNhien);
        }

        for (int i = 0; i < danhSachID.Count; i++)
        {
            int temp = danhSachID[i];
            int r = Random.Range(i, danhSachID.Count);
            danhSachID[i] = danhSachID[r];
            danhSachID[r] = temp;
        }

        for (int i = 0; i < danhSachID.Count; i++)
        {
            GameObject theMoi = Instantiate(mauThePrefab, gridChuaThe);
            CardScript scriptCuaThe = theMoi.GetComponent<CardScript>();
            int id = danhSachID[i];
            scriptCuaThe.CaiDatThe(id, danhSachHinhMatTruoc[id], hinhMatSau, this);
            danhSachThe.Add(scriptCuaThe);
        }
        StartCoroutine(ChoGhiNhoRoutine());
    }

    private void CapNhatUITextSai()
    {
        if (txtSoLanSai != null)
            txtSoLanSai.text = "Số lần sai còn lại: " + (soLanSaiToiDa - soLanSaiHienTai).ToString();
    }

    private IEnumerator ChoGhiNhoRoutine()
    {
        yield return new WaitForSeconds(thoiGianGhiNho);
        foreach (CardScript the in danhSachThe) the.LatUp();
        choPhepClick = true;
    }

    public void XuLyChonThe(CardScript theDuocChon)
    {
        if (theThuNhat == null) theThuNhat = theDuocChon;
        else if (theThuHai == null)
        {
            theThuHai = theDuocChon;
            StartCoroutine(KiemTraCapGiongNhau());
        }
    }

    private IEnumerator KiemTraCapGiongNhau()
    {
        choPhepClick = false;
        yield return new WaitForSeconds(0.5f);

        if (theThuNhat.id_the == theThuHai.id_the)
        {
            PlaySFX(sfxLatTheDung);
            theThuNhat.AnTheDi();
            theThuHai.AnTheDi();
            soCapDaTimThay++;

            if (soCapDaTimThay == (tongSoThe / 2))
            {
                if (panelGameWin != null) panelGameWin.SetActive(true);
            }
        }
        else
        {
            PlaySFX(sfxLatTheSai);
            theThuNhat.LatUp();
            theThuHai.LatUp();
            soLanSaiHienTai++;
            CapNhatUITextSai();
            if (soLanSaiHienTai >= soLanSaiToiDa && panelGameOver != null) panelGameOver.SetActive(true);
        }

        theThuNhat = null;
        theThuHai = null;

        if (soLanSaiHienTai < soLanSaiToiDa && soCapDaTimThay < (tongSoThe / 2)) choPhepClick = true;
    }
    #endregion

    #region PHẦN 3: LOGIC GAME NÉM ĐÁ
    [Header("---- GIAO DIỆN & ĐỐI TƯỢNG (Ném Đá) ----")]
    public GameObject panelGameNemDa;
    public GameObject stonePrefab;
    public Transform viTrisXuatPhat;
    public Slider sliderLucBan;
    public TextMeshProUGUI txtDiemSoNemDa;
    public TextMeshProUGUI txtSoDaConLai;
    public GameObject panelGameOverNemDa;
    public GameObject panelGameWinNemDa;

    [Header("---- CÀI ĐẶT GAME (Ném Đá) ----")]
    public int soDaToiDa = 10;
    public int tongSoBia = 5;
    public GameObject[] danhSachTatCaBia;

    private int diemNemDa = 0;
    private float lucBan = 0f;
    private bool dangGiuChuot = false;
    public float tocDoTangLuc = 40f;
    private Vector2 huongBanThucTe = Vector2.up;

    private int soDaConLai;
    private int soBiaDaPha = 0;
    private bool gameNemDaKetThuc = false;

    private class ThongTinVienDa
    {
        public GameObject objVienDa;
        public Vector2 huongBay;
        public float tocDo;
        public float thoiGianSong = 3f;
    }
    private List<ThongTinVienDa> danhSachVienDaDangBay = new List<ThongTinVienDa>();

    private void Awake()
    {
        if (danhSachTatCaBia == null || danhSachTatCaBia.Length == 0)
        {
            danhSachTatCaBia = GameObject.FindGameObjectsWithTag("Target");
        }
    }

    public void KhoiTaoGameNemDa()
    {
        diemNemDa = 0;
        soBiaDaPha = 0;
        soDaConLai = soDaToiDa;
        gameNemDaKetThuc = false;

        if (danhSachTatCaBia != null && danhSachTatCaBia.Length > 0)
        {
            foreach (GameObject bia in danhSachTatCaBia)
            {
                if (bia != null) bia.SetActive(true);
            }
        }

        CapNhatUIDiemNemDa();
        CapNhatUISoDaConLai();

        foreach (var da in danhSachVienDaDangBay)
        {
            if (da.objVienDa != null) Destroy(da.objVienDa);
        }
        danhSachVienDaDangBay.Clear();

        if (sliderLucBan != null)
        {
            sliderLucBan.minValue = 0f;
            sliderLucBan.maxValue = 60f;
            sliderLucBan.value = 0f;
        }

        if (panelGameOverNemDa != null) panelGameOverNemDa.SetActive(false);
        if (panelGameWinNemDa != null) panelGameWinNemDa.SetActive(false);
    }

    private void Update()
    {
        // --- BẤM PHÍM Z ĐỂ MỞ / ĐÓNG MINI GAME ---
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.zKey.wasPressedThisFrame)
        {
            bool dangMoMiniGame = (panelMenuMiniGame != null && panelMenuMiniGame.activeSelf) ||
                                  (panelGameNemDa != null && panelGameNemDa.activeSelf) ||
                                  (panelMiniGame != null && panelMiniGame.activeSelf);

            if (!dangMoMiniGame)
            {
                MoMenuMiniGame(); // Lần 1: Mở game + Hiện & mở khóa chuột
            }
            else
            {
                DongMenuHoanToan(); // Lần 2: Đóng game + Ẩn & khóa chuột lại
            }
        }

        // Logic bay của viên đá
        for (int i = danhSachVienDaDangBay.Count - 1; i >= 0; i--)
        {
            var da = danhSachVienDaDangBay[i];
            if (da.objVienDa == null)
            {
                danhSachVienDaDangBay.RemoveAt(i);
                continue;
            }

            da.objVienDa.transform.position += (Vector3)(da.huongBay * da.tocDo * Time.deltaTime);
            da.thoiGianSong -= Time.deltaTime;

            bool daTrungBia = false;
            if (danhSachTatCaBia != null)
            {
                foreach (GameObject bia in danhSachTatCaBia)
                {
                    if (bia != null && bia.activeSelf)
                    {
                        float khoangCach = Vector2.Distance(da.objVienDa.transform.position, bia.transform.position);
                        if (khoangCach < 50f)
                        {
                            PlaySFX(sfxNemDaTrung);
                            CongDiemNemDa(10);
                            bia.SetActive(false);
                            Destroy(da.objVienDa);
                            danhSachVienDaDangBay.RemoveAt(i);
                            daTrungBia = true;

                            soBiaDaPha++;

                            if (soBiaDaPha >= tongSoBia)
                            {
                                gameNemDaKetThuc = true;
                                if (panelGameWinNemDa != null) panelGameWinNemDa.SetActive(true);
                            }
                            break;
                        }
                    }
                }
            }

            if (daTrungBia) continue;

            if (da.thoiGianSong <= 0)
            {
                Destroy(da.objVienDa);
                danhSachVienDaDangBay.RemoveAt(i);
            }
        }

        if (panelGameNemDa != null && panelGameNemDa.activeSelf && !gameNemDaKetThuc)
        {
            if (soDaConLai <= 0 && danhSachVienDaDangBay.Count == 0 && soBiaDaPha < tongSoBia)
            {
                gameNemDaKetThuc = true;
                if (panelGameOverNemDa != null) panelGameOverNemDa.SetActive(true);
            }
        }

        // Logic kéo lực ném đá
        if (panelGameNemDa != null && panelGameNemDa.activeSelf && !gameNemDaKetThuc)
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame && soDaConLai > 0)
            {
                dangGiuChuot = true;
                lucBan = 5f;
            }

            if (dangGiuChuot && mouse.leftButton.isPressed)
            {
                lucBan += tocDoTangLuc * Time.deltaTime;
                if (lucBan > 60f) lucBan = 60f;
                if (sliderLucBan != null) sliderLucBan.value = lucBan;
            }

            if (dangGiuChuot && mouse.leftButton.wasReleasedThisFrame)
            {
                dangGiuChuot = false;

                Vector2 viTriChuot = mouse.position.ReadValue();
                Vector2 viTriGoc = viTrisXuatPhat.position;
                huongBanThucTe = (viTriChuot - viTriGoc).normalized;

                ThucHienBanVienDa(lucBan, huongBanThucTe);

                lucBan = 0f;
                if (sliderLucBan != null) sliderLucBan.value = 0f;
            }
        }
    }

    private void ThucHienBanVienDa(float luc, Vector2 huongBan)
    {
        if (stonePrefab == null || viTrisXuatPhat == null || panelGameNemDa == null) return;

        GameObject vienDaMoi = Instantiate(stonePrefab, viTrisXuatPhat.position, Quaternion.identity, panelGameNemDa.transform);
        vienDaMoi.transform.localScale = Vector3.one;

        ThongTinVienDa daMoi = new ThongTinVienDa()
        {
            objVienDa = vienDaMoi,
            huongBay = huongBan,
            tocDo = luc * 18f
        };
        danhSachVienDaDangBay.Add(daMoi);

        soDaConLai--;
        CapNhatUISoDaConLai();
    }

    public void CongDiemNemDa(int diemThem)
    {
        diemNemDa += diemThem;
        CapNhatUIDiemNemDa();
    }

    private void CapNhatUIDiemNemDa()
    {
        if (txtDiemSoNemDa != null)
        {
            txtDiemSoNemDa.text = "Điểm: " + diemNemDa.ToString();
        }
    }

    private void CapNhatUISoDaConLai()
    {
        if (txtSoDaConLai != null)
        {
            txtSoDaConLai.text = "Đá: " + soDaConLai.ToString();
        }
    }
    #endregion
}