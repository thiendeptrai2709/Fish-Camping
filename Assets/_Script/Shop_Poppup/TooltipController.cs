using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipController : MonoBehaviour
{
    // Kéo cái khung nền thông tin (hoặc chữ Txt_MoTa) của CHÍNH Ô VẬT PHẨM ĐÓ vào đây
    public GameObject bangThongTin;

    void Start()
    {
        // Đảm bảo lúc đầu game bảng thông tin của món đồ này luôn ẩn
        if (bangThongTin != null)
        {
            bangThongTin.SetActive(false);
        }
    }

    // Khi di chuột vào nút "!" của ô này, chỉ hiện bảng của RIÊNG ô này
    public void OnPointerEnter()
    {
        if (bangThongTin != null)
        {
            bangThongTin.SetActive(true);
        }
    }

    // Khi di chuột ra ngoài, ẩn bảng đi
    public void OnPointerExit()
    {
        if (bangThongTin != null)
        {
            bangThongTin.SetActive(false);
        }
    }
}