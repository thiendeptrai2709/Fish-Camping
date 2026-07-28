using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TireUIItem : MonoBehaviour
{
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtPrice;
    public TextMeshProUGUI txtDescription;
    public Image imgIcon;
    public Button btnBuy;

    private TireData myData;
    private int myIndex;
    private GarageZone myGarage;

    // Hàm này được GarageZone gọi lúc sinh ra ô lốp xe
    public void SetupUI(TireData data, int index, GarageZone garage)
    {
        myData = data;
        myIndex = index;
        myGarage = garage;

        txtName.text = data.tireName;
        txtPrice.text = data.price.ToString();
        txtDescription.text = data.description;
        imgIcon.sprite = data.tireIcon;

        // Gắn sự kiện click nút mua trực tiếp bằng code
        btnBuy.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        // Gửi số thứ tự lốp và giá tiền về cho GarageZone xử lý
        myGarage.ChangeWheel(myIndex, myData.price);
    }
}