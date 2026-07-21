using UnityEngine;
using UnityEngine.UI;

public class CardScript : MonoBehaviour
{
    [HideInInspector] public int id_the; // ID để so sánh 2 thẻ có giống nhau không

    public Image hinhAnhThe; // Hình ảnh của thẻ
    public Button nutBam;    // Component Button để chặn click

    private Sprite matTruoc;
    private Sprite matSau;
    private GameManager gameManager;
    private bool dangNgua = false;

    // Hàm này GameManager sẽ gọi khi sinh thẻ ra
    public void CaiDatThe(int id, Sprite truoc, Sprite sau, GameManager gm)
    {
        id_the = id;
        matTruoc = truoc;
        matSau = sau;
        gameManager = gm;

        // Mới vào game cho ngửa lên luôn
        LatNgua();
    }

    public void LatNgua()
    {
        hinhAnhThe.sprite = matTruoc;
        dangNgua = true;
    }

    public void LatUp()
    {
        hinhAnhThe.sprite = matSau;
        dangNgua = false;
    }

    // Hàm này để gán vào sự kiện Click chuột
    public void KhiBamVaoThe()
    {
        // Nếu thẻ đang úp và game cho phép click
        if (!dangNgua && gameManager.choPhepClick)
        {
            LatNgua();
            gameManager.XuLyChonThe(this); // Báo cho GameManager biết thẻ này vừa lật
        }
    }

    public void AnTheDi()
    {
        // Chuyển nút thành không thể tương tác và biến mất hình
        nutBam.interactable = false;
        hinhAnhThe.enabled = false;
    }
}