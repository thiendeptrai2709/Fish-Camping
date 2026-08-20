using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class VehicleStats : MonoBehaviour
{
    [Header("Vehicle Base Stats")]
    [SerializeField] private float baseMaxSpeed = 80f;
    [SerializeField] private float baseMotorTorque = 1500f;
    [SerializeField] private TireRepairMinigame[] tireMinigames = new TireRepairMinigame[4];

    [Header("Link Hệ thống")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float maxStatsDistance = 4.5f;
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private ProceduralHinge hoodHinge;
    [SerializeField] private QTEMinigame qteMinigame;
    [SerializeField] private BalanceMinigame balanceMinigame;
    [SerializeField] private EngineRepairMinigame engineMinigame;

    [Header("Link UI")]
    [SerializeField] private GameObject statsCanvasObject;
    [SerializeField] private TextMeshProUGUI engineText;
    [SerializeField] private TextMeshProUGUI coolantText;
    [SerializeField] private TextMeshProUGUI[] tireTexts = new TextMeshProUGUI[4];

    [Header("Chỉ số độ bền (0% - 100%)")]
    public float engineHealth = 100f;
    public float trunkHealth = 100f;
    public float coolantLevel = 100f;
    public float[] tireHealths = new float[4] { 100f, 100f, 100f, 100f };

    [Header("Cấu hình hao hụt (Số Km để mất 1%)")]
    [SerializeField] private float kmPerEnginePercent = 5f;
    [SerializeField] private float kmPerCoolantPercent = 2f;

    // Đã ẩn kmPerTirePercent và badRoadDamageChance đi vì giờ nó sẽ TỰ ĐỌC TỪ TIRE DATA
    private float kmPerTirePercent = 3f;
    private float badRoadCheckInterval = 2f;
    private float badRoadDamageChance = 0.1f;
    private float badRoadDamageAmount = 5f;

    private float badRoadTimer;

    public UnityEvent OnStatsChanged;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        if (statsCanvasObject == null)
        {
            GameObject statOverview = GameObject.Find("Stat_Overview");
            if (statOverview != null)
            {
                statsCanvasObject = statOverview;
            }
            else
            {
                var allCanvas = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (var t in allCanvas)
                {
                    if (t != null && t.name == "Stat_Overview")
                    {
                        statsCanvasObject = t.gameObject;
                        break;
                    }
                }
            }
        }
    }

    private void Start()
    {
        if (playerTransform != null)
        {
            playerMovement = playerTransform.GetComponent<PlayerMovement>();
        }

        // Vừa vào game là đọc thông số lốp ngay lập tức
        UpdateTireStats();

        ApplyDegradationToPhysics();
        UpdateUI();
    }

    // --- HÀM THẦN THÁNH: TỰ ĐỘNG LẤY CHỈ SỐ TỪ SCRIPTABLE OBJECT CỦA BỒ ---
    public void UpdateTireStats()
    {
        int equippedTire = PlayerPrefs.GetInt("EquippedTireIndex", -1);

        // Kiểm tra xem Gara có tồn tại và lốp bồ đang lắp có nằm trong danh sách 8 lốp kia không
        if (GarageZone.Instance != null && equippedTire >= 0 && equippedTire < GarageZone.Instance.allTires.Length)
        {
            // Trích xuất file Data Lốp tương ứng
            TireData currentTireData = GarageZone.Instance.allTires[equippedTire];

            if (currentTireData != null)
            {
                // Áp dụng độ trâu bò từ file của bồ vào xe
                kmPerTirePercent = currentTireData.kmPerTirePercent;
                badRoadDamageChance = currentTireData.badRoadDamageChance;
                Debug.Log($"Đã nạp thành công thông số lốp: {currentTireData.name}");
            }
        }
        else
        {
            // LẮP LỐP MẶC ĐỊNH (Khi chưa mua gì): Thông số cùi bắp
            kmPerTirePercent = 3f; // 3km mất 1%
            badRoadDamageChance = 0.1f; // 10% rách lốp
        }
    }

    private void Update()
    {
        if (statsCanvasObject != null && statsCanvasObject.activeSelf)
        {
            if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > maxStatsDistance)
            {
                statsCanvasObject.SetActive(false);
            }
        }

        bool isCapoClosed = (hoodHinge != null && !hoodHinge.IsFullyOpen);
        bool isEngineHidden = (engineMinigame != null && !engineMinigame.IsEngineOut);
        bool isQTEPlaying = (qteMinigame != null && qteMinigame.IsPlaying);
        bool isBalancePlaying = (balanceMinigame != null && balanceMinigame.IsPlaying);
        bool isTireRepairing = TireRepairMinigame.ActiveTire != null;

        if (playerMovement != null)
        {
            playerMovement.IsMovementLocked = isQTEPlaying || isBalancePlaying || isTireRepairing;
        }

        if (isCapoClosed || isEngineHidden)
        {
            if (isQTEPlaying) qteMinigame.ForceAbort();
            if (isBalancePlaying) balanceMinigame.ForceAbort();
        }

        float currentSpeed = vehicleController.GetCurrentSpeedKmh();
        if (currentSpeed > 5f)
        {
            float distanceThisFrame = (currentSpeed / 3600f) * Time.deltaTime;

            float coolantDrop = distanceThisFrame / kmPerCoolantPercent;
            coolantLevel = Mathf.Clamp(coolantLevel - coolantDrop, 0f, 100f);

            float currentKmPerEngine = (coolantLevel <= 0f) ? (kmPerEnginePercent * 0.1f) : kmPerEnginePercent;
            float engineDrop = distanceThisFrame / currentKmPerEngine;
            engineHealth = Mathf.Clamp(engineHealth - engineDrop, 0f, 100f);

            // Tốc độ mòn lốp lúc này đã được lấy từ ScriptableObject của bồ
            float tireDrop = distanceThisFrame / kmPerTirePercent;
            for (int i = 0; i < 4; i++)
            {
                tireHealths[i] = Mathf.Clamp(tireHealths[i] - tireDrop, 0f, 100f);
            }

            badRoadTimer += Time.deltaTime;
            if (badRoadTimer >= badRoadCheckInterval)
            {
                badRoadTimer = 0f;
                if (Random.value <= badRoadDamageChance)
                {
                    int randomTireIndex = Random.Range(0, 4);
                    tireHealths[randomTireIndex] = Mathf.Clamp(tireHealths[randomTireIndex] - badRoadDamageAmount, 0f, 100f);
                }
            }
            OnStatsChanged?.Invoke();
            ApplyDegradationToPhysics();
            UpdateUI();
        }
    }

    public bool IsOverviewPanelOpen => statsCanvasObject != null && statsCanvasObject.activeSelf;

    public void OpenOverviewPanel()
    {
        if (statsCanvasObject) statsCanvasObject.SetActive(true);
    }

    public void ToggleOverviewPanel()
    {
        if (statsCanvasObject) statsCanvasObject.SetActive(!statsCanvasObject.activeSelf);
    }

    public void CloseOverviewPanel()
    {
        if (statsCanvasObject) statsCanvasObject.SetActive(false);
    }

    public void TryRepairEngine()
    {
        if ((qteMinigame != null && qteMinigame.IsPlaying) || (balanceMinigame != null && balanceMinigame.IsPlaying)) return;
        if (engineMinigame != null && engineMinigame.IsEngineOut) StartRepairEngineQTE();
    }

    public void TryRefillCoolant()
    {
        if ((qteMinigame != null && qteMinigame.IsPlaying) || (balanceMinigame != null && balanceMinigame.IsPlaying)) return;
        if (engineMinigame != null && engineMinigame.IsEngineOut) StartRefillCoolantMinigame();
    }

    public void StartRepairEngineQTE() => qteMinigame.BeginQTE("BẢO DƯỠNG ĐỘNG CƠ", RepairEngine);
    public void StartRepairTrunkQTE() => qteMinigame.BeginQTE("NẮN LẠI BẢN LỀ CỐP", RepairTrunk);
    public void StartRefillCoolantMinigame() => balanceMinigame.BeginMinigame("CHÂM NƯỚC MÁT", RefillCoolant);

    public void RepairEngine()
    {
        engineHealth = 100f;
        ApplyDegradationToPhysics();
        OnStatsChanged?.Invoke();
        UpdateUI();
    }

    public void RepairTrunk()
    {
        trunkHealth = 100f;
        OnStatsChanged?.Invoke();
        UpdateUI();
    }

    public void RefillCoolant()
    {
        coolantLevel = 100f;
        OnStatsChanged?.Invoke();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (engineText) engineText.text = $"ĐỘNG CƠ: {Mathf.RoundToInt(engineHealth)}%";
        if (coolantText) coolantText.text = $"NƯỚC MÁT: {Mathf.RoundToInt(coolantLevel)}%";
        for (int i = 0; i < 4; i++)
        {
            if (tireTexts[i] != null) tireTexts[i].text = $"LỐP {i + 1}: {Mathf.RoundToInt(tireHealths[i])}%";
        }
    }

    public void TryInteractTire(int index)
    {
        if (engineMinigame != null && engineMinigame.IsEngineOut) return;
        if (qteMinigame != null && qteMinigame.IsPlaying) return;
        if (balanceMinigame != null && balanceMinigame.IsPlaying) return;

        for (int i = 0; i < 4; i++)
        {
            if (i != index && tireMinigames[i] != null && tireMinigames[i].IsActive) return;
        }

        if (tireMinigames[index] != null) tireMinigames[index].Interact();
    }

    public void RepairTire(int index)
    {
        tireHealths[index] = 100f;
        OnStatsChanged?.Invoke();
        UpdateUI();
    }

    private void ApplyDegradationToPhysics()
    {
        if (vehicleController == null) return;
        float healthRatio = engineHealth / 100f;
        float speedPercent = Mathf.Lerp(0.25f, 1f, healthRatio);
        float torquePercent = Mathf.Lerp(0.3f, 1f, healthRatio);
        vehicleController.ApplyUpgradedEngine(baseMaxSpeed * speedPercent, baseMotorTorque * torquePercent);
    }
}