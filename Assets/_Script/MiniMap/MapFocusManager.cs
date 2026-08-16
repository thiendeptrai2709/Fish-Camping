using UnityEngine;

public class MapFocusManager : MonoBehaviour
{
    [SerializeField] private MinimapUIManager minimapManager;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject mapTargetMarker;
    private Vector3 currentSelectedPosition;
    private void OnDisable()
    {
        if (mapTargetMarker != null) mapTargetMarker.SetActive(false);
    }

    private void ShowTargetMarker(Vector3 position)
    {
        if (mapTargetMarker != null)
        {
            mapTargetMarker.SetActive(true);
            mapTargetMarker.transform.position = new Vector3(position.x, position.y + 5f, position.z);
        }
    }

    public void FocusOnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            ShowTargetMarker(player.transform.position);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            InstantFocusCamera(player.transform.position);
        }
    }

    public void FocusOnTag(string targetTag)
    {
        GameObject target = GameObject.FindGameObjectWithTag(targetTag);
        if (target != null)
        {
            ShowTargetMarker(target.transform.position);
            SmoothFocusCamera(target.transform.position);

            // Báo cho Tutorial đã bấm icon NPC Shop trên map
            ForcedTutorialManager.Instance?.NotifyShopIconClicked();
        }
    }

    public void FocusOnName(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        if (target != null)
        {
            ShowTargetMarker(target.transform.position);
            SmoothFocusCamera(target.transform.position);

            // Báo cho Tutorial đã bấm icon NPC Shop trên map
            ForcedTutorialManager.Instance?.NotifyShopIconClicked();
        }
    }

    private void InstantFocusCamera(Vector3 position)
    {
        currentSelectedPosition = position; // Lưu lại vị trí

        if (minimapManager != null && minimapManager.fullMapCamera != null)
        {
            FullMapCameraController camController = minimapManager.fullMapCamera.GetComponent<FullMapCameraController>();
            if (camController != null)
            {
                camController.FocusOnPosition(position);
            }
        }
    }

    private void SmoothFocusCamera(Vector3 position)
    {
        currentSelectedPosition = position;

        if (minimapManager != null && minimapManager.fullMapCamera != null)
        {
            FullMapCameraController camController = minimapManager.fullMapCamera.GetComponent<FullMapCameraController>();
            if (camController != null)
            {
                if (confirmPanel != null) confirmPanel.SetActive(false); // Ẩn panel cũ trước khi trôi

                // Gọi trôi mượt và chờ chạy xong (onComplete) thì bật panel
                camController.SmoothFocusOnPosition(position, () =>
                {
                    if (confirmPanel != null) confirmPanel.SetActive(true);
                });
            }
        }
    }
    public void CloseConfirmPanel()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (mapTargetMarker != null) mapTargetMarker.SetActive(false);
    }

    public void AcceptNavigation()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (mapTargetMarker != null) mapTargetMarker.SetActive(false);

        if (NavigationArrow.Instance != null)
        {
            NavigationArrow.Instance.StartNavigation(currentSelectedPosition);
        }

        if (minimapManager != null)
        {
            minimapManager.ForceCloseExpandedMap();
        }
    }
}