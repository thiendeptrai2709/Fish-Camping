using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShopItemButton : MonoBehaviour
{
    [Header("Dữ liệu món hàng")]
    public int giaTien;
    public ItemShapeSO duLieuMonDo; // Nhận data gốc của bạn bồ

    private Button nutMua;

    private void Start()
    {
        nutMua = GetComponent<Button>();

        // Khi game chạy, nó sẽ tự động lắng nghe nút bấm và gọi hàm MuaVatPham với đủ 2 tham số
        nutMua.onClick.AddListener(GoiLenhMuaHang);
    }

    private void GoiLenhMuaHang()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.MuaVatPham(giaTien, duLieuMonDo);
        }
    }
}