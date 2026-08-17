using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum TutorialStage
{
    // === CÁC NHIỆM VỤ Ở MAP 1 (TOWN) ===
    Quest0_WelcomeGame,             // NV 0: Chào mừng bạn đến với Fish-Camping
    Quest1_Movement,                // NV 1: Di chuyển & Chạy nhanh (WASD + Shift hoặc Ctrl)
    Quest1_2_FindOldTruck,          // NV 1.2: Tìm xe tải cũ (Ctrl tiếp tục)
    Quest2_OpenAndCloseTrunk,       // NV 2: Mở cốp rồi bấm Tab đóng (Tab hoặc Ctrl)
    Quest2_2_InspectCar,            // NV 2.2: Di chuột vào xe để kiểm tra xe, lùi ra xa để thoát (Ctrl tiếp tục)
    Quest2_3_OpenHoodAndRepair,     // NV 2.3: Mở nắp capo lên để kiểm tra động cơ, sửa máy và châm nước cho xe (Ctrl tiếp tục)
    Quest3_OpenMap,                 // NV 3: Bấm N mở map (N hoặc Ctrl)
    Quest3_ClickShopIconStep,       // NV 3.1: Nhấp Icon Shop Đồ Câu -> Ctrl tiếp tục
    Quest4_EnterVehicleStep,        // NV 4: Hướng dẫn lên xe -> Ctrl tiếp tục
    Quest4_DriveAndRadioGuide,      // NV 4.1: Hướng dẫn lái xe & Radio -> Ctrl tiếp tục
    Quest5_ShopNPCGuide,            // NV 5: Mua đồ ở Shop -> Bấm E (hoặc Ctrl)
    Quest6_OpenMapUpgradeGuide,     // NV 6.1: Mở map xem nâng cấp -> Bấm N (hoặc Ctrl)
    Quest6_InteractUpgradeNPC,      // NV 6.2: Nâng cấp xe -> Bấm Z (hoặc Ctrl)
    Quest7_FindQuestNPC,            // NV 7: Tìm NPC giao nhiệm vụ -> Ctrl tiếp tục
    Quest8_FindGasStation,          // NV 8.1: Đi tìm cây xăng và đổ xăng (Bấm F tại cây xăng)
    Quest8_1_2_TalkToGasNPC,        // NV 8.1.2: Giao tiếp với người đổ xăng (Ctrl tiếp tục)
    Quest8_GoToPumpAndBuyGas,       // NV 8.2: Xuống xe đi đến trụ xăng để mua can xăng (Ctrl tiếp tục)
    Quest8_3_CheckFuelInTrunk,      // NV 8.3: Kiểm tra xăng ở cốp xe (Bấm Tab hoặc Ctrl tiếp tục)
    Quest9_OpenTravelMap,           // NV 9: Bấm M chọn map câu cá

    // === CÁC NHIỆM VỤ Ở MAP 2 (PINE LAKE) ===
    Map2_Quest1_OpenMapToCamp,          // NV 1: Bấm N mở bản đồ, đi đến địa điểm cắm trại (N hoặc Ctrl)
    Map2_Quest1_2_CheckFishingGear,     // NV 1.2: Kiểm tra trang bị đồ câu (Ctrl tiếp tục)
    Map2_Quest2_OpenBackpack,           // NV 2: Bấm Tab mở balo (Tab hoặc Ctrl)
    Map2_Quest3_EquipFishingItems,      // NV 3: Kéo cần câu, mồi, phao vào trang bị (Ctrl)
    Map2_Quest3_1_WalkToLakeSide,       // NV 3.1: Đi đến ven hồ để câu cá (Ctrl tiếp tục)
    Map2_Quest4_CanFishAtLake,          // NV 4: Bạn có thể câu cá ở hồ (Ctrl)
    Map2_Quest5_FishingGuide,           // NV 5: Click chuột trái vung cần, căn lực giật cá (Chuột trái hoặc Ctrl)
    Map2_Quest6_KeepOrReleaseFish,      // NV 6: Click chuột trái lấy cá / bấm Space thả cá (Space hoặc Chuột trái hoặc Ctrl)
    Map2_Quest6_1_OpenBackpackAfterFish,// NV 6.1: Bấm Tab mở balo (Tab hoặc Ctrl)
    Map2_Quest7_UnequipAndMoveCamp,     // NV 7: Cất cần vào balo, di chuyển lều trại (Ctrl)
    Map2_Quest7_1_FishUsageGuide,       // NV 7.1: Bạn có thể đem cá về bán lấy tiền hoặc chế biến nấu ăn (Ctrl)
    Map2_Quest8_PlaceFirewood,          // NV 8: Bấm B mở đồ cắm trại chọn đống củi đặt vị trí thích hợp (B hoặc Ctrl)
    Map2_Quest9_PlaceCookingRack,       // NV 9: Chọn bộ giá treo nồi dã ngoại lên đống củi (Ctrl)
    Map2_Quest10_CookFish,              // NV 10: Nấu ăn: Kéo con cá vào bếp (Ctrl)
    Map2_Quest11_EatFish,               // NV 11: Lấy cá và ăn để tăng sức lực (Ctrl)
    Map2_Quest12_SleepInTent,           // NV 12: Đi đến lều ngủ, tăng sức lực (Ctrl)
    Map2_Quest13_PlaceLamp,             // NV 13: Bấm B tìm đèn đặt vị trí thích hợp, bật/tắt (B hoặc Ctrl)
    Map2_Quest14_BackToTown,            // NV 14: Di chuyển về thị trấn (Ctrl)
    Map2_Quest15_FishLog,               // NV 15: Bấm J để xem nhật ký cá (J hoặc Ctrl)
    Map2_Quest16_HelpGuide,             // NV 16: Bấm P để xem các chỉ dẫn (P hoặc Ctrl)
    Completed                           // Hoàn thành toàn bộ Tutorial
}

