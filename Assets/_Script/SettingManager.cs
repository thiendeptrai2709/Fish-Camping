using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;

public class SettingManager : MonoBehaviour
{
    [Header("--- Audio ---")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("--- Localization ---")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        // --- Khởi tạo Âm thanh ---
        // AudioMixer dùng thang đo Decibel (từ -80dB đến 0dB), Slider dùng từ 0 đến 1
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        bgmSlider.value = savedBGM;
        sfxSlider.value = savedSFX;

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);

        // Lắng nghe sự kiện thay đổi Slider
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // --- Khởi tạo Ngôn ngữ ---
        int savedLangID = PlayerPrefs.GetInt("LanguageID", 0);
        StartCoroutine(SetLocale(savedLangID));

        languageDropdown.value = savedLangID;
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    // Hàm thay đổi volume BGM
    public void SetBGMVolume(float value)
    {
        // Chuyển đổi từ giá trị slider (0-1) sang Decibel (-80 đến 20)
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