using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerInputHandler inputHandler;
    private PlayerMovement playerMovement;

    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int isHoldingItemHash = Animator.StringToHash("IsHoldingItem");
    private readonly int castTriggerHash = Animator.StringToHash("Cast");
    private readonly int isFishingHash = Animator.StringToHash("IsFishing");
    private readonly int fishBiteHash = Animator.StringToHash("FishBite");

    private readonly int catchSuccessHash = Animator.StringToHash("CatchSuccess");
    private readonly int catchFailHash = Animator.StringToHash("CatchFail");

    private FishingController fishingController;

    private readonly int isDrivingBoolHash = Animator.StringToHash("IsDriving");
    private readonly int jumpTriggerHash = Animator.StringToHash("Jump");
    private readonly int isGroundedBoolHash = Animator.StringToHash("IsGrounded");

    public System.Action onEnterCarComplete;
    public System.Action onExitCarComplete;

    private float currentAnimationSpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();
        playerMovement = GetComponent<PlayerMovement>();
        fishingController = GetComponent<FishingController>();
    }

    public void SetHoldingItemState(bool isHolding)
    {
        if (animator != null)
        {
            animator.SetBool(isHoldingItemHash, isHolding);
        }
    }

    public void TriggerCastAnimation()
    {
        if (animator != null)
        {
            animator.speed = 1f;
            if (animator.layerCount > 1) animator.SetLayerWeight(1, 0f);
            animator.SetBool(isFishingHash, false);
            animator.ResetTrigger(castTriggerHash);
            animator.SetTrigger(castTriggerHash);
            animator.CrossFadeInFixedTime("FishingThrow", 0.1f, 0);
        }
    }

    public void ResumeAnimation()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    public void ResetAnimationSpeed()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    public void OnCastWindUpComplete()
    {
        if (animator != null && fishingController != null && fishingController.IsWaitingForPower())
        {
            animator.speed = 0f;
        }
        if (fishingController != null)
        {
            fishingController.OnWindUpPaused();
        }
    }

    public void PauseWindUpPose()
    {
        if (animator != null && fishingController != null && fishingController.IsWaitingForPower())
        {
            animator.speed = 0f;
        }
    }

    private void Update()
    {
        float targetSpeed = 0f;

        bool isDialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;
        bool isFishing = fishingController != null && fishingController.IsBusyFishing();

        // 1. Đồng bộ trọng số RightHandLayer: Khi câu cá -> Layer 1 = 0 để animation câu cá toàn thân kiểm soát 100%
        if (animator != null && animator.layerCount > 1)
        {
            bool isHolding = animator.GetBool(isHoldingItemHash);
            float targetLayer1Weight = (isFishing || isDriving) ? 0f : (isHolding ? 1f : 0f);
            float currentWeight = animator.GetLayerWeight(1);
            animator.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, targetLayer1Weight, Time.deltaTime * 20f));
        }

        if (playerMovement != null && playerMovement.enabled && !playerMovement.IsMovementLocked && !inputHandler.IsUIOpen && !isDialogueActive && !isFishing)
        {
            if (inputHandler.MoveInput.magnitude > 0.1f)
            {
                bool isActuallySprinting = inputHandler.IsSprinting && playerMovement.CanSprint;
                targetSpeed = isActuallySprinting ? 1f : 0.5f;
            }
        }

        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetSpeed, Time.deltaTime * 25f);
        if (Mathf.Abs(currentAnimationSpeed - targetSpeed) < 0.02f) currentAnimationSpeed = targetSpeed;
        if (animator != null) animator.SetFloat(speedHash, currentAnimationSpeed);
    }

    public void ResetAllFishingTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(castTriggerHash);
            animator.ResetTrigger(fishBiteHash);
            animator.ResetTrigger(catchSuccessHash);
            animator.ResetTrigger(catchFailHash);
        }
    }

    public void SetFishingState(bool isFishing)
    {
        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool(isFishingHash, isFishing);

            if (isFishing)
            {
                if (animator.layerCount > 1) animator.SetLayerWeight(1, 0f);
                animator.CrossFadeInFixedTime("FIshingIdle", 0.15f, 0);
            }
            else
            {
                ResetAllFishingTriggers();
                animator.CrossFadeInFixedTime("Locomotion", 0.2f, 0);
            }
        }
    }

    public void OnCastRelease()
    {
        if (fishingController != null)
        {
            fishingController.OnAnimationCastRelease();
        }
    }

    public void TriggerFishBite()
    {
        if (animator != null)
        {
            animator.speed = 1f;
            if (animator.layerCount > 1) animator.SetLayerWeight(1, 0f);
            animator.SetBool(isFishingHash, true);
            animator.ResetTrigger(fishBiteHash);
            animator.SetTrigger(fishBiteHash);
            animator.CrossFadeInFixedTime("FishBiting", 0.12f, 0);
        }
    }

    public void TriggerCatchSuccess()
    {
        if (animator != null)
        {
            animator.speed = 1f;
            if (animator.layerCount > 1) animator.SetLayerWeight(1, 0f);
            animator.SetBool(isFishingHash, true);
            animator.ResetTrigger(catchSuccessHash);
            animator.SetTrigger(catchSuccessHash);
            animator.CrossFadeInFixedTime("FishingSuccesBegin", 0.1f, 0);
        }
    }

    public void TriggerCatchFail()
    {
        if (animator != null)
        {
            animator.speed = 1f;
            if (animator.layerCount > 1) animator.SetLayerWeight(1, 0f);
            animator.SetBool(isFishingHash, false);
            animator.ResetTrigger(catchFailHash);
            animator.SetTrigger(catchFailHash);
            animator.CrossFadeInFixedTime("FishingMiss", 0.12f, 0);
        }
    }

    public void OnCatchFailAnimationComplete()
    {
        if (fishingController != null)
        {
            fishingController.OnCatchFailComplete();
        }
    }
    [Header("Steering Wheel IK")]
    [SerializeField] private Transform leftHandGripTarget;
    [SerializeField] private Transform rightHandGripTarget;
    [SerializeField] private float ikBlendSpeed = 6f;

    private float drivingIKWeight = 0f;
    private bool isDriving = false;

    public void OnCatchSuccessIntroComplete()
    {
        if (fishingController != null)
        {
            fishingController.OnCatchSuccessIntroComplete();
        }
    }

    public void SetDrivingState(bool driving)
    {
        isDriving = driving;
        if (animator != null) animator.SetBool(isDrivingBoolHash, driving);
        if (!driving)
        {
            leftHandGripTarget = null;
            rightHandGripTarget = null;
        }
    }

    public void TriggerJump()
    {
        if (animator != null) animator.SetTrigger(jumpTriggerHash);
    }

    public void SetGrounded(bool isGrounded)
    {
        if (animator != null) animator.SetBool(isGroundedBoolHash, isGrounded);
    }

    public void SetSteeringGrips(Transform leftGrip, Transform rightGrip)
    {
        leftHandGripTarget = leftGrip;
        rightHandGripTarget = rightGrip;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // Cập nhật trọng số IK mượt mà
        float targetWeight = (isDriving && (leftHandGripTarget != null || rightHandGripTarget != null)) ? 1f : 0f;
        drivingIKWeight = Mathf.MoveTowards(drivingIKWeight, targetWeight, Time.deltaTime * ikBlendSpeed);

        if (drivingIKWeight > 0.001f)
        {
            // Tay trái bám vào cạnh trái vô lăng
            if (leftHandGripTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, drivingIKWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, drivingIKWeight);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandGripTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandGripTarget.rotation);
            }

            // Tay phải bám vào cạnh phải vô lăng
            if (rightHandGripTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, drivingIKWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, drivingIKWeight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandGripTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandGripTarget.rotation);
            }
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }
    }
}