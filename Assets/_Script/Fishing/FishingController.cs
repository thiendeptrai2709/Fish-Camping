using UnityEngine;

public class FishingController : MonoBehaviour
{
    private enum FishingState { Idle, WindingUp, WaitingForPower, Casting, Fishing }

    [SerializeField] private EquipmentSlotUI hotbarSlot;
    [SerializeField] private EquipmentSlotUI baitSlot;
    [SerializeField] private EquipmentSlotUI bobberSlot;
    [SerializeField] private CastingMinigameUI castingUI;

    [SerializeField] private GameObject fishingLinePrefab;

    private PlayerAnimation playerAnimation;
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

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        inputHandler = GetComponent<PlayerInputHandler>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }
    public bool IsBusyFishing()
    {
        return currentState != FishingState.Idle;
    }
    private void Update()
    {
        if (inputHandler != null && inputHandler.InteractTriggered)
        {
            HandleLeftClick();
        }
    }

    private void HandleLeftClick()
    {
        if (currentState == FishingState.Idle)
        {
            currentRod = (hotbarSlot != null && hotbarSlot.GetEquippedItem() != null)
                ? hotbarSlot.GetEquippedItem().GetItemShape() as FishingRodSO : null;

            if (currentRod == null) return;

            currentBait = (baitSlot != null && baitSlot.GetEquippedItem() != null)
                ? baitSlot.GetEquippedItem().GetItemShape() as BaitSO : null;

            currentBobber = (bobberSlot != null && bobberSlot.GetEquippedItem() != null)
                ? bobberSlot.GetEquippedItem().GetItemShape() as BobberSO : null;

            if (currentBait != null && currentBobber != null)
            {
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
            Debug.Log("<color=yellow>[Fishing Controller] Người chơi chủ động thu cần về ban đầu.</color>");
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

        // --- DEBUG LOG CHUNG ---
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

        CharacterHandVisual handVisual = GetComponentInChildren<CharacterHandVisual>();
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
    }

    private void ResetToIdle()
    {
        currentState = FishingState.Idle;
        Debug.Log("<color=white>[Fishing Controller] Về trạng thái ban đầu: IDLE.</color>");

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

        if (playerAnimation != null)
        {
            playerAnimation.SetFishingState(false);
        }

        CharacterHandVisual handVisual = GetComponentInChildren<CharacterHandVisual>();
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