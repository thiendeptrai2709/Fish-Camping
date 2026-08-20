using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private FirebaseAuth auth;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        buttonRegister.onClick.AddListener(RegisterAccountWithFirebase);
        buttonLogin.onClick.AddListener(SignInAccountWithFirebase);
        buttonMoveToRegister.onClick.AddListener(SwitchForm);
        buttonMoveToSignIn.onClick.AddListener(SwitchForm);
    }

    public void RegisterAccountWithFirebase()
    {
        string email = ipRegisterEmail.text;
        string password = ipRegisterPassword.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(Task =>
        {
            if (Task.IsCanceled)
            {
                Debug.LogError("Đăng ký tài khoản bị hủy.");
                return;
            }
            if (Task.IsFaulted)
            {
                Debug.LogError("Đăng ký thất bại: " + Task.Exception.Flatten().InnerExceptions[0].Message);
                return;
            }
            if (Task.IsCompletedSuccessfully)
            {
                // Lấy thông tin user vừa tạo thành công
                var newUser = Task.Result.User;
                Debug.Log($"Đăng ký tài khoản thành công cho: {newUser.Email}");

                // Tự động chuyển sang form Đăng nhập sau khi đăng ký thành công
                SwitchForm();
            }
        });
    }

    public void SignInAccountWithFirebase()
    {
        string email = ipLoginEmail.text;
        string password = ipLoginPassword.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(Task =>
        {
            if (Task.IsCanceled)
            {
                Debug.LogError("Đăng nhập bị hủy.");
                return;
            }
            if (Task.IsFaulted)
            {
                Debug.LogError("Đăng nhập thất bại: " + Task.Exception.Flatten().InnerExceptions[0].Message);
                return;
            }
            if (Task.IsCompletedSuccessfully)
            {
                var user = Task.Result.User;
                Debug.Log($"Đăng nhập thành công cho: {user.Email}");
                string targetScene = PlayerLocationSaveManager.GetSavedSceneName("Map_1_Town");
                SceneManager.LoadScene(targetScene);
            }
        });
    }

    public void SwitchForm()
    {
        if (loginForm.activeSelf)
        {
            loginForm.SetActive(false);
            registerForm.SetActive(true);
        }
        else
        {
            loginForm.SetActive(true);
            registerForm.SetActive(false);
        }
    }
}