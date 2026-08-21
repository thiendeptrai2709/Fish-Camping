using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

public enum TutorialStage
{
    // ========================================================
    // MAP 1 (TOWN) - ATOMIC QUESTS
    // ========================================================
    Quest0_WelcomeGame,             // 0. Chào mừng người chơi
    Quest1_Movement,                // 1. Di chuyển WASD + Shift
    Quest1_1_TogglePerspective,     // 2. Bấm Y đổi góc nhìn FPP / TPP
    Quest1_2_FindOldTruck,          // 3. Tìm xe tải cũ
    Quest2_1_OpenTrunk,             // 3. Click mở cốp xe
    Quest2_2_CloseTrunk,            // 4. Bấm Tab đóng cốp xe
    Quest2_3_InspectCar,            // 5. Kiểm tra tình trạng xe
    Quest2_4_OpenHood,              // 6. Click mở nắp Capo
    Quest2_5_RepairEngine,          // 7. Sửa động cơ / châm nước làm mát
    Quest2_6_CloseHood,             // 8. Click đóng nắp Capo
    Quest3_1_OpenMap,               // 9. Bấm N mở bản đồ
    Quest3_2_ClickShopIcon,         // 10. Click icon Shop trên bản đồ
    Quest4_1_EnterVehicle,          // 11. Click cửa lên xe bán tải
    Quest4_2_DriveToShop,           // 12. Lái xe đến Shop đồ câu
    Quest4_3_ExitVehicle,           // 13. Bấm E xuống xe
    Quest5_1_OpenShopMenu,          // 14. Mở menu Shop đồ câu
    Quest5_2_CloseShopMenu,         // 15. Đóng menu Shop đồ câu
    Quest6_1_OpenMapUpgrade,        // 16. Bấm N mở bản đồ tìm NPC nâng cấp
    Quest6_2_OpenUpgradeMenu,       // 17. Mở menu nâng cấp xe (Gara)
    Quest6_3_CloseUpgradeMenu,      // 18. Đóng menu nâng cấp xe (Z/Đóng)
    Quest8_1_DriveToGasStation,     // 19. Lái xe đến Cây Xăng
    Quest8_2_TalkToGasNPC,          // 20. Nói chuyện với NPC đổ xăng
    Quest8_3_RefuelVehicle,         // 21. Bấm F nạp đầy xăng xe
    Quest8_4_BuyGasCanister,        // 22. Bấm F mua can xăng dự trữ
    Quest8_5_CheckFuelInTrunk,      // 23. Mở cốp kiểm tra can xăng
    Quest9_OpenTravelMap,           // 24. Bấm M mở Travel Map sang Pine Lake

    // ========================================================
    // MAP 2 (PINE LAKE) - ATOMIC QUESTS
    // ========================================================
    Map2_Quest1_1_OpenMapToCamp,    // 25. Bấm N mở map xem vị trí cắm trại
    Map2_Quest1_2_GoToCampSite,     // 26. Di chuyển đến điểm cắm trại
    Map2_Quest1_3_ExitVehicle,      // 27. Bấm E để xuống xe
    Map2_Quest2_1_OpenBackpack,     // 28. Bấm Tab mở Balo
    Map2_Quest2_2_EquipRod,         // 29. Lắp Cần câu vào ô Trang bị
    Map2_Quest2_3_EquipBaitAndBobber,// 30. Lắp Mồi câu & Phao câu
    Map2_Quest2_4_CloseBackpack,    // 31. Bấm Tab đóng Balo
    Map2_Quest3_WalkToLakeSide,     // 32. Đi ra ven bờ hồ
    Map2_Quest4_1_WindUpRod,        // 33. NV 1: Click chuột trái để vung cần
    Map2_Quest4_2_TimingPower,      // 34. NV 2: Căn lực vung cần (Màu đỏ, vàng, xanh, trắng)
    Map2_Quest4_3_ReelFish,         // 35. NV 3: Giữ nhả chuột để giật cá (khi cá cắn câu)
    Map2_Quest4_4_KeepOrReleaseFish,// 36. NV 4: Click chuột trái nhận cá hoặc Space thả cá
    Map2_Quest4_5_OpenBackpackAfterFish, // 37. Mở Balo kiểm tra cá vừa câu
    Map2_Quest5_0_GoToTentCampArea, // 38. Đi đến khu vực cắm trại gần lều
    Map2_Quest5_1_OpenBuildMenu,    // 39. Bấm B mở bảng công cụ
    Map2_Quest5_2_PlaceFirewood,    // 40. Đặt đống củi (B)
    Map2_Quest5_3_PlaceCookingRack, // 41. Đặt giá treo nấu ăn (B)
    Map2_Quest5_4_CookFish,         // 42. Nướng cá trên bếp
    Map2_Quest5_5_EatFish,          // 43. Ăn cá nướng
    Map2_Quest5_6_SleepInTent,      // 44. Ngủ trong lều trại
    Map2_Quest5_7_PlaceLamp,        // 45. Đặt đèn chiếu sáng (B)
    Map2_Quest6_BackToTown,         // 46. Lái xe về lại Thị trấn
    Map2_Quest7_FishLog,            // 47. Bấm J mở Sổ tay nhật ký cá
    Map2_Quest8_HelpGuide,          // 48. Bấm P mở Hướng dẫn phím
    Final_TalkToQuestNPC,           // 49. Đến gặp Cậu chủ làng (NPC Giao Nhiệm Vụ) nhận nhiệm vụ đầu tiên
    Completed                       // 50. Hoàn thành
}

public class ForcedTutorialManager : MonoBehaviour
{
    public static ForcedTutorialManager Instance { get; private set; }

    private const string TUTORIAL_SAVE_KEY = "Saved_TutorialStage";

    [Header("Localization Config")]
    [SerializeField] private string tableName = "Game Text";

    [Header("UI Hiển Thị")]
    [SerializeField] private GameObject questUIPanel;
    [SerializeField] private TextMeshProUGUI instructionTMP;
    [SerializeField] private TextMeshProUGUI progressTMP;
    [SerializeField] private Image completionIcon;
    [SerializeField] private GameObject completionBadge;

    [Header("Cấu Hình Hoàn Thành Nhiệm Vụ")]
    [Tooltip("Thời gian dừng lại hiển thị chữ HOÀN THÀNH trước khi qua nhiệm vụ tiếp theo (giây)")]
    [SerializeField] private float completionDisplayDuration = 1.25f;
    [SerializeField] private AudioClip completionSFX;

    [Header("Cấu Hình Hiệu Ứng Gõ Chữ")]
    [SerializeField] private float typingSpeed = 0.025f;

