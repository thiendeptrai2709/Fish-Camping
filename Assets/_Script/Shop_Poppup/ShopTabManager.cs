using UnityEngine;
using System.Collections.Generic;

public class ShopTabManager : MonoBehaviour
{
    public static ShopTabManager Instance;

    [Header("--- DANH SÁCH CÁC SCROLL VIEW ---")]
    public GameObject scrollViewCanCau;
    public GameObject scrollViewMoiCau;
    public GameObject scrollViewPhuTung;
    public GameObject scrollViewBanCa;

    [Header("--- HỆ THỐNG BÁN CÁ (20 Ô CÓ SẴN TRONG CONTENT) ---")]
    public Transform contentBanCa; // Kéo 'Content' (đã có sẵn 20 ô Item_Template... bên trong) vào đây

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        MoTabCanCau(); // Mặc định vừa vào shop là mở tab Cần Câu
    }

    public void MoTabCanCau()
    {
        if (scrollViewCanCau != null) scrollViewCanCau.SetActive(true);
        if (scrollViewMoiCau != null) scrollViewMoiCau.SetActive(false);
        if (scrollViewPhuTung != null) scrollViewPhuTung.SetActive(false);
        if (scrollViewBanCa != null) scrollViewBanCa.SetActive(false);
    }

    public void MoTabMoiCau()
    {
        if (scrollViewCanCau != null) scrollViewCanCau.SetActive(false);
        if (scrollViewMoiCau != null) scrollViewMoiCau.SetActive(true);
        if (scrollViewPhuTung != null) scrollViewPhuTung.SetActive(false);
        if (scrollViewBanCa != null) scrollViewBanCa.SetActive(false);
    }

    public void MoTabPhuTung()
    {
        if (scrollViewCanCau != null) scrollViewCanCau.SetActive(false);
        if (scrollViewMoiCau != null) scrollViewMoiCau.SetActive(false);
        if (scrollViewPhuTung != null) scrollViewPhuTung.SetActive(true);
        if (scrollViewBanCa != null) scrollViewBanCa.SetActive(false);
    }

    public void MoTabBanCa()
    {
        if (scrollViewCanCau != null) scrollViewCanCau.SetActive(false);
        if (scrollViewMoiCau != null) scrollViewMoiCau.SetActive(false);
        if (scrollViewPhuTung != null) scrollViewPhuTung.SetActive(false);
        if (scrollViewBanCa != null) scrollViewBanCa.SetActive(true);

        LamMoiDanhSachCa(); // Mỗi lần mở tab Bán Cá thì quét lại Balo cho mới nhất
    }

    // Quét Balo lấy hết cá, đổ lần lượt vào 20 ô có sẵn trong Content.
    // Ô nào không đủ cá để hiện thì tự ẩn đi.
    public void LamMoiDanhSachCa()
    {
        if (contentBanCa == null || BackpackMinigameUI.Instance == null) return;

        // 1. Lấy toàn bộ 20 ô có sẵn (kể cả ô đang bị ẩn từ lần quét trước)
        SellFishSlot[] cacO = contentBanCa.GetComponentsInChildren<SellFishSlot>(true);

        // 2. Lấy toàn bộ cá đang có trong Balo
        InventoryItemUI[] doTrongBalo = BackpackMinigameUI.Instance.GetAllItems();
        List<InventoryItemUI> danhSachCa = new List<InventoryItemUI>();
        foreach (InventoryItemUI item in doTrongBalo)
        {
            if (item.GetItemShape() is FishSO) danhSachCa.Add(item);
        }

        // --- DÒNG DEBUG TẠM: xem trong Console khi mở tab Bán Cá để biết chính xác đang bị kẹt ở đâu ---
        Debug.Log($"<color=cyan>[Shop Bán Cá] Tìm thấy {cacO.Length} ô SellFishSlot trong Content | Tìm thấy {danhSachCa.Count} con cá trong Balo.</color>");

        // 3. Đổ cá vào từng ô theo thứ tự, ô nào dư (hết cá) thì ẩn đi
        for (int i = 0; i < cacO.Length; i++)
        {
            if (i < danhSachCa.Count)
            {
                InventoryItemUI item = danhSachCa[i];
                FishSO fishData = item.GetItemShape() as FishSO;

                cacO[i].gameObject.SetActive(true);
                cacO[i].Setup(item, fishData.basePrice);
            }
            else
            {
                cacO[i].gameObject.SetActive(false);
            }
        }

        if (danhSachCa.Count > cacO.Length)
        {
            Debug.LogWarning($"[Shop Bán Cá] Balo có {danhSachCa.Count} con cá nhưng chỉ có {cacO.Length} ô để hiện, dư {danhSachCa.Count - cacO.Length} con chưa hiện được!");
        }
    }
}