public class ForcedTutorialManager : MonoBehaviour
{
    public static ForcedTutorialManager Instance { get; private set; }

    [Header("UI Hiển Thị")]
    [SerializeField] private GameObject questUIPanel;
    [SerializeField] private TextMeshProUGUI instructionTMP;
    [SerializeField] private TextMeshProUGUI progressTMP;

    [Header("Cấu Hình Hiệu Ứng Nhấp Nháy (Dòng gợi ý)")]
    [SerializeField] private float blinkSpeed = 3.5f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 1.0f;

    [Header("Cấu Hình Hiệu Ứng Gõ Chữ")]
    [SerializeField] private float typingSpeed = 0.025f;

    [Header("Cấu Hình Âm Thanh Gõ Chữ (Audio SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSFX;
    [SerializeField] private AudioClip nextQuestSFX;
    [Range(0.05f, 1f)] [SerializeField] private float typeSFXVolume = 0.4f;
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private int soundFrequency = 2;

    [Header("Cài Đặt Nhiệm Vụ")]
    [SerializeField] private float requiredWalkTime = 2.0f;

    private float moveTimer = 0f;
    private bool hasShiftSprint = false;
    private Coroutine typingCoroutine;
    private Coroutine popCoroutine;
    private RectTransform panelRect;

    private bool isTyping = false;
    private string currentInstructionText = "";
    private string currentPromptText = "";

    public TutorialStage currentStage = TutorialStage.Quest0_WelcomeGame;

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
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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
            if (currentStage < TutorialStage.Map2_Quest1_OpenMapToCamp)
            {
                AdvanceToStage(TutorialStage.Map2_Quest1_OpenMapToCamp);
            }
        }
    }

    private void Start() => UpdateQuestUI();

    private void Update()
    {
        HandleSingleTextBlink();

        if (Keyboard.current == null) return;

        // BẤM R ĐỂ QUAY LẠI HƯỚNG DẪN TRƯỚC ĐÓ
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            BackToPreviousStage();
            return;
        }

