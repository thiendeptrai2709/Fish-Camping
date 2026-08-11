using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageSelector : MonoBehaviour
{
    // ID 0: English, ID 1: Vietnamese (tùy theo thứ tự bạn sắp xếp trong Settings)
    public void ChangeLanguage(int localeID)
    {
        StartCoroutine(SetLocale(localeID));
    }

    private IEnumerator SetLocale(int localeID)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];

        // Lưu lại lựa chọn người dùng
        PlayerPrefs.SetInt("SelectedLanguage", localeID);
    }
}