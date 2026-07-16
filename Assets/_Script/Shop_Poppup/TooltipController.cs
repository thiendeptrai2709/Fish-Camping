using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // BẮT BUỘC THÊM DÒNG NÀY để code hiểu được TextMeshPro

public class TooltipController : MonoBehaviour
{
    [Header("--- KẾT NỐI GIAO DIỆN ---")]
    public GameObject bangThongTin; // Vẫn kéo cái khung chứa Text hoặc chính cái Text vào đây
    public TextMeshProUGUI txtMoTa; // Kéo Txt_MoTa vào đây để code điều khiển chữ

    [Header("--- NỘI DUNG HIỂN THỊ ---")]
    [TextArea(2, 5)] // Tạo một ô nhập liệu to rộng trong Inspector cho bồ dễ gõ
    public string noiDungTooltip; // Gõ chức năng của TỪNG món đồ vào đây!

    void Start()
    {
        // Đảm bảo lúc đầu game bảng thông tin của món đồ này luôn ẩn[cite: 1]
        if (bangThongTin != null)
        {
            bangThongTin.SetActive(false); //[cite: 1]
        }
    }

    // Khi di chuột vào nút "!" của ô này, chỉ hiện bảng của RIÊNG ô này[cite: 1]
    public void OnPointerEnter()
    {
        if (bangThongTin != null)
        {
            // BƯỚC NÂNG CẤP: Thay đổi dòng chữ mô tả TRƯỚC KHI hiện bảng lên
            if (txtMoTa != null)
            {
                txtMoTa.text = noiDungTooltip;
            }
            
            bangThongTin.SetActive(true); //[cite: 1]
        }
    }

    // Khi di chuột ra ngoài, ẩn bảng đi[cite: 1]
    public void OnPointerExit()
    {
        if (bangThongTin != null)
        {
            bangThongTin.SetActive(false); //[cite: 1]
        }
    }
}