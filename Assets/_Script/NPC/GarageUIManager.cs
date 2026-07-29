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

    [Header("Upgrade Data Files")]
    [SerializeField] private VehicleUpgradeSO tireUpgradeData;
    [SerializeField] private VehicleUpgradeSO trunkUpgradeData;

    // Lưu trữ cấp độ giả lập cho MVP (Sau này chuyển qua VehicleSystem)
    private int _currentTireLevel = 1;
    private int _currentTrunkLevel = 1;

    private VehicleUpgradeSO _selectedData; // Dữ liệu của bộ phận đang được chọn xem
    private System.Action _onCloseCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        garagePanel.SetActive(false);
    }

    // Hàm mở Gara (Mặc định khi mở sẽ hiển thị tab Lốp xe trước)
    public void OpenGarage(System.Action onClose)
    {
        garagePanel.SetActive(true);
        _onCloseCallback = onClose;
        SelectTireTab(); // Mặc định chọn Lốp
    }

    // Nút bấm Tab Lốp xe gọi hàm này
    public void SelectTireTab()
    {
        _selectedData = tireUpgradeData;
        UpdateGarageUI(_currentTireLevel);
    }

    // Nút bấm Tab Cốp chứa gọi hàm này
    public void SelectTrunkTab()
    {
        _selectedData = trunkUpgradeData;
        UpdateGarageUI(_currentTrunkLevel);
    }

    private void UpdateGarageUI(int currentLevel)
    {
        if (_selectedData == null) return;

        partNameText.text = _selectedData.partName;

        UpgradeLevel currentData = GetUpgradeLevelData(_selectedData, currentLevel);
        int nextLevel = currentLevel + 1;
        bool hasNextLevel = nextLevel <= _selectedData.upgradeLevels.Length;

        if (hasNextLevel)
        {
            UpgradeLevel nextData = GetUpgradeLevelData(_selectedData, nextLevel);
            currentInfoText.text = $"Hiện tại: {currentData.upgradeName}\n👉 Tiếp theo: {nextData.upgradeName}\n({nextData.description})";
            costText.text = $"Chi phí: {nextData.cost}G";

            upgradeButton.interactable = true;
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => TryUpgradePart(nextLevel));
        }
        else
        {
            currentInfoText.text = $"Hiện tại: {currentData.upgradeName}\n🎉 Đã đạt cấp độ tối đa!";
            costText.text = "MAX";
            upgradeButton.interactable = false;
        }
    }

    private UpgradeLevel GetUpgradeLevelData(VehicleUpgradeSO data, int level)
    {
        foreach (var lvl in data.upgradeLevels)
        {
            if (lvl.level == level) return lvl;
        }
        return data.upgradeLevels[0];
    }

    public void TryUpgradePart(int nextLevel)
    {
        // Kiểm tra xem đang nâng cấp cho loại nào để lưu cấp độ vào loại đó
        if (_selectedData == tireUpgradeData)
        {
            _currentTireLevel = nextLevel;
            Debug.Log($"[Gara] Đã nâng cấp thành công Lốp xe lên cấp {nextLevel}!");
            UpdateGarageUI(_currentTireLevel);
        }
        else if (_selectedData == trunkUpgradeData)
        {
            _currentTrunkLevel = nextLevel;
            Debug.Log($"[Gara] Đã nâng cấp thành công Cốp xe lên cấp {nextLevel}!");
            UpdateGarageUI(_currentTrunkLevel);
        }
    }

    public void CloseGarage()
    {
        garagePanel.SetActive(false);
        _onCloseCallback?.Invoke();
    }
}