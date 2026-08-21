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
    [SerializeField] private GameObject externalHUDHotbar;

    [SerializeField] private MonoBehaviour freeLookCamera;

    public bool IsOpen => isOpen;
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

        // Tối ưu UX: Nếu Cốp xe đang mở, bấm Tab sẽ gọi thẳng lệnh đóng toàn bộ Cốp + Balo
        if (TrunkInventory.CurrentOpenTrunk != null)
        {
            TrunkInventory.CurrentOpenTrunk.ForceCloseAll();
            return;
        }

        // Nếu chuẩn bị mở Balo, tự động đóng các UI toàn màn hình khác (Map, Building, Sổ tay)
        if (!isOpen)
        {
            if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanOpenBackpack())
            {
                return; // Khóa mở Balo khi chưa tới bước hướng dẫn
            }

            if (MapUIManager.Instance != null && MapUIManager.Instance.IsOpen)
                MapUIManager.Instance.CloseMap();

            if (BuildingUIManager.Instance != null && BuildingUIManager.Instance.IsOpen)
                BuildingUIManager.Instance.CloseBuildingUI();

            if (FishJournalUI.Instance != null && FishJournalUI.Instance.IsOpen)
                FishJournalUI.Instance.ToggleJournal();

            if (ShopManager.Instance != null && ShopManager.Instance.shopPanel != null && ShopManager.Instance.shopPanel.activeInHierarchy)
                ShopManager.Instance.DongShop();
        }

        isOpen = !isOpen;
        SetUIState(isOpen, true);
    }

    public void CloseBackpack()
    {
        if (isOpen)
        {
            isOpen = false;
            SetUIState(false, true);
        }
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
        if (externalHUDHotbar != null)
        {
            externalHUDHotbar.SetActive(!openUI);
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