using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GarageZone : MonoBehaviour
{
    [Header("=== GIAO DIỆN CHUNG ===")]
    public GameObject pressBPrompt;
    public GameObject garageUIPanel;
    public TMP_Text playerMoneyText;
    public int playerMoney = 5000;

    // Biến dùng cho hiệu ứng nhảy số tiền
    private float displayedMoney;
    private Coroutine moneyAnimCoroutine;

    [Header("=== HỆ THỐNG THAY LỐP XE (3D Model) ===")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelRL;
    public Transform wheelRR;
    public GameObject[] wheelPrefabs;

    [Header("=== HỆ THỐNG UI TỰ ĐỘNG (LỐP XE) ===")]
    public TireData[] allTires;
    public GameObject tireUIPrefab;
    public Transform tireContentParent;

    [Header("=== HỆ THỐNG NÂNG CẤP CỐP XE ===")]
    public int currentTrunkLevel = 0;
    public int[] trunkCapacities = new int[3] { 10, 20, 35 };
    public int[] trunkUpgradeCosts = new int[3] { 200, 500, 1000 };

    [Header("=== UI CỐP XE (3 NÚT) ===")]
    public Button[] trunkButtons = new Button[3];

    [Header("=== THÔNG BÁO (NOTIFICATION) ===")]
    public TMP_Text notificationText;

    [Header("=== CHUỘT ===")]
    public PlayerCursor playerCursor;

    private bool isInsideGara = false;
    private bool isUIOpen = false;

    void Start()
    {
        if (pressBPrompt != null) pressBPrompt.SetActive(false);
        if (garageUIPanel != null) garageUIPanel.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);

        currentTrunkLevel = PlayerPrefs.GetInt("SavedTrunkLevel", 0);

        // Đặt số tiền hiển thị ban đầu bằng với tiền thực tế
        displayedMoney = playerMoney;
        SetMoneyText(displayedMoney);

        LoadTireUI();
        UpdateAllUI();
    }

    void Update()
    {
        if (isInsideGara && Input.GetKeyDown(KeyCode.B))
        {
            ToggleGarageUI();
        }
    }

    void LoadTireUI()
    {
        foreach (Transform child in tireContentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < allTires.Length; i++)
        {
            GameObject newObj = Instantiate(tireUIPrefab, tireContentParent);
            TireUIItem uiItem = newObj.GetComponent<TireUIItem>();
            if (uiItem != null) uiItem.SetupUI(allTires[i], i, this);
        }
    }

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

    void ToggleGarageUI()
    {
        isUIOpen = !isUIOpen;
        if (garageUIPanel != null) garageUIPanel.SetActive(isUIOpen);
        if (pressBPrompt != null) pressBPrompt.SetActive(!isUIOpen);
        if (isUIOpen) UpdateAllUI();

        // Nếu UI mở thì set false (mở khóa chuột), nếu UI đóng thì set true (khóa chuột)
        if (playerCursor != null) playerCursor.SetCursorState(!isUIOpen);
    }

    public void CloseGarageUI()
    {
        isUIOpen = false;
        if (garageUIPanel != null) garageUIPanel.SetActive(false);
        if (pressBPrompt != null && isInsideGara) pressBPrompt.SetActive(true);

        // Khóa chuột lại khi thoát UI
        if (playerCursor != null) playerCursor.SetCursorState(true);
    }

    [Header("=== CĂN CHỈNH BÁNH XE (CHỐNG LỖI 3D) ===")]
    public Vector3 wheelRotationOffset = new Vector3(0, 0, 90); // Trục bẻ lái (Thử 90 hoặc -90 vào X, Y hoặc Z)
    public float wheelScaleMultiplier = 2f; // Độ phóng to bánh xe (Thử 2, 5, 10...)

    public void ChangeWheel(int wheelIndex, int tirePrice)
    {
        if (wheelIndex < 0 || wheelIndex >= wheelPrefabs.Length) return;
        if (playerMoney < tirePrice)
        {
            ShowNotify("Không đủ tiền mua lốp!");
            return;
        }

        playerMoney -= tirePrice;
        PlayerPrefs.SetInt("EquippedTireIndex", wheelIndex);
        PlayerPrefs.Save();
        UpdateAllUI();

        Transform[] roots = new Transform[] { wheelFL, wheelFR, wheelRL, wheelRR };

        // Đã thay đổi thành vòng lặp FOR để phân biệt bánh Trái - Phải
        for (int i = 0; i < roots.Length; i++)
        {
            Transform root = roots[i];
            if (root == null) continue;
            foreach (Transform child in root) Destroy(child.gameObject);

            // Sinh bánh xe mới
            GameObject newWheel = Instantiate(wheelPrefabs[wheelIndex], root);
            newWheel.transform.localPosition = Vector3.zero;

            // Xử lý góc xoay và lật bánh xe bên phải
            Vector3 finalRotation = wheelRotationOffset;

            // KIỂM TRA BÁNH BÊN PHẢI (Vị trí số 1 là FR, số 3 là RR)
            if (i == 1 || i == 3)
            {
                // Xoay thêm 180 độ để lật mặt bánh ra ngoài
                finalRotation.y += 180f;
            }

            newWheel.transform.localEulerAngles = finalRotation;
            newWheel.transform.localScale = wheelPrefabs[wheelIndex].transform.localScale * wheelScaleMultiplier;
        }

        ShowNotify($"Đã trang bị lốp mới!");
    }

    public void BuyTrunkLevel(int targetLevel)
    {
        if (targetLevel > currentTrunkLevel + 1)
        {
            ShowNotify("Hãy nâng cấp Level trước đó!");
            return;
        }

        int cost = trunkUpgradeCosts[targetLevel - 1];
        if (playerMoney >= cost)
        {
            playerMoney -= cost;
            currentTrunkLevel = targetLevel;

            PlayerPrefs.SetInt("SavedTrunkLevel", currentTrunkLevel);
            PlayerPrefs.Save();

            UpdateAllUI();
            ShowNotify($"Nâng cấp Cốp Level {targetLevel} thành công!");
        }
        else
        {
            ShowNotify("Không đủ tiền nâng cấp!");
        }
    }

    public void UpdateAllUI()
    {
        // Kích hoạt hiệu ứng nhảy số tiền
        if (moneyAnimCoroutine != null) StopCoroutine(moneyAnimCoroutine);
        moneyAnimCoroutine = StartCoroutine(CountMoneyRoutine());

        // Logic tự động Khóa/Mở 3 nút cốp xe
        for (int i = 0; i < trunkButtons.Length; i++)
        {
            if (trunkButtons[i] == null) continue;

            int buttonLevel = i + 1;

            if (buttonLevel <= currentTrunkLevel)
            {
                trunkButtons[i].interactable = false;
            }
            else if (buttonLevel == currentTrunkLevel + 1)
            {
                trunkButtons[i].interactable = true;
            }
            else
            {
                trunkButtons[i].interactable = false;
            }
        }
    }

    // ==========================================
    // HỆ THỐNG HIỂN THỊ VÀ NHẢY SỐ TIỀN MƯỢT MÀ
    // ==========================================
    private IEnumerator CountMoneyRoutine()
    {
        float duration = 0.5f; // Thời gian chạy hiệu ứng (0.5 giây)
        float startAmount = displayedMoney;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Cho số tiền hiển thị chạy từ từ tới số tiền thật
            displayedMoney = Mathf.Lerp(startAmount, playerMoney, elapsed / duration);
            SetMoneyText(displayedMoney);
            yield return null;
        }

        displayedMoney = playerMoney;
        SetMoneyText(displayedMoney);
    }

    private void SetMoneyText(float amount)
    {
        if (playerMoneyText == null) return;

        if (amount >= 1000)
        {
            // Chuyển format thành K (VD: 3750 -> 3.8K, 5000 -> 5K)
            playerMoneyText.text = (amount / 1000f).ToString("0.#") + "K";
        }
        else
        {
            // Dưới 1000 thì hiện nguyên số (VD: 850)
            playerMoneyText.text = Mathf.RoundToInt(amount).ToString();
        }
    }

    // ==========================================
    // HỆ THỐNG XỬ LÝ THÔNG BÁO (HIỆN LÊN RỒI MỜ ĐI)
    // ==========================================
    public void ShowNotify(string message)
    {
        if (notificationText == null) return;
        StopAllCoroutines();
        // Đảm bảo Coroutine nhảy tiền vẫn tiếp tục chạy nếu bị ngắt
        if (moneyAnimCoroutine != null) StopCoroutine(moneyAnimCoroutine);
        moneyAnimCoroutine = StartCoroutine(CountMoneyRoutine());

        StartCoroutine(FadeOutNotifyRoutine(message));
    }

    private IEnumerator FadeOutNotifyRoutine(string msg)
    {
        notificationText.text = msg;
        notificationText.gameObject.SetActive(true);

        Color c = notificationText.color;
        c.a = 1f;
        notificationText.color = c;

        yield return new WaitForSeconds(1.5f);

        float fadeDuration = 1f;
        float currentTime = 0f;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            notificationText.color = c;
            yield return null;
        }

        notificationText.gameObject.SetActive(false);
    }
}