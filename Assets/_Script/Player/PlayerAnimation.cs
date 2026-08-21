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
            animator.SetTrigger(castTriggerHash);
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
        if (animator != null)
        {
            animator.speed = 0f;
        }
        if (fishingController != null)
        {
            fishingController.OnWindUpPaused();
        }
    }
    private void Update()
    {
        float targetSpeed = 0f;

        bool isDialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;
        bool isFishing = fishingController != null && fishingController.IsBusyFishing();

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
        animator.SetFloat(speedHash, currentAnimationSpeed);
    }
    public void SetFishingState(bool isFishing)
    {
        if (animator != null)
        {
            animator.SetBool(isFishingHash, isFishing);
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
            animator.SetTrigger(fishBiteHash);
        }
    }
    public void TriggerCatchSuccess()
    {
        if (animator != null)
        {
            animator.SetTrigger(catchSuccessHash);
        }
    }

    public void TriggerCatchFail()
    {
        if (animator != null)
        {
            animator.SetTrigger(catchFailHash);
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