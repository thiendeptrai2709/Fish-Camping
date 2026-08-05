using UnityEngine;

public class MapFocusManager : MonoBehaviour
{
    [SerializeField] private MinimapUIManager minimapManager;

    public void FocusOnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            FocusCamera(player.transform.position);
        }
    }

    public void FocusOnTag(string targetTag)
    {
        GameObject target = GameObject.FindGameObjectWithTag(targetTag);
        if (target != null)
        {
            FocusCamera(target.transform.position);
        }
    }

    public void FocusOnName(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        if (target != null)
        {
            FocusCamera(target.transform.position);
        }
    }

    private void FocusCamera(Vector3 position)
    {
        if (minimapManager != null && minimapManager.fullMapCamera != null)
        {
            FullMapCameraController camController = minimapManager.fullMapCamera.GetComponent<FullMapCameraController>();
            if (camController != null)
            {
                camController.FocusOnPosition(position);
            }
        }
    }
}