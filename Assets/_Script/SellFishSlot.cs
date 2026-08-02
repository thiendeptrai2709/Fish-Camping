using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Gắn script này vào TỪNG ô có sẵn trong Content (cả 20 ô: Item_Template_BanCa, Item_Template (1)...(19))
// KHÔNG cần kéo tay field nào trong Inspector - script tự tìm Icon/Tên/Giá/Nút bên trong chính nó lúc Awake.
public class SellFishSlot : MonoBehaviour
{
    private Image imgIconCa;
    private TextMeshProUGUI txtTenCa;
    private TextMeshProUGUI txtGiaBan;
    private Button btnBan;

    private InventoryItemUI itemDangGiu; // Chính con cá này trong Balo, để biết xóa đúng con nào
    private int giaBanHienTai;

    private void Awake()
    {
        Transform tIcon = transform.Find("Item_Icon");
        if (tIcon != null) imgIconCa = tIcon.GetComponent<Image>();

        Transform tTen = transform.Find("Item_Name");
        if (tTen != null) txtTenCa = tTen.GetComponentInChildren<TextMeshProUGUI>(true);

        Transform tGia = transform.Find("Item_Price/Text (TMP)");
        if (tGia != null) txtGiaBan = tGia.GetComponent<TextMeshProUGUI>();

        // Ô nào cũng chỉ có đúng 1 nút (dù đang tên Btn_Sell hay Btn_Buy) nên tìm theo component Button là chắc ăn nhất
        btnBan = GetComponentInChildren<Button>(true);
    }

    // Được ShopTabManager gọi để nhồi 1 con cá cụ thể vào ô này
    public void Setup(InventoryItemUI item, int giaCoBan)
    {
        itemDangGiu = item;
        giaBanHienTai = giaCoBan;

        FishSO fishData = item.GetItemShape() as FishSO;
        if (fishData == null) return;

        if (imgIconCa != null) imgIconCa.sprite = fishData.itemIcon;
        if (txtTenCa != null) txtTenCa.text = fishData.itemName;
        if (txtGiaBan != null) txtGiaBan.text = giaBanHienTai + "K";

        if (btnBan != null)
        {
            // Ép nhãn nút luôn hiện "Bán", phòng trường hợp ô đó chưa đổi từ "Mua" của template cũ
            TextMeshProUGUI nhanNut = btnBan.GetComponentInChildren<TextMeshProUGUI>();
            if (nhanNut != null) nhanNut.text = "Bán";

            btnBan.onClick.RemoveAllListeners();
            btnBan.onClick.AddListener(OnBamNutBan);
        }
    }

    private void OnBamNutBan()
    {
        if (itemDangGiu == null) return;

        // 1. Cộng tiền cho người chơi
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.BanVatPham(giaBanHienTai);
        }

        // 2. Xóa đúng con cá này khỏi Balo
        if (BackpackMinigameUI.Instance != null)
        {
            BackpackMinigameUI.Instance.RemoveItem(itemDangGiu);
        }

        itemDangGiu = null;

        // 3. Vì ô này là ô CỐ ĐỊNH (không Destroy được), nhờ ShopTabManager quét lại
        //    để dồn cá lên các ô trống và ẩn bớt ô dư
        if (ShopTabManager.Instance != null)
        {
            ShopTabManager.Instance.LamMoiDanhSachCa();
        }
    }
}