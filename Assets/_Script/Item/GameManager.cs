using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ DÙNG TEXTMESHPRO

public class GameManager : MonoBehaviour
{
    [Header("Giao Diện & Đối Tượng")]
    public GameObject panelMiniGame;
    public Transform gridChuaThe;
    public GameObject mauThePrefab;
    public TextMeshProUGUI txtSoLanSai; // Biến chứa chữ hiển thị số lần sai
    public GameObject panelGameOver;    // Biến chứa màn hình Thua Cuộc

    [Header("Hình Ảnh")]
    public Sprite hinhMatSau;
    public Sprite[] danhSachHinhMatTruoc;

    [Header("Cài Đặt Game")]
    public int tongSoThe = 60;
    public float thoiGianGhiNho = 10f;
    public int soLanSaiToiDa = 10;

    [HideInInspector] public bool choPhepClick = false;

    private List<CardScript> danhSachThe = new List<CardScript>();
    private CardScript theThuNhat;
    private CardScript theThuHai;

    private int soCapDaTimThay = 0;
    private int soLanSaiHienTai = 0;

    public void MoGame()
    {
        panelMiniGame.SetActive(true);
        KhoiTaoGameMoi();
    }

    public void TatGame()
    {
        panelMiniGame.SetActive(false);
        panelGameOver.SetActive(false); // Ẩn luôn bảng thua nếu đang bật
    }

    // Hàm gọi khi bấm nút Chơi Lại
    public void ChoiLai()
    {
        KhoiTaoGameMoi();
    }

    private void KhoiTaoGameMoi()
    {
        // Ẩn bảng Game Over nếu nó đang hiện
        panelGameOver.SetActive(false);

        choPhepClick = false;
        soCapDaTimThay = 0;
        soLanSaiHienTai = 0;
        theThuNhat = null;
        theThuHai = null;

        // Cập nhật text số lần sai ngay từ đầu
        CapNhatUITextSai();

        foreach (Transform child in gridChuaThe)
        {
            Destroy(child.gameObject);
        }
        danhSachThe.Clear();

        if (tongSoThe % 2 != 0) tongSoThe += 1;

        int soCapCanTao = tongSoThe / 2;
        List<int> danhSachID = new List<int>();

        for (int i = 0; i < soCapCanTao; i++)
        {
            int idHinhNgauNhien = Random.Range(0, danhSachHinhMatTruoc.Length);
            danhSachID.Add(idHinhNgauNhien);
            danhSachID.Add(idHinhNgauNhien);
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
        // Hiển thị số lần còn lại = Số tối đa - Số lần đã sai
        int soLanConLai = soLanSaiToiDa - soLanSaiHienTai;
        txtSoLanSai.text = "Số lần sai còn lại: " + soLanConLai.ToString();
    }

    private IEnumerator ChoGhiNhoRoutine()
    {
        yield return new WaitForSeconds(thoiGianGhiNho);
        foreach (CardScript the in danhSachThe)
        {
            the.LatUp();
        }
        choPhepClick = true;
    }

    public void XuLyChonThe(CardScript theDuocChon)
    {
        if (theThuNhat == null)
        {
            theThuNhat = theDuocChon;
        }
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

            if (soCapDaTimThay == (tongSoThe / 2))
            {
                Debug.Log("CHÚC MỪNG! BẠN ĐÃ CHIẾN THẮNG!");
                // Nếu muốn làm bảng Thắng thì làm tương tự bảng Thua nhé
            }
        }
        else
        {
            theThuNhat.LatUp();
            theThuHai.LatUp();

            // CỘNG THÊM 1 LẦN SAI VÀ CẬP NHẬT CHỮ TRÊN MÀN HÌNH
            soLanSaiHienTai++;
            CapNhatUITextSai();

            if (soLanSaiHienTai >= soLanSaiToiDa)
            {
                Debug.Log("THẤT BẠI! BẠN ĐÃ SAI QUÁ SỐ LẦN!");
                // Hiện bảng Game Over thay vì tắt game ngay
                panelGameOver.SetActive(true);
            }
        }

        theThuNhat = null;
        theThuHai = null;
        if (soLanSaiHienTai < soLanSaiToiDa)
        {
            choPhepClick = true;
        }
    }
}