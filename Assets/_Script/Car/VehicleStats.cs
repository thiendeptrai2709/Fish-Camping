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

    private TireData cachedTireData;

    // --- HÀM TỰ ĐỘNG LẤY CHỈ SỐ TỪ SCRIPTABLE OBJECT CỦA LỐP ĐANG TRANG BỊ ---
    public void UpdateTireStats()
    {
        int equippedTire = PlayerPrefs.GetInt("EquippedTireIndex", -1);

        // Kiểm tra xem Gara có tồn tại và lốp đang lắp có nằm trong danh sách lốp không
        if (GarageZone.Instance != null && equippedTire >= 0 && equippedTire < GarageZone.Instance.allTires.Length)
        {
            // Trích xuất file Data Lốp tương ứng
            cachedTireData = GarageZone.Instance.allTires[equippedTire];

            if (cachedTireData != null)
            {
                // Áp dụng độ bền từ file vào xe
                kmPerTirePercent = cachedTireData.kmPerTirePercent > 0 ? cachedTireData.kmPerTirePercent : 3f;
                badRoadDamageChance = cachedTireData.badRoadDamageChance;
                Debug.Log($"<color=green>[VehicleStats] Đã nạp thành công thông số lốp: {cachedTireData.tireName}</color>");
            }
        }
        else
        {
            // LẮP LỐP MẶC ĐỊNH (Khi chưa mua gì): Thông số cơ bản
            cachedTireData = null;
            kmPerTirePercent = 3f; // 3km mất 1%
            badRoadDamageChance = 0.1f; // 10% rách lốp
        }

        ApplyDegradationToPhysics();
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
        if (index >= 0 && index < 4)
        {
            tireHealths[index] = 100f;
        }
        ApplyDegradationToPhysics();
        OnStatsChanged?.Invoke();
        UpdateUI();
    }

    private void ApplyDegradationToPhysics()
    {
        if (vehicleController == null) return;

        // 1. TÍNH TOÁN ĐỘ BÁM ĐƯỜNG THEO TỪNG MAP TỪ TIRE DATA
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        float rawMapFriction = 0.15f; // Mặc định nếu không có lốp xịn

        if (cachedTireData != null)
        {
            if (activeScene.Contains("Map_1_Town") || activeScene.Contains("Town") || activeScene.Contains("City"))
            {
                rawMapFriction = cachedTireData.cityFriction;
            }
            else if (activeScene.Contains("Map_2_PineLake") || activeScene.Contains("PineLake") || activeScene.Contains("Forest"))
            {
                rawMapFriction = cachedTireData.forestFriction;
            }
            else if (activeScene.Contains("Map_3_Swamp") || activeScene.Contains("Swamp") || activeScene.Contains("DamLay"))
            {
                rawMapFriction = cachedTireData.swampFriction;
            }
            else if (activeScene.Contains("Map_4_Ocean") || activeScene.Contains("Ocean") || activeScene.Contains("Beach") || activeScene.Contains("Coast"))
            {
                rawMapFriction = cachedTireData.sandFriction;
            }
            else
            {
                rawMapFriction = cachedTireData.cityFriction;
            }
        }

        // Quy đổi độ bám thô (0.07 -> 0.88) sang hệ số độ bám ma sát bánh xe (0.55 -> 1.35)
        float terrainGripMultiplier = Mathf.Lerp(0.55f, 1.35f, Mathf.Clamp01(rawMapFriction / 0.85f));

        // 2. TÍNH TOÁN ĐỘ BÁM CỦA TỪNG BÁNH XE THEO ĐỘ BỀN (MÁU LỐP)
        // Khi lốp tụt về 0% (xẹp lốp), độ bám giảm mạnh xuống còn 35%
        float flGrip = terrainGripMultiplier * Mathf.Lerp(0.35f, 1.0f, tireHealths[0] / 100f);
        float frGrip = terrainGripMultiplier * Mathf.Lerp(0.35f, 1.0f, tireHealths[1] / 100f);
        float rlGrip = terrainGripMultiplier * Mathf.Lerp(0.35f, 1.0f, tireHealths[2] / 100f);
        float rrGrip = terrainGripMultiplier * Mathf.Lerp(0.35f, 1.0f, tireHealths[3] / 100f);

        // 3. TÍNH ĐỘ LỆCH LÁI KHI XẸP LỐP MỘT BÊN
        float leftSideHealth = (tireHealths[0] + tireHealths[2]) * 0.5f;
        float rightSideHealth = (tireHealths[1] + tireHealths[3]) * 0.5f;
        // Nếu lốp bên trái bị xẹp hơn bên phải, sinh ra góc kéo lệch lái nhẹ sang trái (-2.5 độ)
        float steerBias = (rightSideHealth - leftSideHealth) * -0.025f;

        // 4. TÍNH TỔNG QUAN HIỆU NĂNG XE (ĐỘNG CƠ + LỐP)
        float engineRatio = engineHealth / 100f;
        float avgTireHealth = (tireHealths[0] + tireHealths[1] + tireHealths[2] + tireHealths[3]) * 0.25f;
        float tireHealthRatio = avgTireHealth / 100f;

        // Lốp xẹp tăng lực cản lăn, giảm tốc độ tối đa tối đa 35%
        float speedPercent = Mathf.Lerp(0.25f, 1f, engineRatio) * Mathf.Lerp(0.65f, 1f, tireHealthRatio);
        // Địa hình xấu trơn trượt làm giảm lực truyền động hữu dụng
        float torquePercent = Mathf.Lerp(0.3f, 1f, engineRatio) * Mathf.Lerp(0.7f, 1.15f, terrainGripMultiplier);

        vehicleController.ApplyUpgradedEngine(baseMaxSpeed * speedPercent, baseMotorTorque * torquePercent);
        vehicleController.ApplyTirePhysics(flGrip, frGrip, rlGrip, rrGrip, steerBias);
    }
}