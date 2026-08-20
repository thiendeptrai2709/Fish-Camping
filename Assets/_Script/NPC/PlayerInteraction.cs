using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerNpcInteraction : MonoBehaviour
{
    [Header("NPC Raycast Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask npcLayer; // Ông nên đặt riêng 1 Layer cho NPC (Ví dụ: Layer "NPC" hoặc chung "Interactable")
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color highlightCrosshairColor = Color.yellow;
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private InteractionPromptUI promptUI;

    private PlayerInputHandler inputHandler;
    private PlayerMovement playerMovement;
    private Transform cameraTransform;
    private INpcInteractable currentNpcInteractable;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        playerMovement = GetComponent<PlayerMovement>();
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        CheckForNpc();
        HandleNpcInput();
    }

    private static readonly RaycastHit[] npcHitBuffer = new RaycastHit[8];

    private void CheckForNpc()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        int hitCount = Physics.RaycastNonAlloc(ray, npcHitBuffer, interactDistance, npcLayer, QueryTriggerInteraction.Ignore);

        if (hitCount > 0)
        {
            // Sắp xếp theo khoảng cách gần nhất (Non-alloc)
            for (int i = 0; i < hitCount - 1; i++)
            {
                int minIdx = i;
                for (int j = i + 1; j < hitCount; j++)
                {
                    if (npcHitBuffer[j].distance < npcHitBuffer[minIdx].distance) minIdx = j;
                }
                if (minIdx != i)
                {
                    RaycastHit temp = npcHitBuffer[i];
                    npcHitBuffer[i] = npcHitBuffer[minIdx];
                    npcHitBuffer[minIdx] = temp;
                }
            }

            for (int i = 0; i < hitCount; i++)
            {
                var hitCol = npcHitBuffer[i].collider;
                if (hitCol == null || hitCol.isTrigger) continue;

                INpcInteractable npcInteractable = hitCol.GetComponent<INpcInteractable>() ?? hitCol.GetComponentInParent<INpcInteractable>();

                if (npcInteractable != null)
                {
                    if (npcInteractable != currentNpcInteractable)
                    {
                        if (currentNpcInteractable != null)
                        {
                            currentNpcInteractable.OnLoseFocus();
                        }

                        currentNpcInteractable = npcInteractable;
                        currentNpcInteractable.OnFocus();
                    }

                    if (crosshairImage != null) crosshairImage.color = highlightCrosshairColor;
                    if (promptUI != null) promptUI.DisplayPrompt(true, currentNpcInteractable.GetInteractPrompt());
                    return;
                }
            }
        }

        ClearCurrentNpc();
    }

    private void ClearCurrentNpc()
    {
        if (currentNpcInteractable != null)
        {
            currentNpcInteractable.OnLoseFocus();
            currentNpcInteractable = null;
            if (crosshairImage != null) crosshairImage.color = defaultCrosshairColor;
            if (promptUI != null) promptUI.DisplayPrompt(false, "");
        }
    }

    private float lastNpcInteractTime = 0f;
    private const float NPC_INTERACT_COOLDOWN = 0.2f;

    private void HandleNpcInput()
    {
        if (inputHandler != null && inputHandler.InteractTriggered)
        {
            if (Time.unscaledTime - lastNpcInteractTime < NPC_INTERACT_COOLDOWN)
            {
                return;
            }
            lastNpcInteractTime = Time.unscaledTime;

            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.DisplayNextSentence();
            }
            else if (currentNpcInteractable != null)
            {
                MonoBehaviour targetNpc = currentNpcInteractable as MonoBehaviour;
                if (targetNpc != null && playerMovement != null)
                {
                    playerMovement.FaceTarget(targetNpc.transform.position);
                }

                currentNpcInteractable.Interact();
            }
        }
    }

    public bool HasActiveNpc()
    {
        return currentNpcInteractable != null;
    }
}