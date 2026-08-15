using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GameManagerMiniGame : MonoBehaviour
{
    #region PHẦN 0: QUẢN LÝ ÂM THANH (AUDIO MANAGER)
    [Header("---- QUẢN LÝ ÂM THANH (AUDIO) ----")]
    public AudioSource bgmAudioSource;      // AudioSource dùng để phát nhạc nền (Loop = true)
    public AudioSource sfxAudioSource;      // AudioSource dùng để phát âm thanh hiệu ứng (SFX)

    [Header("---- BẢN NHẠC NỀN (BGM) ----")]
    public AudioClip bgmGameNemDa;          // Nhạc nền Game Ném Đá
    public AudioClip bgmGameLatThe;         // Nhạc nền Game Lật Thẻ

    [Header("---- ÂM THANH HIỆU ỨNG (SFX) ----")]
    public AudioClip sfxLatTheDung;         // Âm thanh lật đúng (+Điểm)
    public AudioClip sfxLatTheSai;          // Âm thanh lật sai
    public AudioClip sfxNemDaTrung;         // Âm thanh ném đá trúng bia (+Điểm)

    [Header("---- UI NÚT TẮT/BẬT NHẠC (2 GAME SEPARATE) ----")]
    public Button btnToggleMusicLatThe;     // Nút Tắt/Bật nhạc nằm trong Panel Game Lật Thẻ
    public Button btnToggleMusicNemDa;      // Nút Tắt/Bật nhạc nằm trong Panel Game Ném Đá
    public Sprite iconMusicOn;              // Icon loa Bật
    public Sprite iconMusicOff;             // Icon loa Tắt
    private bool isMusicMuted = false;

    // Chức năng bật/tắt nhạc nền khi bấm nút ở bất kỳ game nào
    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        if (bgmAudioSource != null)
        {
            bgmAudioSource.mute = isMusicMuted;
        }

        // Cập nhật trạng thái Icon cho CẢ 2 NÚT để giữ đồng bộ
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
        bgmAudioSource.mute = isMusicMuted; // Đảm bảo giữ đúng trạng thái Mute hiện tại
        bgmAudioSource.Play();
    }

    private void StopBGM()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }
    #endregion

    #region PHẦN 1: QUẢN LÝ MENU SẢNH
    [Header("---- QUẢN LÝ LUỒNG UI MENU ----")]
    public GameObject panelMenuMiniGame;
    public GameObject canvasChinh;          // Kéo Canvas/UI khác cần tắt/mở vào đây
    public Image imgNenMinhHoa;
    public Sprite[] danhSachAnhNenMenu;
    public Button btnThamGia;

    private int idGameDangChon = 0;

    public void MoMenuMiniGame()
    {
        panelMenuMiniGame.SetActive(true);
        if (panelMiniGame != null) panelMiniGame.SetActive(false);
        if (panelGameNemDa != null) panelGameNemDa.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelGameWin != null) panelGameWin.SetActive(false);

        if (panelGameOverNemDa != null) panelGameOverNemDa.SetActive(false);
        if (panelGameWinNemDa != null) panelGameWinNemDa.SetActive(false);

        // Ẩn cả 2 nút âm thanh khi ở Menu chính
        AnTatCaNutMusic();

        StopBGM(); // Tắt nhạc khi vào Menu sảnh
        ChonGameOMenu(0);
    }

    public void DongMenuHoanToan()
    {
        StopBGM();
        AnTatCaNutMusic();
        if (panelMenuMiniGame != null) panelMenuMiniGame.SetActive(false);
    }

    public void ChonGameOMenu(int idGame)
    {
        idGameDangChon = idGame;
        if (btnThamGia != null) btnThamGia.interactable = true;
        if (danhSachAnhNenMenu.Length > idGame && danhSachAnhNenMenu[idGame] != null)
        {
            imgNenMinhHoa.sprite = danhSachAnhNenMenu[idGame];
        }
    }

    public void NhanThamGia()
    {
        panelMenuMiniGame.SetActive(false);

        if (idGameDangChon == 0) // ID 0: Game Ném Đá
        {
            if (panelGameNemDa != null) panelGameNemDa.SetActive(true);

            // Hiển thị nút nhạc của Game Ném Đá
            if (btnToggleMusicNemDa != null) btnToggleMusicNemDa.gameObject.SetActive(true);
            if (btnToggleMusicLatThe != null) btnToggleMusicLatThe.gameObject.SetActive(false);

            PlayBGM(bgmGameNemDa); // Mở nhạc nền Ném Đá
            KhoiTaoGameNemDa();
        }
        else if (idGameDangChon == 1) // ID 1: Game Lật Thẻ
        {
            if (panelMiniGame != null) panelMiniGame.SetActive(true);

            // Hiển thị nút nhạc của Game Lật Thẻ
            if (btnToggleMusicLatThe != null) btnToggleMusicLatThe.gameObject.SetActive(true);
            if (btnToggleMusicNemDa != null) btnToggleMusicNemDa.gameObject.SetActive(false);

            PlayBGM(bgmGameLatThe); // Mở nhạc nền Lật Thẻ
            KhoiTaoGameMoi();
        }
        else
        {
            Debug.Log("Game này đang phát triển...");
        }
    }

    public void ThoatVeMenu()
    {
        if (panelMiniGame != null) panelMiniGame.SetActive(false);
        if (panelGameNemDa != null) panelGameNemDa.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelGameWin != null) panelGameWin.SetActive(false);

        if (panelGameOverNemDa != null) panelGameOverNemDa.SetActive(false);
        if (panelGameWinNemDa != null) panelGameWinNemDa.SetActive(false);

        AnTatCaNutMusic();
        StopBGM(); // Tắt nhạc khi thoát khỏi game
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

        // Trộn bài
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
            PlaySFX(sfxLatTheDung); // SFX: Lật đúng (+Điểm)
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
            PlaySFX(sfxLatTheSai); // SFX: Lật sai
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
        // --- TÍNH NĂNG NHẤN PHÍM Z ĐỂ CHUYỂN ĐỔI CANVAS VÀ MENU MINI GAME ---
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.zKey.wasPressedThisFrame)
        {
            if (panelMenuMiniGame != null)
            {
                if (!panelMenuMiniGame.activeSelf)
                {
                    // Lần 1: Tắt Canvas chính -> Mở Menu Mini Game
                    if (canvasChinh != null) canvasChinh.SetActive(false);
                    MoMenuMiniGame();
                }
                else
                {
                    // Lần 2: Mở lại Canvas chính -> Đóng Menu Mini Game
                    DongMenuHoanToan();
                    if (canvasChinh != null) canvasChinh.SetActive(true);
                }
            }
        }
        // --------------------------------------------------

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
                            PlaySFX(sfxNemDaTrung); // SFX: Ném đá trúng bia (+Điểm)
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