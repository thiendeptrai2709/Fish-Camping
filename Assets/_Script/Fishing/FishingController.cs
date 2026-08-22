using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class FishingController : MonoBehaviour
{
    // Đã thêm trạng thái Failed để chống kẹt bug spam click
    private enum FishingState { Idle, WindingUp, WaitingForPower, Casting, Fishing, Catching, Failed }

    [SerializeField] private EquipmentSlotUI hotbarSlot;
    [SerializeField] private EquipmentSlotUI baitSlot;
    [SerializeField] private EquipmentSlotUI bobberSlot;
    [SerializeField] private CastingMinigameUI castingUI;
    [SerializeField] private GameObject fishingLinePrefab;
    [SerializeField] private BalanceMinigameUI balanceMinigameUI;

    [Header("--- FISHING AUDIO ---")]
    [Tooltip("Kéo AudioSource vào đây (hoặc để trống, code tự tạo)")]
    [SerializeField] private AudioSource fishingAudioSource;
    [Tooltip("Tiếng vung cần xé gió")]
    [SerializeField] private AudioClip castSwingSound;
    [Tooltip("Tiếng giằng co dây câu / quay cước")]
    [SerializeField] private AudioClip reelingStruggleSound;

    [Header("--- FISHING DATA & INVENTORY ---")]
    [SerializeField] private LayerMask waterLayer;
    private FishingZone currentFishingZone;

    [SerializeField] private float minBiteWaitTime = 3f;
    [SerializeField] private float maxBiteWaitTime = 8f;
    [SerializeField] private float energyCostPerCast = 10f;

    private PlayerAnimation playerAnimation;
    private ActivityEnergyController energyController;
    private PlayerInputHandler inputHandler;
    private PlayerInteraction playerInteraction;
    private FishingState currentState = FishingState.Idle;
    private FishingRodSO currentRod;
    private BaitSO currentBait;
    private BobberSO currentBobber;
    private BobberEntity activeBobberEntity;
    private FishingLineVisual activeLineVisual;

    public FishingRodSO CurrentRod => currentRod;
    public BaitSO CurrentBait => currentBait;
    public BobberSO CurrentBobber => currentBobber;

    private float currentThrowDistance;
    private int currentCastZone;
    private float biteTimer;
    private bool isWaitingForBite;
    private bool isFishBiting;
    private float reelInCooldown;
    private GameObject activeCaughtFish;
    private FishSO currentCaughtFishData;
    [SerializeField] private Transform leftHandFishSocket;
    private CharacterHandVisual handVisual;

    private float caughtFishLength;
    private float caughtFishWeight;
    private FishGrade caughtFishGrade;
    private bool isLastCatchNewRecord = false;

    // Biến chống spam click gây lỗi game
    private float inputCooldown = 0f;
    private float catchingLockTimer = 0f;

    [Header("--- AUTO-STOW FISHING ROD ---")]
    private float zoneCheckTimer = 0f;
    private float dryLandStowTimer = 0f;
    private bool wasNearWaterWithRod = false;
    private FishingZone[] cachedFishingZones;
    private float lastZoneCacheTime = -10f;

    [Header("--- PERFORMANCE SPATIAL CACHING ---")]
    private Vector3 lastWaterCheckPos = new Vector3(-9999f, -9999f, -9999f);
    private Vector3 lastWaterCheckForward = Vector3.zero;
    private bool lastWaterCheckResult = false;
    private float lastWaterCheckTime = -10f;

    [Header("--- FISHING FEEDBACK UI ---")]
    private GameObject feedbackBannerObj;
    private TextMeshProUGUI feedbackText;
    private CanvasGroup feedbackCanvasGroup;
    private float feedbackTimer = 0f;

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        inputHandler = GetComponent<PlayerInputHandler>();
        playerInteraction = GetComponent<PlayerInteraction>();
        handVisual = GetComponentInChildren<CharacterHandVisual>();
        energyController = GetComponent<ActivityEnergyController>();

        // Tự động tìm/tạo AudioSource để phát âm thanh
        if (fishingAudioSource == null)
        {
            fishingAudioSource = GetComponent<AudioSource>();
            if (fishingAudioSource == null)
            {
                fishingAudioSource = gameObject.AddComponent<AudioSource>();
                fishingAudioSource.playOnAwake = false;
            }
        }
    }

    private void OnDisable()
    {
        wasNearWaterWithRod = false;
        dryLandStowTimer = 0f;
        ResetToIdle();
    }

    public bool IsBusyFishing()
    {
        return currentState != FishingState.Idle && currentState != FishingState.Failed;
    }

    public bool IsWaitingForPower()
    {
        return currentState == FishingState.WaitingForPower;
    }

    private void Start()
    {
        EnsureEquipmentSlots();
        Invoke(nameof(CheckEquippedRodOnMapEnter), 1.5f);
    }

    private void CheckEquippedRodOnMapEnter()
    {
        EnsureEquipmentSlots();
        if (hotbarSlot != null && hotbarSlot.GetEquippedItem() != null)
        {
            currentRod = hotbarSlot.GetEquippedItem().GetItemShape() as FishingRodSO;
            if (currentRod != null && !ValidateFishingEquipment(out string errorReason))
            {
                ShowFishingFeedback(errorReason, new Color(1f, 0.65f, 0.25f));
            }
        }
    }

    public void OnRodEquippedCallback(ItemShapeSO itemShape)
    {
        if (itemShape is FishingRodSO rod)
        {
            currentRod = rod;
            int rodTier = rod.rodTier > 0 ? rod.rodTier : GetItemTier(rod);
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isOcean = sceneName.Contains("Map4") || sceneName.Contains("Ocean");

            if (isOcean && rodTier < 5)
            {
                string rodName = GetLocalizedText(rod.itemName, rod.name);
                ShowFishingFeedback($"[{rodName}] quá yếu cho Map 4 (Biển)!\nHãy trang bị Cần câu 5 hoặc 6 (Cần biển).", new Color(1f, 0.65f, 0.25f));
            }
            else if (!isOcean && rodTier >= 5)
            {
                string rodName = GetLocalizedText(rod.itemName, rod.name);
                ShowFishingFeedback($"[{rodName}] là cần biển, quá nặng cho vùng nước ngọt!\nHãy dùng Cần câu 1, 2, 3 hoặc 4.", new Color(1f, 0.65f, 0.25f));
            }
        }
    }

    public bool IsOceanMap()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return sceneName.Contains("Map4") || sceneName.Contains("Ocean");
    }

    public int GetItemTier(ItemShapeSO item)
    {
        if (item == null) return 0;
        if (item is FishingRodSO rod && rod.rodTier > 0) return rod.rodTier;

        string n = (string.IsNullOrEmpty(item.itemID) ? item.name : item.itemID).ToLower();
        for (int i = 8; i >= 1; i--)
        {
            if (n.Contains(i.ToString())) return i;
        }
        return 1;
    }

    public bool ValidateFishingEquipment(out string errorReason)
    {
        errorReason = "";
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isOcean = sceneName.Contains("Map4") || sceneName.Contains("Ocean");
        bool isSwamp = sceneName.Contains("Map3") || sceneName.Contains("Swamp");
        bool isTown = sceneName.Contains("Map_1") || sceneName.Contains("Town");

        if (isTown)
        {
            errorReason = GetLocalizedText("fish_err_town_nowater", "Khu vực Thị Trấn không có điểm câu cá!\nHãy đến Hồ Thông (Map 2), Đầm Lầy (Map 3) hoặc Bờ Biển (Map 4).");
            return false;
        }

        // 1. Kiểm tra Cần câu
        if (currentRod == null)
        {
            errorReason = GetLocalizedText("fish_err_no_rod", "Bạn chưa trang bị Cần câu!\nHãy mở Balo (phím Tab) và kéo Cần câu vào ô Cần câu.");
            return false;
        }

        InventoryItemUI rodItem = (hotbarSlot != null) ? hotbarSlot.GetEquippedItem() : null;
        if (rodItem != null && rodItem.GetDurability() <= 0f)
        {
            errorReason = GetLocalizedText("fish_err_rod_broken", "Cần câu đã bị gãy (0% Độ bền)!\nHãy mở Balo để sửa chữa cần câu.");
            return false;
        }

        int rodTier = currentRod.rodTier > 0 ? currentRod.rodTier : GetItemTier(currentRod);
        string rodName = GetLocalizedText(currentRod.itemName, currentRod.name);
        if (isOcean)
        {
            // Map 4 (Biển): Cần câu cấp 5, 6
            if (rodTier < 5)
            {
                errorReason = GetLocalizedText("fish_err_ocean_rod_weak", $"[{rodName}] (Cấp {rodTier}) quá yếu trước sóng biển Map 4!\nHãy mở Balo trang bị Cần câu Biển (Cần câu 5 hoặc 6).");
                return false;
            }
        }
        else
        {
            // Map 2 (Hồ Thông), Map 3 (Đầm Lầy): Cần câu cấp 1, 2, 3, 4
            if (rodTier >= 5)
            {
                string mapName = isSwamp ? "Đầm Lầy (Map 3)" : "Hồ Thông (Map 2)";
                errorReason = GetLocalizedText("fish_err_lake_rod_heavy", $"[{rodName}] (Cấp {rodTier}) là cần câu biển, quá nặng cho {mapName}!\nHãy mở Balo đổi sang Cần câu 1, 2, 3 hoặc 4.");
                return false;
            }
        }

        // 2. Kiểm tra Mồi câu
        if (currentBait == null)
        {
            errorReason = GetLocalizedText("fish_err_no_bait", "Bạn chưa trang bị Mồi câu!\nHãy mở Balo (phím Tab) và kéo Mồi câu vào ô Mồi.");
            return false;
        }

        InventoryItemUI baitItem = (baitSlot != null) ? baitSlot.GetEquippedItem() : null;
        if (baitItem != null && baitItem.GetRemainingUses() <= 0)
        {
            errorReason = GetLocalizedText("fish_err_bait_depleted", "Hộp mồi câu đã hết sạch!\nHãy mở Balo để trang bị hộp mồi mới.");
            return false;
        }

        int baitTier = GetItemTier(currentBait);
        string baitName = GetLocalizedText(currentBait.itemName, currentBait.name);
        if (isOcean)
        {
            if (baitTier < 5)
            {
                errorReason = GetLocalizedText("fish_err_ocean_bait", $"[{baitName}] là mồi nước ngọt, cá biển Map 4 không cắn câu!\nHãy mở Balo trang bị Mồi câu Biển (Mồi 5 hoặc 6).");
                return false;
            }
        }
        else
        {
            if (baitTier >= 5)
            {
                errorReason = GetLocalizedText("fish_err_lake_bait", $"[{baitName}] là mồi biển, không thích hợp cho cá nước ngọt vùng này!\nHãy mở Balo đổi sang Mồi câu 1, 2, 3 hoặc 4.");
                return false;
            }
        }

        // 3. Kiểm tra Phao câu
        if (currentBobber == null)
        {
            errorReason = GetLocalizedText("fish_err_no_bobber", "Bạn chưa trang bị Phao câu!\nHãy mở Balo (phím Tab) và kéo Phao câu vào ô Phao.");
            return false;
        }

        InventoryItemUI bobberItem = (bobberSlot != null) ? bobberSlot.GetEquippedItem() : null;
        if (bobberItem != null && bobberItem.GetDurability() <= 0f)
        {
            errorReason = GetLocalizedText("fish_err_bobber_broken", "Phao câu đã bị vỡ (0% Độ bền)!\nHãy mở Balo để thay phao câu mới.");
            return false;
        }

        int bobberTier = GetItemTier(currentBobber);
        string bobberName = GetLocalizedText(currentBobber.itemName, currentBobber.name);
        if (isOcean)
        {
            if (bobberTier < 7)
            {
                errorReason = GetLocalizedText("fish_err_ocean_bobber_sink", $"[{bobberName}] quá nhẹ, bị sóng biển Map 4 đánh chìm!\nHãy mở Balo trang bị Phao Biển (Phao 7 hoặc 8).");
                return false;
            }
        }
        else
        {
            if (bobberTier >= 7)
            {
                errorReason = GetLocalizedText("fish_err_lake_bobber_heavy", $"[{bobberName}] là phao biển hạng nặng, quá chìm cho vùng nước hồ/đầm!\nHãy mở Balo đổi sang Phao câu 1 đến 6.");
                return false;
            }
        }

        return true;
    }

    public void ShowFishingFeedback(string message, Color textColor)
    {
        string localizedMessage = GetLocalizedText(message, message);
        EnsureFeedbackUI();
        if (feedbackBannerObj != null)
        {
            feedbackBannerObj.transform.SetAsLastSibling();
            feedbackBannerObj.SetActive(true);
        }
        if (feedbackText != null)
        {
            feedbackText.text = localizedMessage;
            feedbackText.color = textColor;
        }
        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 1f;
        }
        feedbackTimer = 3.5f;
    }

    public string GetLocalizedText(string keyOrText, string fallbackText)
    {
        if (string.IsNullOrEmpty(keyOrText)) return fallbackText;
        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable("Game Text");
            if (table != null)
            {
                var entry = table.GetEntry(keyOrText);
                if (entry != null) return entry.GetLocalizedString();
            }
            return fallbackText;
        }
        catch
        {
            return fallbackText;
        }
    }

    private void EnsureFeedbackUI()
    {
        if (feedbackBannerObj != null && feedbackText != null && feedbackCanvasGroup != null)
        {
            feedbackBannerObj.transform.SetAsLastSibling();
            return;
        }

        Canvas rootCanvas = null;

        // 1. Ưu tiên tìm Canvas chính của Gameplay (Backpack hoặc HUD)
        if (BackpackMinigameUI.Instance != null)
        {
            rootCanvas = BackpackMinigameUI.Instance.GetComponentInParent<Canvas>();
        }

        // 2. Nếu chưa có, quét tìm Canvas đang active trong Scene
        if (rootCanvas == null)
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var c in canvases)
            {
                if (c == null || !c.gameObject.activeInHierarchy) continue;
                string cName = c.gameObject.name.ToLower();
                if (cName.Contains("balan") || cName.Contains("qte") || cName.Contains("login") || cName.Contains("shop") || cName.Contains("loading")) continue;
                if (rootCanvas == null || c.sortingOrder > rootCanvas.sortingOrder)
                {
                    rootCanvas = c;
                }
            }
        }

        // 3. Fallback: Nếu vẫn chưa có Canvas nào, tạo mới Canvas chuyên dụng cho Feedback
        if (rootCanvas == null)
        {
            GameObject canvasObj = new GameObject("FishingFeedback_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            rootCanvas = canvasObj.GetComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 999;
            CanvasScaler cs = canvasObj.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            DontDestroyOnLoad(canvasObj);
        }

        // Tái sử dụng nếu đã có
        Transform existing = rootCanvas.transform.Find("FishingFeedbackBanner");
        if (existing != null)
        {
            feedbackBannerObj = existing.gameObject;
            feedbackText = feedbackBannerObj.GetComponentInChildren<TextMeshProUGUI>(true);
            feedbackCanvasGroup = feedbackBannerObj.GetComponent<CanvasGroup>();
            feedbackBannerObj.transform.SetAsLastSibling();
            return;
        }

        // Tạo UI Feedback Banner động trên Canvas với bố cục nổi bật ở đỉnh màn hình
        feedbackBannerObj = new GameObject("FishingFeedbackBanner", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        feedbackBannerObj.transform.SetParent(rootCanvas.transform, false);
        feedbackBannerObj.transform.SetAsLastSibling();

        // Đảm bảo banner có Canvas con với sortingOrder = 999 để luôn hiển thị trên cùng mọi UI
        Canvas bannerCanvas = feedbackBannerObj.AddComponent<Canvas>();
        bannerCanvas.overrideSorting = true;
        bannerCanvas.sortingOrder = 999;
        feedbackBannerObj.AddComponent<GraphicRaycaster>();

        feedbackCanvasGroup = feedbackBannerObj.GetComponent<CanvasGroup>();
        feedbackCanvasGroup.blocksRaycasts = false;
        feedbackCanvasGroup.interactable = false;

        Image bgImage = feedbackBannerObj.GetComponent<Image>();
        bgImage.color = new Color(0.04f, 0.07f, 0.12f, 0.95f);
        bgImage.raycastTarget = false;

        RectTransform rect = feedbackBannerObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.85f); // Căn giữa phía trên màn hình
        rect.anchorMax = new Vector2(0.5f, 0.85f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(720f, 75f); // Kích thước rộng rãi cho 2 dòng thông báo

        GameObject textObj = new GameObject("FeedbackText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(feedbackBannerObj.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 4f);
        textRect.offsetMax = new Vector2(-16f, -4f);

        feedbackText = textObj.GetComponent<TextMeshProUGUI>();
        feedbackText.fontSize = 18f;
        feedbackText.fontStyle = FontStyles.Bold;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.raycastTarget = false;
        feedbackText.textWrappingMode = TextWrappingModes.Normal;
        feedbackText.overflowMode = TextOverflowModes.Overflow;

        // Gán Font từ các TMP có sẵn trong Scene hoặc TMP_Settings mặc định
        TextMeshProUGUI[] tmps = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in tmps)
        {
            if (t != null && t.font != null)
            {
                feedbackText.font = t.font;
                feedbackText.fontSharedMaterial = t.fontSharedMaterial;
                break;
            }
        }
        if (feedbackText.font == null && TMP_Settings.defaultFontAsset != null)
        {
            feedbackText.font = TMP_Settings.defaultFontAsset;
        }

        feedbackBannerObj.SetActive(false);
    }

    private void Update()
    {
        // Cập nhật đếm ngược mờ dần thông báo Feedback luôn luôn chạy (kể cả khi mở UI)
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.unscaledDeltaTime;
            if (feedbackTimer <= 0.8f && feedbackCanvasGroup != null)
            {
                feedbackCanvasGroup.alpha = Mathf.Clamp01(feedbackTimer / 0.8f);
            }
            if (feedbackTimer <= 0f)
            {
                if (feedbackBannerObj != null) feedbackBannerObj.SetActive(false);
            }
        }

        // Tự động kiểm tra và cất cần câu vào Balo khi rời khỏi khu vực câu cá (luôn chạy, kể cả khi UI mở)
        zoneCheckTimer -= Time.deltaTime;
        if (zoneCheckTimer <= 0f)
        {
            zoneCheckTimer = 0.2f;
            CheckAutoStowRodWhenLeavingFishingArea();
        }

        if (inputHandler != null && inputHandler.IsUIOpen) return;

        // Giảm thời gian đếm ngược chống spam
        if (inputCooldown > 0f) inputCooldown -= Time.deltaTime;
        if (catchingLockTimer > 0f) catchingLockTimer -= Time.deltaTime;

        if (reelInCooldown > 0f)
        {
            reelInCooldown -= Time.deltaTime;
        }

        if (isWaitingForBite && currentState == FishingState.Fishing)
        {
            biteTimer -= Time.deltaTime;
            if (biteTimer <= 0f)
            {
                TriggerFishBitingEvent();
            }
        }

        if (inputHandler != null && inputHandler.InteractTriggered)
        {
            HandleLeftClick();
        }

        if (currentState == FishingState.Catching && inputHandler != null && inputHandler.MinigameTriggered)
        {
            // Chỉ cho phép vứt cá khi đã qua thời gian khóa (chống spam phím Space)
            if (catchingLockTimer <= 0f)
            {
                Debug.Log("<color=yellow>[Fishing Controller] Bấm Space -> Thả cá đi!</color>");
                ShowFishingFeedback("Đã thả cá về lại vùng nước!", Color.yellow);
                ForcedTutorialManager.Instance?.NotifyKeepOrReleaseFish();
                ResetToIdle();
            }
        }
    }

    private void HandleLeftClick()
    {
        // Giảm thời gian chống spam cực nhỏ để nhận click ngay lập tức mà không trễ 1s nào
        if (inputCooldown > 0f) return;
        inputCooldown = 0.06f;

        if (currentState == FishingState.Idle)
        {
            if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanStartFishing()) return;
            if (playerInteraction != null && playerInteraction.HasActiveInteractable()) return;

            EnsureEquipmentSlots();

            currentRod = (hotbarSlot != null && hotbarSlot.GetEquippedItem() != null)
                ? hotbarSlot.GetEquippedItem().GetItemShape() as FishingRodSO : null;

            currentBait = (baitSlot != null && baitSlot.GetEquippedItem() != null)
                ? baitSlot.GetEquippedItem().GetItemShape() as BaitSO : null;

            currentBobber = (bobberSlot != null && bobberSlot.GetEquippedItem() != null)
                ? bobberSlot.GetEquippedItem().GetItemShape() as BobberSO : null;

            // 1. KIỂM TRA ĐIỀU KIỆN TRANG BỊ THEO MAP TRƯỚC (Cần câu, Mồi câu, Phao câu)
            if (!ValidateFishingEquipment(out string errorReason))
            {
                ShowFishingFeedback(errorReason, new Color(1f, 0.45f, 0.45f));
                Debug.LogWarning($"<color=yellow>[Fishing Controller] {errorReason}</color>");
                return;
            }

            // 2. KIỂM TRA VỊ TRÍ: Người chơi phải đứng gần bờ hồ / hướng về phía mặt nước mới được vung cần câu
            if (!IsPlayerNearValidFishingWater())
            {
                ShowFishingFeedback("Hãy tiến lại gần bờ hồ / bờ biển để câu cá!", new Color(1f, 0.75f, 0.25f));
                Debug.LogWarning("<color=yellow>[Fishing Controller] Không thể vung cần khi đứng quá xa bờ hồ!</color>");
                return;
            }

            if (currentBait != null && currentBobber != null)
            {
                if (energyController != null)
                {
                    CharacterStatsManager statsManager = energyController.statsManager;
                    if (statsManager != null && statsManager.GetStatValue(StatType.Energy) <= 0)
                    {
                        ShowFishingFeedback("Bạn đã cạn kiệt thể lực! Hãy ăn uống hoặc nghỉ ngơi.", new Color(1f, 0.6f, 0.2f));
                        Debug.Log("<color=red>[Fishing Controller] Bạn đã cạn kiệt thể lực, không thể tiếp tục câu!</color>");
                        return;
                    }
                }

                Vector3 lookDir = Camera.main.transform.forward;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    transform.forward = lookDir.normalized;
                }

                StartWindUp();
                ForcedTutorialManager.Instance?.NotifyWalkToLakeSide();
                ForcedTutorialManager.Instance?.NotifyWindUpRod();
            }
            else
            {
                ShowFishingFeedback("Bạn cần trang bị đầy đủ Mồi câu và Phao câu!", new Color(1f, 0.6f, 0.2f));
                Debug.Log($"<color=yellow>[Fishing Controller] Đã cầm cần nhưng chưa thể quăng! Thiếu Mồi hoặc Phao.</color>");
            }
        }
        else if (currentState == FishingState.WaitingForPower)
        {
            ExecuteCast();
        }
        else if (currentState == FishingState.Fishing)
        {
            if (isFishBiting || reelInCooldown > 0f)
            {
                return;
            }
            else
            {
                Debug.Log("<color=yellow>[Fishing Controller] Thu cần sớm khi cá chưa cắn!</color>");
            }
            inputCooldown = 0.35f; // Chặn click tiếp tục ngay lập tức để không bị ăn nhầm lệnh vung cần mới
            reelInCooldown = 0.35f;
            ResetToIdle();
        }
        else if (currentState == FishingState.Catching)
        {
            // Nếu đang mở Bảng xem cá 3D (FishInspectionUI), người chơi sẽ tương tác qua nút bấm [Nhận cá] / [Thả cá]
            if (FishInspectionUI.Instance != null && FishInspectionUI.Instance.IsOpen)
            {
                return;
            }

            if (catchingLockTimer > 0f) return;

            if (currentCaughtFishData != null)
            {
                BackpackMinigameUI backpack = BackpackMinigameUI.Instance;
                if (backpack == null)
                {
                    backpack = Object.FindFirstObjectByType<BackpackMinigameUI>(FindObjectsInactive.Include);
                }

                if (backpack != null)
                {
                    bool added = backpack.TryAutoAddFish(currentCaughtFishData, caughtFishLength, caughtFishWeight, caughtFishGrade);
                    if (added)
                    {
                        string fName = GetLocalizedText(currentCaughtFishData.itemName, currentCaughtFishData.itemName);
                        ShowFishingFeedback($"Đã cất [{fName}] ({caughtFishLength:F1}cm) vào Balo!", Color.green);
                        Debug.Log($"<color=green>[Fishing Controller] Đã cất [{fName}] vào Balo!</color>");

                        // Tự động cập nhật tiến độ nhiệm vụ
                        if (QuestManager.Instance != null)
                        {
                            QuestManager.Instance.NotifyFishCaught(currentCaughtFishData, caughtFishLength, caughtFishWeight, caughtFishGrade);
                        }
                    }
                    else
                    {
                        ShowFishingFeedback("Balo đã đầy! Không còn chỗ trống để cất cá.", new Color(1f, 0.5f, 0.2f));
                        Debug.Log("<color=red>[Fishing Controller] Balo đầy! Không thể cất cá, đã thả đi.</color>");
                    }
                }
                else
                {
                    Debug.LogWarning("<color=yellow>[Fishing Controller] Không tìm thấy BackpackMinigameUI trong Scene!</color>");
                }
            }

            if (activeCaughtFish != null)
            {
                Destroy(activeCaughtFish);
                activeCaughtFish = null;
            }

            inputCooldown = 0.35f;
            ForcedTutorialManager.Instance?.NotifyKeepOrReleaseFish();
            ResetToIdle();
        }
    }

    private void TriggerFishBitingEvent()
    {
        isWaitingForBite = false;

        // ========================================================
        // TÍNH TOÁN TỈ LỆ SẢY CÁ / CÂU HỤT KHI CÁ CẮN MỒI
        // ========================================================
        float escapeChance = 0.12f; // Tỉ lệ sảy cơ bản 12%
        if (currentRod != null) escapeChance -= (currentRod.rodTier * 0.015f);
        if (currentBobber != null) escapeChance -= (currentBobber.attractivenessBonus * 0.002f);
        escapeChance = Mathf.Clamp(escapeChance, 0.03f, 0.25f);

        if (Random.value < escapeChance)
        {
            Debug.Log("<color=yellow>[Fishing Controller] SẢY CÁ! Cá cắn mồi rồi giật tuột mất.</color>");
            ShowFishingFeedback("Sảy cá rồi! Cá đã cắn mồi nhưng giật tuột mất.", new Color(1f, 0.35f, 0.35f));

            if (playerAnimation != null)
            {
                playerAnimation.SetFishingState(false);
                playerAnimation.TriggerCatchFail();
            }

            StopStruggleSound();
            if (activeLineVisual != null) { Destroy(activeLineVisual.gameObject); activeLineVisual = null; }
            if (activeBobberEntity != null) { Destroy(activeBobberEntity.gameObject); activeBobberEntity = null; }

            Invoke(nameof(ResetToIdle), 1.5f);
            return;
        }

        isFishBiting = true;
        ForcedTutorialManager.Instance?.NotifyReelFish();

        if (currentFishingZone != null)
        {
            int rarityBonus = currentBait != null ? currentBait.targetRarityBonus : 0;
            currentCaughtFishData = currentFishingZone.GetRandomFish(rarityBonus);
        }
        else
        {
            currentCaughtFishData = null;
        }

        string fishName = currentCaughtFishData != null ? currentCaughtFishData.itemName : "Cá bí ẩn";
        Debug.Log($"<color=red>[Fishing Controller] {fishName.ToUpper()} CẮN CÂU! Kích hoạt Balance Minigame.</color>");

        if (playerAnimation != null)
        {
            playerAnimation.TriggerFishBite();
        }

        if (activeLineVisual != null)
        {
            activeLineVisual.SetBitingState(true);
        }

        if (activeBobberEntity != null)
        {
            activeBobberEntity.StartBiting();
        }

        // --- PHÁT TIẾNG GIẰNG CO LIÊN TỤC ---
        if (fishingAudioSource != null && reelingStruggleSound != null)
        {
            fishingAudioSource.clip = reelingStruggleSound;
            fishingAudioSource.loop = true;
            fishingAudioSource.Play();
            Debug.Log("<color=green>[Audio] Đang phát tiếng kéo cá giằng co!</color>");
        }

        // --- CẢNH BÁO CHIẾN ĐẤU KHI DÍNH CÁ LỚN / HIẾM ---
        if (currentCaughtFishData != null)
        {
            if (currentCaughtFishData.rarity == FishRarity.Legendary)
            {
                ShowFishingFeedback("CÁ HUYỀN THOẠI CẮN CÂU! Hãy ghìm chặt cước!", new Color(1f, 0.3f, 0.3f));
            }
            else if (currentCaughtFishData.rarity == FishRarity.Rare)
            {
                ShowFishingFeedback("Cá lớn cắn câu! Giằng co quyết liệt!", new Color(1f, 0.8f, 0.2f));
            }
        }

        if (balanceMinigameUI != null)
        {
            balanceMinigameUI.StartMinigame(inputHandler, this, currentCaughtFishData);
        }
        else
        {
            Invoke(nameof(ResetToIdle), 3.5f);
        }
    }

    public void OnMinigameEnd(bool isSuccess)
    {
        // --- TẮT ÂM THANH KHI MINIGAME KẾT THÚC ---
        StopStruggleSound();

        if (activeLineVisual != null)
        {
            Destroy(activeLineVisual.gameObject);
            activeLineVisual = null;
        }
        if (activeBobberEntity != null)
        {
            Destroy(activeBobberEntity.gameObject);
            activeBobberEntity = null;
        }

        if (isSuccess)
        {
            Debug.Log("<color=green>[Fishing Controller] CÂN BẰNG THÀNH CÔNG! Chuyển sang animation dâng cá.</color>");
            ShowFishingFeedback("Cắn câu thành công! Đang kéo cá lên...", new Color(0.4f, 1f, 0.5f));

            if (currentCaughtFishData != null)
            {
                currentCaughtFishData.GenerateRandomSize(out caughtFishLength, out caughtFishWeight);

                // 1. Phao câu xịn (Bobber) -> Tăng chiều dài & cân nặng cá (+% attractivenessBonus)
                if (currentBobber != null && currentBobber.attractivenessBonus > 0f)
                {
                    float sizeMultiplier = 1f + (currentBobber.attractivenessBonus / 100f);
                    caughtFishLength = Mathf.Round(caughtFishLength * sizeMultiplier * 10f) / 10f;
                    caughtFishWeight = Mathf.Round(caughtFishWeight * (sizeMultiplier * sizeMultiplier) * 100f) / 100f;
                }

                // 2. Mồi câu xịn (Bait) -> Tăng tỷ lệ phẩm chất cá Bạc / Vàng Kim (+% targetRarityBonus)
                float gradeBonus = currentBait != null ? currentBait.targetRarityBonus * 4f : 0f;
                caughtFishGrade = currentCaughtFishData.GenerateRandomGrade(gradeBonus);

                isLastCatchNewRecord = false;
                if (FishJournalManager.Instance != null)
                {
                    isLastCatchNewRecord = FishJournalManager.Instance.RecordCatch(currentCaughtFishData.itemID, caughtFishLength, caughtFishWeight, caughtFishGrade);
                    if (isLastCatchNewRecord)
                    {
                        Debug.Log($"<color=yellow>[Sổ Tay] KỶ LỰC MỚI!</color>");
                    }
                }
            }

            currentState = FishingState.Catching;
            catchingLockTimer = 1.4f; // Khóa 1.4s để nhân vật thực hiện xong động tác giật cần kéo cá lên
            ForcedTutorialManager.Instance?.NotifyFishCaught();
            if (playerAnimation != null)
            {
                playerAnimation.TriggerCatchSuccess();
            }

            // Lên lịch sinh model cá đúng lúc động tác giật cần hoàn tất (1.33s)
            CancelInvoke(nameof(SpawnCaughtFishModel));
            Invoke(nameof(SpawnCaughtFishModel), 1.33f);
        }
        else
        {
            Debug.Log("<color=red>[Fishing Controller] CÂN BẰNG THẤT BẠI!</color>");
            currentState = FishingState.Failed;
            ShowFishingFeedback("Sảy cá rồi! Dây cước chùng khiến cá trốn thoát.", new Color(1f, 0.35f, 0.35f));

            if (ForcedTutorialManager.Instance != null && ForcedTutorialManager.Instance.GetCurrentStage() == TutorialStage.Map2_Quest4_3_ReelFish)
            {
                ForcedTutorialManager.Instance.AdvanceToStage(TutorialStage.Map2_Quest4_1_WindUpRod);
            }
            if (playerAnimation != null)
            {
                playerAnimation.SetFishingState(false);
                playerAnimation.TriggerCatchFail();
            }
        }

        // --- TIÊU HAO MỒI CÂU, ĐỘ BỀN CẦN CÂU VÀ PHAO CÂU ---
        // 1. Tiêu hao Mồi câu (1 lần dùng khi cá đã cắn câu)
        InventoryItemUI equippedBait = (baitSlot != null) ? baitSlot.GetEquippedItem() : null;
        if (equippedBait != null)
        {
            equippedBait.ConsumeUse(1);
            if (equippedBait.GetRemainingUses() <= 0)
            {
                baitSlot.RemoveEquippedItem();
                Destroy(equippedBait.gameObject);
                ShowFishingFeedback(GetLocalizedText("fish_notify_bait_empty", "Mồi câu đã hết! Hãy trang bị mồi mới."), new Color(1f, 0.6f, 0.2f));
            }
        }

        // 2. Tiêu hao Độ bền Cần câu (Thành công mất ~2.5 điểm, Thất bại giật mạnh mất ~4.5 điểm)
        InventoryItemUI equippedRod = (hotbarSlot != null) ? hotbarSlot.GetEquippedItem() : null;
        if (equippedRod != null)
        {
            float rodWear = isSuccess ? 2.5f : 4.5f;
            equippedRod.ConsumeDurability(rodWear);

            if (equippedRod.GetDurability() <= 0f)
            {
                ShowFishingFeedback(GetLocalizedText("fish_notify_rod_broke", "CẦN CÂU ĐÃ BỊ GÃY! Hãy sửa chữa trong Balo."), new Color(1f, 0.25f, 0.25f));
            }
            else if (equippedRod.GetDurability() <= 20f)
            {
                ShowFishingFeedback(GetLocalizedText("fish_notify_rod_low", $"Cần câu sắp gãy! (Độ bền: {Mathf.RoundToInt(equippedRod.GetDurability())}%)"), new Color(1f, 0.75f, 0.2f));
            }
        }

        // 3. Tiêu hao Độ bền Phao câu (1 điểm mỗi lần kéo)
        InventoryItemUI equippedBobber = (bobberSlot != null) ? bobberSlot.GetEquippedItem() : null;
        if (equippedBobber != null)
        {
            equippedBobber.ConsumeDurability(1f);
            if (equippedBobber.GetDurability() <= 0f)
            {
                bobberSlot.RemoveEquippedItem();
                Destroy(equippedBobber.gameObject);
                ShowFishingFeedback(GetLocalizedText("fish_notify_bobber_broke", "Phao câu đã bị vỡ/hỏng!"), new Color(1f, 0.35f, 0.35f));
            }
        }

        BackpackMinigameUI.Instance?.SaveBackpack();
    }

    public void OnCatchSuccessIntroComplete()
    {
        if (currentState == FishingState.Catching)
        {
            CancelInvoke(nameof(SpawnCaughtFishModel));
            SpawnCaughtFishModel();
        }
    }

    public void SpawnCaughtFishModel()
    {
        if (activeCaughtFish != null)
        {
            Destroy(activeCaughtFish);
            activeCaughtFish = null;
        }

        if (currentCaughtFishData == null) return;

        GameObject prefabToSpawn = currentCaughtFishData.caughtFishPrefab != null 
            ? currentCaughtFishData.caughtFishPrefab 
            : currentCaughtFishData.equippedModelPrefab;

        if (prefabToSpawn == null) return;

        Transform targetSocket = leftHandFishSocket;
        if (targetSocket == null)
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            foreach (var t in allChildren)
            {
                string lower = t.name.ToLower();
                if (lower == "lefthand" || lower.Contains("hand_l") || lower.Contains("hand.l") || lower.Contains("left_hand"))
                {
                    targetSocket = t;
                    break;
                }
            }
        }
        if (targetSocket == null && handVisual != null)
        {
            targetSocket = handVisual.GetTipSocketTransform();
        }
        if (targetSocket == null)
        {
            targetSocket = transform;
        }

        activeCaughtFish = Instantiate(prefabToSpawn, targetSocket.position, targetSocket.rotation, targetSocket);
        activeCaughtFish.transform.localPosition = Vector3.zero;
        activeCaughtFish.transform.localRotation = Quaternion.identity;
        activeCaughtFish.transform.localScale = Vector3.one;

        foreach (var col in activeCaughtFish.GetComponentsInChildren<Collider>()) col.enabled = false;
        foreach (var rb in activeCaughtFish.GetComponentsInChildren<Rigidbody>()) rb.isKinematic = true;

        Debug.Log($"<color=green>[Fishing Controller] Đã hiển thị mô hình cá [{currentCaughtFishData.itemName}] trên tay!</color>");

        // Tự động tìm / kích hoạt FishInspectionUI từ GameObject FishViewingChart nếu có
        if (FishInspectionUI.Instance == null)
        {
            GameObject chartObj = GameObject.Find("FishViewingChart");
            if (chartObj != null)
            {
                chartObj.AddComponent<FishInspectionUI>();
            }
        }

        // Bật Bảng Xem Cá 3D và Thông Số Chi Tiết
        if (FishInspectionUI.Instance != null)
        {
            FishInspectionUI.Instance.ShowFish(
                currentCaughtFishData,
                caughtFishLength,
                caughtFishWeight,
                caughtFishGrade,
                isLastCatchNewRecord,
                onKeep: () => {
                    if (activeCaughtFish != null) { Destroy(activeCaughtFish); activeCaughtFish = null; }
                    inputCooldown = 0.35f;
                    ForcedTutorialManager.Instance?.NotifyKeepOrReleaseFish();
                    ResetToIdle();
                },
                onRelease: () => {
                    if (activeCaughtFish != null) { Destroy(activeCaughtFish); activeCaughtFish = null; }
                    inputCooldown = 0.35f;
                    ForcedTutorialManager.Instance?.NotifyKeepOrReleaseFish();
                    ResetToIdle();
                }
            );
        }
    }

    public void OnCatchFailComplete()
    {
        if (currentState == FishingState.Failed)
        {
            ResetToIdle();
        }
    }

    private void StartWindUp()
    {
        currentState = FishingState.WaitingForPower;
        if (castingUI != null)
        {
            castingUI.StartMinigame();
        }
        if (playerAnimation != null)
        {
            playerAnimation.TriggerCastAnimation();
        }
    }

    public void OnWindUpPaused()
    {
        if (currentState == FishingState.WaitingForPower && playerAnimation != null)
        {
            playerAnimation.PauseWindUpPose();
        }
    }

    private void ExecuteCast()
    {
        if (castingUI == null) return;

        castingUI.StopMinigame(out int zone, out float powerRatio);

        if (energyController != null)
        {
            float energyCost = energyCostPerCast;
            if (PlayerBuffManager.Instance != null && PlayerBuffManager.Instance.HasBuff(BuffType.Endurance))
            {
                energyCost *= PlayerBuffManager.Instance.GetBuffMultiplier(BuffType.Endurance, 0.5f);
            }
            energyController.TryConsumeEnergy(energyCost);
        }

        if (zone == 0)
        {
            currentState = FishingState.Idle;
            ShowFishingFeedback("Quăng cần hụt! Hãy căn lực vào vùng màu xanh.", Color.yellow);
            if (playerAnimation != null) playerAnimation.ResetAnimationSpeed();
            if (ForcedTutorialManager.Instance != null && ForcedTutorialManager.Instance.GetCurrentStage() == TutorialStage.Map2_Quest4_2_TimingPower)
            {
                ForcedTutorialManager.Instance.AdvanceToStage(TutorialStage.Map2_Quest4_1_WindUpRod);
            }
            return;
        }

        currentState = FishingState.Casting;
        ForcedTutorialManager.Instance?.NotifyRodCasted();
        if (playerAnimation != null)
        {
            playerAnimation.ResumeAnimation();
        }

        CancelInvoke(nameof(FallbackCastRelease));
        Invoke(nameof(FallbackCastRelease), 0.75f);

        currentCastZone = zone;

        if (currentRod != null && currentRod.zoneDistanceMultipliers != null && zone < currentRod.zoneDistanceMultipliers.Length)
        {
            currentThrowDistance = currentRod.castDistance * currentRod.zoneDistanceMultipliers[zone];
        }
        else
        {
            currentThrowDistance = currentRod != null ? currentRod.castDistance * (zone / 3f) : 0f;
        }
    }

    private void EnterFishingState()
    {
        currentState = FishingState.Fishing;

        // --- PHÁT ÂM THANH VUNG CẦN ---
        if (fishingAudioSource != null && castSwingSound != null)
        {
            fishingAudioSource.PlayOneShot(castSwingSound);
        }

        if (playerAnimation != null)
        {
            playerAnimation.SetFishingState(true);
        }

        if (handVisual != null)
        {
            handVisual.SetBobberVisualActive(false);
        }

        if (currentBobber != null && currentBobber.bobberPrefab != null)
        {
            Transform tipTransform = transform;
            if (handVisual != null && handVisual.GetTipSocketTransform() != null)
            {
                tipTransform = handVisual.GetTipSocketTransform();
            }

            Vector3 startPos = tipTransform.position;
            Vector3 castDirection = transform.forward;
            castDirection.y = 0f;
            castDirection.Normalize();

            Vector3 targetPos = transform.position + castDirection * currentThrowDistance;

            bool isWaterHit = false;
            Vector3 rayStart = new Vector3(targetPos.x, transform.position.y + 10f, targetPos.z);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f))
            {
                targetPos.y = hit.point.y;

                if ((waterLayer.value & (1 << hit.collider.gameObject.layer)) > 0)
                {
                    isWaterHit = true;
                    currentFishingZone = hit.collider.GetComponent<FishingZone>();
                    if (currentFishingZone == null)
                    {
                        currentFishingZone = hit.collider.GetComponentInParent<FishingZone>();
                    }
                }
            }
            else
            {
                targetPos.y = transform.position.y;
            }

            GameObject bobberObj = Instantiate(currentBobber.bobberPrefab, startPos, Quaternion.identity);
            activeBobberEntity = bobberObj.GetComponent<BobberEntity>();
            if (activeBobberEntity == null)
            {
                activeBobberEntity = bobberObj.AddComponent<BobberEntity>();
            }

            if (fishingLinePrefab != null)
            {
                GameObject lineObj = Instantiate(fishingLinePrefab, Vector3.zero, Quaternion.identity);
                lineObj.transform.position = Vector3.zero;
                lineObj.transform.rotation = Quaternion.identity;
                activeLineVisual = lineObj.GetComponent<FishingLineVisual>();
                if (activeLineVisual != null)
                {
                    activeLineVisual.Setup(tipTransform, activeBobberEntity.transform);
                }
            }

            float dynamicDuration = Mathf.Max(0.5f, Mathf.Sqrt(currentThrowDistance) * 0.35f);
            float dynamicHeight = Mathf.Max(1f, currentThrowDistance * 0.2f * (currentCastZone * 0.5f));

            activeBobberEntity.Cast(startPos, targetPos, dynamicDuration, dynamicHeight, activeLineVisual);

            if (isWaterHit)
            {
                isFishBiting = false;
                isWaitingForBite = true;
                reelInCooldown = 1.0f;
                float baseWait = Random.Range(minBiteWaitTime, maxBiteWaitTime);

                // 1. Giảm thời gian do Cần Câu (% Giảm thời gian chờ)
                if (currentRod != null && currentRod.waitTimeReductionPercentage > 0f)
                {
                    baseWait *= (1f - Mathf.Clamp01(currentRod.waitTimeReductionPercentage / 100f));
                }

                // 2. Giảm thời gian do Mồi Câu (trừ trực tiếp số giây + % thu hút)
                if (currentBait != null)
                {
                    baseWait -= currentBait.waitTimeReduction;
                    baseWait -= (currentBait.attractivenessBonus * 0.03f);
                }

                // 3. Giảm thời gian do Phao Câu (% thu hút)
                if (currentBobber != null && currentBobber.attractivenessBonus > 0f)
                {
                    baseWait -= (currentBobber.attractivenessBonus * 0.02f);
                }

                // 4. Giảm thời gian do Khung Giờ Sinh Thái (Sáng sớm / Hoàng hôn)
                if (FishEcologyManager.Instance != null)
                {
                    baseWait *= FishEcologyManager.Instance.GetBiteWaitMultiplier();
                }

                biteTimer = Mathf.Max(1.0f, baseWait);
            }
            else
            {
                isFishBiting = false;
                isWaitingForBite = false;
                reelInCooldown = dynamicDuration + 0.5f;
                Invoke(nameof(ResetToIdle), dynamicDuration + 0.2f);
            }
        }
    }

    private void ResetToIdle()
    {
        CancelInvoke(nameof(ResetToIdle));
        CancelInvoke(nameof(FallbackCastRelease));
        StopStruggleSound(); // Tắt luôn âm thanh khi reset trạng thái

        isWaitingForBite = false;
        isFishBiting = false;
        currentCaughtFishData = null;
        currentFishingZone = null;
        currentState = FishingState.Idle;

        if (balanceMinigameUI != null) balanceMinigameUI.ForceStopMinigame();
        if (castingUI != null) castingUI.StopMinigame(out _, out _);
        if (activeLineVisual != null) { Destroy(activeLineVisual.gameObject); activeLineVisual = null; }
        if (activeBobberEntity != null) { Destroy(activeBobberEntity.gameObject); activeBobberEntity = null; }
        if (activeCaughtFish != null) { Destroy(activeCaughtFish); activeCaughtFish = null; }
        if (playerAnimation != null)
        {
            playerAnimation.ResetAllFishingTriggers();
            playerAnimation.SetFishingState(false);
        }
        if (handVisual != null) handVisual.SetBobberVisualActive(true);
    }

    private void StopStruggleSound()
    {
        if (fishingAudioSource != null && fishingAudioSource.isPlaying && fishingAudioSource.clip == reelingStruggleSound)
        {
            fishingAudioSource.Stop();
            fishingAudioSource.loop = false;
        }
    }

    public void OnAnimationCastRelease()
    {
        CancelInvoke(nameof(FallbackCastRelease));
        if (currentState == FishingState.Casting)
        {
            EnterFishingState();
        }
    }

    private void FallbackCastRelease()
    {
        if (currentState == FishingState.Casting)
        {
            EnterFishingState();
        }
    }

    public void EnsureEquipmentSlots()
    {
        if (handVisual == null) handVisual = GetComponentInChildren<CharacterHandVisual>() ?? Object.FindFirstObjectByType<CharacterHandVisual>();
        if (playerAnimation == null) playerAnimation = GetComponent<PlayerAnimation>() ?? Object.FindFirstObjectByType<PlayerAnimation>();

        if (hotbarSlot == null || baitSlot == null || bobberSlot == null)
        {
            EquipmentSlotUI[] allSlots = Object.FindObjectsByType<EquipmentSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var slot in allSlots)
            {
                if (slot == null) continue;
                if (hotbarSlot == null && slot.GetSlotRequirement() == EquipmentSlotUI.SlotRequirement.OnlyFishingRod)
                {
                    hotbarSlot = slot;
                }
                else if (baitSlot == null && slot.GetSlotRequirement() == EquipmentSlotUI.SlotRequirement.OnlyBait)
                {
                    baitSlot = slot;
                }
                else if (bobberSlot == null && slot.GetSlotRequirement() == EquipmentSlotUI.SlotRequirement.OnlyBobber)
                {
                    bobberSlot = slot;
                }
            }
        }

        if (hotbarSlot != null)
        {
            hotbarSlot.OnItemEquipped -= OnRodEquippedCallback;
            hotbarSlot.OnItemEquipped += OnRodEquippedCallback;
        }
    }

    private FishingZone[] GetAllFishingZones()
    {
        if (cachedFishingZones == null || cachedFishingZones.Length == 0 || Time.time - lastZoneCacheTime > 5f)
        {
            cachedFishingZones = Object.FindObjectsByType<FishingZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            lastZoneCacheTime = Time.time;
        }
        return cachedFishingZones;
    }

    /// <summary>
    /// Kiểm tra xem tại vị trí (x, z) có bề mặt NƯỚC LỘ THIÊN (không bị đất/địa hình che phủ) hay không.
    /// Hoạt động chính xác 100% trên cả 4 Map (kể cả Map 3 có mặt phẳng nước ngầm bên dưới đất).
    /// </summary>
    private bool IsOpenWaterAtXZ(float x, float z)
    {
        int waterLayerIndex = LayerMask.NameToLayer("Water");
        if (waterLayerIndex == -1) waterLayerIndex = 4;

        // Bắn RaycastAll từ trên cao (player.y + 12m) xuống dưới 25m
        Vector3 rayStart = new Vector3(x, transform.position.y + 12f, z);
        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, 25f, Physics.AllLayers, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return false;

        float highestGroundY = float.MinValue;
        float highestWaterY = float.MinValue;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            // Bỏ qua chính player và các object con
            if (hit.collider.transform.root == transform.root) continue;

            bool isWater = (hit.collider.gameObject.layer == waterLayerIndex) ||
                           (hit.collider.name.ToLower().Contains("water")) ||
                           (hit.collider.GetComponent<FishingZone>() != null) ||
                           (hit.collider.GetComponentInParent<FishingZone>() != null);

            if (isWater)
            {
                if (hit.point.y > highestWaterY)
                {
                    highestWaterY = hit.point.y;
                }
            }
            else
            {
                // Bỏ qua các trigger phụ trợ (như Interaction triggers, checkpoint)
                if (hit.collider.isTrigger) continue;

                if (hit.point.y > highestGroundY)
                {
                    highestGroundY = hit.point.y;
                }
            }
        }

        // Nếu không phát hiện nước tại vị trí này -> False
        if (highestWaterY == float.MinValue) return false;

        // Nếu mặt đất cao hơn mặt nước hơn 0.35m -> Mặt đất che phủ nước (nước ngầm dưới đất liền) -> False
        if (highestGroundY > highestWaterY + 0.35f)
        {
            return false;
        }

        // Mặt nước lộ thiên hợp lệ!
        return true;
    }

    /// <summary>
    /// Kiểm tra người chơi có đang đứng gần bờ hồ / mặt nước lộ thiên hợp lệ để câu cá hay không (cả 4 Map)
    /// Đã tối ưu hiệu năng: Sử dụng Spatial Movement Caching khi người chơi đứng yên
    /// </summary>
    public bool IsPlayerNearValidFishingWater()
    {
        // Tối ưu hiệu năng: Nếu nhân vật đứng yên hoặc di chuyển cực nhỏ (< 0.35m) và không quay người nhiều, dùng kết quả cache
        if ((transform.position - lastWaterCheckPos).sqrMagnitude < 0.12f &&
            Vector3.Angle(transform.forward, lastWaterCheckForward) < 15f &&
            Time.time - lastWaterCheckTime < 0.8f)
        {
            return lastWaterCheckResult;
        }

        lastWaterCheckPos = transform.position;
        lastWaterCheckForward = transform.forward;
        lastWaterCheckTime = Time.time;

        // 1. Kiểm tra ngay vị trí người chơi đang đứng
        if (IsOpenWaterAtXZ(transform.position.x, transform.position.z))
        {
            lastWaterCheckResult = true;
            return true;
        }

        // 2. Kiểm tra hướng nhìn phía trước mặt người chơi (0.8m, 1.6m, 2.5m, 3.5m)
        Vector3 forwardDir = transform.forward;
        forwardDir.y = 0f;
        if (forwardDir.sqrMagnitude > 0.01f) forwardDir.Normalize();

        for (float dist = 0.8f; dist <= 3.5f; dist += 0.9f)
        {
            Vector3 testPos = transform.position + forwardDir * dist;
            if (IsOpenWaterAtXZ(testPos.x, testPos.z))
            {
                lastWaterCheckResult = true;
                return true;
            }
        }

        // 3. Kiểm tra quét xung quanh 360 độ (bán kính 1.2m và 2.5m)
        float[] radii = { 1.2f, 2.5f };
        int angleSteps = 8;
        for (int i = 0; i < angleSteps; i++)
        {
            float rad = i * (Mathf.PI * 2f / angleSteps);
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
            foreach (float r in radii)
            {
                Vector3 testPos = transform.position + offset * r;
                if (IsOpenWaterAtXZ(testPos.x, testPos.z))
                {
                    lastWaterCheckResult = true;
                    return true;
                }
            }
        }

        lastWaterCheckResult = false;
        return false;
    }

    private void CheckAutoStowRodWhenLeavingFishingArea()
    {
        EnsureEquipmentSlots();
        if (hotbarSlot == null) return;
        InventoryItemUI equippedRod = hotbarSlot.GetEquippedItem();

        // Nếu người chơi không trang bị cần câu trong ô trang bị
        if (equippedRod == null)
        {
            wasNearWaterWithRod = false;
            dryLandStowTimer = 0f;
            if (currentRod != null && handVisual != null)
            {
                handVisual.ClearCurrentVisual();
                currentRod = null;
                if (playerAnimation != null) playerAnimation.SetHoldingItemState(false);
            }
            return;
        }

        // Nếu người chơi đang mở giao diện Balo / Menu UI thì giữ nguyên để người chơi thoải mái thao tác
        if (inputHandler != null && inputHandler.IsUIOpen)
        {
            dryLandStowTimer = 0f;
            return;
        }
        bool isBackpackPanelOpen = BackpackController.Instance != null && BackpackController.Instance.IsOpen;
        if (isBackpackPanelOpen)
        {
            dryLandStowTimer = 0f;
            return;
        }

        // Nếu đang trong tiến trình câu cá (quăng dây, giằng co, kéo cá) thì không cất
        if (currentState != FishingState.Idle)
        {
            dryLandStowTimer = 0f;
            return;
        }

        bool isNearWater = IsPlayerNearValidFishingWater();

        if (isNearWater)
        {
            // Người chơi đang ở gần bờ hồ / mặt nước -> Ghi nhận trạng thái đã tiếp cận vùng câu cá
            wasNearWaterWithRod = true;
            dryLandStowTimer = 0f;
        }
        else
        {
            // CHỈ tự động thu hồi cần câu nếu trước đó người chơi ĐÃ TỪNG ở gần mặt nước rồi sau đó RỜI KHỎI MẶT NƯỚC
            // (Người chơi tự trang bị cần câu ở trên đất liền thì giữ nguyên để người chơi cầm đi ra bờ hồ)
            if (wasNearWaterWithRod)
            {
                dryLandStowTimer += 0.2f;
                if (dryLandStowTimer >= 1.0f)
                {
                    dryLandStowTimer = 0f;
                    wasNearWaterWithRod = false;
                    AutoStowRodToBackpack(equippedRod);
                }
            }
            else
            {
                dryLandStowTimer = 0f;
            }
        }
    }

    public void AutoStowRodToBackpack(InventoryItemUI rodItem = null)
    {
        wasNearWaterWithRod = false;
        dryLandStowTimer = 0f;

        EnsureEquipmentSlots();
        if (hotbarSlot == null) return;
        if (rodItem == null) rodItem = hotbarSlot.GetEquippedItem();
        if (rodItem == null) return;

        bool stowed = false;
        if (BackpackMinigameUI.Instance != null)
        {
            stowed = BackpackMinigameUI.Instance.TryAutoFitItemToGrid(rodItem);
        }

        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        if (stowed)
        {
            hotbarSlot.RemoveEquippedItem();

            if (handVisual != null)
            {
                handVisual.ClearCurrentVisual();
            }
            currentRod = null;
            if (playerAnimation != null) playerAnimation.SetHoldingItemState(false);

            ShowFishingFeedback(
                isVietnamese ? "Đã rời khỏi vùng nước. Cần câu đã tự động cất vào Balo!" : "Left the fishing area. Fishing rod automatically stowed into backpack!",
                new Color(0.4f, 1f, 0.6f)
            );
            Debug.Log("<color=green>[FishingController] Đã tự động cất cần câu vào Balo khi rời khỏi vùng nước.</color>");
        }
        else
        {
            ShowFishingFeedback(
                isVietnamese ? "Balo đã đầy, không thể tự cất cần câu!" : "Backpack full, cannot auto-stow fishing rod!",
                new Color(1f, 0.45f, 0.45f)
            );
        }
    }
}