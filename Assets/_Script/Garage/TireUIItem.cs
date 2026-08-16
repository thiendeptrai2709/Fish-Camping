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

    public void SetupUI(TireData data, int index, GarageZone garage)
    {
        myData = data;
        myIndex = index;
        myGarage = garage;

        txtName.text = data.tireName;
        txtPrice.text = data.price.ToString();
        txtDescription.text = data.description;
        imgIcon.sprite = data.tireIcon;

        // Xóa listener cũ trước khi add để tránh bị kích hoạt nhiều lần nếu tái sử dụng ô UI
        btnBuy.onClick.RemoveListener(OnBuyClicked);
        btnBuy.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        // Kích hoạt âm thanh click ngay khi bấm nút mua
        if (UIButtonSoundManager.Instance != null)
        {
            UIButtonSoundManager.Instance.PlayClickSound();
        }

        // Gửi số thứ tự lốp và giá tiền về cho GarageZone xử lý
        if (myGarage != null && myData != null)
        {
            myGarage.ChangeWheel(myIndex, myData.price);
        }
    }
}