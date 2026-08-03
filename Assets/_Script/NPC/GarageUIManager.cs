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

    public void OpenGarage(System.Action onClose)
    {
        _onCloseCallback = onClose;

        try
        {
            if (garagePanel != null)
            {
                garagePanel.SetActive(true);
                garagePanel.transform.SetAsLastSibling();
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            SelectTireTab(); // Mặc định chọn Lốp
        }
        catch (System.Exception e)
        {
            // NẾU CÓ LỖI (quên kéo UI, thiếu data...), HỆ THỐNG SẼ TỰ ĐỘNG BÁO LỖI VÀ THẢ NPC RA NGAY LẬP TỨC!
            Debug.LogError($"[GarageUIManager] Lỗi khi mở UI: {e.Message}. Đã ép thả khóa NPC!");
            CloseGarage();
        }
    }

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

    // Đã thêm các lớp khiên bảo vệ (if != null) cho toàn bộ UI
    private void UpdateGarageUI(int currentLevel)
    {
        if (_selectedData == null) return;

        if (partNameText != null) partNameText.text = _selectedData.partName;

        UpgradeLevel currentData = GetUpgradeLevelData(_selectedData, currentLevel);
        int nextLevel = currentLevel + 1;
        bool hasNextLevel = nextLevel <= _selectedData.upgradeLevels.Length;

        if (hasNextLevel)
        {
            UpgradeLevel nextData = GetUpgradeLevelData(_selectedData, nextLevel);

            if (currentInfoText != null)
                currentInfoText.text = $"Hiện tại: {currentData.upgradeName}\n👉 Tiếp theo: {nextData.upgradeName}\n({nextData.description})";

            if (costText != null)
                costText.text = $"Chi phí: {nextData.cost}G";

            if (upgradeButton != null)
            {
                upgradeButton.interactable = true;
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => TryUpgradePart(nextLevel));
            }
        }
        else
        {
            if (currentInfoText != null) currentInfoText.text = $"Hiện tại: {currentData.upgradeName}\n🎉 Đã đạt cấp độ tối đa!";
            if (costText != null) costText.text = "MAX";
            if (upgradeButton != null) upgradeButton.interactable = false;
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