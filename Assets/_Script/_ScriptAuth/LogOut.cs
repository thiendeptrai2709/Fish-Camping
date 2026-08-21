using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogOut : MonoBehaviour
{
    // Hàm gọi khi nhấn nút "Log Out"
    public void LogOutButtonPressed()
    {
        // 1. Gọi lệnh đăng xuất của Firebase
        try
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[LogOut] Lỗi khi SignOut Firebase: {e.Message}");
        }

        // 2. Xóa session UID lưu trong máy để yêu cầu đăng nhập lại
        PlayerPrefs.DeleteKey("Firebase_User_UID");
        PlayerPrefs.DeleteKey("Last_Active_User_UID");
        PlayerPrefs.Save();

        Debug.Log("<color=yellow>[LogOut] Đã đăng xuất và xóa phiên làm việc thành công!</color>");

        // 3. Chuyển hướng người dùng quay trở lại LoginScene
        SceneManager.LoadScene("LoginScene");
    }
}
