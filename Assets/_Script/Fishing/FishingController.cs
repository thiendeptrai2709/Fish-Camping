using UnityEngine;

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

    // Biến chống spam click gây lỗi game
    private float inputCooldown = 0f;
    private float catchingLockTimer = 0f;

    [Header("--- AUTO-STOW FISHING ROD ---")]
    [SerializeField] private float autoStowDistance = 20f;
    private float zoneCheckTimer = 0f;
    private FishingZone[] cachedFishingZones;
    private float lastZoneCacheTime = -10f;

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

    public bool IsBusyFishing()
    {
        return currentState != FishingState.Idle;
    }

    private void Update()
    {
        if (inputHandler != null && inputHandler.IsUIOpen) return;

        // Giảm thời gian đếm ngược chống spam
        if (inputCooldown > 0f) inputCooldown -= Time.deltaTime;
        if (catchingLockTimer > 0f) catchingLockTimer -= Time.deltaTime;

        // Tự động kiểm tra và cất cần câu vào Balo khi rời khỏi khu vực câu cá
        zoneCheckTimer -= Time.deltaTime;
        if (zoneCheckTimer <= 0f)
        {
            zoneCheckTimer = 0.5f;
            CheckAutoStowRodWhenLeavingFishingArea();
        }

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
                ForcedTutorialManager.Instance?.NotifyKeepOrReleaseFish();
                ResetToIdle();
            }
        }
    }

    private void HandleLeftClick()
    {
        // Khóa click chuột quá nhanh chống hỏng chuột
        if (inputCooldown > 0f) return;
        inputCooldown = 0.3f;

        if (currentState == FishingState.Idle)
        {
            if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanStartFishing()) return;
            if (playerInteraction != null && playerInteraction.HasActiveInteractable()) return;

            currentRod = (hotbarSlot != null && hotbarSlot.GetEquippedItem() != null)
                ? hotbarSlot.GetEquippedItem().GetItemShape() as FishingRodSO : null;

            if (currentRod == null) return;

            currentBait = (baitSlot != null && baitSlot.GetEquippedItem() != null)
                ? baitSlot.GetEquippedItem().GetItemShape() as BaitSO : null;

            currentBobber = (bobberSlot != null && bobberSlot.GetEquippedItem() != null)
                ? bobberSlot.GetEquippedItem().GetItemShape() as BobberSO : null;

            if (currentBait != null && currentBobber != null)
            {
                if (energyController != null)
                {
                    CharacterStatsManager statsManager = energyController.statsManager;
                    if (statsManager != null && statsManager.GetStatValue(StatType.Energy) <= 0)
                    {
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
            ResetToIdle();
        }
        else if (currentState == FishingState.Catching)
        {
            // ==========================================
            // FIX BUG: CHẶN CẤT CÁ KHI CHƯA HIỆN MODEL
            // ==========================================
            if (activeCaughtFish == null)
            {
                // Nếu model cá chưa được Instantiate ra tay -> Bỏ qua lệnh cất cá!
                return;
            }

            // Vẫn giữ khóa 1.5 giây dự phòng nếu bồ muốn ngắm cá lâu hơn
            if (catchingLockTimer > 0f) return;

            if (currentCaughtFishData != null && BackpackMinigameUI.Instance != null)
            {
                bool added = BackpackMinigameUI.Instance.TryAutoAddFish(currentCaughtFishData, caughtFishLength, caughtFishWeight, caughtFishGrade);
                if (added)
                {
                    Debug.Log($"<color=green>[Fishing Controller] Đã cất [{currentCaughtFishData.itemName}] vào Balo!</color>");
                }
                else
                {
                    Debug.Log("<color=red>[Fishing Controller] Balo đầy! Không thể cất cá, đã thả đi.</color>");
                }
            }
            else
            {
                Debug.Log("<color=yellow>[Fishing Controller] Thiếu data cá hoặc chưa có BackpackMinigameUI trong Scene -> Thả cá đi!</color>");
            }
            ForcedTutorialManager.Instance?.NotifyKeepOrReleaseFish();
            ResetToIdle();
        }
    }

    private void TriggerFishBitingEvent()
    {
        isWaitingForBite = false;
        isFishBiting = true;
        ForcedTutorialManager.Instance?.NotifyReelFish();

        if (currentFishingZone != null)
        {
            currentCaughtFishData = currentFishingZone.GetRandomFish();
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

        // --- FIX: ÉP BUỘC PHÁT TIẾNG GIẰNG CO LIÊN TỤC ---
        if (fishingAudioSource != null && reelingStruggleSound != null)
        {
            fishingAudioSource.clip = reelingStruggleSound;
            fishingAudioSource.loop = true;
            fishingAudioSource.Play();
            Debug.Log("<color=green>[Audio] Đang phát tiếng kéo cá giằng co!</color>");
        }

        if (balanceMinigameUI != null)
        {
            balanceMinigameUI.StartMinigame(inputHandler, this);
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

            if (currentCaughtFishData != null && FishJournalManager.Instance != null)
            {
                currentCaughtFishData.GenerateRandomSize(out caughtFishLength, out caughtFishWeight);
                caughtFishGrade = currentCaughtFishData.GenerateRandomGrade();

                bool isNewRecord = FishJournalManager.Instance.RecordCatch(currentCaughtFishData.itemID, caughtFishLength, caughtFishWeight, caughtFishGrade);

                if (isNewRecord)
                {
                    Debug.Log($"<color=yellow>[Sổ Tay] KỶ LỰC MỚI!</color>");
                }
            }

            currentState = FishingState.Catching;
            catchingLockTimer = 1.5f; // Khóa thao tác 1.5s để người chơi ngắm cá
            ForcedTutorialManager.Instance?.NotifyFishCaught();
            if (playerAnimation != null)
            {
                playerAnimation.TriggerCatchSuccess();
            }
        }
        else
        {
            Debug.Log("<color=red>[Fishing Controller] CÂN BẰNG THẤT BẠI!</color>");
            currentState = FishingState.Failed;
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
    }

    public void OnCatchSuccessIntroComplete()
    {
        if (currentState == FishingState.Catching)
        {
            if (activeCaughtFish != null) return;

            Transform targetSocket = leftHandFishSocket;
            if (targetSocket == null && handVisual != null)
            {
                targetSocket = handVisual.GetTipSocketTransform();
            }

            GameObject prefabToSpawn = (currentCaughtFishData != null && currentCaughtFishData.caughtFishPrefab != null)
                  ? currentCaughtFishData.caughtFishPrefab : null;

            if (targetSocket != null && prefabToSpawn != null)
            {
                activeCaughtFish = Instantiate(prefabToSpawn, targetSocket.position, Quaternion.identity, targetSocket);
                activeCaughtFish.transform.localPosition = Vector3.zero;
                activeCaughtFish.transform.localRotation = Quaternion.identity;
            }
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
        currentState = FishingState.WindingUp;
        if (playerAnimation != null)
        {
            playerAnimation.TriggerCastAnimation();
        }
    }

    public void OnWindUpPaused()
    {
        if (currentState == FishingState.WindingUp)
        {
            currentState = FishingState.WaitingForPower;
            if (castingUI != null)
            {
                castingUI.StartMinigame();
            }
        }
    }

    private void ExecuteCast()
    {
        if (castingUI == null) return;

        castingUI.StopMinigame(out int zone, out float powerRatio);

        if (energyController != null)
        {
            energyController.TryConsumeEnergy(energyCostPerCast);
        }

        if (zone == 0)
        {
            currentState = FishingState.Idle;
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
                biteTimer = Random.Range(minBiteWaitTime, maxBiteWaitTime);

                if (currentRod != null && currentRod.waitTimeReductionPercentage > 0f)
                {
                    biteTimer *= (1f - Mathf.Clamp01(currentRod.waitTimeReductionPercentage / 100f));
                }
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
        StopStruggleSound(); // Tắt luôn âm thanh khi reset trạng thái

        isWaitingForBite = false;
        isFishBiting = false;
        currentCaughtFishData = null;
        currentFishingZone = null;
        currentState = FishingState.Idle;

        if (balanceMinigameUI != null) balanceMinigameUI.ForceStopMinigame();
        if (activeLineVisual != null) { Destroy(activeLineVisual.gameObject); activeLineVisual = null; }
        if (activeBobberEntity != null) { Destroy(activeBobberEntity.gameObject); activeBobberEntity = null; }
        if (activeCaughtFish != null) { Destroy(activeCaughtFish); activeCaughtFish = null; }
        if (playerAnimation != null) playerAnimation.SetFishingState(false);
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
        if (currentState == FishingState.Casting)
        {
            EnterFishingState();
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

    private void CheckAutoStowRodWhenLeavingFishingArea()
    {
        if (hotbarSlot == null) return;
        InventoryItemUI equippedRod = hotbarSlot.GetEquippedItem();
        if (equippedRod == null) return;

        // Nếu đang câu cá (quăng dây, giằng co, kéo cá) thì không cất
        if (currentState != FishingState.Idle) return;

        bool isNearLake = false;

        // 1. Kiểm tra Raycast nước dưới chân hoặc trước mặt
        int waterMask = waterLayer.value;
        if (waterMask == 0) waterMask = LayerMask.GetMask("Water");
        if (waterMask == 0) waterMask = 1 << 4;

        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, waterMask))
        {
            isNearLake = true;
        }
        else if (Physics.Raycast(transform.position + Vector3.up * 2f + transform.forward * 4f, Vector3.down, out RaycastHit hitFwd, 6f, waterMask))
        {
            isNearLake = true;
        }

        // 2. Kiểm tra khoảng cách tới các FishingZone trong Scene
        if (!isNearLake)
        {
            var zones = GetAllFishingZones();
            if (zones != null && zones.Length > 0)
            {
                for (int i = 0; i < zones.Length; i++)
                {
                    var zone = zones[i];
                    if (zone == null) continue;
                    Collider col = zone.GetComponent<Collider>();
                    if (col != null)
                    {
                        Vector3 closest = col.ClosestPoint(transform.position);
                        if (Vector3.Distance(transform.position, closest) <= autoStowDistance)
                        {
                            isNearLake = true;
                            break;
                        }
                    }
                    else if (Vector3.Distance(transform.position, zone.transform.position) <= autoStowDistance)
                    {
                        isNearLake = true;
                        break;
                    }
                }
            }
        }

        // Nếu người chơi đã đi xa khỏi bờ hồ -> Tự động cất cần câu vào Balo!
        if (!isNearLake)
        {
            AutoStowRodToBackpack(equippedRod);
        }
    }

    public void AutoStowRodToBackpack(InventoryItemUI rodItem = null)
    {
        if (hotbarSlot == null) return;
        if (rodItem == null) rodItem = hotbarSlot.GetEquippedItem();
        if (rodItem == null) return;

        if (BackpackMinigameUI.Instance != null && BackpackMinigameUI.Instance.TryAutoFitItemToGrid(rodItem))
        {
            hotbarSlot.RemoveEquippedItem();
            Debug.Log("<color=green>[FishingController] Đã tự động cất cần câu vào Balo khi rời khỏi khu vực câu cá.</color>");
        }
    }
}