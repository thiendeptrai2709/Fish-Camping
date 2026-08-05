using UnityEngine;

public class MinimapUIManager : MonoBehaviour
{
    public PlayerInputHandler inputHandler;
    public GameObject smallMapUI;
    public GameObject expandedMapUI;
    public GameObject minimapCamera; // Camera của map góc màn hình
    public GameObject fullMapCamera; // Camera của map lớn
    public MonoBehaviour freeLookCamera;

    private bool isExpanded = false;

    private void Update()
    {
        if (inputHandler != null && inputHandler.ExpandMapTriggered)
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

        if (inputHandler != null)
        {
            inputHandler.IsUIOpen = isExpanded;
        }
        if (freeLookCamera != null)
        {
            MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
            if (cm3Input != null) cm3Input.enabled = !isExpanded;

            MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
            if (cm2Input != null) cm2Input.enabled = !isExpanded;
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
}