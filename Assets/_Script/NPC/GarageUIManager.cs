using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GarageUIManager : MonoBehaviour
{
    public static GarageUIManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject garagePanel;
    [SerializeField] private TextMeshProUGUI partNameText;
    [SerializeField] private TextMeshProUGUI currentInfoText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;

    [Header("Data Config")]
    [SerializeField] private VehicleUpgradeSO tireUpgradeData;

    // Giả lập lưu trữ cấp độ hiện tại của xe (MVP tạm thời)
    // Sau này bạn chuyển biến này sang GameManager hoặc VehicleSystem thực tế
    private int _currentTireLevel = 1;
    private System.Action _onCloseCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        garagePanel.SetActive(false);
    }

    public void OpenGarage(System.Action onClose)
    {
        garagePanel.SetActive(true);
        _onCloseCallback = onClose;
        UpdateGarageUI();
    }

    public void UpdateGarageUI()
    {
        partNameText.text = tireUpgradeData.partName;

        // Tìm thông tin của cấp độ hiện tại
        UpgradeLevel currentData = GetUpgradeLevelData(_currentTireLevel);

        // Tìm thông tin của cấp độ tiếp theo
        int nextLevel = _currentTireLevel + 1;
        bool hasNextLevel = nextLevel <= tireUpgradeData.upgradeLevels.Length;

        if (hasNextLevel)
        {
            UpgradeLevel nextData = GetUpgradeLevelData(nextLevel);
            currentInfoText.text = $"Hiện tại: {currentData.upgradeName}\n👉 Tiếp theo: {nextData.upgradeName}\n({nextData.description})";
            costText.text = $"Chi phí: {nextData.cost}G";

            upgradeButton.interactable = true;
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => TryUpgradeTire(nextData));
        }
        else
        {
            currentInfoText.text = $"Hiện tại: {currentData.upgradeName}\n🎉 Đã đạt cấp độ tối đa!";
            costText.text = "MAX";
            upgradeButton.interactable = false;
        }
    }

    private UpgradeLevel GetUpgradeLevelData(int level)
    {
        foreach (var data in tireUpgradeData.upgradeLevels)
        {
            if (data.level == level) return data;
        }
        return tireUpgradeData.upgradeLevels[0];
    }

    private void TryUpgradeTire(UpgradeLevel nextData)
    {
        // GIẢ LẬP CHECK TIỀN: Ở đây coi như người chơi luôn đủ tiền cho MVP
        // Sau này bạn kết hợp với hệ thống ví tiền của Player: if (PlayerWallet.Gold >= nextData.cost)

        _currentTireLevel = nextData.level;
        Debug.Log($"[Gara] Nâng cấp thành công lên: {nextData.upgradeName}!");

        // Phát tiếng động lanh canh sửa xe (ASMR) tại đây theo GDD

        UpdateGarageUI();
    }

    public void CloseGarage()
    {
        garagePanel.SetActive(false);
        _onCloseCallback?.Invoke();
    }
}