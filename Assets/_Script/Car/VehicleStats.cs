using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class VehicleStats : MonoBehaviour
{
    [Header("Link Hệ thống")]
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private VehicleInput vehicleInput;       // Đọc phím Tab & F
    [SerializeField] private ProceduralHinge hoodHinge;       // Check bản lề Capo
    [SerializeField] private QTEMinigame qteMinigame;
    [SerializeField] private BalanceMinigame balanceMinigame;
    [SerializeField] private EngineRepairMinigame engineMinigame;

    [Header("Link UI")]
    [SerializeField] private GameObject statsCanvasObject;    // Bảng Overview (Bật/tắt bằng Tab)
    [SerializeField] private TextMeshProUGUI engineText;
    [SerializeField] private TextMeshProUGUI coolantText;

    [Header("Chỉ số độ bền (0% - 100%)")]
    public float engineHealth = 100f;
    public float trunkHealth = 100f;
    public float coolantLevel = 100f;

    [Header("Cấu hình hao hụt (Số Km để mất 1%)")]
    [SerializeField] private float kmPerEnginePercent = 5f;
    [SerializeField] private float kmPerCoolantPercent = 2f;

    public UnityEvent OnStatsChanged;

    private void OnEnable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnToggleStatsUIEvent += ToggleOverviewPanel;
            vehicleInput.OnQuickRepairEvent += TryRepairEngineByKeyF;
            vehicleInput.OnRefillCoolantEvent += TryRefillCoolantByKeyG;
        }
    }

    private void OnDisable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnToggleStatsUIEvent -= ToggleOverviewPanel;
            vehicleInput.OnQuickRepairEvent -= TryRepairEngineByKeyF;
            vehicleInput.OnRefillCoolantEvent -= TryRefillCoolantByKeyG;
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