using UnityEngine;

public interface INpcInteractable
{
    void Interact();
    string GetInteractPrompt();
    void OnFocus();
    void OnLoseFocus();
}