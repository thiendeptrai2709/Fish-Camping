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

    private void CheckForNpc()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance, npcLayer, QueryTriggerInteraction.Ignore);

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;

                INpcInteractable npcInteractable = hit.collider.GetComponent<INpcInteractable>();
                if (npcInteractable == null)
                {
                    npcInteractable = hit.collider.GetComponentInParent<INpcInteractable>();
                }

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

    private void HandleNpcInput()
    {
        // Kiểm tra xem Dialogue Canvas có đang mở sẵn không (nếu có thì dùng Click chuột trái để tua chữ tiếp theo)
        bool dialogueActive = GameObject.Find("DialogueCanvas") != null && GameObject.Find("DialogueCanvas").activeInHierarchy;

        if (inputHandler != null && inputHandler.InteractTriggered)
        {
            if (dialogueActive && DialogueManager.Instance != null)
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