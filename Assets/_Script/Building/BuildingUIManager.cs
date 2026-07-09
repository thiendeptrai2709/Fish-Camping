using UnityEngine;

public class BuildingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject buildingUIPanel;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private PlayerCursor playerCursor;
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
        }
        else
        {
            if (playerCursor != null) playerCursor.SetCursorState(true);
        }
    }

    // Hàm này sẽ được gán vào sự kiện OnClick của các nút (Button) trên UI
    public void SelectItemToBuild(BuildableItemSO item)
    {
        ToggleBuildingUI(); // Đóng UI lại
        if (placementController != null)
        {
            placementController.StartPlacement(item); // Bắt đầu chế độ đặt đồ
        }
    }
}