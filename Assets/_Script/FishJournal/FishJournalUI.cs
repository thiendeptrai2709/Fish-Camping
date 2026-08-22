using UnityEngine;
using TMPro;

public class FishJournalUI : MonoBehaviour
{
    public static FishJournalUI Instance { get; private set; }

    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private GameObject journalPanel; // Panel chính của UI Sổ tay
    [SerializeField] private MonoBehaviour freeLookCamera; // Camera của Cinemachine

    [Header("=== GIAO DIỆN THÔNG BÁO TIẾN ĐỘ DƯỚI ĐÁY SỔ TAY ===")]
    [SerializeField] private GameObject dow;
    [SerializeField] private TextMeshProUGUI Text_dow;

    [Header("=== ICON DIARY TRÊN HUD ===")]
    [SerializeField] private UnityEngine.UI.Image diaryIcon;
    [SerializeField] private CanvasGroup diaryCanvasGroup;

    public bool IsOpen => isOpen;
    private bool isOpen = false;
    private Coroutine diaryFadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ẩn panel khi mới vào game
        if (journalPanel != null) journalPanel.SetActive(false);

        // Mặc định lúc bình thường: Icon Diary mờ đi 50%
        SetDiaryVisual(false, false);
    }

    private void Update()
    {
        // Đọc phím J từ PlayerInputHandler
        if (playerInput != null && playerInput.JournalTriggered)
        {
            ToggleJournal();
        }
    }

    public void ToggleJournal()
    {
        if (!isOpen)
        {
            if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanOpenJournal())
            {
                return; // Khóa mở Sổ tay cá khi chưa đến bước hướng dẫn
            }

            if (BackpackController.Instance != null && BackpackController.Instance.IsOpen)
                BackpackController.Instance.CloseBackpack();

            if (MapUIManager.Instance != null && MapUIManager.Instance.IsOpen)
                MapUIManager.Instance.CloseMap();

            if (BuildingUIManager.Instance != null && BuildingUIManager.Instance.IsOpen)
                BuildingUIManager.Instance.CloseBuildingUI();

            if (ShopManager.Instance != null && ShopManager.Instance.shopPanel != null && ShopManager.Instance.shopPanel.activeInHierarchy)
                ShopManager.Instance.DongShop();

            FishingController fc = Object.FindFirstObjectByType<FishingController>();
            if (fc != null && fc.IsBusyFishing())
            {
                fc.AutoStowRodToBackpack();
            }
        }

        isOpen = !isOpen;
        SetUIState(isOpen);

        // Vẽ lại danh sách cá và cập nhật thông báo tiến độ mỗi khi mở sổ
        if (isOpen)
        {
            FishJournalGridUI gridUI = GetComponent<FishJournalGridUI>();
            if (gridUI != null)
            {
                gridUI.RefreshGrid();
            }

            UpdateNotificationReminder();
        }
    }

    /// <summary>
    /// Cập nhật nội dung nhắc nhở mục tiêu mở Map vào Text_dow bên trong panel dow
    /// </summary>
    public void UpdateNotificationReminder()
    {
        // 1. Tự động tìm chính xác GameObject tên Text_dow trong journalPanel hoặc trong panel dow
        if (Text_dow == null && journalPanel != null)
        {
            Transform dowTrans = journalPanel.transform.Find("dow");
            if (dowTrans != null)
            {
                dow = dowTrans.gameObject;
                Transform textTrans = dowTrans.Find("Text_dow");
                if (textTrans != null)
                {
                    Text_dow = textTrans.GetComponent<TextMeshProUGUI>();
                }
            }

            if (Text_dow == null)
            {
                TextMeshProUGUI[] allTexts = journalPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in allTexts)
                {
                    if (t != null && t.gameObject.name == "Text_dow")
                    {
                        Text_dow = t;
                        if (dow == null && t.transform.parent != null)
                        {
                            dow = t.transform.parent.gameObject;
                        }
                        break;
                    }
                }
            }
        }

        if (Text_dow == null) return;

        if (dow != null) dow.SetActive(true);
        Text_dow.gameObject.SetActive(true);

        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        int lakeFishCount = FishJournalManager.Instance != null 
            ? FishJournalManager.Instance.GetUnlockedFishCountByMap("Hồ") 
            : 0;

        int swampFishCount = FishJournalManager.Instance != null 
            ? FishJournalManager.Instance.GetUnlockedFishCountByMap("Đầm") 
            : 0;

        int totalFishCount = FishJournalManager.Instance != null 
            ? FishJournalManager.Instance.GetTotalUnlockedFishCount() 
            : 0;

        int equippedTireIndex = PlayerPrefs.GetInt("EquippedTireIndex", -1);
        bool hasSwampTire = equippedTireIndex == 4 || equippedTireIndex == 5 || equippedTireIndex == 6 || equippedTireIndex == 7;
        bool hasSandTire = equippedTireIndex == 2 || equippedTireIndex == 6 || equippedTireIndex == 7;

        string message = "";

        // Giai đoạn 1: Ở Map 1 hoặc Map 2 -> Đang cần mở Map 3 (Đầm Lầy)
        if (currentScene.Contains("Map_1") || currentScene.Contains("Town") || 
            currentScene.Contains("Map_2") || currentScene.Contains("PineLake"))
        {
            if (lakeFishCount < 4)
            {
                int remain = 4 - lakeFishCount;
                message = isVietnamese
                    ? $"<b>Mục tiêu mở Đầm Lầy (Map 3):</b> Đã câu <b>{lakeFishCount}/4</b> loài cá Hồ Pine Lake (Cần thêm <b>{remain}</b> loài nữa)."
                    : $"<b>Goal for Swamp (Map 3):</b> Recorded <b>{lakeFishCount}/4</b> Pine Lake fish (Need <b>{remain}</b> more species).";
            }
            else if (!hasSwampTire)
            {
                message = isVietnamese
                    ? "<b>Đã câu đủ 4/4 loài cá Hồ!</b> Hãy lái xe về <b>Garage (Thị Trấn)</b> mua <b>Lốp Bùn ($850)</b> để mở đường sang <b>Map 3 (Đầm Lầy)</b>."
                    : "<b>Lake fish collection complete (4/4)!</b> Visit <b>Garage (Town)</b> to buy <b>Mud Tires ($850)</b> to unlock <b>Map 3 (Swamp)</b>.";
            }
            else
            {
                message = isVietnamese
                    ? "<b>Đã đủ điều kiện mở Map 3 (Đầm Lầy)!</b> Bấm phím <b>[M]</b> để mở Bản đồ và bắt đầu chuyến đi."
                    : "<b>Map 3 (Swamp) is unlocked!</b> Press <b>[M]</b> to open Map and travel.";
            }
        }
        // Giai đoạn 2: Ở Map 3 (Đầm Lầy) -> Đang cần mở Map 4 (Bờ Biển)
        else if (currentScene.Contains("Map3") || currentScene.Contains("Swamp"))
        {
            if (swampFishCount < 3 && totalFishCount < 8)
            {
                int remain = 3 - swampFishCount;
                message = isVietnamese
                    ? $"<b>Mục tiêu mở Bờ Biển (Map 4):</b> Đã câu <b>{swampFishCount}/3</b> loài cá Đầm Lầy (Cần thêm <b>{remain}</b> loài nữa)."
                    : $"<b>Goal for Coast (Map 4):</b> Recorded <b>{swampFishCount}/3</b> Swamp fish (Need <b>{remain}</b> more species).";
            }
            else if (!hasSandTire)
            {
                message = isVietnamese
                    ? "<b>Đã câu đủ cá Đầm Lầy!</b> Hãy đến <b>Garage</b> trang bị <b>Lốp Bám Cát ($700)</b> để mở đường sang <b>Map 4 (Bờ Biển)</b>."
                    : "<b>Swamp fish goal met!</b> Equip <b>Sand Tires ($700)</b> at Garage to unlock <b>Map 4 (Coast)</b>.";
            }
            else
            {
                message = isVietnamese
                    ? "<b>Đã đủ điều kiện mở Map 4 (Bờ Biển)!</b> Bấm phím <b>[M]</b> để dịch chuyển sang Bờ Biển."
                    : "<b>Map 4 (Ocean Coast) is unlocked!</b> Press <b>[M]</b> to open Map and travel.";
            }
        }
        // Giai đoạn 3: Ở Map 4 (Bờ Biển)
        else
        {
            message = isVietnamese
                ? $"<b>Hải Trình Bờ Biển:</b> Tổng số loài cá đã ghi nhận trong Sổ tay: <b>{totalFishCount}</b> loài. Hãy tiếp tục săn lùng cá Huyền Thoại!"
                : $"<b>Ocean Coast:</b> Total recorded fish in journal: <b>{totalFishCount}</b> species. Continue hunting for Legendary fish!";
        }

        Text_dow.text = message;
    }

    private void SetUIState(bool openUI)
    {
        if (playerInput != null) playerInput.IsUIOpen = openUI;
        if (journalPanel != null) journalPanel.SetActive(openUI);

        // Đổi độ mờ của icon Diary: Mở sổ -> rõ 100%, Đóng sổ (bình thường) -> mờ 50%
        SetDiaryVisual(openUI, true);

        // Khóa di chuyển
        if (playerMovement != null) playerMovement.enabled = !openUI;

        // Ẩn tâm ngắm
        if (crosshairUI != null) crosshairUI.SetActive(!openUI);

        // Hiện chuột để click chọn cá
        if (playerCursor != null) playerCursor.SetCursorState(!openUI);

        // Khóa xoay Camera
        if (freeLookCamera != null)
        {
            MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
            if (cm3Input != null) cm3Input.enabled = !openUI;

            MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
            if (cm2Input != null) cm2Input.enabled = !openUI;
        }
    }

    /// <summary>
    /// Điều chỉnh độ mờ của Icon Diary trên HUD (1.0 khi mở sổ, 0.5 khi bình thường)
    /// </summary>
    private void SetDiaryVisual(bool isJournalOpen, bool animate = true)
    {
        if (diaryIcon == null && diaryCanvasGroup == null)
        {
            FindDiaryReference();
        }

        float targetAlpha = isJournalOpen ? 1.0f : 0.5f;

        if (!animate || !gameObject.activeInHierarchy)
        {
            ApplyDiaryAlpha(targetAlpha);
            return;
        }

        if (diaryFadeCoroutine != null) StopCoroutine(diaryFadeCoroutine);
        diaryFadeCoroutine = StartCoroutine(AnimateDiaryAlpha(targetAlpha));
    }

    private void FindDiaryReference()
    {
        // 1. Tìm trong root Canvas
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            Transform diaryTrans = rootCanvas.transform.Find("Diary");
            if (diaryTrans != null)
            {
                diaryIcon = diaryTrans.GetComponent<UnityEngine.UI.Image>();
                diaryCanvasGroup = diaryTrans.GetComponent<CanvasGroup>();
                return;
            }

            // Quét tìm trong toàn bộ Canvas
            var allImages = rootCanvas.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (var img in allImages)
            {
                if (img != null && img.gameObject.name.Equals("Diary", System.StringComparison.OrdinalIgnoreCase))
                {
                    diaryIcon = img;
                    diaryCanvasGroup = img.GetComponent<CanvasGroup>();
                    return;
                }
            }
        }

        // 2. Tìm toàn bộ scene nếu chưa thấy
        GameObject diaryObj = GameObject.Find("Diary");
        if (diaryObj != null)
        {
            diaryIcon = diaryObj.GetComponent<UnityEngine.UI.Image>();
            diaryCanvasGroup = diaryObj.GetComponent<CanvasGroup>();
        }
    }

    private void ApplyDiaryAlpha(float alpha)
    {
        if (diaryCanvasGroup != null)
        {
            diaryCanvasGroup.alpha = alpha;
        }
        else if (diaryIcon != null)
        {
            Color c = diaryIcon.color;
            c.a = alpha;
            diaryIcon.color = c;
        }
    }

    private System.Collections.IEnumerator AnimateDiaryAlpha(float targetAlpha)
    {
        float currentAlpha = 0.5f;
        if (diaryCanvasGroup != null) currentAlpha = diaryCanvasGroup.alpha;
        else if (diaryIcon != null) currentAlpha = diaryIcon.color.a;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float a = Mathf.Lerp(currentAlpha, targetAlpha, t);
            ApplyDiaryAlpha(a);
            yield return null;
        }

        ApplyDiaryAlpha(targetAlpha);
    }

    private void OnDisable()
    {
        if (isOpen)
        {
            isOpen = false;
            SetUIState(false);
        }
        else
        {
            SetDiaryVisual(false, false);
        }
    }
}