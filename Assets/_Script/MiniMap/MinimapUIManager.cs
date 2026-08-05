using UnityEngine;

public class MinimapUIManager : MonoBehaviour
{
    public PlayerInputHandler inputHandler;
    public GameObject smallMapUI;
    public GameObject expandedMapUI;
    public GameObject minimapCamera; // Camera của map góc màn hình
    public GameObject fullMapCamera; // Camera của map lớn

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
    }
}