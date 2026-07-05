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

    private FishingController fishingController;


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

        if (playerMovement != null && playerMovement.enabled)
        {
            if (inputHandler.MoveInput.magnitude > 0.1f)
            {
                targetSpeed = inputHandler.IsSprinting ? 1f : 0.5f;
            }
        }

        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetSpeed, Time.deltaTime * 10f);
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
}