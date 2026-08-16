using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GarageZone : MonoBehaviour
{
    public static GarageZone Instance { get; private set; }

    [Header("=== GIAO DIỆN CHUNG ===")]
    public GameObject garageUIPanel;
    public Button btnDongGarage;

    [Header("=== ẨN UI KHÁC KHI ĐANG MỞ GARAGE ===")]
    public GameObject[] cacUIAnKhiMoGarage;
    private List<GameObject> _danhSachUIDangBatTruocDo = new List<GameObject>();

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
    private Coroutine _notifyCoroutine;
    private Canvas _garageCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // Gắn Canvas riêng để kiểm soát thứ tự hiển thị độc lập
        if (garageUIPanel != null)
        {
            _garageCanvas = garageUIPanel.GetComponent<Canvas>();
            if (_garageCanvas == null)
            {
                _garageCanvas = garageUIPanel.AddComponent<Canvas>();
            }
            _garageCanvas.overrideSorting = true;
            _garageCanvas.sortingOrder = 850;

            if (garageUIPanel.GetComponent<GraphicRaycaster>() == null)
            {
                garageUIPanel.AddComponent<GraphicRaycaster>();
            }
        }
    }

    void Start()
    {
        if (garageUIPanel != null) garageUIPanel.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);

        currentTrunkLevel = PlayerPrefs.GetInt("SavedTrunkLevel", 0);

        if (btnDongGarage != null) 
        {
            btnDongGarage.onClick.RemoveAllListeners();
            btnDongGarage.onClick.AddListener(CloseGarageUI);
        }

        LoadTireUI();
        UpdateAllUI();

        int equippedTireIndex = PlayerPrefs.GetInt("EquippedTireIndex", -1);
        if (equippedTireIndex >= 0 && wheelPrefabs != null && equippedTireIndex < wheelPrefabs.Length)
        {
            ApplyTireVisual(equippedTireIndex);
        }
    }

    void Update()
    {
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
        if (tireContentParent == null || tireUIPrefab == null || allTires == null) return;

        foreach (Transform child in tireContentParent) Destroy(child.gameObject);

        for (int i = 0; i < allTires.Length; i++)
        {
            GameObject newObj = Instantiate(tireUIPrefab, tireContentParent);
            TireUIItem uiItem = newObj.GetComponent<TireUIItem>();
            if (uiItem != null) uiItem.SetupUI(allTires[i], i, this);
        }

        if (UIButtonSoundManager.Instance != null)
        {
            UIButtonSoundManager.Instance.RegisterAllButtonsInScene();
        }
    }

    public void OpenGarage(System.Action onCloseCallback = null)
    {
        _onGarageClosed = onCloseCallback;

        // 1. Ẩn các UI xung đột
        AnCacUIKhac();

        // 2. Kích hoạt Panel và đưa lên trên cùng (KHÔNG bóp méo hay đổi Anchor/Kích thước)
        if (garageUIPanel != null)
        {
            RectTransform rect = garageUIPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Chỉ reset trục Z về 0 để không bị lệch chiều sâu 3D, giữ nguyên tọa độ X, Y và Size
                Vector3 curPos = rect.localPosition;
                curPos.z = 0f;
                rect.localPosition = curPos;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }

            if (_garageCanvas != null)
            {
                _garageCanvas.overrideSorting = true;
                _garageCanvas.sortingOrder = 850;
            }

            garageUIPanel.SetActive(true);
            garageUIPanel.transform.SetAsLastSibling();
        }

        UpdateAllUI();

        if (UIButtonSoundManager.Instance != null)
        {
            UIButtonSoundManager.Instance.RegisterAllButtonsInScene();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseGarageUI()
    {
        if (garageUIPanel != null) garageUIPanel.SetActive(false);

        KhoiPhucCacUIKhac();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _onGarageClosed?.Invoke();
        _onGarageClosed = null;
    }

    private void AnCacUIKhac()
    {
        _danhSachUIDangBatTruocDo.Clear();
        if (cacUIAnKhiMoGarage == null) return;

        foreach (GameObject obj in cacUIAnKhiMoGarage)
        {
            if (obj != null && obj.activeSelf)
            {
                _danhSachUIDangBatTruocDo.Add(obj);
                obj.SetActive(false);
            }
        }
    }

    private void KhoiPhucCacUIKhac()
    {
        foreach (GameObject obj in _danhSachUIDangBatTruocDo)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
        _danhSachUIDangBatTruocDo.Clear();
    }

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

        ShowNotify("Đã trang bị lốp mới!");
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
        if (trunkButtons == null) return;
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
        if (_notifyCoroutine != null) StopCoroutine(_notifyCoroutine);
        _notifyCoroutine = StartCoroutine(FadeOutNotifyRoutine(message));
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