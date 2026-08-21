using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractableEngine : MonoBehaviour, IInteractable
{
    [Header("Liên kết chức năng")]
    [SerializeField] private EngineRepairMinigame engineRepairMinigame;
    [SerializeField] private InteractableHood interactableHood;

    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        if (engineRepairMinigame == null)
        {
            engineRepairMinigame = GetComponent<EngineRepairMinigame>() ?? GetComponentInParent<EngineRepairMinigame>() ?? Object.FindFirstObjectByType<EngineRepairMinigame>(FindObjectsInactive.Include);
        }

        if (interactableHood == null)
        {
            interactableHood = GetComponentInParent<InteractableHood>() ?? Object.FindFirstObjectByType<InteractableHood>(FindObjectsInactive.Include);
        }

        SetLayerRecursively(gameObject, normalLayer);
    }

    public bool CanInteract()
    {
        // Chỉ cho phép tương tác với động cơ khi nắp Capo đang MỞ
        if (interactableHood != null && !interactableHood.IsHoodOpen)
        {
            return false;
        }
        return true;
    }

    public void OnFocus()
    {
        if (!CanInteract()) return;
        SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        if (engineRepairMinigame != null && !engineRepairMinigame.IsEngineOut)
        {
            engineRepairMinigame.TryToggleRepairMode();
            ForcedTutorialManager.Instance?.NotifyEngineRepaired();
        }
    }

    public string GetInteractPrompt()
    {
        if (!CanInteract()) return "";

        if (engineRepairMinigame != null && engineRepairMinigame.IsEngineOut)
        {
            return "";
        }

        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        return isVietnamese ? "[Chuột Trái] Kiểm tra động cơ" : "[Left Click] Inspect Engine";
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child != null) SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}