using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerMiniGame : MonoBehaviour
{
    #region PHẦN 1: QUẢN LÝ MENU SẢNH
    [Header("---- QUẢN LÝ LUỒNG UI MENU ----")]
    public GameObject panelMenuMiniGame;
    public Image imgNenMinhHoa;
    public Sprite[] danhSachAnhNenMenu;
    public Button btnThamGia;

    private int idGameDangChon = 0;

    public void MoMenuMiniGame()
    {
        panelMenuMiniGame.SetActive(true);
        panelMiniGame.SetActive(false);
        panelGameOver.SetActive(false);
        ChonGameOMenu(0);
    }

    public void DongMenuHoanToan() => panelMenuMiniGame.SetActive(false);

    public void ChonGameOMenu(int idGame)
    {
        idGameDangChon = idGame;
        btnThamGia.interactable = true;
        if (danhSachAnhNenMenu.Length > idGame && danhSachAnhNenMenu[idGame] != null)
        {
            imgNenMinhHoa.sprite = danhSachAnhNenMenu[idGame];
        }
    }

    public void NhanThamGia()
    {
        panelMenuMiniGame.SetActive(false);
        if (idGameDangChon == 1) // ID game Lật thẻ là 1
        {
            panelMiniGame.SetActive(true);
            KhoiTaoGameMoi();
        }
        else
        {
            Debug.Log("Game này đang phát triển...");
        }
    }

    public void ThoatVeMenu()
    {
        panelMiniGame.SetActive(false);
        panelGameOver.SetActive(false);
        MoMenuMiniGame();
    }
    #endregion

    #region PHẦN 2: LOGIC GAME LẬT THẺ
    [Header("---- GIAO DIỆN & ĐỐI TƯỢNG (Lật Thẻ) ----")]
    public GameObject panelMiniGame;
    public Transform gridChuaThe;
    public GameObject mauThePrefab;
    public TextMeshProUGUI txtSoLanSai;
    public GameObject panelGameOver;

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
        panelGameOver.SetActive(false);
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

    private void CapNhatUITextSai() => txtSoLanSai.text = "Số lần sai còn lại: " + (soLanSaiToiDa - soLanSaiHienTai).ToString();

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
            theThuNhat.AnTheDi();
            theThuHai.AnTheDi();
            soCapDaTimThay++;
            if (soCapDaTimThay == (tongSoThe / 2)) Debug.Log("CHIẾN THẮNG!");
        }
        else
        {
            theThuNhat.LatUp();
            theThuHai.LatUp();
            soLanSaiHienTai++;
            CapNhatUITextSai();
            if (soLanSaiHienTai >= soLanSaiToiDa) panelGameOver.SetActive(true);
        }

        theThuNhat = null;
        theThuHai = null;
        if (soLanSaiHienTai < soLanSaiToiDa) choPhepClick = true;
    }
    #endregion
}