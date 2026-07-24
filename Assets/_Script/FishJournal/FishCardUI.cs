using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishCardUI : MonoBehaviour
{
    [SerializeField] private Image fishIcon;
    [SerializeField] private TextMeshProUGUI fishNameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private GameObject lockedOverlay;

    public void Setup(FishSO fishData, FishRecord record)
    {
        if (record != null && record.isUnlocked)
        {
            lockedOverlay.SetActive(false);
            fishIcon.sprite = fishData.itemIcon;
            fishIcon.color = Color.white;
            fishNameText.text = fishData.itemName;
            statsText.text = $"L: {record.maxLength:F1}cm\nW: {record.maxWeight:F1}kg\nTotal: {record.totalCaught}";
        }
        else
        {
            lockedOverlay.SetActive(true);
            fishIcon.sprite = fishData.itemIcon;
            fishIcon.color = Color.black;
            fishNameText.text = "???";
            statsText.text = "";
        }
    }
}