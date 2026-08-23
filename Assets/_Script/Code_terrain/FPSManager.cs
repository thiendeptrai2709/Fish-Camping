using UnityEngine;

public class FPSManager : MonoBehaviour
{
    [Header("Cài đặt FPS (Nhập 120 hoặc -1 để không giới hạn)")]
    public int targetFPS = 120; // 120 FPS

    [Header("Hiển thị FPS lên màn hình")]
    public bool showFPS = true;

    private float deltaTime = 0.0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SetTargetFPS()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS > 0 ? targetFPS : 120;
    }

    void Start()
    {
        // 1. TẮT V-SYNC (Bắt buộc): Nếu không tắt, Unity sẽ tự khóa FPS theo tần số màn hình
        QualitySettings.vSyncCount = 0;

        // 2. ÉP FPS: Mức 120 FPS
        Application.targetFrameRate = targetFPS > 0 ? targetFPS : 120;
    }

    void Update()
    {
        if (showFPS)
        {
            // Tính toán thời gian giữa các khung hình để suy ra FPS thực tế
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        // Vẽ bộ đếm FPS lên góc trái màn hình khi chạy game trong Unity Editor
        if (showFPS)
        {
            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(20, 20, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 50;

            float fps = 1.0f / deltaTime;

            // Nếu FPS > 50 thì chữ màu xanh, dưới 50 thì chữ màu đỏ báo động
            if (fps >= 50f)
                style.normal.textColor = Color.green;
            else
                style.normal.textColor = Color.red;

            string text = string.Format("{0:0.} FPS", fps);
            GUI.Label(rect, text, style);
        }
    }
#endif
}