        if (currentStage == TutorialStage.Completed) return;

        bool isCtrlPressed = Keyboard.current.leftCtrlKey.wasPressedThisFrame || 
                             Keyboard.current.rightCtrlKey.wasPressedThisFrame;

        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool isLeftClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !isPointerOverUI;

        if (isTyping && isCtrlPressed)
        {
            CompleteTypingInstantly();
            return;
        }

        // ========================================================
        // MAP 1 (TOWN)
        // ========================================================
        if (currentStage == TutorialStage.Quest0_WelcomeGame)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Quest1_Movement);
        }
        else if (currentStage == TutorialStage.Quest1_Movement)
        {
            bool isMoving = Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed ||
                            Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed;

            if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
                hasShiftSprint = true;

            if (isMoving) moveTimer += Time.deltaTime;

            if ((moveTimer >= requiredWalkTime && hasShiftSprint) || isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest1_2_FindOldTruck);
        }
        else if (currentStage == TutorialStage.Quest1_2_FindOldTruck)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Quest2_OpenAndCloseTrunk);
        }
        else if (currentStage == TutorialStage.Quest2_OpenAndCloseTrunk)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest2_2_InspectCar);
        }
        else if (currentStage == TutorialStage.Quest2_2_InspectCar)
        {
            if (isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest2_3_OpenHoodAndRepair);
        }
        else if (currentStage == TutorialStage.Quest2_3_OpenHoodAndRepair)
        {
            if (isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest3_OpenMap);
        }
        else if (currentStage == TutorialStage.Quest3_OpenMap)
        {
            if (Keyboard.current.nKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest3_ClickShopIconStep);
        }
        else if (currentStage == TutorialStage.Quest3_ClickShopIconStep)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Quest4_EnterVehicleStep);
        }
        else if (currentStage == TutorialStage.Quest4_EnterVehicleStep)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Quest4_DriveAndRadioGuide);
        }
        else if (currentStage == TutorialStage.Quest4_DriveAndRadioGuide)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Quest5_ShopNPCGuide);
        }
        else if (currentStage == TutorialStage.Quest5_ShopNPCGuide)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest6_OpenMapUpgradeGuide);
        }
        else if (currentStage == TutorialStage.Quest6_OpenMapUpgradeGuide)
        {
            if (Keyboard.current.nKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest6_InteractUpgradeNPC);
        }
        else if (currentStage == TutorialStage.Quest6_InteractUpgradeNPC)
        {
            if (Keyboard.current.zKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest7_FindQuestNPC);
        }
        else if (currentStage == TutorialStage.Quest7_FindQuestNPC)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Quest8_FindGasStation);
        }
        else if (currentStage == TutorialStage.Quest8_FindGasStation)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
                AdvanceToStage(TutorialStage.Quest8_1_2_TalkToGasNPC);
        }
        else if (currentStage == TutorialStage.Quest8_1_2_TalkToGasNPC)
        {
            if (isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest8_GoToPumpAndBuyGas);
        }
        else if (currentStage == TutorialStage.Quest8_GoToPumpAndBuyGas)
        {
            if (isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest8_3_CheckFuelInTrunk);
        }
        else if (currentStage == TutorialStage.Quest8_3_CheckFuelInTrunk)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Quest9_OpenTravelMap);
        }
        else if (currentStage == TutorialStage.Quest9_OpenTravelMap)
        {
            if (Keyboard.current.mKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest1_OpenMapToCamp);
        }

        // ========================================================
        // MAP 2 (PINE LAKE)
        // ========================================================
        else if (currentStage == TutorialStage.Map2_Quest1_OpenMapToCamp)
        {
            if (Keyboard.current.nKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest1_2_CheckFishingGear);
        }
        else if (currentStage == TutorialStage.Map2_Quest1_2_CheckFishingGear)
        {
            if (isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest2_OpenBackpack);
        }
        else if (currentStage == TutorialStage.Map2_Quest2_OpenBackpack)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest3_EquipFishingItems);
        }
        else if (currentStage == TutorialStage.Map2_Quest3_EquipFishingItems)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest3_1_WalkToLakeSide);
        }
        else if (currentStage == TutorialStage.Map2_Quest3_1_WalkToLakeSide)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest4_CanFishAtLake);
        }
        else if (currentStage == TutorialStage.Map2_Quest4_CanFishAtLake)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest5_FishingGuide);
        }
        else if (currentStage == TutorialStage.Map2_Quest5_FishingGuide)
        {
            if (isLeftClick || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest6_KeepOrReleaseFish);
        }
        else if (currentStage == TutorialStage.Map2_Quest6_KeepOrReleaseFish)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || isLeftClick || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest6_1_OpenBackpackAfterFish);
        }
        else if (currentStage == TutorialStage.Map2_Quest6_1_OpenBackpackAfterFish)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest7_UnequipAndMoveCamp);
        }
        else if (currentStage == TutorialStage.Map2_Quest7_UnequipAndMoveCamp)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest7_1_FishUsageGuide);
        }
        else if (currentStage == TutorialStage.Map2_Quest7_1_FishUsageGuide)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest8_PlaceFirewood);
        }
        else if (currentStage == TutorialStage.Map2_Quest8_PlaceFirewood)
        {
            if (Keyboard.current.bKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest9_PlaceCookingRack);
        }
        else if (currentStage == TutorialStage.Map2_Quest9_PlaceCookingRack)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest10_CookFish);
        }
        else if (currentStage == TutorialStage.Map2_Quest10_CookFish)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest11_EatFish);
        }
        else if (currentStage == TutorialStage.Map2_Quest11_EatFish)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest12_SleepInTent);
        }
        else if (currentStage == TutorialStage.Map2_Quest12_SleepInTent)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest13_PlaceLamp);
        }
        else if (currentStage == TutorialStage.Map2_Quest13_PlaceLamp)
        {
            if (Keyboard.current.bKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest14_BackToTown);
        }
        else if (currentStage == TutorialStage.Map2_Quest14_BackToTown)
        {
            if (isCtrlPressed) AdvanceToStage(TutorialStage.Map2_Quest15_FishLog);
        }
        else if (currentStage == TutorialStage.Map2_Quest15_FishLog)
        {
            if (Keyboard.current.jKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Map2_Quest16_HelpGuide);
        }
        else if (currentStage == TutorialStage.Map2_Quest16_HelpGuide)
        {
            if (Keyboard.current.pKey.wasPressedThisFrame || isCtrlPressed)
                AdvanceToStage(TutorialStage.Completed);
        }
    }

    private void BackToPreviousStage()
    {
        // Nếu đang ở màn 1 và chưa tới đích thì lùi tối đa về Quest0
        if (currentStage > TutorialStage.Quest0_WelcomeGame)
        {
            AdvanceToStage(currentStage - 1);
        }
    }

    private void HandleSingleTextBlink()
    {
        if (isTyping || instructionTMP == null || string.IsNullOrEmpty(currentPromptText)) return;

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * blinkSpeed) + 1.0f) / 2.0f);
        byte alphaByte = (byte)(alpha * 255);
        string hexAlpha = alphaByte.ToString("X2");

        instructionTMP.text = $"{currentInstructionText}\n<size=80%><color=#F1C40F{hexAlpha}>({currentPromptText})</color></size>";
    }

    public void NotifyTrunkOpened() { }

    public void NotifyMapOpened()
    {
        if (currentStage == TutorialStage.Quest3_OpenMap)
            AdvanceToStage(TutorialStage.Quest3_ClickShopIconStep);
    }

    public void NotifyShopIconClicked()
    {
        if (currentStage == TutorialStage.Quest3_ClickShopIconStep)
            AdvanceToStage(TutorialStage.Quest4_EnterVehicleStep);
    }

    public void NotifyEnteredVehicle()
    {
        if (currentStage == TutorialStage.Quest4_EnterVehicleStep)
            AdvanceToStage(TutorialStage.Quest4_DriveAndRadioGuide);
    }

    public void NotifyReachedShopOrPurchased()
    {
        if (currentStage == TutorialStage.Quest5_ShopNPCGuide)
            AdvanceToStage(TutorialStage.Quest6_OpenMapUpgradeGuide);
    }

    public void AdvanceToStage(TutorialStage nextStage)
    {
        currentStage = nextStage;

        if (audioSource != null && nextQuestSFX != null)
            audioSource.PlayOneShot(nextQuestSFX);

        TriggerPopEffect();
        UpdateQuestUI();
    }

    private void UpdateQuestUI()
    {
        if (questUIPanel != null) questUIPanel.SetActive(true);
        if (instructionTMP == null) return;

        if (progressTMP != null)
        {
            if (currentStage == TutorialStage.Completed)
            {
                progressTMP.text = "HOÀN THÀNH";
            }
            else if (currentStage < TutorialStage.Map2_Quest1_OpenMapToCamp)
            {
                int currentMap1 = (int)currentStage;
                int totalMap1 = (int)TutorialStage.Quest9_OpenTravelMap;
                if (currentMap1 == 0) currentMap1 = 1;
                progressTMP.text = $"HƯỚNG DẪN ({currentMap1}/{totalMap1})";
            }
            else
            {
                int currentMap2 = (int)currentStage - (int)TutorialStage.Map2_Quest1_OpenMapToCamp + 1;
                int totalMap2 = (int)TutorialStage.Completed - (int)TutorialStage.Map2_Quest1_OpenMapToCamp;
                progressTMP.text = $"HƯỚNG DẪN ({currentMap2}/{totalMap2})";
            }
        }

        switch (currentStage)
        {
            case TutorialStage.Quest0_WelcomeGame:
                currentInstructionText = "Chào mừng bạn đến với <color=#B388FF><b>Fish-Camping</b></color>!";
                currentPromptText = "Nhấn [Ctrl] để bắt đầu";
                break;
            case TutorialStage.Quest1_Movement:
                currentInstructionText = "Dùng phím <color=#B388FF><b>W, A, S, D</b></color> để di chuyển và giữ <color=#B388FF><b>Shift</b></color> để chạy nhanh.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest1_2_FindOldTruck:
                currentInstructionText = "Hãy quan sát xung quanh và tìm chiếc <color=#B388FF><b>Xe tải cũ</b></color> của bạn.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest2_OpenAndCloseTrunk:
                currentInstructionText = "Nhấp <color=#B388FF><b>Chuột trái</b></color> vào cốp xe để xem và nhấn phím <color=#B388FF><b>Tab</b></color> để đóng.";
                currentPromptText = "Nhấn [Tab] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest2_2_InspectCar:
                currentInstructionText = "Di <color=#B388FF><b>Chuột</b></color> vào xe để kiểm tra tình trạng xe, lùi ra xa để thoát.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest2_3_OpenHoodAndRepair:
                currentInstructionText = "Mở <color=#B388FF><b>Nắp capo</b></color> lên để kiểm tra động cơ, sửa máy và châm nước làm mát cho xe.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest3_OpenMap:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>N</b></color> để mở Bản đồ.";
                currentPromptText = "Nhấn [N] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest3_ClickShopIconStep:
                currentInstructionText = "Nhấp vào <color=#B388FF><b>Icon Shop Đồ Câu</b></color> trên bản đồ để định vị đường đi.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest4_EnterVehicleStep:
                currentInstructionText = "Đi đến cửa xe và nhấp <color=#B388FF><b>Chuột trái</b></color> để lên xe bán tải.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest4_DriveAndRadioGuide:
                currentInstructionText = "Lái xe đến Shop (Bấm <color=#B388FF><b>E</b></color> xuống xe).\n(Radio: <color=#B388FF><b>L</b></color> Bật/Tắt | <color=#B388FF><b>K</b></color> Đổi bài | <color=#B388FF><b>[ ]</b></color> Âm lượng | <color=#B388FF><b>G</b></color> Đèn pha).";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest5_ShopNPCGuide:
                currentInstructionText = "Đến gần <color=#B388FF><b>NPC Bán Đồ</b></color> để mua vật phẩm, nhấn <color=#B388FF><b>E</b></color> để đóng cửa hàng.";
                currentPromptText = "Nhấn [E] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest6_OpenMapUpgradeGuide:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>N</b></color> mở bản đồ để xem vị trí của <color=#B388FF><b>NPC Nâng Cấp Xe</b></color>.";
                currentPromptText = "Nhấn [N] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest6_InteractUpgradeNPC:
                currentInstructionText = "Tương tác với <color=#B388FF><b>NPC Nâng Cấp</b></color> để nâng cấp xe của bạn, nhấn <color=#B388FF><b>Z</b></color> để thoát.";
                currentPromptText = "Nhấn [Z] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest7_FindQuestNPC:
                currentInstructionText = "Tìm <color=#B388FF><b>NPC Giao Nhiệm Vụ</b></color> trong khu vực để nhận nhiệm vụ đầu tiên.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest8_FindGasStation:
                currentInstructionText = "Hãy lái xe đi quanh thị trấn tìm <color=#B388FF><b>Cây Xăng</b></color> và nạp đầy nhiên liệu cho xe.";
                currentPromptText = "[R] Quay lại";
                break;
            case TutorialStage.Quest8_1_2_TalkToGasNPC:
                currentInstructionText = "Giao tiếp với <color=#B388FF><b>Người đổ xăng</b></color> để tìm hiểu thêm thông tin.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest8_GoToPumpAndBuyGas:
                currentInstructionText = "Xuống xe đi đến trụ xăng để mua can xăng dự trữ.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest8_3_CheckFuelInTrunk:
                currentInstructionText = "Mở <color=#B388FF><b>Cốp xe</b></color> ra để kiểm tra xem can xăng dự trữ đã nằm trong cốp chưa.";
                currentPromptText = "Nhấn [Tab] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Quest9_OpenTravelMap:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>M</b></color> để mở bản đồ du lịch và chọn di chuyển tới <color=#B388FF><b>Pine Lake</b></color>.";
                currentPromptText = "Nhấn [M] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;

            // === MAP 2 ===
            case TutorialStage.Map2_Quest1_OpenMapToCamp:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>N</b></color> mở bản đồ, sau đó di chuyển đến <color=#B388FF><b>Địa điểm cắm trại</b></color>.";
                currentPromptText = "Nhấn [N] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest1_2_CheckFishingGear:
                currentInstructionText = "Hãy kiểm tra lại toàn bộ <color=#B388FF><b>Trang bị đồ câu</b></color> trước khi bắt đầu.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest2_OpenBackpack:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>Tab</b></color> để mở Balo của bạn.";
                currentPromptText = "Nhấn [Tab] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest3_EquipFishingItems:
                currentInstructionText = "Kéo <color=#B388FF><b>Cần câu, Mồi câu, Phao câu</b></color> vào ô Trang bị tương ứng.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest3_1_WalkToLakeSide:
                currentInstructionText = "Di chuyển đến khu vực <color=#B388FF><b>Ven bờ hồ</b></color> để chuẩn bị buông cần.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest4_CanFishAtLake:
                currentInstructionText = "Bạn có thể câu cá tại các khu vực ven bờ hồ Pine Lake.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest5_FishingGuide:
                currentInstructionText = "<color=#B388FF><b>Câu cá:</b></color> Nhấp <color=#B388FF><b>Chuột trái</b></color> để vung cần, giữ hoặc nhấp <color=#B388FF><b>Chuột trái</b></color> để căn lực giật cá.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest6_KeepOrReleaseFish:
                currentInstructionText = "Nhấp <color=#B388FF><b>Chuột trái</b></color> để lấy cá bỏ vào balo hoặc nhấn <color=#B388FF><b>Space</b></color> để thả cá.";
                currentPromptText = "Nhấn [Space] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest6_1_OpenBackpackAfterFish:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>Tab</b></color> để mở Balo kiểm tra lại chiến lợi phẩm.";
                currentPromptText = "Nhấn [Tab] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest7_UnequipAndMoveCamp:
                currentInstructionText = "Cất cần câu vào balo và di chuyển về khu vực <color=#B388FF><b>Lều trại</b></color>.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest7_1_FishUsageGuide:
                currentInstructionText = "Bạn có thể đem cá về bán lấy tiền hoặc chế biến nấu ăn.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest8_PlaceFirewood:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>B</b></color> mở đồ cắm trại, chọn <color=#B388FF><b>Đống củi</b></color> và đặt tại vị trí thích hợp.";
                currentPromptText = "Nhấn [B] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest9_PlaceCookingRack:
                currentInstructionText = "Chọn <color=#B388FF><b>Bộ giá treo nồi dã ngoại</b></color> đặt khớp lên đống củi vừa dựng.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest10_CookFish:
                currentInstructionText = "<color=#B388FF><b>Nấu ăn:</b></color> Kéo con cá vừa câu được vào bếp lửa để nướng.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest11_EatFish:
                currentInstructionText = "Lấy cá đã nướng chín và ăn để hồi phục lại sức lực.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest12_SleepInTent:
                currentInstructionText = "Đi đến bên trong lều để ngủ giúp hồi phục tối đa sức lực.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest13_PlaceLamp:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>B</b></color> tìm đến <color=#B388FF><b>Chiếc đèn</b></color>, đặt tại vị trí thích hợp (có thể Bật/Tắt).";
                currentPromptText = "Nhấn [B] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest14_BackToTown:
                currentInstructionText = "Sau khi cắm trại, hãy lên xe và lái về lại <color=#B388FF><b>Thị trấn</b></color>.";
                currentPromptText = "Nhấn [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest15_FishLog:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>J</b></color> để mở xem <color=#B388FF><b>Nhật ký các loài cá</b></color> bạn đã câu được.";
                currentPromptText = "Nhấn [J] hoặc [Ctrl] để tiếp tục | [R] Quay lại";
                break;
            case TutorialStage.Map2_Quest16_HelpGuide:
                currentInstructionText = "Nhấn phím <color=#B388FF><b>P</b></color> bất kỳ lúc nào để xem lại toàn bộ <color=#B388FF><b>Chỉ dẫn và phím bấm</b></color>.";
                currentPromptText = "Nhấn [P] hoặc [Ctrl] để hoàn thành | [R] Quay lại";
                break;

            case TutorialStage.Completed:
                currentInstructionText = "Đã hoàn thành nv.";
                currentPromptText = "[R] Xem lại hướng dẫn trước";
                break;
        }

        string fullText = string.IsNullOrEmpty(currentPromptText)
            ? currentInstructionText
            : $"{currentInstructionText}\n<size=80%><color=#F1C40F>({currentPromptText})</color></size>";

        PlayTypewriterEffect(fullText);
    }

    private void CompleteTypingInstantly()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        string fullText = string.IsNullOrEmpty(currentPromptText)
            ? currentInstructionText
            : $"{currentInstructionText}\n<size=80%><color=#F1C40F>({currentPromptText})</color></size>";

        instructionTMP.text = fullText;
        instructionTMP.maxVisibleCharacters = instructionTMP.textInfo.characterCount;
        isTyping = false;
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
            yield return new WaitForSeconds(typingSpeed);
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