    [Header("Cấu Hình Âm Thanh Gõ Chữ (Audio SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSFX;
    [SerializeField] private AudioClip nextQuestSFX;
    [Range(0.05f, 1f)][SerializeField] private float typeSFXVolume = 0.4f;
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private int soundFrequency = 2;

    [Header("Cài Đặt Nhiệm Vụ")]
    [SerializeField] private float requiredWalkTime = 2.0f;
    [SerializeField] private Transform truckTransform;
    [SerializeField] private float truckDetectionRadius = 5.0f;
    [SerializeField] private Transform shopTransform;
    [SerializeField] private float shopDetectionRadius = 12.0f;
    [SerializeField] private Transform campTransform;
    [SerializeField] private float campDetectionRadius = 25.0f;
    [SerializeField] private float raycastMaxDistance = 15.0f;

    private float moveTimer = 0f;
    private bool hasShiftSprint = false;
    private Coroutine typingCoroutine;
    private Coroutine popCoroutine;
    private Coroutine completionCoroutine;
    private RectTransform panelRect;

    private bool isTyping = false;
    public bool IsTyping => isTyping;
    private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;
    private string currentInstructionText = "";

    public TutorialStage currentStage = TutorialStage.Quest0_WelcomeGame;

    // Performance Caching
    private Camera mainCamera;
    private Transform cachedPlayerTransform;
    private VehicleEnterExit cachedVehicleEnterExit;
    private VehicleController cachedVehicleController;
    private NPCFishingShop cachedFishingShop;
    private FishingZone[] cachedFishingZones;
    private WaitForSeconds cachedTypingWait;

    private static readonly RaycastHit[] tutorialHitBuffer = new RaycastHit[16];
    private static readonly Collider[] waterHitBuffer = new Collider[16];

    private Transform GetPlayerTransform()
    {
        if (cachedPlayerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) cachedPlayerTransform = p.transform;
        }
        return cachedPlayerTransform;
    }

    private VehicleEnterExit GetVehicleEnterExit()
    {
        if (cachedVehicleEnterExit == null)
        {
            cachedVehicleEnterExit = Object.FindFirstObjectByType<VehicleEnterExit>(FindObjectsInactive.Include);
        }
        return cachedVehicleEnterExit;
    }

    private VehicleController GetVehicleController()
    {
        if (cachedVehicleController == null)
        {
            cachedVehicleController = Object.FindFirstObjectByType<VehicleController>(FindObjectsInactive.Include);
        }
        return cachedVehicleController;
    }

    private NPCFishingShop GetFishingShop()
    {
        if (cachedFishingShop == null)
        {
            cachedFishingShop = Object.FindFirstObjectByType<NPCFishingShop>(FindObjectsInactive.Include);
        }
        return cachedFishingShop;
    }

    private FishingZone[] GetFishingZones()
    {
        if (cachedFishingZones == null || cachedFishingZones.Length == 0)
        {
            cachedFishingZones = Object.FindObjectsByType<FishingZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
        return cachedFishingZones;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (questUIPanel != null)
            panelRect = questUIPanel.GetComponent<RectTransform>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        cachedTypingWait = new WaitForSeconds(typingSpeed);
        mainCamera = Camera.main;

        LoadTutorialProgress();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(UnityEngine.Localization.Locale locale)
    {
        UpdateQuestUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cachedPlayerTransform = null;
        cachedVehicleEnterExit = null;
        cachedVehicleController = null;
        cachedFishingShop = null;
        cachedFishingZones = null;
        truckTransform = null;
        shopTransform = null;
        campTransform = null;
        mainCamera = Camera.main;

        if (questUIPanel == null || instructionTMP == null || progressTMP == null)
        {
            var foundPanel = GameObject.Find("QuestUIPanel");
            if (foundPanel != null)
            {
                questUIPanel = foundPanel;
                panelRect = questUIPanel.GetComponent<RectTransform>();

                TextMeshProUGUI[] tmps = questUIPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (tmps.Length >= 2)
                {
                    progressTMP = tmps[0];
                    instructionTMP = tmps[1];
                }
                else if (tmps.Length == 1)
                {
                    instructionTMP = tmps[0];
                }
            }
        }

        if (scene.name.Contains("Map_2_PineLake") || scene.name.Contains("PineLake"))
        {
            if (currentStage < TutorialStage.Map2_Quest1_1_OpenMapToCamp)
            {
                AdvanceToStage(TutorialStage.Map2_Quest1_1_OpenMapToCamp);
            }
        }
        else if (scene.name.Contains("Map_1_Town") || scene.name.Contains("Town") || scene.name.Contains("Map1"))
        {
            if (currentStage == TutorialStage.Map2_Quest6_BackToTown)
            {
                AdvanceToStage(TutorialStage.Map2_Quest7_FishLog);
            }
        }

        UpdateQuestUI();
    }

    private void Start() => UpdateQuestUI();

    private void Update()
    {
        if (currentStage == TutorialStage.Completed || isTransitioning) return;

        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool isLeftClick = ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) || Input.GetMouseButtonDown(0)) && !isPointerOverUI;

        // Xử lý Raycast 3D Click trực tiếp khi click chuột trái không đè lên UI
        if (isLeftClick)
        {
            Handle3DObjectClick();
        }

        if (Keyboard.current == null) return;

        // ========================================================
        // MAP 1 (TOWN) - ATOMIC INPUT CHECKS
        // ========================================================
        if (currentStage == TutorialStage.Quest0_WelcomeGame)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.wKey.isPressed ||
                Keyboard.current.sKey.isPressed ||
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.dKey.isPressed ||
                isLeftClick)
            {
                AdvanceToStage(TutorialStage.Quest1_Movement);
            }
        }
        else if (currentStage == TutorialStage.Quest1_Movement)
        {
            bool isMoving = Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed ||
                            Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed;

            if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
                hasShiftSprint = true;

            if (isMoving) moveTimer += Time.deltaTime;

            if (moveTimer >= requiredWalkTime && hasShiftSprint)
                AdvanceToStage(TutorialStage.Quest1_1_TogglePerspective);
        }
        else if (currentStage == TutorialStage.Quest1_1_TogglePerspective)
        {
            if (Keyboard.current.yKey.wasPressedThisFrame || Input.GetKeyDown(KeyCode.Y))
            {
                AdvanceToStage(TutorialStage.Quest1_2_FindOldTruck);
            }
        }
        else if (currentStage == TutorialStage.Quest1_2_FindOldTruck)
        {
            if (truckTransform == null)
            {
                var vehicle = GetVehicleEnterExit();
                if (vehicle != null) truckTransform = vehicle.transform;
            }

            if (truckTransform != null)
            {
                var player = GetPlayerTransform();
                if (player != null && Vector3.Distance(player.position, truckTransform.position) <= truckDetectionRadius)
                {
                    AdvanceToStage(TutorialStage.Quest2_1_OpenTrunk);
                }
            }
        }
        else if (currentStage == TutorialStage.Quest2_2_CloseTrunk)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest2_3_InspectCar);
        }
        else if (currentStage == TutorialStage.Quest3_1_OpenMap)
        {
            if (Keyboard.current.nKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest3_2_ClickShopIcon);
        }
        else if (currentStage == TutorialStage.Quest4_2_DriveToShop)
        {
            if (shopTransform == null)
            {
                var shop = GetFishingShop();
                if (shop != null) shopTransform = shop.transform;
            }

            if (shopTransform != null)
            {
                var vehicle = GetVehicleController();
                Transform targetTransform = vehicle != null ? vehicle.transform : null;

                if (targetTransform == null)
                {
                    var player = GetPlayerTransform();
                    if (player != null) targetTransform = player;
                }

                if (targetTransform != null && Vector3.Distance(targetTransform.position, shopTransform.position) <= shopDetectionRadius)
                {
                    AdvanceToStage(TutorialStage.Quest4_3_ExitVehicle);
                }
            }
        }
        else if (currentStage == TutorialStage.Quest4_3_ExitVehicle)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest5_1_OpenShopMenu);
        }
        else if (currentStage == TutorialStage.Quest5_2_CloseShopMenu)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest6_1_OpenMapUpgrade);
        }
        else if (currentStage == TutorialStage.Quest6_1_OpenMapUpgrade)
        {
            if (Keyboard.current.nKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest6_2_OpenUpgradeMenu);
        }
        else if (currentStage == TutorialStage.Quest6_3_CloseUpgradeMenu)
        {
            if (Keyboard.current.zKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest8_1_DriveToGasStation);
        }
        else if (currentStage == TutorialStage.Quest8_3_RefuelVehicle)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest8_2_TalkToGasNPC);
        }
        else if (currentStage == TutorialStage.Quest8_4_BuyGasCanister)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest8_5_CheckFuelInTrunk);
        }
        else if (currentStage == TutorialStage.Quest8_5_CheckFuelInTrunk)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest9_OpenTravelMap);
        }

        // ========================================================
        // MAP 2 (PINE LAKE) - ATOMIC INPUT CHECKS
        // ========================================================
        else if (currentStage == TutorialStage.Map2_Quest1_1_OpenMapToCamp)
        {
            if (Keyboard.current.nKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest1_2_GoToCampSite);
        }
        else if (currentStage == TutorialStage.Map2_Quest1_2_GoToCampSite)
        {
            Transform targetTransform = null;
            var vehicle = GetVehicleController();
            if (vehicle != null && vehicle.gameObject.activeInHierarchy) targetTransform = vehicle.transform;

            if (targetTransform == null)
            {
                var player = GetPlayerTransform();
                if (player != null) targetTransform = player;
            }

            if (targetTransform != null)
            {
                if (CampBuildZone.Instance != null)
                {
                    if (CampBuildZone.Instance.IsInsideBuildZone(targetTransform.position) ||
                        Vector3.Distance(targetTransform.position, CampBuildZone.Instance.transform.position) <= campDetectionRadius)
                    {
                        AdvanceToStage(TutorialStage.Map2_Quest1_3_ExitVehicle);
                    }
                }
                else
                {
                    if (campTransform == null)
                    {
                        var campObj = GameObject.Find("CampBuildArea") ?? GameObject.Find("campsite") ?? GameObject.FindGameObjectWithTag("Camp");
                        if (campObj != null) campTransform = campObj.transform;
                    }

                    if (campTransform != null && Vector3.Distance(targetTransform.position, campTransform.position) <= campDetectionRadius)
                    {
                        AdvanceToStage(TutorialStage.Map2_Quest1_3_ExitVehicle);
                    }
                }
            }
        }
        else if (currentStage == TutorialStage.Map2_Quest1_3_ExitVehicle)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest2_1_OpenBackpack);
        }
        else if (currentStage == TutorialStage.Map2_Quest2_1_OpenBackpack)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest2_2_EquipRod);
        }
        else if (currentStage == TutorialStage.Map2_Quest2_2_EquipRod)
        {
            EquipmentSlotUI[] allSlots = Object.FindObjectsByType<EquipmentSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var slot in allSlots)
            {
                if (slot != null && slot.GetSlotRequirement() == EquipmentSlotUI.SlotRequirement.OnlyFishingRod)
                {
                    if (slot.GetEquippedItem() != null)
                    {
                        AdvanceToStage(TutorialStage.Map2_Quest2_3_EquipBaitAndBobber);
                        break;
                    }
                }
            }
        }
        else if (currentStage == TutorialStage.Map2_Quest2_3_EquipBaitAndBobber)
        {
            EquipmentSlotUI[] allSlots = Object.FindObjectsByType<EquipmentSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool hasBaitOrBobber = false;
            foreach (var slot in allSlots)
            {
                if (slot != null && (slot.GetSlotRequirement() == EquipmentSlotUI.SlotRequirement.OnlyBait || slot.GetSlotRequirement() == EquipmentSlotUI.SlotRequirement.OnlyBobber))
                {
                    if (slot.GetEquippedItem() != null)
                    {
                        hasBaitOrBobber = true;
                        break;
                    }
                }
            }
            if (hasBaitOrBobber)
            {
                AdvanceToStage(TutorialStage.Map2_Quest2_4_CloseBackpack);
            }
        }
        else if (currentStage == TutorialStage.Map2_Quest2_4_CloseBackpack)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest3_WalkToLakeSide);
        }
        else if (currentStage == TutorialStage.Map2_Quest3_WalkToLakeSide)
        {
            var player = GetPlayerTransform();
            if (player != null)
            {
                bool isNearWater = false;
                int waterMask = LayerMask.GetMask("Water");
                if (waterMask == 0) waterMask = 1 << 4;

                // 1. Kiểm tra Raycast thẳng phía trước mặt người chơi (xuống dưới mép hồ)
                if (Physics.Raycast(player.position + Vector3.up * 1.5f + player.forward * 2.5f, Vector3.down, out RaycastHit hitForward, 4f, waterMask))
                {
                    isNearWater = true;
                }
                else if (Physics.Raycast(player.position + Vector3.up * 1.5f, Vector3.down, out RaycastHit hitDirect, 3f, waterMask))
                {
                    isNearWater = true;
                }

                // 2. Kiểm tra khoảng cách chính xác đến Collider của Hồ Câu Cá (FishingZone)
                if (!isNearWater)
                {
                    FishingZone[] fishingZones = GetFishingZones();
                    if (fishingZones != null)
                    {
                        for (int i = 0; i < fishingZones.Length; i++)
                        {
                            var fz = fishingZones[i];
                            if (fz == null) continue;
                            Collider col = fz.GetComponent<Collider>();
                            if (col != null)
                            {
                                Vector3 closest = col.ClosestPoint(player.position);
                                float dist = Vector3.Distance(player.position, closest);
                                float heightDiff = Mathf.Abs(player.position.y - closest.y);
                                if (dist <= 6.5f && heightDiff <= 3.5f)
                                {
                                    isNearWater = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (isNearWater)
                {
                    AdvanceToStage(TutorialStage.Map2_Quest4_1_WindUpRod);
                }
            }
        }
        else if (currentStage == TutorialStage.Map2_Quest4_1_WindUpRod)
        {
            if (isLeftClick)
                AdvanceToStage(TutorialStage.Map2_Quest4_2_TimingPower);
        }
        else if (currentStage == TutorialStage.Map2_Quest4_4_KeepOrReleaseFish)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || isLeftClick)
                AdvanceToStage(TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish);
        }
        else if (currentStage == TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest5_0_GoToTentCampArea);
        }
        else if (currentStage == TutorialStage.Map2_Quest5_0_GoToTentCampArea)
        {
            var player = GetPlayerTransform();
            if (player != null)
            {
                if (CampBuildZone.Instance != null)
                {
                    if (CampBuildZone.Instance.IsInsideBuildZone(player.position) ||
                        Vector3.Distance(player.position, CampBuildZone.Instance.transform.position) <= campDetectionRadius)
                    {
                        AdvanceToStage(TutorialStage.Map2_Quest5_1_OpenBuildMenu);
                    }
                }
                else if (campTransform != null && Vector3.Distance(player.position, campTransform.position) <= campDetectionRadius)
                {
                    AdvanceToStage(TutorialStage.Map2_Quest5_1_OpenBuildMenu);
                }
            }
        }
        else if (currentStage == TutorialStage.Map2_Quest5_1_OpenBuildMenu)
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest5_2_PlaceFirewood);
        }
        else if (currentStage == TutorialStage.Map2_Quest5_2_PlaceFirewood)
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest5_3_PlaceCookingRack);
        }
        else if (currentStage == TutorialStage.Map2_Quest5_3_PlaceCookingRack)
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest5_4_CookFish);
        }
        else if (currentStage == TutorialStage.Map2_Quest5_7_PlaceLamp)
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest6_BackToTown);
        }
        else if (currentStage == TutorialStage.Map2_Quest6_BackToTown)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName.Contains("Map_1_Town") || currentSceneName.Contains("Town") || currentSceneName.Contains("Map1"))
            {
                AdvanceToStage(TutorialStage.Map2_Quest7_FishLog);
            }
        }
        else if (currentStage == TutorialStage.Map2_Quest7_FishLog)
        {
            if (Keyboard.current.jKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Map2_Quest8_HelpGuide);
        }
        else if (currentStage == TutorialStage.Map2_Quest8_HelpGuide)
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Final_TalkToQuestNPC);
        }
    }

    // ========================================================
    // XỬ LÝ RAYCAST 3D CLICK TƯƠNG TÁC TRỰC TIẾP (Zero-Alloc)
    // ========================================================

    private void Handle3DObjectClick()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Ray ray;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        }
        else if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            ray = mainCamera.ScreenPointToRay(mousePos);
        }
        else
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        }

        int hitCount = Physics.RaycastNonAlloc(ray, tutorialHitBuffer, raycastMaxDistance);
        if (hitCount == 0) return;

        // In-place sort
        for (int i = 0; i < hitCount - 1; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < hitCount; j++)
            {
                if (tutorialHitBuffer[j].distance < tutorialHitBuffer[minIdx].distance) minIdx = j;
            }
            if (minIdx != i)
            {
                RaycastHit temp = tutorialHitBuffer[i];
                tutorialHitBuffer[i] = tutorialHitBuffer[minIdx];
                tutorialHitBuffer[minIdx] = temp;
            }
        }

        for (int i = 0; i < hitCount; i++)
        {
            var hit = tutorialHitBuffer[i];
            if (hit.collider == null) continue;
            GameObject hitObj = hit.collider.gameObject;
            string objName = hitObj.name.ToLower();

            // 1. Cốp xe (Click mở cốp)
            if (hit.collider.GetComponentInParent<InteractableTrunk>() != null ||
                objName.Contains("trunk") || objName.Contains("cop"))
            {
                if (currentStage == TutorialStage.Quest1_2_FindOldTruck || currentStage == TutorialStage.Quest2_1_OpenTrunk)
                {
                    NotifyTrunkOpened();
                    return;
                }
                else if (currentStage == TutorialStage.Quest8_5_CheckFuelInTrunk)
                {
                    NotifyCheckFuelInTrunk();
                    return;
                }
            }

            // 2. Nắp Capo (Click mở / đóng capo)
            if (hit.collider.GetComponentInParent<InteractableHood>() != null ||
                objName.Contains("hood") || objName.Contains("capo"))
            {
                if (currentStage == TutorialStage.Quest2_3_InspectCar || currentStage == TutorialStage.Quest2_4_OpenHood)
                {
                    NotifyHoodOpened();
                    return;
                }
                else if (currentStage == TutorialStage.Quest2_5_RepairEngine || currentStage == TutorialStage.Quest2_6_CloseHood)
                {
                    NotifyHoodClosed();
                    return;
                }
            }

            // 3. Động cơ xe (Click kiểm tra động cơ)
            if (hit.collider.GetComponentInParent<InteractableEngine>() != null ||
                objName.Contains("engine") || objName.Contains("dongco") || objName.Contains("motor"))
            {
                if (currentStage == TutorialStage.Quest2_4_OpenHood || currentStage == TutorialStage.Quest2_5_RepairEngine)
                {
                    NotifyEngineRepaired();
                    return;
                }
            }

            // 4. Cửa xe (Click lên xe)
            if (hit.collider.GetComponentInParent<CarDoor>() != null ||
                hit.collider.GetComponentInParent<VehicleEnterExit>() != null ||
                objName.Contains("door") || objName.Contains("cua_xe") || objName.Contains("seat"))
            {
                if (currentStage == TutorialStage.Quest4_1_EnterVehicle)
                {
                    NotifyEnteredVehicle();
                    return;
                }
            }

            // 5. Kiểm tra xe (Tình trạng xe)
            if (hit.collider.GetComponentInParent<InteractableVehicleStats>() != null ||
                hit.collider.GetComponentInParent<VehicleBody>() != null)
            {
                if (currentStage == TutorialStage.Quest2_3_InspectCar)
                {
                    NotifyInspectCar();
                    return;
                }
            }

            // 6. NPC Bán Đồ (Shop)
            if (hit.collider.GetComponentInParent<NPCFishingShop>() != null ||
                objName.Contains("shop") || objName.Contains("bando"))
            {
                if (currentStage == TutorialStage.Quest5_1_OpenShopMenu)
                {
                    NotifyShopOpened();
                    return;
                }
            }

            // 7. NPC Nâng Cấp (Garage)
            if (hit.collider.GetComponentInParent<NPCOffroadUpgrade>() != null ||
                objName.Contains("upgrade") || objName.Contains("garage") || objName.Contains("nangcap"))
            {
                if (currentStage == TutorialStage.Quest6_2_OpenUpgradeMenu)
                {
                    NotifyUpgradeMenuOpened();
                    return;
                }
            }

            // 8. NPC Giao Nhiệm Vụ (Quest Giver)
            if (hit.collider.GetComponentInParent<QuestGiver>() != null ||
                objName.Contains("questgiver") || objName.Contains("nhiemvu"))
            {
                if (currentStage == TutorialStage.Final_TalkToQuestNPC ||
                    currentStage == TutorialStage.Map2_Quest6_BackToTown ||
                    currentStage == TutorialStage.Map2_Quest7_FishLog ||
                    currentStage == TutorialStage.Map2_Quest8_HelpGuide)
                {
                    NotifyQuestNPCTalked();
                    return;
                }
            }

            // 9. NPC Cây Xăng / Người đổ xăng
            if (objName.Contains("gas_npc") || objName.Contains("doxang") || objName.Contains("fuel_npc"))
            {
                if (currentStage == TutorialStage.Quest8_2_TalkToGasNPC)
                {
                    NotifyGasNPCTalked();
                    return;
                }
            }

            // 9.5. Giá treo nấu ăn / Bếp lửa
            if (hit.collider.GetComponentInParent<CookingRack>() != null ||
                objName.Contains("cooking") || objName.Contains("rack") || objName.Contains("giatreo") || objName.Contains("bep"))
            {
                if (currentStage == TutorialStage.Map2_Quest5_3_PlaceCookingRack)
                {
                    NotifyPlaceCookingRack();
                    return;
                }
            }

            // 10. Giường / Lều trại (Ngủ trong lều)
            if (hit.collider.GetComponentInParent<InteractableBed>() != null ||
                objName.Contains("bed") || objName.Contains("tent") || objName.Contains("leu"))
            {
                if (currentStage == TutorialStage.Map2_Quest5_6_SleepInTent)
                {
                    NotifySleepInTent();
                    return;
                }
            }

            // 11. Con cá khi câu được
            if (objName.Contains("fish") || objName.Contains("ca_"))
            {
                if (currentStage == TutorialStage.Map2_Quest4_4_KeepOrReleaseFish)
                {
                    NotifyFishKeptOrReleased();
                    return;
                }
            }
        }
    }

    // ========================================================
    // NOTIFICATION HOOKS FOR GAMEPLAY SCRIPTS (MAP 1 & MAP 2)
    // ========================================================

    // 0. Bắt đầu game
    public void NotifyWelcomeCompleted()
    {
        if (currentStage == TutorialStage.Quest0_WelcomeGame)
            AdvanceToStage(TutorialStage.Quest1_Movement);
    }
    public void NotifyGameStarted() => NotifyWelcomeCompleted();

    // 1. Di chuyển
    public void NotifyMovementCompleted()
    {
        if (currentStage == TutorialStage.Quest1_Movement)
            AdvanceToStage(TutorialStage.Quest1_1_TogglePerspective);
    }

    // 1.1 Đổi góc nhìn
    public void NotifyPerspectiveToggled()
    {
        if (currentStage == TutorialStage.Quest1_1_TogglePerspective)
            AdvanceToStage(TutorialStage.Quest1_2_FindOldTruck);
    }

    // 2. Tìm xe tải
    public void NotifyFindOldTruck()
    {
        if (currentStage == TutorialStage.Quest1_2_FindOldTruck)
            AdvanceToStage(TutorialStage.Quest2_1_OpenTrunk);
    }
    public void NotifyApproachedTruck() => NotifyFindOldTruck();
    public void NotifyTruckFound() => NotifyFindOldTruck();

    // 3. Mở cốp xe
    public void NotifyTrunkOpened()
    {
        if (currentStage == TutorialStage.Quest1_2_FindOldTruck || currentStage == TutorialStage.Quest2_1_OpenTrunk)
            AdvanceToStage(TutorialStage.Quest2_2_CloseTrunk);
        else if (currentStage == TutorialStage.Quest8_5_CheckFuelInTrunk)
            AdvanceToStage(TutorialStage.Quest9_OpenTravelMap);
    }
    public void NotifyOpenTrunk() => NotifyTrunkOpened();

    // 4. Đóng cốp xe
    public void NotifyTrunkClosed()
    {
        if (currentStage == TutorialStage.Quest2_2_CloseTrunk)
            AdvanceToStage(TutorialStage.Quest2_3_InspectCar);
    }
    public void NotifyCloseTrunk() => NotifyTrunkClosed();

    // 5. Kiểm tra xe
    public void NotifyInspectCar()
    {
        if (currentStage == TutorialStage.Quest2_3_InspectCar)
            AdvanceToStage(TutorialStage.Quest2_4_OpenHood);
    }
    public void NotifyCarInspected() => NotifyInspectCar();

    // 6. Mở nắp Capo
    public void NotifyHoodOpened()
    {
        if (currentStage == TutorialStage.Quest2_3_InspectCar || currentStage == TutorialStage.Quest2_4_OpenHood)
            AdvanceToStage(TutorialStage.Quest2_5_RepairEngine);
    }
    public void NotifyOpenHood() => NotifyHoodOpened();

    // 7. Sửa động cơ
    public void NotifyEngineRepaired()
    {
        if (currentStage == TutorialStage.Quest2_4_OpenHood || currentStage == TutorialStage.Quest2_5_RepairEngine)
            AdvanceToStage(TutorialStage.Quest2_6_CloseHood);
    }
    public void NotifyCarRepaired() => NotifyEngineRepaired();
    public void NotifyRepairEngine() => NotifyEngineRepaired();

    // 8. Đóng nắp Capo
    public void NotifyHoodClosed()
    {
        if (currentStage == TutorialStage.Quest2_5_RepairEngine || currentStage == TutorialStage.Quest2_6_CloseHood)
            AdvanceToStage(TutorialStage.Quest3_1_OpenMap);
    }
    public void NotifyCloseHood() => NotifyHoodClosed();

    // 9. Mở bản đồ
    public void NotifyMapOpened()
    {
        if (currentStage == TutorialStage.Quest3_1_OpenMap)
            AdvanceToStage(TutorialStage.Quest3_2_ClickShopIcon);
        else if (currentStage == TutorialStage.Quest6_1_OpenMapUpgrade)
            AdvanceToStage(TutorialStage.Quest6_2_OpenUpgradeMenu);
        else if (currentStage == TutorialStage.Map2_Quest1_1_OpenMapToCamp)
            AdvanceToStage(TutorialStage.Map2_Quest1_2_GoToCampSite);
    }
    public void NotifyOpenMap() => NotifyMapOpened();

    // 10. Click icon Shop trên bản đồ
    public void NotifyShopIconClicked()
    {
        if (currentStage == TutorialStage.Quest3_2_ClickShopIcon)
            AdvanceToStage(TutorialStage.Quest4_1_EnterVehicle);
    }
    public void NotifyClickShopIcon() => NotifyShopIconClicked();

    // 11. Lên xe
    public void NotifyEnteredVehicle()
    {
        if (currentStage == TutorialStage.Quest4_1_EnterVehicle)
            AdvanceToStage(TutorialStage.Quest4_2_DriveToShop);
    }
    public void NotifyEnterVehicle() => NotifyEnteredVehicle();

    // 12. Lái xe đến shop
    public void NotifyDriveToShop()
    {
        if (currentStage == TutorialStage.Quest4_2_DriveToShop)
            AdvanceToStage(TutorialStage.Quest4_3_ExitVehicle);
    }
    public void NotifyReachedShop() => NotifyDriveToShop();

    // 13. Xuống xe
    public void NotifyExitedVehicle()
    {
        if (currentStage == TutorialStage.Quest4_2_DriveToShop || currentStage == TutorialStage.Quest4_3_ExitVehicle)
            AdvanceToStage(TutorialStage.Quest5_1_OpenShopMenu);
    }
    public void NotifyExitVehicle() => NotifyExitedVehicle();

    // 14. Mở Shop NPC
    public void NotifyShopOpened()
    {
        if (currentStage == TutorialStage.Quest5_1_OpenShopMenu)
            AdvanceToStage(TutorialStage.Quest5_2_CloseShopMenu);
    }
    public void NotifyShopInteracted() => NotifyShopOpened();
    public void NotifyReachedShopOrPurchased() => NotifyShopOpened();

    // 15. Đóng Shop NPC
    public void NotifyShopClosed()
    {
        if (currentStage == TutorialStage.Quest5_2_CloseShopMenu)
            AdvanceToStage(TutorialStage.Quest6_1_OpenMapUpgrade);
    }
    public void NotifyCloseShop() => NotifyShopClosed();

    // 16. Mở Map tìm NPC nâng cấp
    public void NotifyOpenMapUpgrade()
    {
        if (currentStage == TutorialStage.Quest6_1_OpenMapUpgrade)
            AdvanceToStage(TutorialStage.Quest6_2_OpenUpgradeMenu);
    }

    // 17. Mở menu Nâng cấp
    public void NotifyUpgradeMenuOpened()
    {
        if (currentStage == TutorialStage.Quest6_2_OpenUpgradeMenu)
            AdvanceToStage(TutorialStage.Quest6_3_CloseUpgradeMenu);
    }
    public void NotifyUpgradeNPCInteracted() => NotifyUpgradeMenuOpened();
    public void NotifyInteractUpgradeNPC() => NotifyUpgradeMenuOpened();

    // 18. Đóng menu Nâng cấp
    public void NotifyUpgradeMenuClosed()
    {
        if (currentStage == TutorialStage.Quest6_3_CloseUpgradeMenu)
            AdvanceToStage(TutorialStage.Quest8_1_DriveToGasStation);
    }
    public void NotifyGarageClosed() => NotifyUpgradeMenuClosed();
    public void NotifyVehicleUpgraded() => NotifyUpgradeMenuClosed();

    // 19. Nói chuyện NPC Nhiệm vụ (khi hoàn thành chuyến đi và nhận quest đầu tiên)
    public void NotifyQuestNPCTalked()
    {
        if (currentStage == TutorialStage.Final_TalkToQuestNPC ||
            currentStage == TutorialStage.Map2_Quest6_BackToTown ||
            currentStage == TutorialStage.Map2_Quest7_FishLog ||
            currentStage == TutorialStage.Map2_Quest8_HelpGuide)
        {
            AdvanceToStage(TutorialStage.Completed);
        }
    }
    public void NotifyFindQuestNPC() => NotifyQuestNPCTalked();
    public void NotifyQuestNPCInteracted() => NotifyQuestNPCTalked();

    // 20. Đến cây xăng
    public void NotifyGasStationFound()
    {
        if (currentStage == TutorialStage.Quest8_1_DriveToGasStation)
        {
            AdvanceToStage(TutorialStage.Quest8_3_RefuelVehicle);
        }
    }
    public void NotifyDriveToGasStation() => NotifyGasStationFound();
    public void NotifyFindGasStation() => NotifyGasStationFound();

    // 22. Nạp xăng xe
    public void NotifyRefueled()
    {
        if (currentStage == TutorialStage.Quest8_1_DriveToGasStation ||
            currentStage == TutorialStage.Quest8_3_RefuelVehicle)
        {
            AdvanceToStage(TutorialStage.Quest8_2_TalkToGasNPC);
        }
    }
    public void NotifyVehicleRefueled() => NotifyRefueled();

    // 21. Nói chuyện NPC đổ xăng
    public void NotifyGasNPCTalked()
    {
        if (currentStage == TutorialStage.Quest8_2_TalkToGasNPC)
        {
            AdvanceToStage(TutorialStage.Quest8_4_BuyGasCanister);
        }
    }
    public void NotifyTalkToGasNPC() => NotifyGasNPCTalked();

    // 23. Mua can xăng
    public void NotifyGasCanisterBought()
    {
        if (currentStage == TutorialStage.Quest8_4_BuyGasCanister)
            AdvanceToStage(TutorialStage.Quest8_5_CheckFuelInTrunk);
    }
    public void NotifyBuyGasCanister() => NotifyGasCanisterBought();
    public void NotifyBuyGas() => NotifyGasCanisterBought();

    // 24. Kiểm tra xăng trong cốp
    public void NotifyCheckFuelInTrunk()
    {
        if (currentStage == TutorialStage.Quest8_5_CheckFuelInTrunk)
            AdvanceToStage(TutorialStage.Quest9_OpenTravelMap);
    }
    public void NotifyFuelInTrunkChecked() => NotifyCheckFuelInTrunk();

    // 25. Mở Travel Map
    public void NotifyOpenTravelMap()
    {
        // Chờ chuyển Scene sang Pine Lake thành công mới chuyển sang Map 2 trong OnSceneLoaded
    }
    public void NotifyTravelMapOpened() => NotifyOpenTravelMap();
    public void NotifyTraveledToPineLake() => NotifyOpenTravelMap();

    // 26. Map 2: Mở map cắm trại
    public void NotifyOpenMapToCamp()
    {
        if (currentStage == TutorialStage.Map2_Quest1_1_OpenMapToCamp)
            AdvanceToStage(TutorialStage.Map2_Quest1_2_GoToCampSite);
    }

    // 27. Map 2: Đến điểm cắm trại
    public void NotifyGoToCampSite()
    {
        if (currentStage == TutorialStage.Map2_Quest1_2_GoToCampSite)
            AdvanceToStage(TutorialStage.Map2_Quest1_3_ExitVehicle);
    }
    public void NotifyReachedCampSite() => NotifyGoToCampSite();

    // 28. Map 2: Mở Balo
    public void NotifyBackpackOpened()
    {
        if (currentStage == TutorialStage.Map2_Quest2_1_OpenBackpack)
            AdvanceToStage(TutorialStage.Map2_Quest2_2_EquipRod);
        else if (currentStage == TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish)
            AdvanceToStage(TutorialStage.Map2_Quest5_0_GoToTentCampArea);
    }
    public void NotifyOpenBackpack() => NotifyBackpackOpened();

    // 29. Map 2: Lắp cần câu
    public void NotifyRodEquipped()
    {
        if (currentStage == TutorialStage.Map2_Quest2_2_EquipRod)
            AdvanceToStage(TutorialStage.Map2_Quest2_3_EquipBaitAndBobber);
    }
    public void NotifyEquipRod() => NotifyRodEquipped();

    // 30. Map 2: Lắp mồi & phao
    public void NotifyBaitAndBobberEquipped()
    {
        if (currentStage == TutorialStage.Map2_Quest2_3_EquipBaitAndBobber)
            AdvanceToStage(TutorialStage.Map2_Quest2_4_CloseBackpack);
    }
    public void NotifyEquipBaitAndBobber() => NotifyBaitAndBobberEquipped();
    public void NotifyEquipFishingItems() => NotifyBaitAndBobberEquipped();
    public void NotifyFishingItemsEquipped() => NotifyBaitAndBobberEquipped();

    // 31. Map 2: Đóng Balo
    public void NotifyBackpackClosed()
    {
        if (currentStage == TutorialStage.Map2_Quest2_4_CloseBackpack)
            AdvanceToStage(TutorialStage.Map2_Quest3_WalkToLakeSide);
        else if (currentStage == TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish)
            AdvanceToStage(TutorialStage.Map2_Quest5_0_GoToTentCampArea);
    }
    public void NotifyCloseBackpack() => NotifyBackpackClosed();

    // 32. Map 2: Đi ra bờ hồ
    public void NotifyWalkToLakeSide()
    {
        if (currentStage == TutorialStage.Map2_Quest3_WalkToLakeSide)
            AdvanceToStage(TutorialStage.Map2_Quest4_1_WindUpRod);
    }
    public void NotifyReachedLakeSide() => NotifyWalkToLakeSide();
    public void NotifyCanFishAtLake() => NotifyWalkToLakeSide();
    public void NotifyReadyToFish() => NotifyWalkToLakeSide();

    // 33. Map 2: NV 1 - Vung cần
    public void NotifyWindUpRod()
    {
        if (currentStage == TutorialStage.Map2_Quest4_1_WindUpRod)
            AdvanceToStage(TutorialStage.Map2_Quest4_2_TimingPower);
    }
    public void NotifyStartWindUp() => NotifyWindUpRod();
    public void NotifyCastRod() => NotifyWindUpRod();

    // 34. Map 2: NV 2 - Căn lực quăng cần
    public void NotifyRodCasted()
    {
        if (currentStage == TutorialStage.Map2_Quest4_2_TimingPower)
        {
            // Cần đã quăng xuống nước, chờ cá cắn câu
        }
    }
    public void NotifyCastPowerSelected() => NotifyRodCasted();

    // 35. Map 2: NV 3 - Cá cắn câu -> Hiện hướng dẫn giật cá
    public void NotifyFishBiting()
    {
        if (currentStage == TutorialStage.Map2_Quest4_2_TimingPower || currentStage == TutorialStage.Map2_Quest4_1_WindUpRod)
            AdvanceToStage(TutorialStage.Map2_Quest4_3_ReelFish);
    }
    public void NotifyReelFish() => NotifyFishBiting();

    // 36. Map 2: NV 4 - Kéo cá thành công -> Hiện hướng dẫn Nhận hoặc Thả cá
    public void NotifyFishCaught()
    {
        if (currentStage == TutorialStage.Map2_Quest4_3_ReelFish)
            AdvanceToStage(TutorialStage.Map2_Quest4_4_KeepOrReleaseFish);
    }
    public void NotifyFishReeled() => NotifyFishCaught();
    public void NotifyFishingCompleted() => NotifyFishCaught();
    public void NotifyFishingGuide() => NotifyFishCaught();

    // 37. Map 2: Nhận cá (Chuột trái) hoặc Thả cá (Space)
    public void NotifyFishKeptOrReleased()
    {
        if (currentStage == TutorialStage.Map2_Quest4_4_KeepOrReleaseFish)
            AdvanceToStage(TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish);
    }
    public void NotifyKeepOrReleaseFish() => NotifyFishKeptOrReleased();

    // 38. Map 2: Mở Balo kiểm tra cá
    public void NotifyOpenBackpackAfterFish()
    {
        if (currentStage == TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish)
            AdvanceToStage(TutorialStage.Map2_Quest5_0_GoToTentCampArea);
    }

    // 37. Map 2: Về khu vực lều cắm trại
    public void NotifyGoToTentCampArea()
    {
        if (currentStage == TutorialStage.Map2_Quest5_0_GoToTentCampArea)
            AdvanceToStage(TutorialStage.Map2_Quest5_1_OpenBuildMenu);
    }

    // 38. Map 2: Mở menu công cụ B
    public void NotifyOpenBuildMenu()
    {
        if (currentStage == TutorialStage.Map2_Quest5_1_OpenBuildMenu)
            AdvanceToStage(TutorialStage.Map2_Quest5_2_PlaceFirewood);
    }

    // 39. Map 2: Đặt củi
    public void NotifyPlaceFirewood()
    {
        if (currentStage == TutorialStage.Map2_Quest5_2_PlaceFirewood)
            AdvanceToStage(TutorialStage.Map2_Quest5_3_PlaceCookingRack);
    }
    public void NotifyFirewoodPlaced() => NotifyPlaceFirewood();

    // 40. Map 2: Đặt giá treo
    public void NotifyPlaceCookingRack()
    {
        if (currentStage == TutorialStage.Map2_Quest5_3_PlaceCookingRack)
            AdvanceToStage(TutorialStage.Map2_Quest5_4_CookFish);
    }
    public void NotifyCookingRackPlaced() => NotifyPlaceCookingRack();

    // 41. Map 2: Nướng cá
    public void NotifyCookFish()
    {
        if (currentStage == TutorialStage.Map2_Quest5_4_CookFish)
            AdvanceToStage(TutorialStage.Map2_Quest5_5_EatFish);
    }
    public void NotifyFishCooked() => NotifyCookFish();

    // 42. Map 2: Ăn cá
    public void NotifyEatFish()
    {
        if (currentStage == TutorialStage.Map2_Quest5_5_EatFish)
            AdvanceToStage(TutorialStage.Map2_Quest5_6_SleepInTent);
    }
    public void NotifyFishEaten() => NotifyEatFish();

    // 43. Map 2: Ngủ trong lều
    public void NotifySleepInTent()
    {
        if (currentStage == TutorialStage.Map2_Quest5_6_SleepInTent)
            AdvanceToStage(TutorialStage.Map2_Quest5_7_PlaceLamp);
    }
    public void NotifySleptInTent() => NotifySleepInTent();

    // 44. Map 2: Đặt đèn
    public void NotifyPlaceLamp()
    {
        if (currentStage == TutorialStage.Map2_Quest5_7_PlaceLamp)
            AdvanceToStage(TutorialStage.Map2_Quest6_BackToTown);
    }
    public void NotifyLampPlaced() => NotifyPlaceLamp();

    // 44. Map 2: Về thị trấn
    public void NotifyBackToTown()
    {
        if (currentStage == TutorialStage.Map2_Quest6_BackToTown)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName.Contains("Map_1_Town") || currentSceneName.Contains("Town") || currentSceneName.Contains("Map1"))
            {
                AdvanceToStage(TutorialStage.Map2_Quest7_FishLog);
            }
        }
    }
    public void NotifyReturnedToTown() => NotifyBackToTown();

    // 45. Map 2: Sổ tay cá
    public void NotifyFishLog()
    {
        if (currentStage == TutorialStage.Map2_Quest7_FishLog)
            AdvanceToStage(TutorialStage.Map2_Quest8_HelpGuide);
    }
    public void NotifyFishLogOpened() => NotifyFishLog();

    // 46. Map 2: Hướng dẫn phím
    public void NotifyHelpGuide()
    {
        if (currentStage == TutorialStage.Map2_Quest8_HelpGuide)
            AdvanceToStage(TutorialStage.Final_TalkToQuestNPC);
    }
    public void NotifyHelpGuideOpened() => NotifyHelpGuide();

    // ========================================================
    // CORE STAGE MANAGEMENT & PROGRESS
    // ========================================================

    private float lastAdvanceTime = 0f;
    private const float ADVANCE_COOLDOWN = 0.2f;

    public TutorialStage GetCurrentStage() => currentStage;
    public bool IsTutorialCompleted => currentStage == TutorialStage.Completed;

    public bool CanEnterVehicle()
    {
        if (currentStage == TutorialStage.Completed) return true;

        // Chỉ khóa lên xe ở phần đầu game trước khi hoàn thành mở nắp Capo (Quest 1.2 -> 2.4)
        if (currentStage < TutorialStage.Quest2_5_RepairEngine)
        {
            return false;
        }

        // Toàn bộ các giai đoạn khác (Map 1 & Map 2) -> ĐẢM BẢO 100% CỬA XE LUÔN MỞ ĐỂ VÀO XE!
        return true;
    }

    public bool CanOpenBackpack()
    {
        if (currentStage == TutorialStage.Completed) return true;

        // Cho phép đóng/mở khi đang mở Cốp xe ở Map 1
        if (TrunkInventory.CurrentOpenTrunk != null)
        {
            return true;
        }

        // Cho phép đóng/mở khi đang tương tác Nấu ăn
        if (CookingUIManager.Instance != null && CookingUIManager.Instance.IsOpen())
        {
            return true;
        }

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        // Map 1: Khóa mở Balo tự do, chỉ mở khi tương tác Cốp xe
        if (sceneName.Contains("Map_1_Town") || sceneName.Contains("Town") || sceneName.Contains("Map1"))
        {
            return false;
        }

        // Map 3, Map 4 & các map cắm trại khác: Luôn mở Balo 100%
        if (sceneName.Contains("Map3") || sceneName.Contains("Swamp") || sceneName.Contains("Map4") || sceneName.Contains("Ocean"))
        {
            return true;
        }

        // Map 2 & các map khác: Mở từ Quest2_1 trở đi
        return currentStage >= TutorialStage.Map2_Quest2_1_OpenBackpack;
    }

    public bool CanOpenBuildMenu()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        // Khóa Menu xây dựng ở Map 1 (Town)
        if (sceneName.Contains("Map_1_Town") || sceneName.Contains("Town") || sceneName.Contains("Map1"))
        {
            return false;
        }

        if (currentStage == TutorialStage.Completed) return true;

        // Map 3, Map 4: Luôn mở Menu xây dựng 100%
        if (sceneName.Contains("Map3") || sceneName.Contains("Swamp") || sceneName.Contains("Map4") || sceneName.Contains("Ocean"))
        {
            return true;
        }

        // Chỉ cho phép mở khi tới nhiệm vụ xây dựng trại cắm ở Map 2 (Quest5_1 trở đi)
        return currentStage >= TutorialStage.Map2_Quest5_1_OpenBuildMenu;
    }

    public bool CanOpenJournal()
    {
        if (currentStage == TutorialStage.Completed) return true;

        // Mở từ Map2_Quest7_FishLog trở đi
        return currentStage >= TutorialStage.Map2_Quest7_FishLog;
    }

    public bool CanOpenMap()
    {
        if (currentStage == TutorialStage.Completed) return true;

        // Map 1: Cho phép ở Quest 3.1, 3.2, 6.1, 6.2, 8.1, 9
        if (currentStage >= TutorialStage.Quest3_1_OpenMap && currentStage <= TutorialStage.Quest3_2_ClickShopIcon) return true;
        if (currentStage >= TutorialStage.Quest6_1_OpenMapUpgrade && currentStage <= TutorialStage.Quest6_2_OpenUpgradeMenu) return true;
        if (currentStage >= TutorialStage.Quest8_1_DriveToGasStation) return true;

        // Map 2: Cho phép từ Map2_Quest1_1 trở đi
        if (currentStage >= TutorialStage.Map2_Quest1_1_OpenMapToCamp) return true;

        return false;
    }

    public bool CanStartFishing()
    {
        if (currentStage == TutorialStage.Completed) return true;

        // Chỉ cho phép câu cá tại bờ hồ Map 2 từ bước vung cần (Map2_Quest4_1) trở đi
        return currentStage >= TutorialStage.Map2_Quest4_1_WindUpRod;
    }

    public bool CanTravelToMap2()
    {
        if (currentStage == TutorialStage.Completed) return true;

        // Chỉ cho phép đi sang Map 2 khi đã hoàn thành toàn bộ chuỗi nhiệm vụ ở Map 1 (từ Quest9_OpenTravelMap trở đi)
        return currentStage >= TutorialStage.Quest9_OpenTravelMap;
    }

    public bool CanInteractWith(IInteractable interactable)
    {
        if (currentStage == TutorialStage.Completed) return true;
        if (interactable == null) return false;

        // ========================================================
        // 1. TƯƠNG TÁC XE (Cốp, Capo, Động cơ, Thân xe, Cửa, Lốp)
        // ========================================================
        if (interactable is InteractableTrunk ||
            interactable is VehicleBody ||
            interactable is InteractableVehicleStats ||
            interactable is InteractableHood ||
            interactable is InteractableEngine ||
            interactable is InteractableTire ||
            interactable is CarDoor)
        {
            // Nếu chưa hoàn thành mở nắp Capo (từ Quest 1.2 -> Quest 2.4): Giữ đúng từng bước để hướng dẫn
            if (currentStage < TutorialStage.Quest2_5_RepairEngine)
            {
                if (interactable is InteractableTrunk)
                {
                    return currentStage == TutorialStage.Quest1_2_FindOldTruck ||
                           currentStage == TutorialStage.Quest2_1_OpenTrunk ||
                           currentStage == TutorialStage.Quest2_2_CloseTrunk;
                }
                if (interactable is VehicleBody || interactable is InteractableVehicleStats)
                {
                    return currentStage == TutorialStage.Quest2_3_InspectCar;
                }
                if (interactable is InteractableHood)
                {
                    return currentStage == TutorialStage.Quest2_4_OpenHood;
                }
                if (interactable is CarDoor)
                {
                    return CanEnterVehicle();
                }
                return false;
            }

            // TỪ BƯỚC MỞ NẮP CAPO XONG (Quest2_5 trở đi) -> MỞ HẾT 100% TƯƠNG TÁC XE!
            if (interactable is CarDoor)
            {
                return CanEnterVehicle();
            }
            return true;
        }

        // Tương tác NPC
        if (interactable is NPCBase npc)
        {
            // 1. NPC Shop Đồ Câu: Cho phép từ lúc lái xe đến shop / xuống xe / mở shop trở đi
            if (npc.IsFishingShop)
            {
                return currentStage >= TutorialStage.Quest4_2_DriveToShop;
            }

            // 2. NPC Gara Nâng Cấp Xe: Cho phép từ lúc mở map tìm gara trở đi
            if (npc.IsTireUpgrader)
            {
                return currentStage >= TutorialStage.Quest6_1_OpenMapUpgrade;
            }

            // 3. NPC Giao Nhiệm Vụ: Cho phép khi đến bước nhận nhiệm vụ đầu tiên hoặc sau khi hoàn thành tutorial
            if (npc.IsQuestGiver)
            {
                return currentStage >= TutorialStage.Final_TalkToQuestNPC || currentStage == TutorialStage.Completed;
            }

            // 4. Các NPC Cây Xăng / Dân làng khác
            return currentStage >= TutorialStage.Quest8_1_DriveToGasStation;
        }

        string currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isTownMap1 = currentActiveScene.Contains("Map_1_Town") || currentActiveScene.Contains("Town") || currentActiveScene.Contains("Map1");
        bool isOtherCampingMap = currentActiveScene.Contains("Map3") || currentActiveScene.Contains("Swamp") || 
                                 currentActiveScene.Contains("Map4") || currentActiveScene.Contains("Ocean");

        // Map 3, Map 4: Luôn cho phép mọi tương tác tự do 100%
        if (isOtherCampingMap) return true;

        // 1. Giá Treo Nấu Ăn (CookingRack)
        if (interactable is CookingRack)
        {
            // Khóa tuyệt đối ở Map 1 (Town)
            if (isTownMap1) return false;

            // Ở Map 2: Mở từ Quest5_4 trở đi và VĨNH VIỄN MỞ kể cả khi đổi scene qua lại!
            return currentStage >= TutorialStage.Map2_Quest5_4_CookFish || currentStage == TutorialStage.Completed;
        }

        // 2. Lều Ngủ (Bed)
        if (interactable is InteractableBed)
        {
            if (isTownMap1) return false;
            return currentStage >= TutorialStage.Map2_Quest5_6_SleepInTent || currentStage == TutorialStage.Completed;
        }

        // Các vật thể tự do khác ở Map 2
        return currentStage >= TutorialStage.Map2_Quest1_1_OpenMapToCamp;
    }

    public void AdvanceToStage(TutorialStage nextStage)
    {
        if (currentStage == nextStage) return;
        if (currentStage == TutorialStage.Completed) return;
        if (isTransitioning) return;

        // Chống spam phím / double-triggering liên tục nhiều stage trong 1 frame
        if (Time.unscaledTime - lastAdvanceTime < ADVANCE_COOLDOWN)
        {
            return;
        }
        lastAdvanceTime = Time.unscaledTime;

        if (completionCoroutine != null) StopCoroutine(completionCoroutine);
        completionCoroutine = StartCoroutine(CompleteAndAdvanceRoutine(nextStage));
    }

    private IEnumerator CompleteAndAdvanceRoutine(TutorialStage nextStage)
    {
        isTransitioning = true;

        // 1. Dừng hiệu ứng gõ chữ cũ nếu đang chạy
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // 2. Phát âm thanh chúc mừng / hoàn thành
        AudioClip clipToPlay = completionSFX != null ? completionSFX : nextQuestSFX;
        if (audioSource != null && clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }

        // 3. Hiển thị chữ và Icon Hoàn Thành nổi bật (dấu v)
        bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        string completedTitle = GetLocalizedText("tut_quest_completed_title", isVietnamese ? "HOÀN THÀNH!" : "COMPLETED!");
        string completedSub = GetLocalizedText("tut_quest_completed_sub", isVietnamese ? "Đang chuyển tiếp nhiệm vụ mới..." : "Transitioning to next stage...");

        if (instructionTMP != null)
        {
            instructionTMP.maxVisibleCharacters = 99999;
            instructionTMP.text = $"<color=#00FF7F><b><size=135%><color=#39FF14>v</color> {completedTitle}</b></size></color>\n<size=80%><color=#A8E6CF><i>{completedSub}</i></color></size>";
        }

        if (progressTMP != null)
        {
            progressTMP.text = $"<color=#00FF7F><b><color=#39FF14>v</color> {completedTitle}</b></color>";
        }

        if (completionIcon != null) completionIcon.gameObject.SetActive(true);
        if (completionBadge != null) completionBadge.SetActive(true);

        // Hiệu ứng nảy Pop ăn mừng
        TriggerCompletionPopEffect();

        // 4. Giữ nguyên hiệu ứng hoàn thành để người chơi kịp nhìn thấy
        yield return new WaitForSeconds(completionDisplayDuration);

        // 5. Tắt icon/badge hoàn thành (nếu có)
        if (completionIcon != null) completionIcon.gameObject.SetActive(false);
        if (completionBadge != null) completionBadge.SetActive(false);

        // 6. Chuyển sang Stage tiếp theo
        moveTimer = 0f;
        hasShiftSprint = false;
        currentStage = nextStage;
        SaveTutorialProgress();

        if (audioSource != null && nextQuestSFX != null && completionSFX != null)
        {
            audioSource.PlayOneShot(nextQuestSFX);
        }

        TriggerPopEffect();
        UpdateQuestUI();

        isTransitioning = false;
        completionCoroutine = null;
    }

    private void SaveTutorialProgress()
    {
        PlayerPrefs.SetInt(TUTORIAL_SAVE_KEY, (int)currentStage);
        PlayerPrefs.Save();
    }

    private void LoadTutorialProgress()
    {
        if (PlayerPrefs.HasKey(TUTORIAL_SAVE_KEY))
        {
            int savedIndex = PlayerPrefs.GetInt(TUTORIAL_SAVE_KEY);
            currentStage = (TutorialStage)savedIndex;
        }
    }

    [ContextMenu("Reset Tutorial Progress")]
    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_SAVE_KEY);
        PlayerPrefs.DeleteKey("QuestSystem_Unlocked");
        PlayerPrefs.Save();
        currentStage = TutorialStage.Quest0_WelcomeGame;
        moveTimer = 0f;
        hasShiftSprint = false;
        UpdateQuestUI();
    }

    private void UpdateQuestUI()
    {
        if (currentStage == TutorialStage.Completed)
        {
            if (questUIPanel != null) questUIPanel.SetActive(false);
            return;
        }

        if (questUIPanel != null) questUIPanel.SetActive(true);
        if (instructionTMP == null) return;

        // Dịch tiêu đề "HƯỚNG DẪN"
        string guideTitle = GetLocalizedText("tut_guide_header", "HƯỚNG DẪN");

        if (progressTMP != null)
        {
            if (currentStage < TutorialStage.Map2_Quest1_1_OpenMapToCamp)
            {
                int currentMap1 = (int)currentStage + 1;
                int totalMap1 = (int)TutorialStage.Map2_Quest1_1_OpenMapToCamp;
                progressTMP.text = $"{guideTitle} ({currentMap1}/{totalMap1})";
            }
            else
            {
                int currentMap2 = (int)currentStage - (int)TutorialStage.Map2_Quest1_1_OpenMapToCamp + 1;
                int totalMap2 = (int)TutorialStage.Completed - (int)TutorialStage.Map2_Quest1_1_OpenMapToCamp;
                progressTMP.text = $"{guideTitle} ({currentMap2}/{totalMap2})";
            }
        }

        // Tự động gán Key theo từng Stage (có Fallback tiếng Việt nếu chưa nhập Key)
        switch (currentStage)
        {
            // === MAP 1 (TOWN) ===
            case TutorialStage.Quest0_WelcomeGame:
                currentInstructionText = GetLocalizedText("TUT_Quest0_WelcomeGame", "Chào mừng bạn đến với <color=#B388FF><b>Fish-Camping</b></color>! Nhấn phím bất kỳ để bắt đầu.");
                break;
            case TutorialStage.Quest1_Movement:
                currentInstructionText = GetLocalizedText("TUT_Quest1_Movement", "Dùng phím <color=#B388FF><b>W, A, S, D</b></color> để di chuyển và giữ <color=#B388FF><b>Shift</b></color> để chạy nhanh.");
                break;
            case TutorialStage.Quest1_1_TogglePerspective:
                currentInstructionText = GetLocalizedText("TUT_Quest1_1_TogglePerspective", "Nhấn phím <color=#B388FF><b>Y</b></color> để đổi qua lại giữa góc nhìn thứ nhất (FPP) và thứ ba (TPP).");
                break;
            case TutorialStage.Quest1_2_FindOldTruck:
                currentInstructionText = GetLocalizedText("TUT_Quest1_2_FindOldTruck", "Hãy quan sát xung quanh và đi đến gần chiếc <color=#B388FF><b>Xe tải cũ</b></color> của bạn.");
                break;
            case TutorialStage.Quest2_1_OpenTrunk:
                currentInstructionText = GetLocalizedText("TUT_Quest2_1_OpenTrunk", "Nhấp <color=#B388FF><b>Chuột trái</b></color> vào Cốp xe để mở.");
                break;
            case TutorialStage.Quest2_2_CloseTrunk:
                currentInstructionText = GetLocalizedText("TUT_Quest2_2_CloseTrunk", "Nhấn phím <color=#B388FF><b>Tab</b></color> để đóng Cốp xe.");
                break;
            case TutorialStage.Quest2_3_InspectCar:
                currentInstructionText = GetLocalizedText("TUT_Quest2_3_InspectCar", "Nhấp <color=#B388FF><b>Chuột trái</b></color> vào thân xe để kiểm tra tình trạng xe, lùi ra xa để thoát.");
                break;
            case TutorialStage.Quest2_4_OpenHood:
                currentInstructionText = GetLocalizedText("TUT_Quest2_4_OpenHood", "Nhấp <color=#B388FF><b>Chuột trái</b></color> vào nắp Capo để mở.");
                break;
            case TutorialStage.Quest2_5_RepairEngine:
                currentInstructionText = GetLocalizedText("TUT_Quest2_5_RepairEngine", "Kiểm tra động cơ, sửa máy và châm nước làm mát cho xe.");
                break;
            case TutorialStage.Quest2_6_CloseHood:
                currentInstructionText = GetLocalizedText("TUT_Quest2_6_CloseHood", "Nhấp <color=#B388FF><b>Chuột trái</b></color> để đóng nắp Capo lại.");
                break;
            case TutorialStage.Quest3_1_OpenMap:
                currentInstructionText = GetLocalizedText("TUT_Quest3_1_OpenMap", "Nhấn phím <color=#B388FF><b>N</b></color> để mở Bản đồ.");
                break;
            case TutorialStage.Quest3_2_ClickShopIcon:
                currentInstructionText = GetLocalizedText("TUT_Quest3_2_ClickShopIcon", "Nhấp vào <color=#B388FF><b>Icon Shop Đồ Câu</b></color> trên bản đồ để định vị đường đi.");
                break;
            case TutorialStage.Quest4_1_EnterVehicle:
                currentInstructionText = GetLocalizedText("TUT_Quest4_1_EnterVehicle", "Đi đến cửa xe và nhấp <color=#B388FF><b>Chuột trái</b></color> để lên xe bán tải.");
                break;
            case TutorialStage.Quest4_2_DriveToShop:
                currentInstructionText = GetLocalizedText("TUT_Quest4_2_DriveToShop", "Lái xe đến chỗ <color=#B388FF><b>Anh Cần thủ</b></color> (Shop Đồ Câu) theo chỉ dẫn.\n(Radio: <color=#B388FF><b>L</b></color> Bật/Tắt | <color=#B388FF><b>K</b></color> Đổi bài | <color=#B388FF><b>[ ]</b></color> Âm lượng | <color=#B388FF><b>G</b></color> Đèn pha).");
                break;
            case TutorialStage.Quest4_3_ExitVehicle:
                currentInstructionText = GetLocalizedText("TUT_Quest4_3_ExitVehicle", "Nhấn phím <color=#B388FF><b>E</b></color> để xuống xe.");
                break;
            case TutorialStage.Quest5_1_OpenShopMenu:
                currentInstructionText = GetLocalizedText("TUT_Quest5_1_OpenShopMenu", "Đến gần <color=#B388FF><b>Anh Cần thủ</b></color> và nhấp <color=#B388FF><b>Chuột trái</b></color> để mở cửa hàng.");
                break;
            case TutorialStage.Quest5_2_CloseShopMenu:
                currentInstructionText = GetLocalizedText("TUT_Quest5_2_CloseShopMenu", "Nhấn phím <color=#B388FF><b>E</b></color> hoặc nút Đóng để thoát cửa hàng.");
                break;
            case TutorialStage.Quest6_1_OpenMapUpgrade:
                currentInstructionText = GetLocalizedText("TUT_Quest6_1_OpenMapUpgrade", "Nhấn phím <color=#B388FF><b>N</b></color> mở bản đồ để xem vị trí <color=#B388FF><b>Bác thợ máy</b></color>.");
                break;
            case TutorialStage.Quest6_2_OpenUpgradeMenu:
                currentInstructionText = GetLocalizedText("TUT_Quest6_2_OpenUpgradeMenu", "Tương tác với <color=#B388FF><b>Bác thợ máy</b></color> để mở menu nâng cấp xe.");
                break;
            case TutorialStage.Quest6_3_CloseUpgradeMenu:
                currentInstructionText = GetLocalizedText("TUT_Quest6_3_CloseUpgradeMenu", "Nhấn phím <color=#B388FF><b>Z</b></color> hoặc nút Đóng để thoát giao diện nâng cấp.");
                break;
            case TutorialStage.Quest8_1_DriveToGasStation:
                currentInstructionText = GetLocalizedText("TUT_Quest8_1_DriveToGasStation", "Lái xe tìm đến <color=#B388FF><b>Cây Xăng</b></color> của thị trấn.");
                break;
            case TutorialStage.Quest8_3_RefuelVehicle:
                currentInstructionText = GetLocalizedText("TUT_Quest8_3_RefuelVehicle", "Đứng gần trụ xăng và nhấn phím <color=#B388FF><b>F</b></color> để đổ xăng cho xe.");
                break;
            case TutorialStage.Quest8_2_TalkToGasNPC:
                currentInstructionText = GetLocalizedText("TUT_Quest8_2_TalkToGasNPC", "Xuống xe và đi đến <color=#B388FF><b>NPC Cây Xăng</b></color> để tương tác.");
                break;
            case TutorialStage.Quest8_4_BuyGasCanister:
                currentInstructionText = GetLocalizedText("TUT_Quest8_4_BuyGasCanister", "Đi đến gần cột xăng và nhấn phím <color=#B388FF><b>F</b></color> để mua can xăng dự trữ.");
                break;
            case TutorialStage.Quest8_5_CheckFuelInTrunk:
                currentInstructionText = GetLocalizedText("TUT_Quest8_5_CheckFuelInTrunk", "Mở <color=#B388FF><b>Cốp xe</b></color> (Tab hoặc click cốp) để kiểm tra can xăng dự trữ.");
                break;
            case TutorialStage.Quest9_OpenTravelMap:
                currentInstructionText = GetLocalizedText("TUT_Quest9_OpenTravelMap", "Nhấn phím <color=#B388FF><b>M</b></color> để mở bản đồ du lịch và chọn di chuyển tới <color=#B388FF><b>Pine Lake</b></color>.");
                break;

            // === MAP 2 (PINE LAKE) ===
            case TutorialStage.Map2_Quest1_1_OpenMapToCamp:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest1_1_OpenMapToCamp", "Nhấn phím <color=#B388FF><b>N</b></color> để mở Bản đồ xem vị trí cắm trại.");
                break;
            case TutorialStage.Map2_Quest1_2_GoToCampSite:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest1_2_GoToCampSite", "Di chuyển đến khu vực <color=#B388FF><b>Địa điểm cắm trại</b></color> ven hồ.");
                break;
            case TutorialStage.Map2_Quest1_3_ExitVehicle:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest1_3_ExitVehicle", "Nhấn phím <color=#B388FF><b>E</b></color> để xuống xe.");
                break;
            case TutorialStage.Map2_Quest2_1_OpenBackpack:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest2_1_OpenBackpack", "Nhấn phím <color=#B388FF><b>Tab</b></color> để mở Balo của bạn.");
                break;
            case TutorialStage.Map2_Quest2_2_EquipRod:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest2_2_EquipRod", "Kéo <color=#B388FF><b>Cần câu</b></color> vào ô Trang bị Cần câu.");
                break;
            case TutorialStage.Map2_Quest2_3_EquipBaitAndBobber:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest2_3_EquipBaitAndBobber", "Kéo <color=#B388FF><b>Mồi câu và Phao câu</b></color> vào các ô Trang bị tương ứng.");
                break;
            case TutorialStage.Map2_Quest2_4_CloseBackpack:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest2_4_CloseBackpack", "Nhấn phím <color=#B388FF><b>Tab</b></color> để đóng Balo lại.");
                break;
            case TutorialStage.Map2_Quest3_WalkToLakeSide:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest3_WalkToLakeSide", "Di chuyển ra ven <color=#B388FF><b>Bờ hồ</b></color> để chuẩn bị câu cá.");
                break;
            case TutorialStage.Map2_Quest4_1_WindUpRod:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest4_1_WindUpRod", "Click <color=#B388FF><b>Chuột Trái</b></color> để vung cần.");
                break;
            case TutorialStage.Map2_Quest4_2_TimingPower:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest4_2_TimingPower", "<color=#FF5252>Màu đỏ</color>: Vung xa, <color=#FFD700>Màu vàng</color>: Vung vừa, <color=#69F0AE>Màu xanh</color>: Vung gần, <color=#FFFFFF>Màu trắng</color>: Thất bại. Click chuột để căn lực!");
                break;
            case TutorialStage.Map2_Quest4_3_ReelFish:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest4_3_ReelFish", "Giữ / nhả <color=#B388FF><b>Chuột Trái</b></color> để căn lực kéo sao cho vạch xanh lọt vào vùng xanh lá.");
                break;
            case TutorialStage.Map2_Quest4_4_KeepOrReleaseFish:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest4_4_KeepOrReleaseFish", "Nhấn <color=#B388FF><b>Chuột Trái</b></color> để nhận cá vào Balo hoặc nhấn phím <color=#B388FF><b>Space</b></color> để thả cá đi.");
                break;
            case TutorialStage.Map2_Quest4_5_OpenBackpackAfterFish:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest4_5_OpenBackpackAfterFish", "Hãy kiểm tra Balo của mình bằng phím <color=#B388FF><b>Tab</b></color> để xem cá.");
                break;
            case TutorialStage.Map2_Quest5_0_GoToTentCampArea:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest5_0_GoToTentCampArea", "Di chuyển về khu vực <color=#B388FF><b>Cắm trại gần lều</b></color>.");
                break;
            case TutorialStage.Map2_Quest5_1_OpenBuildMenu:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest5_1_OpenBuildMenu", "Nhấn phím <color=#B388FF><b>B</b></color> để mở Bảng công cụ xây dựng.");
                break;
            case TutorialStage.Map2_Quest5_2_PlaceFirewood:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest5_2_PlaceFirewood", "Chọn vào <color=#B388FF><b>Đống củi</b></color> trong bảng công cụ và đặt xuống đất.");
                break;
            case TutorialStage.Map2_Quest5_3_PlaceCookingRack:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest5_3_PlaceCookingRack", "Chọn <color=#B388FF><b>Giá treo nấu ăn</b></color> trong bảng công cụ (B) và đặt lên trên đống củi để nấu ăn.");
                break;
            case TutorialStage.Map2_Quest5_4_CookFish:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest5_4_CookFish", "Tương tác với giá treo và kéo cá vào bếp lửa để nướng chín.");
                break;
            case TutorialStage.Map2_Quest5_5_EatFish:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest5_5_EatFish", "Lấy cá nướng chín trong balo và ăn để hồi phục sức lực.");
                break;
            case TutorialStage.Map2_Quest5_6_SleepInTent:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest5_6_SleepInTent", "Đi vào bên trong lều trại và nhấp chuột trái để ngủ hồi phục sức lực.");
                break;
            case TutorialStage.Map2_Quest5_7_PlaceLamp:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest5_7_PlaceLamp", "Nhấn phím <color=#B388FF><b>B</b></color> chọn <color=#B388FF><b>Chiếc đèn</b></color> đặt tại vị trí thích hợp (có thể Bật/Tắt).");
                break;
            case TutorialStage.Map2_Quest6_BackToTown:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest6_BackToTown", "Sau khi cắm trại, hãy lên xe và lái về lại <color=#B388FF><b>Thị trấn</b></color>.");
                break;
            case TutorialStage.Map2_Quest7_FishLog:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest7_FishLog", "Nhấn phím <color=#B388FF><b>J</b></color> để mở xem <color=#B388FF><b>Nhật ký các loài cá</b></color> bạn đã câu được.");
                break;
            case TutorialStage.Map2_Quest8_HelpGuide:
                currentInstructionText = GetLocalizedText("TUT_Map2_Quest8_HelpGuide", "Nhấn phím <color=#B388FF><b>P</b></color> để xem lại toàn bộ <color=#B388FF><b>Chỉ dẫn và phím bấm</b></color>.");
                break;
            case TutorialStage.Final_TalkToQuestNPC:
                currentInstructionText = GetLocalizedText("TUT_Final_TalkToQuestNPC", "Đến gặp <color=#B388FF><b>Cậu chủ làng (NPC Giao Nhiệm Vụ)</b></color> để nhận nhiệm vụ đầu tiên và hoàn tất hướng dẫn tân thủ.");
                break;
            case TutorialStage.Completed:
                break;
        }

        PlayTypewriterEffect(currentInstructionText);
    }

    private string GetLocalizedText(string key, string fallbackVietnameseText)
    {
        if (string.IsNullOrEmpty(key)) return fallbackVietnameseText;

        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (table != null)
            {
                var entry = table.GetEntry(key);
                if (entry != null) return entry.GetLocalizedString();
            }
        }
        catch { }

        bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        if (isVietnamese) return fallbackVietnameseText;

        // English smart fallback for all tutorial stages
        switch (key)
        {
            case "tut_quest_completed_title": return "COMPLETED!";
            case "tut_quest_completed_sub": return "Transitioning to next stage...";
            case "tut_guide_header": return "TUTORIAL";

            case "TUT_Quest0_WelcomeGame":
                return "Welcome to <color=#B388FF><b>Fish-Camping</b></color>! Press any key to begin.";
            case "TUT_Quest1_Movement":
                return "Use <color=#B388FF><b>W, A, S, D</b></color> to move and hold <color=#B388FF><b>Shift</b></color> to sprint.";
            case "TUT_Quest1_1_TogglePerspective":
                return "Press <color=#B388FF><b>Y</b></color> to toggle between First Person (FPP) and Third Person (TPP).";
            case "TUT_Quest1_2_FindOldTruck":
                return "Look around and walk towards your <color=#B388FF><b>Old Truck</b></color>.";
            case "TUT_Quest2_1_OpenTrunk":
                return "Click <color=#B388FF><b>Left Mouse</b></color> on the Trunk to open it.";
            case "TUT_Quest2_2_CloseTrunk":
                return "Press <color=#B388FF><b>Tab</b></color> to close the Trunk.";
            case "TUT_Quest2_3_InspectCar":
                return "Click <color=#B388FF><b>Left Mouse</b></color> on the truck body to inspect it. Step away to exit.";
            case "TUT_Quest2_4_OpenHood":
                return "Click <color=#B388FF><b>Left Mouse</b></color> on the Hood to open it.";
            case "TUT_Quest2_5_RepairEngine":
                return "Inspect the engine, repair parts, and refill coolant.";
            case "TUT_Quest2_6_CloseHood":
                return "Click <color=#B388FF><b>Left Mouse</b></color> to close the Hood.";
            case "TUT_Quest3_1_OpenMap":
                return "Press <color=#B388FF><b>N</b></color> to open the Map.";
            case "TUT_Quest3_2_ClickShopIcon":
                return "Click the <color=#B388FF><b>Fishing Shop Icon</b></color> on the map to set a navigation marker.";
            case "TUT_Quest4_1_EnterVehicle":
                return "Walk to the driver door and click <color=#B388FF><b>Left Mouse</b></color> to enter the truck.";
            case "TUT_Quest4_2_DriveToShop":
                return "Drive to the <color=#B388FF><b>Angler's Shop</b></color> following the marker.\n(Radio: <color=#B388FF><b>L</b></color> On/Off | <color=#B388FF><b>K</b></color> Next Song | <color=#B388FF><b>[ ]</b></color> Volume | <color=#B388FF><b>G</b></color> Headlights).";
            case "TUT_Quest4_3_ExitVehicle":
                return "Press <color=#B388FF><b>E</b></color> to exit the vehicle.";
            case "TUT_Quest5_1_OpenShopMenu":
                return "Approach the <color=#B388FF><b>Angler</b></color> and click <color=#B388FF><b>Left Mouse</b></color> to open the shop.";
            case "TUT_Quest5_2_CloseShopMenu":
                return "Press <color=#B388FF><b>E</b></color> or click Close to exit the shop.";
            case "TUT_Quest6_1_OpenMapUpgrade":
                return "Press <color=#B388FF><b>N</b></color> to open the map and locate the <color=#B388FF><b>Mechanic</b></color>.";
            case "TUT_Quest6_2_OpenUpgradeMenu":
                return "Interact with the <color=#B388FF><b>Mechanic</b></color> to open the vehicle upgrade menu.";
            case "TUT_Quest6_3_CloseUpgradeMenu":
                return "Press <color=#B388FF><b>Z</b></color> or click Close to exit upgrade menu.";
            case "TUT_Quest8_1_DriveToGasStation":
                return "Drive your truck to the town's <color=#B388FF><b>Gas Station</b></color>.";
            case "TUT_Quest8_3_RefuelVehicle":
                return "Stand near the fuel pump and press <color=#B388FF><b>F</b></color> to refuel your vehicle.";
            case "TUT_Quest8_2_TalkToGasNPC":
                return "Exit vehicle and approach the <color=#B388FF><b>Gas Station NPC</b></color> to interact.";
            case "TUT_Quest8_4_BuyGasCanister":
                return "Stand near the fuel pump and press <color=#B388FF><b>F</b></color> to buy a spare fuel canister.";
            case "TUT_Quest8_5_CheckFuelInTrunk":
                return "Open the <color=#B388FF><b>Trunk</b></color> (Tab or click trunk) to check your spare fuel canister.";
            case "TUT_Quest9_OpenTravelMap":
                return "Press <color=#B388FF><b>M</b></color> to open the world map and travel to <color=#B388FF><b>Pine Lake</b></color>.";

            case "TUT_Map2_Quest1_1_OpenMapToCamp":
                return "Press <color=#B388FF><b>N</b></color> to open the Map and find the camping area.";
            case "TUT_Map2_Quest1_2_GoToCampSite":
                return "Drive to the <color=#B388FF><b>Camping Area</b></color> by the lake.";
            case "TUT_Map2_Quest1_3_ExitVehicle":
                return "Press <color=#B388FF><b>E</b></color> to exit the vehicle.";
            case "TUT_Map2_Quest2_1_OpenBackpack":
                return "Press <color=#B388FF><b>Tab</b></color> to open your Backpack.";
            case "TUT_Map2_Quest2_2_EquipRod":
                return "Drag a <color=#B388FF><b>Fishing Rod</b></color> into the Fishing Rod slot.";
            case "TUT_Map2_Quest2_3_EquipBaitAndBobber":
                return "Drag <color=#B388FF><b>Bait and Bobber</b></color> into their respective equipment slots.";
            case "TUT_Map2_Quest2_4_CloseBackpack":
                return "Press <color=#B388FF><b>Tab</b></color> to close your Backpack.";
            case "TUT_Map2_Quest3_WalkToLakeSide":
                return "Walk to the <color=#B388FF><b>Lakeside</b></color> to prepare for fishing.";
            case "TUT_Map2_Quest4_1_WindUpRod":
                return "Click <color=#B388FF><b>Left Mouse</b></color> to wind up your rod.";
            case "TUT_Map2_Quest4_2_TimingPower":
                return "<color=#FF5252>Red</color>: Far, <color=#FFD700>Yellow</color>: Medium, <color=#69F0AE>Green</color>: Close, <color=#FFFFFF>White</color>: Miss. Click to time your cast power!";
            case "TUT_Map2_Quest4_3_ReelFish":
                return "Hold / release <color=#B388FF><b>Left Mouse</b></color> to keep tension in the green balance zone.";
            case "TUT_Map2_Quest4_4_KeepOrReleaseFish":
                return "Press <color=#B388FF><b>Left Mouse</b></color> to keep fish in backpack, or press <color=#B388FF><b>Space</b></color> to release.";
            case "TUT_Map2_Quest4_5_OpenBackpackAfterFish":
                return "Press <color=#B388FF><b>Tab</b></color> to check your Backpack and inspect your caught fish.";
            case "TUT_Map2_Quest5_0_GoToTentCampArea":
                return "Walk to the <color=#B388FF><b>Tent campsite</b></color>.";
            case "TUT_Map2_Quest5_1_OpenBuildMenu":
                return "Press <color=#B388FF><b>B</b></color> to open the Construction / Camping menu.";
            case "TUT_Map2_Quest5_2_PlaceFirewood":
                return "Select <color=#B388FF><b>Firewood</b></color> from the menu and place it on the ground.";
            case "TUT_Map2_Quest5_3_PlaceCookingRack":
                return "Select the <color=#B388FF><b>Cooking Tripod</b></color> (B) and place it over the campfire.";
            case "TUT_Map2_Quest5_4_CookFish":
                return "Interact with the cooking tripod and place fish onto the grill to cook.";
            case "TUT_Map2_Quest5_5_EatFish":
                return "Take the cooked fish from your backpack and eat it to restore energy.";
            case "TUT_Map2_Quest5_6_SleepInTent":
                return "Go inside the tent and click left mouse to sleep and recover energy.";
            case "TUT_Map2_Quest5_7_PlaceLamp":
                return "Press <color=#B388FF><b>B</b></color> to place the <color=#B388FF><b>Lantern</b></color> (can toggle on/off).";
            case "TUT_Map2_Quest6_BackToTown":
                return "After camping, enter your truck and drive back to <color=#B388FF><b>Town</b></color>.";
            case "TUT_Map2_Quest7_FishLog":
                return "Press <color=#B388FF><b>J</b></color> to open and inspect your <color=#B388FF><b>Fish Journal</b></color>.";
            case "TUT_Map2_Quest8_HelpGuide":
                return "Press <color=#B388FF><b>P</b></color> anytime to review the <color=#B388FF><b>Help Guide & Controls</b></color>.";
            case "TUT_Final_TalkToQuestNPC":
                return "Talk to the <color=#B388FF><b>Village Master (Quest Giver)</b></color> to accept your first quest and finish the tutorial.";

            default: return fallbackVietnameseText;
        }
    }

    private void TriggerPopEffect()
    {
        if (panelRect == null) return;
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        popCoroutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        panelRect.localScale = Vector3.one * 1.08f;
        float elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            panelRect.localScale = Vector3.Lerp(Vector3.one * 1.08f, Vector3.one, elapsed / 0.15f);
            yield return null;
        }
        panelRect.localScale = Vector3.one;
    }

    private void TriggerCompletionPopEffect()
    {
        if (panelRect == null) return;
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        popCoroutine = StartCoroutine(CompletionPopRoutine());
    }

    private IEnumerator CompletionPopRoutine()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1.0f + Mathf.Sin(t * Mathf.PI) * 0.14f;
            panelRect.localScale = Vector3.one * scale;
            yield return null;
        }
        panelRect.localScale = Vector3.one;
    }

    private void PlayTypewriterEffect(string fullText)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypewriterRoutine(fullText));
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isTyping = true;
        instructionTMP.text = fullText;
        instructionTMP.maxVisibleCharacters = 0;
        instructionTMP.ForceMeshUpdate();

        int totalVisibleCharacters = instructionTMP.textInfo.characterCount;
        int currentVisibleCharacters = 0;

        while (currentVisibleCharacters <= totalVisibleCharacters)
        {
            instructionTMP.maxVisibleCharacters = currentVisibleCharacters;

            if (currentVisibleCharacters > 0 && currentVisibleCharacters % soundFrequency == 0)
            {
                PlayTypeSound();
            }

            currentVisibleCharacters++;
            if (cachedTypingWait == null) cachedTypingWait = new WaitForSeconds(typingSpeed);
            yield return cachedTypingWait;
        }

        isTyping = false;
    }

    private void PlayTypeSound()
    {
        if (audioSource == null || typeSFX == null) return;

        if (randomizePitch)
            audioSource.pitch = Random.Range(0.92f, 1.08f);
        else
            audioSource.pitch = 1.0f;

        audioSource.PlayOneShot(typeSFX, typeSFXVolume);
    }
}