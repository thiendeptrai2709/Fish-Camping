using UnityEngine;

public class BanCaThuCong : MonoBehaviour
{
    [Header("Kéo file Data Cá của ô này vào đây")]
    public FishSO dataCaCanBan;

    public void ThucHienBanCa()
    {
        if (dataCaCanBan == null)
        {
            Debug.LogWarning("Chưa gắn data cá!");
            return;
        }

        InventoryItemUI conCaCanXoa = null;

        if (BackpackMinigameUI.Instance != null)
        {
            InventoryItemUI[] doTrongBalo = BackpackMinigameUI.Instance.GetComponentsInChildren<InventoryItemUI>(true);
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
            BackpackMinigameUI.Instance.RemoveItem(conCaCanXoa);

            // TODO: Bồ gọi script cộng tiền của bồ ở đây nhé

            Debug.Log("<color=green>Đã bán 1 con " + dataCaCanBan.itemName + "</color>");
        }
        else
        {
            Debug.Log("<color=red>Không có cá này trong balo!</color>");
        }
    }
}