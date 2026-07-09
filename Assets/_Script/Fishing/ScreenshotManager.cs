using System.IO;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
    private PlayerInputHandler inputHandler;
    private string screenshotDirectory;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();

        // Tạo đường dẫn thư mục "Screenshots" nằm cùng cấp với thư mục Assets (khi chạy Editor) 
        // hoặc nằm cùng cấp với file chạy .exe (khi đã Build game)
        screenshotDirectory = Path.Combine(Application.dataPath, "../Screenshots");

        if (!Directory.Exists(screenshotDirectory))
        {
            Directory.CreateDirectory(screenshotDirectory);
        }
    }

    private void Update()
    {
        if (inputHandler != null && inputHandler.ScreenshotTriggered)
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        // Đặt tên file ảnh theo ngày giờ chi tiết để không bao giờ bị trùng tên
        string fileName = $"Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        string filePath = Path.Combine(screenshotDirectory, fileName);

        // Hàm chụp ảnh màn hình mặc định của Unity (superSize = 1 là chụp đúng độ phân giải hiện tại)
        ScreenCapture.CaptureScreenshot(filePath, 1);

        Debug.Log($"<color=green>[Screenshot Manager] Đã chụp ảnh màn hình và lưu tại: {filePath}</color>");
    }
}