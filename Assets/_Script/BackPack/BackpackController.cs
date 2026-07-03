using UnityEngine;

public class BackpackController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private GameObject backpackPanel;

    private bool isOpen = false;

    private void Update()
    {
        if (playerInput != null && playerInput.BackpackTriggered)
        {
            ToggleBackpack();
        }
    }

    public void ToggleBackpack()
    {
        isOpen = !isOpen;

        if (playerInput != null)
        {
            playerInput.IsUIOpen = isOpen;
        }

        if (backpackPanel != null)
        {
            backpackPanel.SetActive(isOpen);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = !isOpen;
        }

        if (crosshairUI != null)
        {
            crosshairUI.SetActive(!isOpen);
        }

        if (playerCursor != null)
        {
            playerCursor.SetCursorState(!isOpen);
        }

        Time.timeScale = isOpen ? 0f : 1f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}