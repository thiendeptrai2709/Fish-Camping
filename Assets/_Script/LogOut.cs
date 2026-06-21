using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogOut : MonoBehaviour
{
    // Hàm gọi khi nhấn nút "Log Out"
    public void LogOutButtonPressed()
    {
        // 1. Gọi lệnh đăng xuất của Firebase
        FirebaseAuth.DefaultInstance.SignOut();

        Debug.Log("Đã đăng xuất thành công!");

        // 2. Chuyển hướng người dùng quay trở lại LoginScene
        // Thay "LoginScene" bằng tên chính xác Scene đăng nhập của bạn
        SceneManager.LoadScene(1);
    }
}
