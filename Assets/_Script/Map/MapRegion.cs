using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Button))]
public class MapRegion : MonoBehaviour
{
    public enum TireTerrainCheckType
    {
        None,
        City,
        Forest,
        Swamp,
        Sand
    }

    [SerializeField] private MapInteractionManager interactionManager;
    [SerializeField] private string sceneName;
    [SerializeField] private string spawnID;
    [SerializeField] private string mapDisplayName;

    [Header("=== ĐIỀU KIỆN MỞ KHÓA (SỔ TAY CÁ + GARA) ===")]
    [SerializeField] private bool checkFishJournal;
    [SerializeField] private int requiredFishCount = 0;
    [SerializeField] private string requiredFishMapKeyword = "Hồ";

    [SerializeField] private bool requireTireUpgrade;
    [SerializeField] private TireTerrainCheckType terrainCheckType = TireTerrainCheckType.None;
    [SerializeField] private float minTerrainFriction = 0.60f;
    [SerializeField] private int legacyTireIndex = 0;
    [SerializeField] private string requiredTireDescription = "Lốp xe chuyên dụng";
    [SerializeField] private string tireHint = "Đến Garage ở Thị Trấn để nâng cấp lốp xe.";

    [Header("=== BIỂU TƯỢNG KHÓA & TEXT NHẮC ===")]
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private TextMeshProUGUI locktext;

    private RectTransform rectTransform;
    private Button button;
    private bool isLocked;

