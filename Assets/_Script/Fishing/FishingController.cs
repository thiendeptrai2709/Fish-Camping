using UnityEngine;

public class FishingController : MonoBehaviour
{
    private enum FishingState { Idle, WindingUp, WaitingForPower, Casting, Fishing, Catching }
    [SerializeField] private EquipmentSlotUI hotbarSlot;
    [SerializeField] private EquipmentSlotUI baitSlot;
    [SerializeField] private EquipmentSlotUI bobberSlot;
    [SerializeField] private CastingMinigameUI castingUI;
    [SerializeField] private GameObject fishingLinePrefab;
    [SerializeField] private BalanceMinigameUI balanceMinigameUI;
    [SerializeField] private GameObject caughtFishPrefab;

    [SerializeField] private float minBiteWaitTime = 3f;
    [SerializeField] private float maxBiteWaitTime = 8f;
    [SerializeField] private float energyCostPerCast = 10f; // Lượng năng lượng tiêu hao mỗi lần quăng câu

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
    [SerializeField] private Transform leftHandFishSocket;
    private CharacterHandVisual handVisual;


    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        inputHandler = GetComponent<PlayerInputHandler>();
        playerInteraction = GetComponent<PlayerInteraction>();
        handVisual = GetComponentInChildren<CharacterHandVisual>();
        energyController = GetComponent<ActivityEnergyController>();
    }
    public bool IsBusyFishing()
    {
        return currentState != FishingState.Idle;
    }
    private void Update()
    {
        if (inputHandler != null && inputHandler.IsUIOpen) return;

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
            Debug.Log("<color=yellow>[Fishing Controller] Bấm Space -> Thả cá đi!</color>");
            ResetToIdle();
        }
    }

    private void HandleLeftClick()
    {
        if (currentState == FishingState.Idle)
        {
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
            }
            else
            {
                string baitStatus = currentBait != null ? "ĐÃ CÓ" : "THIẾU (Hoặc chưa gán Slot)";
                string bobberStatus = currentBobber != null ? "ĐÃ CÓ" : "THIẾU (Hoặc chưa gán Slot)";

                Debug.Log($"<color=yellow>[Fishing Controller] Đã cầm cần nhưng chưa thể quăng! Kiểm tra trang bị đi kèm:\n" +
                          $"• Mồi Câu: {baitStatus}\n" +
                          $"• Phao Câu: {bobberStatus}</color>");
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
            Debug.Log("<color=green>[Fishing Controller] Bấm Chuột Trái -> Cất cá vào Balo!</color>");
            ResetToIdle();
        }
    }

    private void TriggerFishBitingEvent()
    {
        isWaitingForBite = false;
        isFishBiting = true;

        Debug.Log("<color=red>[Fishing Controller] CÁ CẮN CÂU! Kích hoạt Balance Minigame.</color>");

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
        // Dọn dẹp phao và dây câu trước khi chạy animation kết quả
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
            currentState = FishingState.Catching;
            if (playerAnimation != null)
            {
                playerAnimation.TriggerCatchSuccess();
            }
        }
        else
        {
            Debug.Log("<color=red>[Fishing Controller] CÂN BẰNG THẤT BẠI! Chuyển sang animation câu hụt.</color>");
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

            if (targetSocket != null && caughtFishPrefab != null)
            {
                activeCaughtFish = Instantiate(caughtFishPrefab, targetSocket.position, Quaternion.identity, targetSocket);
                activeCaughtFish.transform.localPosition = Vector3.zero;
                activeCaughtFish.transform.localRotation = Quaternion.identity;
            }
        }
    }
    public void OnCatchFailComplete()
    {
        if (currentState == FishingState.Fishing)
        {
            Debug.Log("<color=yellow>[Fishing Controller] Animation buồn đã diễn xong -> Reset về Idle!</color>");
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
            Debug.Log($"<color=orange>[Fishing Controller] Đã tiêu hao {energyCostPerCast} Năng lượng cho cú quăng câu!</color>");
        }

        Debug.Log($"<color=cyan>[Casting Minigame] Kim dừng ở ZONE: {zone} | Tỷ lệ lực bấm: {(powerRatio * 100f):F1}%</color>");

        if (zone == 0)
        {
            Debug.Log("<color=red>[Casting Minigame] Quăng XỊT (Zone 0)! Hủy thao tác ném cần.</color>");
            currentState = FishingState.Idle;
            if (playerAnimation != null)
            {
                playerAnimation.ResetAnimationSpeed();
            }
            return;
        }

        currentState = FishingState.Casting;
        if (playerAnimation != null)
        {
            playerAnimation.ResumeAnimation();
        }

        float finalPower = currentRod != null ? currentRod.CalculateDamageToFish() * powerRatio : 0f;

        currentCastZone = zone;

        // Tính khoảng cách dựa theo hệ số của Zone trong Data Cần Câu
        if (currentRod != null && currentRod.zoneDistanceMultipliers != null && zone < currentRod.zoneDistanceMultipliers.Length)
        {
            currentThrowDistance = currentRod.castDistance * currentRod.zoneDistanceMultipliers[zone];
        }
        else
        {
            currentThrowDistance = currentRod != null ? currentRod.castDistance * (zone / 3f) : 0f;
        }

        string baitName = currentBait != null ? currentBait.itemName : "Không có";
        string bobberName = currentBobber != null ? currentBobber.itemName : "Phao mặc định";

        // --- DEBUG LOG KHI QUĂNG THÀNH CÔNG ---
        Debug.Log($"<color=green>[Casting Minigame] Quăng THÀNH CÔNG (Zone {zone})!\n" +
                  $"• Cần: {currentRod.itemName} | Mồi: {baitName} | Phao: {bobberName}\n" +
                  $"• Lực kéo: {finalPower:F1} | Khoảng cách ném: {currentThrowDistance:F1}m</color>");

        // Không dùng Invoke đoán thời gian nữa, chờ Animation Event (OnAnimationCastRelease) gọi
    }

    private void EnterFishingState()
    {
        currentState = FishingState.Fishing;
        Debug.Log("<color=cyan>[Fishing Controller] Bắt đầu vào trạng thái: ĐANG CÂU (Fishing).</color>");

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

            Vector3 castDirection = Camera.main.transform.forward;
            castDirection.y = 0f;
            castDirection.Normalize();

            Vector3 targetPos = transform.position + castDirection * currentThrowDistance;
            targetPos.y = transform.position.y;

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
                    Debug.Log($"<color=cyan>[Fishing Line] Đã nối dây từ: {tipTransform.name} ({tipTransform.position}) tới Phao ({activeBobberEntity.transform.position})</color>");
                }
            }

            float dynamicDuration = Mathf.Max(0.5f, Mathf.Sqrt(currentThrowDistance) * 0.35f);
            float dynamicHeight = Mathf.Max(1f, currentThrowDistance * 0.2f * (currentCastZone * 0.5f));

            activeBobberEntity.Cast(startPos, targetPos, dynamicDuration, dynamicHeight, activeLineVisual);
        }
        isFishBiting = false;
        isWaitingForBite = true;
        reelInCooldown = 1.0f;
        biteTimer = Random.Range(minBiteWaitTime, maxBiteWaitTime);

        if (currentRod != null && currentRod.waitTimeReductionPercentage > 0f)
        {
            biteTimer *= (1f - Mathf.Clamp01(currentRod.waitTimeReductionPercentage / 100f));
        }
        Debug.Log($"<color=cyan>[Fishing Controller] Đã thả phao! Thời gian chờ cá cắn ngẫu nhiên: {biteTimer:F1} giây.</color>");
    }
    private void ResetToIdle()
    {
        CancelInvoke(nameof(ResetToIdle));
        isWaitingForBite = false;
        isFishBiting = false;
        currentState = FishingState.Idle;
        Debug.Log("<color=white>[Fishing Controller] Về trạng thái ban đầu: IDLE.</color>");

        if (balanceMinigameUI != null)
        {
            balanceMinigameUI.ForceStopMinigame();
        }

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

        if (activeCaughtFish != null)
        {
            Destroy(activeCaughtFish);
            activeCaughtFish = null;
        }

        if (playerAnimation != null)
        {
            playerAnimation.SetFishingState(false);
        }

        if (handVisual != null)
        {
            handVisual.SetBobberVisualActive(true);
        }
    }
    public void OnAnimationCastRelease()
    {
        // Chỉ kích hoạt bay phao nếu đang ở trạng thái chuẩn bị ném (Casting)
        if (currentState == FishingState.Casting)
        {
            EnterFishingState();
        }
    }
}