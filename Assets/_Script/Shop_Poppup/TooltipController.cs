using UnityEngine;

public class TooltipController : MonoBehaviour
{
    // Kéo cái Bang_ThongTin bồ vừa thiết kế vào đây
    public GameObject bangThongTin;

    void Start()
    {
        // Đảm bảo lúc đầu game bảng này luôn ẩn
        if (bangThongTin != null)
        {
            bangThongTin.SetActive(false);
        }
    }

    // Hàm này sẽ gọi khi DI CHUỘT VÀO nút "!"
    public void OnPointerEnter()
    {
        if (bangThongTin != null)
        {
            bangThongTin.SetActive(true); // Hiện bảng lên
        }
    }

    // Hàm này sẽ gọi khi DI CHUỘT RA KHỎI nút "!"
    public void OnPointerExit()
    {
        if (bangThongTin != null)
        {
            bangThongTin.SetActive(false); // Ẩn bảng đi
        }
    }
}