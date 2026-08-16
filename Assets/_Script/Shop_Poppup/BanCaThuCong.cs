using UnityEngine;
using UnityEngine.UI;

public class BanCaThuCong : MonoBehaviour
{
    [Header("Kéo file Data Cá của ô này vào đây")]
    public FishSO dataCaCanBan;

    [Header("Nút bấm bán (Tự động tìm nếu để trống)")]
    [SerializeField] private Button btnSell;

    private void Start()
    {
        // Tự động tìm Button 'Btn_Sell' trong các object con nếu chưa kéo vào Inspector
        if (btnSell == null)
        {
            btnSell = GetComponentInChildren<Button>();
        }

        if (btnSell != null)
        {
            btnSell.onClick.RemoveListener(ThucHienBanCa);
            btnSell.onClick.AddListener(ThucHienBanCa);
        }
    }

    public void ThucHienBanCa()
    {
        if (dataCaCanBan == null)
        {
            Debug.LogWarning("[Bán Cá] Ô này chưa được gán Data Cá!");
            return;
        }

        InventoryItemUI conCaCanXoa = null;

        // Quét tìm đúng con cá trong Balo
        if (BackpackMinigameUI.Instance != null)
        {
            InventoryItemUI[] doTrongBalo = BackpackMinigameUI.Instance.GetComponentsInChildren<InventoryItemUI>();
            foreach (var item in doTrongBalo)
            {
                if (item.GetItemShape() == dataCaCanBan)
                {
                    conCaCanXoa = item;
                    break;
                }
            }
        }

        if (conCaCanXoa != null)
        {
            int giaTien = dataCaCanBan.basePrice;

            // 1. Tắt ngay lập tức để không bị quét trúng ở frame hiện tại
            conCaCanXoa.gameObject.SetActive(false);

            // 2. Xóa khỏi logic Balo
            BackpackMinigameUI.Instance.RemoveItem(conCaCanXoa);

            // 3. Cộng tiền vào ví
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.BanVatPham(giaTien);
            }

            // 4. Phát âm thanh click nút nếu có
            if (UIButtonSoundManager.Instance != null)
            {
                UIButtonSoundManager.Instance.PlayClickSound();
            }

            Debug.Log($"<color=green>[Bán Cá] Đã bán 1 con {dataCaCanBan.itemName} thành công! +{giaTien} vàng</color>");
        }
        else
        {
            Debug.Log($"<color=red>[Bán Cá] Không còn con cá {dataCaCanBan.itemName} nào trong Balo để bán!</color>");
        }
    }
}