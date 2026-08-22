using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// LanguageManager:
/// Quản lý hệ thống chuyển đổi ngôn ngữ chuẩn Form theo Unity Localization Package.
/// - Hỗ trợ Tiếng Việt (vi) và Tiếng Anh (en).
/// - Tự động lưu và tải cấu hình ngôn ngữ của người chơi qua PlayerPrefs.
/// - Cung cấp sự kiện OnLanguageChanged để các UI tự động cập nhật ngay lập tức.
/// - Phím nóng F9 để chuyển đổi qua lại nhanh trong Play Mode.
/// - Menu Editor trong Tools -> Language.
/// </summary>
public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    private const string LANGUAGE_SAVE_KEY = "Saved_LanguageCode";

    [Header("--- Phím Tắt Đổi Ngôn Ngữ ---")]
    [Tooltip("Phím nóng để đổi ngôn ngữ nhanh khi đang chơi game")]
    [SerializeField] private KeyCode toggleLanguageHotkey = KeyCode.F9;

    [Header("--- Ngôn Ngữ Mặc Định ---")]
    [Tooltip("Mã ngôn ngữ mặc định cho người chơi mới (vi = Tiếng Việt, en = Tiếng Anh)")]
    [SerializeField] private string defaultLanguageCode = "vi";

    public static event Action<string> OnLanguageChanged;

    public string CurrentLanguageCode
    {
        get
        {
            if (LocalizationSettings.SelectedLocale != null)
            {
                return LocalizationSettings.SelectedLocale.Identifier.Code;
            }
            return PlayerPrefs.GetString(LANGUAGE_SAVE_KEY, defaultLanguageCode);
        }
    }

    public bool IsVietnamese => CurrentLanguageCode.StartsWith("vi", StringComparison.OrdinalIgnoreCase);
    public bool IsEnglish => CurrentLanguageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(InitLanguageRoutine());
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleLanguageHotkey))
        {
            ToggleLanguage();
        }
    }

    private IEnumerator InitLanguageRoutine()
    {
        // Đợi hệ thống Localization của Unity khởi tạo xong
        yield return LocalizationSettings.InitializationOperation;

        string savedCode = PlayerPrefs.GetString(LANGUAGE_SAVE_KEY, defaultLanguageCode);
        SetLanguage(savedCode);
    }

    private void HandleLocaleChanged(Locale locale)
    {
        if (locale != null)
        {
            string code = locale.Identifier.Code;
            PlayerPrefs.SetString(LANGUAGE_SAVE_KEY, code);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke(code);
            Debug.Log($"<color=cyan>[LanguageManager] Ngôn ngữ hiện tại: {locale.LocaleName} ({code})</color>");
        }
    }

    /// <summary>
    /// Chuyển đổi qua lại giữa Tiếng Việt và Tiếng Anh
    /// </summary>
    public void ToggleLanguage()
    {
        if (IsVietnamese)
        {
            SetLanguage("en");
        }
        else
        {
            SetLanguage("vi");
        }
    }

    /// <summary>
    /// Đặt ngôn ngữ theo mã chuẩn ("vi", "en")
    /// </summary>
    public void SetLanguage(string localeCode)
    {
        StartCoroutine(SetLanguageRoutine(localeCode));
    }

    private IEnumerator SetLanguageRoutine(string localeCode)
    {
        yield return LocalizationSettings.InitializationOperation;

        var availableLocales = LocalizationSettings.AvailableLocales.Locales;
        Locale targetLocale = null;

        foreach (var loc in availableLocales)
        {
            if (loc.Identifier.Code.Equals(localeCode, StringComparison.OrdinalIgnoreCase) ||
                loc.Identifier.Code.StartsWith(localeCode, StringComparison.OrdinalIgnoreCase))
            {
                targetLocale = loc;
                break;
            }
        }

        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            PlayerPrefs.SetString(LANGUAGE_SAVE_KEY, targetLocale.Identifier.Code);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke(targetLocale.Identifier.Code);
        }
        else
        {
            Debug.LogWarning($"[LanguageManager] Không tìm thấy Locale cho mã: {localeCode}");
        }
    }

    /// <summary>
    /// Helper tĩnh để lấy chuỗi song ngữ nhanh trong code
    /// </summary>
    public static string GetBilingualText(string vietnameseText, string englishText)
    {
        if (Instance != null && Instance.IsEnglish)
        {
            return englishText;
        }

        if (LocalizationSettings.SelectedLocale != null &&
            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            return englishText;
        }

        return vietnameseText;
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Language/Chuyển Sang Tiếng Việt (vi) &v", priority = 10)]
    public static void SetLanguageVietnameseEditor()
    {
        SetEditorLanguage("vi");
    }

    [MenuItem("Tools/Language/Switch to English (en) &e", priority = 11)]
    public static void SetLanguageEnglishEditor()
    {
        SetEditorLanguage("en");
    }

    [MenuItem("Tools/Language/Toggle Ngôn Ngữ (F9) _F9", priority = 12)]
    public static void ToggleLanguageEditor()
    {
        if (LocalizationSettings.SelectedLocale != null &&
            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi", StringComparison.OrdinalIgnoreCase))
        {
            SetEditorLanguage("en");
        }
        else
        {
            SetEditorLanguage("vi");
        }
    }

    private static void SetEditorLanguage(string code)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var loc in locales)
        {
            if (loc.Identifier.Code.StartsWith(code, StringComparison.OrdinalIgnoreCase))
            {
                LocalizationSettings.SelectedLocale = loc;
                PlayerPrefs.SetString(LANGUAGE_SAVE_KEY, loc.Identifier.Code);
                PlayerPrefs.Save();
                Debug.Log($"<color=green>[LanguageManager Editor] Đã chuyển ngôn ngữ sang: {loc.LocaleName} ({loc.Identifier.Code})</color>");
                return;
            }
        }
        Debug.LogWarning($"[LanguageManager Editor] Không tìm thấy Locale cho: {code}");
    }
#endif
}
