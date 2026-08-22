using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class Login : MonoBehaviour
{
    //Đăng Ký
    [Header("Đăng Ký")]
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;
    public Button buttonRegister;

    //Đăng Nhập
    [Header("Đăng Nhập")]
    public InputField ipLoginEmail;
    public InputField ipLoginPassword;
    public Button buttonLogin;

    [Header("Chuyển đổi trạng thái giữa Đăng ký và Đăng nhập")]
    public Button buttonMoveToRegister;
    public Button buttonMoveToSignIn;

    public GameObject loginForm;
    public GameObject registerForm;

    [Header("--- Thông Báo Trạng Thái UI (Tùy chọn) ---")]
    [Tooltip("Kéo Text hoặc TextMeshProUGUI hiển thị thông báo lỗi/thành công vào đây")]
    public Text statusTextLegacy;
    public TextMeshProUGUI statusTMP;

    private FirebaseAuth auth;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (buttonRegister != null) buttonRegister.onClick.AddListener(RegisterAccountWithFirebase);
        if (buttonLogin != null) buttonLogin.onClick.AddListener(SignInAccountWithFirebase);
        if (buttonMoveToRegister != null) buttonMoveToRegister.onClick.AddListener(SwitchForm);
        if (buttonMoveToSignIn != null) buttonMoveToSignIn.onClick.AddListener(SwitchForm);

        // 1. Tự động điền email đã lưu lần trước nếu có
        string savedEmail = PlayerPrefs.GetString("Saved_Login_Email", "");
        if (!string.IsNullOrEmpty(savedEmail))
        {
            if (ipLoginEmail != null) ipLoginEmail.text = savedEmail;
            if (ipRegisterEmail != null) ipRegisterEmail.text = savedEmail;
        }

        // 2. Kiểm tra nếu đã có phiên đăng nhập Firebase hợp lệ
        if (auth.CurrentUser != null)
        {
            SaveUserSession(auth.CurrentUser);
            Debug.Log($"[Firebase] Tự động nhận diện phiên đăng nhập: {auth.CurrentUser.Email} (UID: {auth.CurrentUser.UserId})");
        }
    }

    private void SaveUserSession(FirebaseUser user)
    {
        if (user == null) return;

        PlayerPrefs.SetString("Last_Active_User_UID", user.UserId);
        PlayerPrefs.SetString("Firebase_User_UID", user.UserId);
        PlayerPrefs.SetString("Firebase_User_Email", user.Email ?? "");
        PlayerPrefs.SetString("Saved_Login_Email", user.Email ?? "");
        PlayerPrefs.Save();
        Debug.Log($"<color=green>[Firebase] Đã lưu thông tin xác thực thành công cho UID: {user.UserId}</color>");
    }

    public void RegisterAccountWithFirebase()
    {
        string email = ipRegisterEmail != null ? ipRegisterEmail.text.Trim() : "";
        string password = ipRegisterPassword != null ? ipRegisterPassword.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus(GetLocalizedText("auth_enter_all_fields", "Vui lòng nhập đầy đủ Email và Mật khẩu!"), Color.red);
            return;
        }

        if (password.Length < 6)
        {
            ShowStatus(GetLocalizedText("auth_password_too_short", "Mật khẩu phải có ít nhất 6 ký tự!"), Color.red);
            return;
        }

        ShowStatus(GetLocalizedText("auth_registering", "Đang tạo tài khoản..."), Color.yellow);

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                ShowStatus(GetLocalizedText("auth_cancelled", "Đăng ký bị hủy."), Color.red);
                return;
            }
            if (task.IsFaulted)
            {
                string errMsg = task.Exception != null && task.Exception.Flatten().InnerExceptions.Count > 0
                    ? task.Exception.Flatten().InnerExceptions[0].Message
                    : "Lỗi không xác định";
                Debug.LogError("Đăng ký thất bại: " + errMsg);
                ShowStatus(GetLocalizedAuthError(errMsg, false), Color.red);
                return;
            }
            if (task.IsCompletedSuccessfully)
            {
                var newUser = task.Result.User;
                Debug.Log($"<color=cyan>[Firebase] Đăng ký tài khoản mới thành công: {newUser.Email} (UID: {newUser.UserId})</color>");

                // 1. XÓA SẠCH TOÀN BỘ DỮ LIỆU CŨ TRÊN MÁY CHO TÀI KHOẢN MỚI
                GameDataResetManager.ResetAllGameData(reloadScene: false);

                // 2. Lưu session người dùng mới
                SaveUserSession(newUser);

                if (ipLoginEmail != null) ipLoginEmail.text = email;
                if (ipLoginPassword != null) ipLoginPassword.text = password;

                ShowStatus(GetLocalizedText("auth_register_success", "Đăng ký thành công! Đang chuyển sang đăng nhập..."), Color.green);

                // Tự động chuyển sang form Đăng nhập sau khi đăng ký thành công
                SwitchForm();
            }
        });
    }

    public void SignInAccountWithFirebase()
    {
        string email = ipLoginEmail != null ? ipLoginEmail.text.Trim() : "";
        string password = ipLoginPassword != null ? ipLoginPassword.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus(GetLocalizedText("auth_enter_all_fields", "Vui lòng nhập đầy đủ Email và Mật khẩu!"), Color.red);
            return;
        }

        ShowStatus(GetLocalizedText("auth_signing_in", "Đang đăng nhập..."), Color.yellow);

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                ShowStatus(GetLocalizedText("auth_cancelled", "Đăng nhập bị hủy."), Color.red);
                return;
            }
            if (task.IsFaulted)
            {
                string errMsg = task.Exception != null && task.Exception.Flatten().InnerExceptions.Count > 0
                    ? task.Exception.Flatten().InnerExceptions[0].Message
                    : "Lỗi không xác định";
                Debug.LogError("Đăng nhập thất bại: " + errMsg);
                ShowStatus(GetLocalizedAuthError(errMsg, true), Color.red);
                return;
            }
            if (task.IsCompletedSuccessfully)
            {
                var user = task.Result.User;
                Debug.Log($"Đăng nhập thành công cho: {user.Email} (UID: {user.UserId})");

                // Nếu phát hiện đăng nhập bằng một tài khoản khác với tài khoản trước trên máy
                string lastActiveUID = PlayerPrefs.GetString("Last_Active_User_UID", "");
                if (string.IsNullOrEmpty(lastActiveUID) || lastActiveUID != user.UserId)
                {
                    Debug.Log($"<color=yellow>[Firebase] Phát hiện đăng nhập tài khoản khác (UID cũ: '{lastActiveUID}' != UID mới: '{user.UserId}'). Khởi tạo dữ liệu sạch cho tài khoản mới...</color>");
                    GameDataResetManager.ResetAllGameData(reloadScene: false);
                }

                // Lưu toàn bộ session thông tin user vào PlayerPrefs
                SaveUserSession(user);

                ShowStatus(GetLocalizedText("auth_login_success", "Đăng nhập thành công! Đang đồng bộ dữ liệu..."), Color.green);

                if (GameDatabaseManager.Instance != null)
                {
                    GameDatabaseManager.Instance.LoadFromCloud(user.UserId, (success) =>
                    {
                        string targetScene = PlayerLocationSaveManager.GetSavedSceneName("Map_1_Town");
                        StartCoroutine(LoginAndTransition(targetScene));
                    });
                }
                else
                {
                    string targetScene = PlayerLocationSaveManager.GetSavedSceneName("Map_1_Town");
                    StartCoroutine(LoginAndTransition(targetScene));
                }
            }
        });
    }

    private IEnumerator LoginAndTransition(string targetScene)
    {
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(0.35f);
        }
        SceneManager.LoadScene(targetScene);
    }

    public void SwitchForm()
    {
        if (loginForm == null || registerForm == null) return;

        bool isLoginActive = loginForm.activeSelf;
        loginForm.SetActive(!isLoginActive);
        registerForm.SetActive(isLoginActive);

        // Xóa thông báo cũ khi đổi form
        ShowStatus("", Color.white);
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusTMP != null)
        {
            statusTMP.text = message;
            statusTMP.color = color;
            statusTMP.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        if (statusTextLegacy != null)
        {
            statusTextLegacy.text = message;
            statusTextLegacy.color = color;
            statusTextLegacy.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }

    private string GetLocalizedAuthError(string rawFirebaseError, bool isLogin)
    {
        bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        string lower = rawFirebaseError.ToLowerInvariant();

        if (lower.Contains("email-already-in-use") || lower.Contains("already in use") || lower.Contains("already exists"))
        {
            return isVietnamese ? "Email này đã được sử dụng!" : "This email is already in use!";
        }
        if (lower.Contains("invalid-email") || lower.Contains("badly formatted") || lower.Contains("invalid email"))
        {
            return isVietnamese ? "Định dạng Email không hợp lệ!" : "Invalid email address format!";
        }
        if (lower.Contains("wrong-password") || lower.Contains("invalid-credential") || lower.Contains("invalid password"))
        {
            return isVietnamese ? "Sai mật khẩu hoặc email không tồn tại!" : "Incorrect password or account not found!";
        }
        if (lower.Contains("user-not-found"))
        {
            return isVietnamese ? "Không tìm thấy tài khoản với email này!" : "No user found with this email!";
        }
        if (lower.Contains("weak-password"))
        {
            return isVietnamese ? "Mật khẩu quá yếu! Tối thiểu 6 ký tự." : "Password is too weak! Minimum 6 characters.";
        }
        if (lower.Contains("network") || lower.Contains("connection"))
        {
            return isVietnamese ? "Lỗi kết nối mạng, vui lòng thử lại!" : "Network error, please check connection!";
        }

        return isLogin 
            ? (isVietnamese ? "Đăng nhập thất bại: " + rawFirebaseError : "Login failed: " + rawFirebaseError)
            : (isVietnamese ? "Đăng ký thất bại: " + rawFirebaseError : "Registration failed: " + rawFirebaseError);
    }

    private string GetLocalizedText(string key, string fallbackText)
    {
        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable("Game Text");
            if (table != null)
            {
                var entry = table.GetEntry(key);
                if (entry != null) return entry.GetLocalizedString();
            }
            return fallbackText;
        }
        catch
        {
            return fallbackText;
        }
    }
}