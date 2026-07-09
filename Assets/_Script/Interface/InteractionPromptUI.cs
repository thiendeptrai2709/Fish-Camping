using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    public void DisplayPrompt(bool isVisible, string message = "")
    {
        if (promptPanel != null && promptPanel.activeSelf != isVisible)
        {
            promptPanel.SetActive(isVisible);
        }

        if (isVisible && promptText != null)
        {
            promptText.text = message;
        }
    }
}