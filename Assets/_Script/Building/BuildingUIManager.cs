using UnityEngine;
using UnityEngine.UI;
public class BuildingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject buildingUIPanel;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private MonoBehaviour freeLookCamera;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBuildingUI);
        }
    }
    private void Update()
    {
        if (inputHandler != null && inputHandler.BuildTriggered)
        {
            ToggleBuildingUI();
        }
    }

    public void ToggleBuildingUI()
    {
        if (buildingUIPanel == null) return;

        bool isOpening = !buildingUIPanel.activeSelf;
        buildingUIPanel.SetActive(isOpening);

        if (inputHandler != null)
        {
            inputHandler.IsUIOpen = isOpening;
        }

        if (playerCursor == null)
        {
            playerCursor = FindFirstObjectByType<PlayerCursor>();
        }

        if (isOpening)
        {
            if (playerCursor != null) playerCursor.SetCursorState(false);
            if (placementController != null) placementController.CancelPlacement();
            if (CampBuildZone.Instance != null) CampBuildZone.Instance.ToggleZoneVisual(true);
        }
        else
        {
            if (playerCursor != null) playerCursor.SetCursorState(true);
            if (CampBuildZone.Instance != null) CampBuildZone.Instance.ToggleZoneVisual(false);
        }

        if (freeLookCamera != null)
        {
            MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
            if (cm3Input != null) cm3Input.enabled = !isOpening;

            MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
            if (cm2Input != null) cm2Input.enabled = !isOpening;
        }
    }

    public void SelectItemToBuild(BuildableItemSO item)
    {
        ToggleBuildingUI(); // Đóng UI lại
        if (placementController != null)
        {
            placementController.StartPlacement(item); // Bắt đầu chế độ đặt đồ
        }
    }

    public void CloseBuildingUI()
    {
        if (buildingUIPanel != null && buildingUIPanel.activeSelf)
        {
            ToggleBuildingUI();
        }
    }
}