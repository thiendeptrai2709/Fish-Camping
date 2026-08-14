using UnityEngine;

public class BanCaThuCong : MonoBehaviour
{
    [Header("Kéo file Data Cá của ô này vào đây")]
    public FishSO dataCaCanBan; //[cite: 8]

    public void ThucHienBanCa()
    {
        // Ổ KHÓA CHỐNG SPAM: Nếu ô này bị gỡ data rồi thì không cho bấm nữa
        if (dataCaCanBan == null)
        {
            Debug.LogWarning("Chưa gắn data cá hoặc cá này vừa bị bán rồi!"); //[cite: 8]
            return;
        }

        InventoryItemUI conCaCanXoa = null; //[cite: 8]

        if (BackpackMinigameUI.Instance != null) //[cite: 8]
        {
            InventoryItemUI[] doTrongBalo = BackpackMinigameUI.Instance.GetComponentsInChildren<InventoryItemUI>(true); //[cite: 8]
            foreach (var item in doTrongBalo) //[cite: 8]
            {
                if (item.GetItemShape() == dataCaCanBan) //[cite: 8]
                {
                    conCaCanXoa = item; //[cite: 8]
                    break; //[cite: 8]
                }
            }
        }

        if (conCaCanXoa != null) //[cite: 8]
        {
            // 1. Xóa cá khỏi Balo
            BackpackMinigameUI.Instance.RemoveItem(conCaCanXoa); //[cite: 8]

            // 2. Gọi hàm cộng tiền bên ShopManager (Lấy giá tiền từ FishSO)
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.BanVatPham(dataCaCanBan.basePrice);
            }

            // 3. KHÓA LIỀN TAY: Chuyển data = null ngay lập tức để bồ có x10 click cũng vô dụng!
            dataCaCanBan = null;

            // 4. Ra lệnh cho Shop Bán Cá tự động giật load lại giao diện ngay tắp lự
            if (ShopTabManager.Instance != null)
            {
                ShopTabManager.Instance.LamMoiDanhSachCa();
            }

            Debug.Log("<color=green>Đã bán 1 con thành công!</color>"); //[cite: 8]
        }
        else
        {
            Debug.Log("<color=red>Không có cá này trong balo!</color>"); //[cite: 8]
        }
    }
}