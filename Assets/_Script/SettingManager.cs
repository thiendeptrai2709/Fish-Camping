using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;

public class SettingManager : MonoBehaviour
{
    [Header("--- UI Panel ---")]
    [SerializeField] private GameObject settingPanel; // Kéo Panel Setting vào đây

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

        bgmSlider.value = savedBGM;
        sfxSlider.value = savedSFX;

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);

        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // --- Khởi tạo Ngôn ngữ ---
        int savedLangID = PlayerPrefs.GetInt("LanguageID", 0);
        StartCoroutine(SetLocale(savedLangID));

        languageDropdown.value = savedLangID;
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void Update()
    {
        // Lắng nghe khi người chơi bấm phím ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSetting();
        }
    }

    // Hàm bật/tắt Panel Setting
    public void ToggleSetting()
    {
        if (settingPanel != null)
        {
            // Bật nếu đang tắt, tắt nếu đang bật
            bool isActive = !settingPanel.activeSelf;
            settingPanel.SetActive(isActive);

            // (Tùy chọn) Khóa / Mở con trỏ chuột khi bật bảng Setting
            Cursor.visible = isActive;
            Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    // Hàm thay đổi volume BGM
    public void SetBGMVolume(float value)
    {
        float dB = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("BGMVolume", dB);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    // Hàm thay đổi volume SFX
    public void SetSFXVolume(float value)
    {
        float dB = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("SFXVolume", dB);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // Hàm xử lý khi chọn Dropdown ngôn ngữ
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
}