    // Tên các loại lốp mặc định tương ứng với index 0 - 7
    private static readonly string[] defaultTireNames = new string[]
    {
        "Lốp Đô Thị",           // 0
        "Lốp Bán Địa Hình",     // 1
        "Lốp Bám Cát",          // 2
        "Lốp Leo Đá, Rễ Cây",   // 3
        "Lốp Bùn Thường",       // 4
        "Lốp Bùn Chuyên Dụng",  // 5
        "Lốp Thủy Sinh",        // 6
        "Lốp Đua Địa Hình"      // 7
    };

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        AutoConfigureDefaults();
    }

    private void AutoConfigureDefaults()
    {
        // Tự động nhận diện và cấu hình theo tên Scene nếu chưa được điền thủ công trên Inspector
        if (string.IsNullOrEmpty(sceneName)) return;

        if (sceneName.Contains("Map_1") || sceneName.Contains("Town"))
        {
            if (string.IsNullOrEmpty(mapDisplayName)) mapDisplayName = "Thị Trấn (Town)";
            checkFishJournal = false;
            requireTireUpgrade = false;
        }
        else if (sceneName.Contains("Map_2") || sceneName.Contains("PineLake"))
        {
            if (string.IsNullOrEmpty(mapDisplayName)) mapDisplayName = "Hồ Pine Lake";
            checkFishJournal = false;
            requireTireUpgrade = false;
        }
        else if (sceneName.Contains("Map3") || sceneName.Contains("Swamp"))
        {
            if (string.IsNullOrEmpty(mapDisplayName)) mapDisplayName = "Đầm Lầy (Swamp)";
            checkFishJournal = true;
            if (requiredFishCount <= 0) requiredFishCount = 4;
            if (string.IsNullOrEmpty(requiredFishMapKeyword)) requiredFishMapKeyword = "Hồ";

            requireTireUpgrade = true;
            terrainCheckType = TireTerrainCheckType.Swamp;
            minTerrainFriction = 0.60f;
            if (string.IsNullOrEmpty(requiredTireDescription)) requiredTireDescription = "Lốp Bùn Thường ($850) hoặc Lốp Bùn Chuyên Dụng ($1.400)";
            if (string.IsNullOrEmpty(tireHint)) tireHint = "Lái xe về Garage ở Thị Trấn mua Lốp Bùn để xe không bị lún sình.";
        }
        else if (sceneName.Contains("Map4") || sceneName.Contains("Ocean") || sceneName.Contains("Coast"))
        {
            if (string.IsNullOrEmpty(mapDisplayName)) mapDisplayName = "Bờ Biển (Ocean Coast)";
            checkFishJournal = true;
            if (requiredFishCount <= 0) requiredFishCount = 3;
            if (string.IsNullOrEmpty(requiredFishMapKeyword)) requiredFishMapKeyword = "Đầm";

            requireTireUpgrade = true;
            terrainCheckType = TireTerrainCheckType.Sand;
            minTerrainFriction = 0.70f;
            if (string.IsNullOrEmpty(requiredTireDescription)) requiredTireDescription = "Lốp Bám Cát ($700), Lốp Thủy Sinh ($1.600) hoặc Lốp Đua ($2.800)";
            if (string.IsNullOrEmpty(tireHint)) tireHint = "Đến Garage ở Thị Trấn trang bị Lốp Bám Cát hoặc Lốp Đua để vượt cồn cát biển.";
        }
    }

    private void OnEnable()
    {
        UpdateLockState();
    }

    public bool CheckUnlockStatus(
        out bool isTutorialLocked,
        out string tutorialHint,
        out bool fishConditionMet,
        out int curFish,
        out int targetFish,
        out bool tireConditionMet,
        out string currentTireName)
    {
        isTutorialLocked = false;
        tutorialHint = "";
        fishConditionMet = true;
        curFish = 0;
        targetFish = requiredFishCount;
        tireConditionMet = true;
        currentTireName = "Lốp Mặc Định";

        // 1. Kiểm tra khóa theo Cốt truyện Map 1 -> Map 2
        bool isMap2 = !string.IsNullOrEmpty(sceneName) && 
                      (sceneName.Contains("Map_2") || sceneName.Contains("PineLake") || sceneName.Contains("Map2"));

        if (isMap2 && ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanTravelToMap2())
        {
            isTutorialLocked = true;
            tutorialHint = "Hãy hoàn thành chuỗi nhiệm vụ hướng dẫn tại Thị Trấn (Đổ xăng xe) để mở đường sang Map 2.";
            return false;
        }

        // 2. Kiểm tra điều kiện Sổ Tay Cá
        if (checkFishJournal && targetFish > 0)
        {
            if (FishJournalManager.Instance != null)
            {
                curFish = FishJournalManager.Instance.GetUnlockedFishCountByMap(requiredFishMapKeyword);
                // Nếu lọc theo từ khóa không đủ, kiểm tra tổng số lượng đã câu trong toàn bộ sổ tay
                int totalFish = FishJournalManager.Instance.GetTotalUnlockedFishCount();
                if (curFish < targetFish && totalFish >= targetFish + 2)
                {
                    curFish = targetFish; // Đã câu nhiều cá tổng thể vượt bậc
                }
            }
            fishConditionMet = curFish >= targetFish;
        }

        // 3. Kiểm tra điều kiện Lốp Xe Garage
        int equippedTireIndex = PlayerPrefs.GetInt("EquippedTireIndex", -1);
        if (equippedTireIndex >= 0 && equippedTireIndex < defaultTireNames.Length)
        {
            currentTireName = defaultTireNames[equippedTireIndex];
        }

        if (requireTireUpgrade)
        {
            tireConditionMet = false;

            // Cách 1: Trích xuất trực tiếp thông số từ Gara nếu đang nạp
            if (GarageZone.Instance != null && GarageZone.Instance.allTires != null && 
                equippedTireIndex >= 0 && equippedTireIndex < GarageZone.Instance.allTires.Length)
            {
                TireData data = GarageZone.Instance.allTires[equippedTireIndex];
                if (data != null)
                {
                    currentTireName = data.tireName;
                    float friction = 0f;
                    switch (terrainCheckType)
                    {
                        case TireTerrainCheckType.City: friction = data.cityFriction; break;
                        case TireTerrainCheckType.Forest: friction = data.forestFriction; break;
                        case TireTerrainCheckType.Swamp: friction = data.swampFriction; break;
                        case TireTerrainCheckType.Sand: friction = data.sandFriction; break;
                    }
                    tireConditionMet = friction >= minTerrainFriction;
                }
            }
            // Cách 2: Kiểm tra fallback theo danh sách index lốp đạt chuẩn
            if (!tireConditionMet)
            {
                if (terrainCheckType == TireTerrainCheckType.Swamp)
                {
                    // Lốp 4 (Bùn thường), 5 (Bùn chuyên dụng), 6 (Thủy sinh), 7 (Đua)
                    tireConditionMet = equippedTireIndex == 4 || equippedTireIndex == 5 || equippedTireIndex == 6 || equippedTireIndex == 7;
                }
                else if (terrainCheckType == TireTerrainCheckType.Sand)
                {
                    // Lốp 2 (Bám cát), 6 (Thủy sinh), 7 (Đua)
                    tireConditionMet = equippedTireIndex == 2 || equippedTireIndex == 6 || equippedTireIndex == 7;
                }
                else if (legacyTireIndex > 0)
                {
                    tireConditionMet = equippedTireIndex >= legacyTireIndex;
                }
            }
        }

        bool allUnlocked = !isTutorialLocked && fishConditionMet && tireConditionMet;
        return allUnlocked;
    }

    private Coroutine pulseCoroutine;

    private Transform actualLockIconTransform;

    public void UpdateLockState()
    {
        bool unlocked = CheckUnlockStatus(
            out bool isTutorialLocked,
            out string tutorialHint,
            out bool fishMet,
            out int curFish,
            out int targetFish,
            out bool tireMet,
            out string currentTireName
        );

        isLocked = !unlocked;

        if (lockIcon != null)
        {
            lockIcon.SetActive(isLocked);
            lockIcon.transform.localScale = Vector3.one; // Đảm bảo BlackGround luôn đứng im tỉ lệ 1.0

            FindActualLockIcon();

            if (isLocked)
            {
                if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
                if (gameObject.activeInHierarchy)
                {
                    pulseCoroutine = StartCoroutine(PulseLockIcon());
                }
            }
            else
            {
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                    pulseCoroutine = null;
                }
                if (actualLockIconTransform != null) actualLockIconTransform.localScale = Vector3.one;
            }
        }

        UpdateLockText(isLocked);
    }

    private void FindActualLockIcon()
    {
        if (actualLockIconTransform != null) return;
        if (lockIcon == null) return;

        // Nếu lockIcon chính là đối tượng hình cái khóa
        if (lockIcon.name.Equals("LockIcon", System.StringComparison.OrdinalIgnoreCase))
        {
            actualLockIconTransform = lockIcon.transform;
            return;
        }

        // Nếu lockIcon là BlackGround, tìm con bên trong có tên LockIcon
        Transform child = lockIcon.transform.Find("LockIcon");
        if (child != null)
        {
            actualLockIconTransform = child;
            return;
        }

        // Tìm kiếm các con khác có chứa từ icon
        foreach (Transform t in lockIcon.transform)
        {
            if (t.name.ToLower().Contains("lockicon") || t.name.ToLower().Contains("icon"))
            {
                actualLockIconTransform = t;
                return;
            }
        }
    }

    private void UpdateLockText(bool locked)
    {
        if (locktext == null)
        {
            // Tự động tìm GameObject tên locktext trong MapRegion hoặc trong BlackGround
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                if (t != null && t.gameObject.name.ToLower().Contains("locktext"))
                {
                    locktext = t;
                    break;
                }
            }
        }

        if (locktext != null)
        {
            locktext.gameObject.SetActive(locked);
            if (locked)
            {
                bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                                    UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

                locktext.text = isVietnamese ? "• BẤM ĐỂ XEM NHIỆM VỤ •" : "• CLICK TO VIEW GOALS •";
            }
        }
    }

    private IEnumerator PulseLockIcon()
    {
        // Đảm bảo BlackGround luôn đứng im
        if (lockIcon != null) lockIcon.transform.localScale = Vector3.one;

        Transform target = actualLockIconTransform != null ? actualLockIconTransform : (lockIcon != null && lockIcon.name == "LockIcon" ? lockIcon.transform : null);
        if (target == null) yield break;

        // Chỉ phóng to co giãn nhẹ nhàng cái icon ổ khóa bên trong
        while (isLocked && target != null && target.gameObject.activeInHierarchy)
        {
            float scale = 1f + Mathf.Sin(Time.unscaledTime * 3.5f) * 0.08f;
            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        if (target != null) target.localScale = Vector3.one;
    }

    private void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        if (lockIcon != null)
        {
            lockIcon.transform.localScale = Vector3.one;
        }
        if (actualLockIconTransform != null)
        {
            actualLockIconTransform.localScale = Vector3.one;
        }
    }

    private void OnClick()
    {
        UpdateLockState();

        if (isLocked)
        {
            CheckUnlockStatus(
                out bool isTutorialLocked,
                out string tutorialHint,
                out bool fishMet,
                out int curFish,
                out int targetFish,
                out bool tireMet,
                out string currentTireName
            );

            // Bật Popup Checklist điều kiện trực quan
            if (MapRequirementPopupUI.Instance != null)
            {
                MapRequirementPopupUI.Instance.Show(
                    mapDisplayName,
                    isTutorialLocked,
                    tutorialHint,
                    fishMet,
                    curFish,
                    targetFish,
                    requiredFishMapKeyword,
                    tireMet,
                    currentTireName,
                    requiredTireDescription,
                    tireHint
                );
            }
            else
            {
                // Fallback nếu chưa có instance popup: tự tạo Popup trên runtime
                GameObject popupObj = new GameObject("MapRequirementPopupManager", typeof(MapRequirementPopupUI));
                MapRequirementPopupUI popup = popupObj.GetComponent<MapRequirementPopupUI>();
                popup.Show(
                    mapDisplayName,
                    isTutorialLocked,
                    tutorialHint,
                    fishMet,
                    curFish,
                    targetFish,
                    requiredFishMapKeyword,
                    tireMet,
                    currentTireName,
                    requiredTireDescription,
                    tireHint
                );
            }
            return;
        }

        interactionManager.ExpandMap(rectTransform, sceneName, spawnID);
    }
}