using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Localization Tables")]
    [SerializeField] private string primaryTable = "Game Text";
    [SerializeField] private string secondaryTable = "NPC Text";

    private string _lastRawMessage = "";

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(UnityEngine.Localization.Locale locale)
    {
        if (promptPanel != null && promptPanel.activeSelf && promptText != null && !string.IsNullOrEmpty(_lastRawMessage))
        {
            promptText.text = GetLocalizedText(_lastRawMessage);
        }
    }

    public void DisplayPrompt(bool isVisible, string message = "")
    {
        if (promptPanel != null && promptPanel.activeSelf != isVisible)
        {
            promptPanel.SetActive(isVisible);
        }

        _lastRawMessage = message;

        if (isVisible && promptText != null)
        {
            promptText.text = GetLocalizedText(message);
        }
    }

    private string GetLocalizedText(string keyOrText)
    {
        if (string.IsNullOrEmpty(keyOrText)) return "";

        try
        {
            var gameTable = LocalizationSettings.StringDatabase.GetTable(primaryTable);
            if (gameTable != null)
            {
                var entry = gameTable.GetEntry(keyOrText);
                if (entry != null) return entry.GetLocalizedString();
            }

            if (!string.IsNullOrEmpty(secondaryTable))
            {
                var npcTable = LocalizationSettings.StringDatabase.GetTable(secondaryTable);
                if (npcTable != null)
                {
                    var entry = npcTable.GetEntry(keyOrText);
                    if (entry != null) return entry.GetLocalizedString();
                }
            }

            return keyOrText;
        }
        catch
        {
            return keyOrText;
        }
    }
}