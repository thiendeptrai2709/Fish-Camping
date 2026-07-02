using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerInputHandler inputHandler;
    private PlayerMovement playerMovement;

    private readonly int speedHash = Animator.StringToHash("Speed");
    private float currentAnimationSpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();
        playerMovement = GetComponent<PlayerMovement>();
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
}