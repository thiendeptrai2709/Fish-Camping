using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GarageZone : MonoBehaviour
{
    [Header("=== GIAO DIỆN CHUNG ===")]
    public GameObject pressBPrompt;      // UI thông báo "Bấm B để vào Gara"
    public GameObject garageUIPanel;     // Bảng UI Gara
    public TMP_Text playerMoneyText;     // Text hiển thị tiền người chơi
    public int playerMoney = 5000;       // Tiền hiện có (Demo)

    [Header("=== HỆ THỐNG THAY LỐP XE ===")]
    public Transform wheelFL;            // Vị trí Trước-Trái (Tire_FL_Root)
    public Transform wheelFR;            // Vị trí Trước-Phải (Tire_FR_Root)
    public Transform wheelRL;            // Vị trí Sau-Trái (Tire_BL_Root)
    public Transform wheelRR;            // Vị trí Sau-Phải (Tire_BR_Root)
    public GameObject[] wheelPrefabs;    // Danh sách Prefab Lốp (wheel_01 -> wheel_12)
    public int wheelChangeCost = 500;    // Giá mỗi lần thay lốp

    [Header("=== HỆ THỐNG NÂNG CẤP CỐP XE ===")]
    public int currentTrunkLevel = 1;
    public int maxTrunkLevel = 5;
    // Sức chứa tương ứng từ Level 1 đến Level 5
    public int[] trunkCapacities = new int[5] { 10, 20, 35, 50, 70 };
    // Giá tiền nâng cấp cho từng Level (Lvl 1->2, 2->3, 3->4, 4->5)
    public int[] trunkUpgradeCosts = new int[4] { 200, 500, 1000, 2000 };

    [Header("=== UI CỐP XE ===")]
    public TMP_Text trunkLevelText;            // Hiển thị: "Cốp Level: 1/5"
    public TMP_Text trunkCapacityText;         // Hiển thị: "Sức chứa: 10 ô"
    public TMP_Text trunkCostText;             // Hiển thị: "Giá: $200"
    public Button upgradeTrunkBtn;             // Nút nâng cấp trên UI

    private bool isInsideGara = false;
    private bool isUIOpen = false;

    void Start()
    {
        if (pressBPrompt != null) pressBPrompt.SetActive(false);
        if (garageUIPanel != null) garageUIPanel.SetActive(false);

        // Load lại Level cốp xe đã lưu (mặc định là 1 nếu chưa mua gì)
        currentTrunkLevel = PlayerPrefs.GetInt("SavedTrunkLevel", 1);

        UpdateAllUI();
    }

    void Update()
    {
        if (isInsideGara && Input.GetKeyDown(KeyCode.B))
        {
            ToggleGarageUI();
        }
    }

    // --- XỬ LÝ VÙNG CẢM ỨNG (TRIGGER) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Car") || other.GetComponentInParent<Rigidbody>() != null)
        {
            isInsideGara = true;
            if (pressBPrompt != null && !isUIOpen) pressBPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Car"))
        {
            isInsideGara = false;
            if (pressBPrompt != null) pressBPrompt.SetActive(false);
            if (isUIOpen) CloseGarageUI();
        }
    }

    // --- BẬT / TẮT GIAO DIỆN ---
    void ToggleGarageUI()
    {
        isUIOpen = !isUIOpen;
        if (garageUIPanel != null) garageUIPanel.SetActive(isUIOpen);
        if (pressBPrompt != null) pressBPrompt.SetActive(!isUIOpen);
        if (isUIOpen) UpdateAllUI();
    }

    public void CloseGarageUI()
    {
        isUIOpen = false;
        if (garageUIPanel != null) garageUIPanel.SetActive(false);
        if (pressBPrompt != null && isInsideGara) pressBPrompt.SetActive(true);
    }

    // --- CHỨC NĂNG 1: THAY LỐP XE ---
    public void ChangeWheel(int wheelIndex)
    {
        if (wheelIndex < 0 || wheelIndex >= wheelPrefabs.Length) return;

        if (playerMoney < wheelChangeCost)
        {
            Debug.Log("Không đủ tiền thay lốp!");
            return;
        }

        playerMoney -= wheelChangeCost;
        UpdateAllUI();

        Transform[] roots = new Transform[] { wheelFL, wheelFR, wheelRL, wheelRR };

        foreach (Transform root in roots)
        {
            if (root == null) continue;

            // Xóa toàn bộ lốp cũ đang nằm trong Root này
            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }

            // Spawn lốp mới vào đúng Root
            GameObject newWheel = Instantiate(wheelPrefabs[wheelIndex], root);

            // Đặt lại tọa độ cho chuẩn tâm bánh xe
            newWheel.transform.localPosition = Vector3.zero;
            newWheel.transform.localRotation = Quaternion.identity;
            newWheel.transform.localScale = Vector3.one;
        }

        Debug.Log("Đã thay lốp thành công!");
    }

    // --- CHỨC NĂNG 2: NÂNG CẤP CỐP XE ---
    public void UpgradeTrunk()
    {
        if (currentTrunkLevel >= maxTrunkLevel)
        {
            Debug.Log("Cốp xe đã đạt cấp tối đa (Level 5)!");
            return;
        }

        int cost = trunkUpgradeCosts[currentTrunkLevel - 1];

        if (playerMoney >= cost)
        {
            playerMoney -= cost;
            currentTrunkLevel++;

            // Lưu lại Level mới vào máy tính
            PlayerPrefs.SetInt("SavedTrunkLevel", currentTrunkLevel);
            PlayerPrefs.Save();

            // Lấy sức chứa mới
            int newCapacity = trunkCapacities[currentTrunkLevel - 1];

            // GHI CHÚ: Truyền newCapacity này sang script Inventory của bồ ở dòng dưới
            // Ví dụ: FindObjectOfType<CarInventory>().maxSlots = newCapacity;

            UpdateAllUI();
            Debug.Log($"Nâng cấp Cốp thành công! Level: {currentTrunkLevel}, Sức chứa mới: {newCapacity} ô");
        }
        else
        {
            Debug.Log("Không đủ tiền nâng cấp Cốp!");
        }
    }

    // --- CẬP NHẬT TẤT CẢ GIAO DIỆN ---
    void UpdateAllUI()
    {
        if (playerMoneyText != null)
            playerMoneyText.text = $"Tiền: ${playerMoney}";

        // Cập nhật thông số Cốp xe
        if (trunkLevelText != null)
            trunkLevelText.text = $"Cốp Level: {currentTrunkLevel}/{maxTrunkLevel}";

        if (trunkCapacityText != null)
            trunkCapacityText.text = $"Sức chứa: {trunkCapacities[currentTrunkLevel - 1]} ô";

        if (currentTrunkLevel >= maxTrunkLevel)
        {
            if (trunkCostText != null) trunkCostText.text = "MAX LEVEL";
            if (upgradeTrunkBtn != null) upgradeTrunkBtn.interactable = false;
        }
        else
        {
            int nextCost = trunkUpgradeCosts[currentTrunkLevel - 1];
            if (trunkCostText != null) trunkCostText.text = $"Giá: ${nextCost}";
            if (upgradeTrunkBtn != null) upgradeTrunkBtn.interactable = true;
        }
    }
}