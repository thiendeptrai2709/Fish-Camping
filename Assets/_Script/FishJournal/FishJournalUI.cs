using UnityEngine;

public class FishJournalUI : MonoBehaviour
{
    public static FishJournalUI Instance { get; private set; }

    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private GameObject journalPanel; // Panel chính của UI Sổ tay
    [SerializeField] private MonoBehaviour freeLookCamera; // Camera của Cinemachine

    public bool IsOpen => isOpen;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ẩn panel khi mới vào game
        if (journalPanel != null) journalPanel.SetActive(false);
    }

    private void Update()
    {
        // Đọc phím J từ PlayerInputHandler
        if (playerInput != null && playerInput.JournalTriggered)
        {
            ToggleJournal();
        }
    }

    public void ToggleJournal()
    {
        if (!isOpen)
        {
            if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanOpenJournal())
            {
                return; // Khóa mở Sổ tay cá khi chưa đến bước hướng dẫn
            }

            if (BackpackController.Instance != null && BackpackController.Instance.IsOpen)
                BackpackController.Instance.CloseBackpack();

            if (MapUIManager.Instance != null && MapUIManager.Instance.IsOpen)
                MapUIManager.Instance.CloseMap();

            if (BuildingUIManager.Instance != null && BuildingUIManager.Instance.IsOpen)
                BuildingUIManager.Instance.CloseBuildingUI();

            if (ShopManager.Instance != null && ShopManager.Instance.shopPanel != null && ShopManager.Instance.shopPanel.activeInHierarchy)
                ShopManager.Instance.DongShop();

            FishingController fc = Object.FindFirstObjectByType<FishingController>();
            if (fc != null && fc.IsBusyFishing())
            {
                fc.AutoStowRodToBackpack();
            }
        }

        isOpen = !isOpen;
        SetUIState(isOpen);

        // THÊM ĐOẠN NÀY: Vẽ lại danh sách cá mới nhất mỗi khi mở sổ
        if (isOpen)
        {
            FishJournalGridUI gridUI = GetComponent<FishJournalGridUI>();
            if (gridUI != null)
            {
                gridUI.RefreshGrid();
            }
        }
    }
    private void SetUIState(bool openUI)
    {
        if (playerInput != null) playerInput.IsUIOpen = openUI;
        if (journalPanel != null) journalPanel.SetActive(openUI);

        // Khóa di chuyển
        if (playerMovement != null) playerMovement.enabled = !openUI;

        // Ẩn tâm ngắm
        if (crosshairUI != null) crosshairUI.SetActive(!openUI);

        // Hiện chuột để click chọn cá
        if (playerCursor != null) playerCursor.SetCursorState(!openUI);

        // Khóa xoay Camera
        if (freeLookCamera != null)
        {
            MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
            if (cm3Input != null) cm3Input.enabled = !openUI;

            MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
            if (cm2Input != null) cm2Input.enabled = !openUI;
        }
    }

    private void OnDisable()
    {
        if (isOpen)
        {
            isOpen = false;
            SetUIState(false);
        }
    }
}