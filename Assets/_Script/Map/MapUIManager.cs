using UnityEngine;
using UnityEngine.SceneManagement;

public class MapUIManager : MonoBehaviour
{
    [SerializeField] private GameObject mapUIPanel;
    [SerializeField] private MapInteractionManager mapInteractionManager;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private VehicleInput vehicleInput;
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private MonoBehaviour freeLookCamera;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /* Đóng map và trả lại điều khiển khi load xong scene mới */
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mapUIPanel != null && mapUIPanel.activeSelf)
        {
            ToggleMap();
        }
    }

    private void Start()
    {
        if (mapUIPanel != null)
        {
            mapUIPanel.SetActive(false);
        }
    }

    private void Update()
    {
        bool isPlayerMapTriggered = playerInputHandler != null && playerInputHandler.enabled && playerInputHandler.MapTriggered;
        bool isVehicleMapTriggered = vehicleInput != null && vehicleInput.enabled && vehicleInput.MapTriggered;

        if (isPlayerMapTriggered || isVehicleMapTriggered)
        {
            ToggleMap();
        }
    }
    public void OpenMap()
    {
        if (mapUIPanel != null && !mapUIPanel.activeSelf)
        {
            ToggleMap();
        }
    }
    private void ToggleMap()
    {
        if (mapUIPanel != null)
        {
            bool isActive = !mapUIPanel.activeSelf;
            mapUIPanel.SetActive(isActive);
            playerInputHandler.IsUIOpen = isActive;

            /* Reset trạng thái phóng to bản đồ khi mở lên */
            if (isActive && mapInteractionManager != null)
            {
                mapInteractionManager.RestoreMapInstantly();
            }

            if (playerMovement != null)
            {
                playerMovement.enabled = !isActive;
            }

            if (freeLookCamera != null)
            {
                MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
                if (cm3Input != null) cm3Input.enabled = !isActive;

                MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
                if (cm2Input != null) cm2Input.enabled = !isActive;
            }
            if (vehicleInput != null)
            {
                vehicleInput.IsUIOpen = isActive;
            }

            if (playerCursor != null)
            {
                playerCursor.SetCursorState(!isActive);
            }
        }
    }
}