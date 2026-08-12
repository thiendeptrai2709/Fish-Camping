using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GarageZone : MonoBehaviour
{
    public static GarageZone Instance { get; private set; }

    [Header("=== GIAO DIỆN CHUNG ===")]
    public GameObject garageUIPanel;
    public Button btnDongGarage;

    [Header("=== ẨN UI KHÁC KHI ĐANG MỞ GARAGE ===")]
    public GameObject[] cacUIAnKhiMoGarage;

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
    public Button[] trunkButtons = new Button[3];

    [Header("=== THÔNG BÁO ===")]
    public TMP_Text notificationText;

    private System.Action _onGarageClosed;

    private void Awake()
    {
        // [CODE MẠNH] 1: KIỂM TRA BẢN SAO
        // Nếu đã có 1 thằng GarageZone từ Map trước sống sót chạy sang đây, 
        // thì lập tức TIÊU DIỆT thằng mới vừa được sinh ra để bảo vệ dữ liệu cũ!
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // [CODE MẠNH] 2: PHONG VƯƠNG VÀ BAN LỆNH BẤT TỬ
        Instance = this;

        // Tự động bứt rễ ra khỏi Map hiện tại để không bị chết khi đổi Scene
        transform.SetParent(null);

        // Gắn mác Bất tử (Sẽ tự động chui vào vùng DontDestroyOnLoad)
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (garageUIPanel != null) garageUIPanel.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);

        currentTrunkLevel = PlayerPrefs.GetInt("SavedTrunkLevel", 0);

        if (btnDongGarage != null) btnDongGarage.onClick.AddListener(CloseGarageUI);

        LoadTireUI();
        UpdateAllUI();

        // Khôi phục lốp xe
        int equippedTireIndex = PlayerPrefs.GetInt("EquippedTireIndex", -1);
        if (equippedTireIndex >= 0 && wheelPrefabs != null && equippedTireIndex < wheelPrefabs.Length)
        {
            ApplyTireVisual(equippedTireIndex);
        }
    }

    void Update()
    {
        // Bấm phím Z để đóng bảng Garage khi đang mở, tránh trùng lặp với E (NPC) và ESC (Setting)
        if (garageUIPanel != null && garageUIPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                CloseGarageUI();
            }
        }
    }

    void LoadTireUI()
    {
        foreach (Transform child in tireContentParent) Destroy(child.gameObject);

        for (int i = 0; i < allTires.Length; i++)
        {
            GameObject newObj = Instantiate(tireUIPrefab, tireContentParent);
            TireUIItem uiItem = newObj.GetComponent<TireUIItem>();
            if (uiItem != null) uiItem.SetupUI(allTires[i], i, this);
        }
    }

    // ==========================================
    // NPC SẼ GỌI HÀM NÀY ĐỂ MỞ GARAGE
    // ==========================================
    public void OpenGarage(System.Action onCloseCallback = null)
    {
        _onGarageClosed = onCloseCallback;

        if (garageUIPanel != null)
        {
            garageUIPanel.SetActive(true);
            garageUIPanel.transform.SetAsLastSibling(); // Ép nổi lên trên cùng (Sửa lỗi tàng hình)
        }

        AnHienCacUIKhac(false);
        UpdateAllUI();

        // Mở khóa chuột bằng lệnh hệ thống (Chuẩn như Shop)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseGarageUI()
    {
        if (garageUIPanel != null) garageUIPanel.SetActive(false);
        AnHienCacUIKhac(true);

        // Khóa chuột lại
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _onGarageClosed?.Invoke();
        _onGarageClosed = null;
    }

    private void AnHienCacUIKhac(bool hienRa)
    {
        if (cacUIAnKhiMoGarage == null) return;
        foreach (GameObject obj in cacUIAnKhiMoGarage)
        {
            if (obj != null) obj.SetActive(hienRa);
        }
    }

    // ==========================================
    // CÁC HÀM XỬ LÝ LỐP XE VÀ CỐP (GIỮ NGUYÊN)
    // ==========================================
    public Vector3 wheelRotationOffset = new Vector3(0, 0, 90);
    public float wheelScaleMultiplier = 2f;

    public void ChangeWheel(int wheelIndex, int tirePrice)
    {
        if (wheelIndex < 0 || wheelIndex >= wheelPrefabs.Length) return;
        if (MoneyManager.Instance == null || !MoneyManager.Instance.CoDuTien(tirePrice))
        {
            ShowNotify("Không đủ tiền mua lốp!");
            return;
        }

        MoneyManager.Instance.TruTien(tirePrice);
        PlayerPrefs.SetInt("EquippedTireIndex", wheelIndex);
        PlayerPrefs.Save();

        UpdateAllUI();
        ApplyTireVisual(wheelIndex);

        VehicleStats stats = Object.FindFirstObjectByType<VehicleStats>(FindObjectsInactive.Include);
        if (stats != null)
        {
            stats.UpdateTireStats();
        }

        ShowNotify($"Đã trang bị lốp mới!");
    }

    private void ApplyTireVisual(int wheelIndex)
    {
        if (wheelPrefabs == null || wheelIndex < 0 || wheelIndex >= wheelPrefabs.Length) return;
        Transform[] roots = new Transform[] { wheelFL, wheelFR, wheelRL, wheelRR };
        for (int i = 0; i < roots.Length; i++)
        {
            Transform root = roots[i];
            if (root == null) continue;
            foreach (Transform child in root) Destroy(child.gameObject);

            GameObject newWheel = Instantiate(wheelPrefabs[wheelIndex], root);
            newWheel.transform.localPosition = Vector3.zero;

            Vector3 finalRotation = wheelRotationOffset;
            if (i == 1 || i == 3) finalRotation.y += 180f;

            newWheel.transform.localEulerAngles = finalRotation;
            newWheel.transform.localScale = wheelPrefabs[wheelIndex].transform.localScale * wheelScaleMultiplier;
        }
    }

    public void BuyTrunkLevel(int targetLevel)
    {
        if (targetLevel > currentTrunkLevel + 1)
        {
            ShowNotify("Hãy nâng cấp Level trước đó!");
            return;
        }
        int cost = trunkUpgradeCosts[targetLevel - 1];
        if (MoneyManager.Instance != null && MoneyManager.Instance.TruTien(cost))
        {
            currentTrunkLevel = targetLevel;
            PlayerPrefs.SetInt("SavedTrunkLevel", currentTrunkLevel);
            PlayerPrefs.Save();
            if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.RefreshGridVisuals();
            UpdateAllUI();
            ShowNotify($"Nâng cấp Cốp Level {targetLevel} thành công!");
        }
        else ShowNotify("Không đủ tiền nâng cấp!");
    }

    public void UpdateAllUI()
    {
        for (int i = 0; i < trunkButtons.Length; i++)
        {
            if (trunkButtons[i] == null) continue;
            int buttonLevel = i + 1;
            if (buttonLevel <= currentTrunkLevel) trunkButtons[i].interactable = false;
            else if (buttonLevel == currentTrunkLevel + 1) trunkButtons[i].interactable = true;
            else trunkButtons[i].interactable = false;
        }
    }

    public void ShowNotify(string message)
    {
        if (notificationText == null) return;
        StopAllCoroutines();
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