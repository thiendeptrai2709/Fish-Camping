using UnityEngine;

public class MinimapUIManager : MonoBehaviour
{
    public PlayerInputHandler inputHandler;
    public VehicleInput vehicleInput;
    public GameObject smallMapUI;
    public GameObject expandedMapUI;
    public GameObject minimapCamera; // Camera của map góc màn hình
    public GameObject fullMapCamera; // Camera của map lớn
    public MonoBehaviour freeLookCamera;
    public MonoBehaviour carFreeLookCamera;
    private bool isExpanded = false;
    public PlayerCursor playerCursor;
    private void Update()
    {
        bool isPlayerOpening = (inputHandler != null && inputHandler.ExpandMapTriggered);
        bool isVehicleOpening = (vehicleInput != null && vehicleInput.ExpandMapTriggered);

        if (isPlayerOpening || isVehicleOpening)
        {
            isExpanded = !isExpanded;
            UpdateMapUI();
        }
    }

    private void UpdateMapUI()
    {
        // Bật/tắt UI
        if (smallMapUI != null) smallMapUI.SetActive(!isExpanded);
        if (expandedMapUI != null) expandedMapUI.SetActive(isExpanded);

        // Bật/tắt Camera tương ứng
        if (minimapCamera != null) minimapCamera.SetActive(!isExpanded);
        if (fullMapCamera != null) fullMapCamera.SetActive(isExpanded);

        if (inputHandler != null) inputHandler.IsUIOpen = isExpanded;
        if (vehicleInput != null) vehicleInput.IsUIOpen = isExpanded;

        if (playerCursor != null)
        {
            playerCursor.SetCursorState(!isExpanded);
        }

        ToggleCameraInput(freeLookCamera, !isExpanded);
        ToggleCameraInput(carFreeLookCamera, !isExpanded);
    }
    private void ToggleCameraInput(MonoBehaviour cam, bool isEnabled)
    {
        if (cam != null)
        {
            MonoBehaviour cm3Input = cam.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
            if (cm3Input != null) cm3Input.enabled = isEnabled;

            MonoBehaviour cm2Input = cam.GetComponent("CinemachineInputProvider") as MonoBehaviour;
            if (cm2Input != null) cm2Input.enabled = isEnabled;
        }
    }
    // Hàm này để các Camera ở từng map tự động báo cáo cho Manager khi load xong
    public void SetFullMapCamera(GameObject sceneCamera)
    {
        fullMapCamera = sceneCamera;

        if (fullMapCamera != null)
        {
            // Đảm bảo trạng thái bật/tắt khớp với UI hiện tại khi vừa sang map mới
            fullMapCamera.SetActive(isExpanded);
        }
    }
    public void ForceCloseExpandedMap()
    {
        if (isExpanded)
        {
            isExpanded = false;
            UpdateMapUI();
        }
    }
}