using UnityEngine;

public class SceneFullMapCamera : MonoBehaviour
{
    private void Start()
    {
        // Tìm Manager đang được DontDestroyOnLoad bảo vệ từ map cũ sang
        MinimapUIManager manager = FindFirstObjectByType<MinimapUIManager>();

        if (manager != null)
        {
            // Tự động gán chính camera này vào hệ thống UI
            manager.SetFullMapCamera(gameObject);
        }
    }
}