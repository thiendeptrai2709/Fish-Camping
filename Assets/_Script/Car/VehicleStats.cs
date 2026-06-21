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
    [SerializeField] private EngineRepairMinigame engineMinigame;

    [Header("Link UI")]
    [SerializeField] private GameObject statsCanvasObject;    // Bảng Overview (Bật/tắt bằng Tab)
    [SerializeField] private TextMeshProUGUI engineText;
    [SerializeField] private TextMeshProUGUI trunkText;
    [SerializeField] private TextMeshProUGUI coolantText;

    [Header("Chỉ số độ bền (0% - 100%)")]
    public float engineHealth = 100f;
    public float trunkHealth = 100f;
    public float coolantLevel = 100f;

    public UnityEvent OnStatsChanged;

    private void OnEnable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnToggleStatsUIEvent += ToggleOverviewPanel; // Lắng nghe Tab
            vehicleInput.OnQuickRepairEvent += TryRepairEngineByKeyF; // Lắng nghe F
        }
    }

    private void OnDisable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnToggleStatsUIEvent -= ToggleOverviewPanel;
            vehicleInput.OnQuickRepairEvent -= TryRepairEngineByKeyF;
        }
    }

    private void Start()
    {
        ApplyDegradationToPhysics();
        UpdateUI();
    }

    private void Update()
    {
        if (qteMinigame != null && qteMinigame.IsPlaying)
        {
            // Bị sập nắp Capo HOẶC bấm T cất cục máy đi -> Giật sập QTE!
            bool isCapoClosed = (hoodHinge != null && !hoodHinge.IsFullyOpen);
            bool isEngineHidden = (engineMinigame != null && !engineMinigame.IsEngineOut);

            if (isCapoClosed || isEngineHidden)
            {
                qteMinigame.ForceAbort();
                Debug.Log("Hủy QTE do thay đổi trạng thái khoang máy!");
            }
        }

        if (vehicleController.GetCurrentSpeedKmh() > 5f)
        {
            coolantLevel = Mathf.Clamp(coolantLevel - (Time.deltaTime * 0.5f), 0f, 100f);

            float engineDamageRate = (coolantLevel <= 0f) ? 2.0f : 0.05f;
            engineHealth = Mathf.Clamp(engineHealth - (Time.deltaTime * engineDamageRate), 0f, 100f);
            trunkHealth = Mathf.Clamp(trunkHealth - (Time.deltaTime * 0.02f), 0f, 100f);

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

    // ================= 2. XỬ LÝ PHÍM F =================
    private void TryRepairEngineByKeyF()
    {
        // Kiểm tra an ninh: Capo phải lật hết 100% mới cho gõ búa!
        if (engineMinigame != null && engineMinigame.IsEngineOut)
        {
            StartRepairEngineQTE();
        }
        else
        {
            Debug.LogWarning("Phải bấm T bốc cục máy ra trước mặt đã rồi mới gõ F được!");
        }
    }

    // ================= KÍCH HOẠT QTE =================
    public void StartRepairEngineQTE() => qteMinigame.BeginQTE("BẢO DƯỠNG ĐỘNG CƠ", RepairEngine);
    public void StartRepairTrunkQTE() => qteMinigame.BeginQTE("NẮN LẠI BẢN LỀ CỐP", RepairTrunk);
    public void StartRefillCoolantQTE() => qteMinigame.BeginQTE("CHÂM NƯỚC MÁT", RefillCoolant);

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
        if (trunkText) trunkText.text = $"CỐP XE: {Mathf.RoundToInt(trunkHealth)}%";
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