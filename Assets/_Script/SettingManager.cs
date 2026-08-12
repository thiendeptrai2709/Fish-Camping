using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;

public class SettingManager : MonoBehaviour
{
    [Header("--- UI Panel ---")]
    [SerializeField] private GameObject settingPanel;

    [Header("--- Audio ---")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("--- Localization ---")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        // --- Khởi tạo Âm thanh ---
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        if (bgmSlider != null) bgmSlider.value = savedBGM;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);

        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // --- Khởi tạo Ngôn ngữ ---
        int savedLangID = PlayerPrefs.GetInt("LanguageID", 0);
        StartCoroutine(SetLocale(savedLangID));

        if (languageDropdown != null)
        {
            languageDropdown.value = savedLangID;
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSetting();
        }
    }

    public void ToggleSetting()
    {
        if (settingPanel != null)
        {
            bool isActive = !settingPanel.activeSelf;
            settingPanel.SetActive(isActive);
            Cursor.visible = isActive;
            Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void SetBGMVolume(float value)
    {
        float dB = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
        if (audioMixer != null) audioMixer.SetFloat("BGMVolume", dB);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        float dB = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
        if (audioMixer != null) audioMixer.SetFloat("SFXVolume", dB);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void OnLanguageChanged(int index)
    {
        StartCoroutine(SetLocale(index));
    }

    private IEnumerator SetLocale(int localeID)
    {
        yield return LocalizationSettings.InitializationOperation;

        if (localeID < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
            PlayerPrefs.SetInt("LanguageID", localeID);
        }
    }

    // ================= CHỨC NĂNG MỚI THÊM =================

    // 1. Hàm bật/tắt chế độ toàn màn hình (Cửa sổ)
    public void ToggleFullScreen()
    {
        // Nếu đang full thì đổi thành cửa sổ, và ngược lại
        Screen.fullScreen = !Screen.fullScreen;

        if (Screen.fullScreen)
        {
            Debug.Log("Đã chuyển sang chế độ: CỬA SỔ");
        }
        else
        {
            Debug.Log("Đã chuyển sang chế độ: TOÀN MÀN HÌNH");
        }
    }

    // 2. Hàm thoát game
    public void QuitGame()
    {
        Debug.Log("Đang thoát game...");
        Application.Quit();
    }
}