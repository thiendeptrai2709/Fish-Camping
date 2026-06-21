using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class VehicleStats : MonoBehaviour
{

    [SerializeField] private TireRepairMinigame[] tireMinigames = new TireRepairMinigame[4];

    [Header("Link Hệ thống")]
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private VehicleInput vehicleInput;       // Đọc phím Tab & F
    [SerializeField] private ProceduralHinge hoodHinge;       // Check bản lề Capo
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
    [SerializeField] private float kmPerTirePercent = 3f;

    [Header("Cấu hình rủi ro đường xấu (RNG Lốp)")]
    [SerializeField] private float badRoadCheckInterval = 2f; // Cứ 2s chạy xe thì tung xúc xắc 1 lần
    [SerializeField] private float badRoadDamageChance = 0.1f; // Tỉ lệ 10% bị thủng
    [SerializeField] private float badRoadDamageAmount = 5f; // Tụt 5% nếu xui
    private float badRoadTimer;

    public UnityEvent OnStatsChanged;

    private void OnEnable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnToggleStatsUIEvent += ToggleOverviewPanel;
            vehicleInput.OnQuickRepairEvent += TryRepairEngineByKeyF;
            vehicleInput.OnRefillCoolantEvent += TryRefillCoolantByKeyG;

            vehicleInput.OnInteractTire1 += () => TryInteractTire(0);
            vehicleInput.OnInteractTire2 += () => TryInteractTire(1);
            vehicleInput.OnInteractTire3 += () => TryInteractTire(2);
            vehicleInput.OnInteractTire4 += () => TryInteractTire(3);
        }
    }

    private void OnDisable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnToggleStatsUIEvent -= ToggleOverviewPanel;
            vehicleInput.OnQuickRepairEvent -= TryRepairEngineByKeyF;
            vehicleInput.OnRefillCoolantEvent -= TryRefillCoolantByKeyG;

            vehicleInput.OnInteractTire1 -= () => TryInteractTire(0);
            vehicleInput.OnInteractTire2 -= () => TryInteractTire(1);
            vehicleInput.OnInteractTire3 -= () => TryInteractTire(2);
            vehicleInput.OnInteractTire4 -= () => TryInteractTire(3);
        }
    }

    private void Start()
    {
        ApplyDegradationToPhysics();
        UpdateUI();
    }

    private void Update()
    {
        bool isCapoClosed = (hoodHinge != null && !hoodHinge.IsFullyOpen);
        bool isEngineHidden = (engineMinigame != null && !engineMinigame.IsEngineOut);

        if (isCapoClosed || isEngineHidden)
        {
            if (qteMinigame != null && qteMinigame.IsPlaying) qteMinigame.ForceAbort();
            if (balanceMinigame != null && balanceMinigame.IsPlaying) balanceMinigame.ForceAbort();
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

    // ================= 1. XỬ LÝ PHÍM TAB =================
    private void ToggleOverviewPanel()
    {
        if (statsCanvasObject)
            statsCanvasObject.SetActive(!statsCanvasObject.activeSelf);
    }

    private void TryRepairEngineByKeyF()
    {
        bool isQTEPlaying = (qteMinigame != null && qteMinigame.IsPlaying);
        bool isBalancePlaying = (balanceMinigame != null && balanceMinigame.IsPlaying);

        if (isQTEPlaying || isBalancePlaying) return;

        if (engineMinigame != null && engineMinigame.IsEngineOut)
        {
            StartRepairEngineQTE();
        }
    }

    private void TryRefillCoolantByKeyG()
    {
        bool isQTEPlaying = (qteMinigame != null && qteMinigame.IsPlaying);
        bool isBalancePlaying = (balanceMinigame != null && balanceMinigame.IsPlaying);

        if (isQTEPlaying || isBalancePlaying) return;

        if (engineMinigame != null && engineMinigame.IsEngineOut)
        {
            StartRefillCoolantMinigame();
        }
    }
    // ================= KÍCH HOẠT QTE =================
    public void StartRepairEngineQTE() => qteMinigame.BeginQTE("BẢO DƯỠNG ĐỘNG CƠ", RepairEngine);
    public void StartRepairTrunkQTE() => qteMinigame.BeginQTE("NẮN LẠI BẢN LỀ CỐP", RepairTrunk);
    public void StartRefillCoolantMinigame() => balanceMinigame.BeginMinigame("CHÂM NƯỚC MÁT", RefillCoolant);
    // ================= HẬU QTE (HỒI MÁU) =================
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
            if (tireTexts[i] != null)
            {
                tireTexts[i].text = $"LỐP {i + 1}: {Mathf.RoundToInt(tireHealths[i])}%";
            }
        }
    }

    private void TryInteractTire(int index)
    {
        if (tireMinigames[index] != null)
        {
            tireMinigames[index].Interact();
        }
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

        vehicleController.ApplyUpgradedEngine(120f * speedPercent, 3800f * torquePercent);
    }
}