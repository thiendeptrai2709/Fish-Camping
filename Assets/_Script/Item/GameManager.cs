using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Giao Diện & Đối Tượng")]
    public GameObject panelMiniGame;
    public Transform gridChuaThe;
    public GameObject mauThePrefab;

    [Header("Hình Ảnh")]
    public Sprite hinhMatSau;
    public Sprite[] danhSachHinhMatTruoc;

    [Header("Cài Đặt Game")]
    [Tooltip("Tổng số thẻ trên màn hình. BẮT BUỘC PHẢI LÀ SỐ CHẴN!")]
    public int tongSoThe = 60; // Thêm biến này để lấp đầy màn hình

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
    }

    private void KhoiTaoGameMoi()
    {
        choPhepClick = false;
        soCapDaTimThay = 0;
        soLanSaiHienTai = 0;
        theThuNhat = null;
        theThuHai = null;

        foreach (Transform child in gridChuaThe)
        {
            Destroy(child.gameObject);
        }
        danhSachThe.Clear();

        // Đảm bảo tổng số thẻ luôn là số chẵn
        if (tongSoThe % 2 != 0) tongSoThe += 1;

        int soCapCanTao = tongSoThe / 2;
        List<int> danhSachID = new List<int>();

        // TẠO CÁC CẶP THẺ (Hình ảnh lặp lại thoải mái)
        for (int i = 0; i < soCapCanTao; i++)
        {
            // Chọn ngẫu nhiên 1 hình từ danh sách ảnh có sẵn
            int idHinhNgauNhien = Random.Range(0, danhSachHinhMatTruoc.Length);

            // Thêm 2 thẻ giống nhau (1 cặp)
            danhSachID.Add(idHinhNgauNhien);
            danhSachID.Add(idHinhNgauNhien);
        }

        // Xáo trộn vị trí thẻ
        for (int i = 0; i < danhSachID.Count; i++)
        {
            int temp = danhSachID[i];
            int r = Random.Range(i, danhSachID.Count);
            danhSachID[i] = danhSachID[r];
            danhSachID[r] = temp;
        }

        // Sinh thẻ ra màn hình
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

            // Sửa lại điều kiện thắng: dựa vào tổng số cặp
            if (soCapDaTimThay == (tongSoThe / 2))
            {
                Debug.Log("CHÚC MỪNG! BẠN ĐÃ CHIẾN THẮNG!");
            }
        }
        else
        {
            theThuNhat.LatUp();
            theThuHai.LatUp();
            soLanSaiHienTai++;

            if (soLanSaiHienTai >= soLanSaiToiDa)
            {
                Debug.Log("THẤT BẠI! BẠN ĐÃ SAI QUÁ SỐ LẦN!");
                TatGame();
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