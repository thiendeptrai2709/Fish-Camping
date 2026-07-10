using UnityEngine;

public class BackpackController : MonoBehaviour
{
    public static BackpackController Instance { get; private set; }

    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private GameObject backpackPanel;
    [SerializeField] private GameObject hotbarPanel;

    [SerializeField] private MonoBehaviour freeLookCamera;

    private bool isOpen = false;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Update()
    {
        if (playerInput != null && playerInput.BackpackTriggered)
        {
            ToggleBackpack();
        }
    }

    public void ToggleBackpack()
    {
        // Tối ưu UX: Nếu Nồi Nấu đang mở, bấm Tab sẽ gọi thẳng lệnh đóng Nồi (hàm này đã tự dọn dẹp và đóng cả Balo)
        if (CookingUIManager.Instance != null && CookingUIManager.Instance.IsOpen())
        {
            CookingUIManager.Instance.CloseCookingUI();
            return;
        }

        isOpen = !isOpen;
        SetUIState(isOpen, true);
    }

    public void OpenForCooking(bool open)
    {
        isOpen = open;
        SetUIState(isOpen, false);
    }

    private void SetUIState(bool openUI, bool showHotbar)
    {
        if (playerInput != null)
        {
            playerInput.IsUIOpen = openUI;
        }

        if (backpackPanel != null)
        {
            backpackPanel.SetActive(openUI);
        }

        if (hotbarPanel != null)
        {
            hotbarPanel.SetActive(openUI && showHotbar);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = !openUI;
        }

        if (crosshairUI != null)
        {
            crosshairUI.SetActive(!openUI);
        }

        if (playerCursor != null)
        {
            playerCursor.SetCursorState(!openUI);
        }
        if (freeLookCamera != null)
        {
            // Dò tìm và tắt Input của Cinemachine 3 (Unity 6 mặc định)
            MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
            if (cm3Input != null) cm3Input.enabled = !openUI;

            // Dò tìm và tắt Input của Cinemachine 2 (Phiên bản cũ hơn)
            MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
            if (cm2Input != null) cm2Input.enabled = !openUI;
        }
        //Time.timeScale = openUI ? 0f : 1f;
    }

    private void OnDisable()
    {
        //Time.timeScale = 1f;
    }
}