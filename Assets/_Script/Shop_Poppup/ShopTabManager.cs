using UnityEngine;

public class ShopTabManager : MonoBehaviour
{
    [Header("--- DANH SÁCH CÁC SCROLL VIEW ---")]
    public GameObject scrollViewCanCau;
    public GameObject scrollViewMoiCau;
    public GameObject scrollViewPhuTung;
    public GameObject scrollViewBanCa; // THÊM MỚI: Nơi chứa danh sách cá của Player

    void Start()
    {
        // Mặc định lúc mở shop chỉ hiện cần câu
        MoTabCanCau();
    }

    public void MoTabCanCau()
    {
        scrollViewCanCau.SetActive(true);
        scrollViewMoiCau.SetActive(false);
        scrollViewPhuTung.SetActive(false);
        if (scrollViewBanCa != null) scrollViewBanCa.SetActive(false);
    }

    public void MoTabMoiCau()
    {
        scrollViewCanCau.SetActive(false);
        scrollViewMoiCau.SetActive(true);
        scrollViewPhuTung.SetActive(false);
        if (scrollViewBanCa != null) scrollViewBanCa.SetActive(false);
    }

    public void MoTabPhuTung()
    {
        scrollViewCanCau.SetActive(false);
        scrollViewMoiCau.SetActive(false);
        scrollViewPhuTung.SetActive(true);
        if (scrollViewBanCa != null) scrollViewBanCa.SetActive(false);
    }

    // THÊM MỚI: Hàm mở Tab Bán Cá
    public void MoTabBanCa()
    {
        scrollViewCanCau.SetActive(false);
        scrollViewMoiCau.SetActive(false);
        scrollViewPhuTung.SetActive(false);
        if (scrollViewBanCa != null) scrollViewBanCa.SetActive(true);
    }
}