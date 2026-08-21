using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractableHood : MonoBehaviour, IInteractable
{
    [SerializeField] private ProceduralHinge hoodHinge;

    private int normalLayer;
    private int outlineLayer;
    private bool isHoodOpen = false;

    public bool IsHoodOpen => isHoodOpen;

    private void Awake()
    {
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        if (hoodHinge == null)
        {
            hoodHinge = GetComponent<ProceduralHinge>() ?? GetComponentInChildren<ProceduralHinge>() ?? GetComponentInParent<ProceduralHinge>();
        }

        SetLayerRecursively(gameObject, normalLayer);
    }

    public void OnFocus()
    {
        SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }

    public void Interact()
    {
        isHoodOpen = !isHoodOpen;

        if (hoodHinge != null)
        {
            hoodHinge.Toggle();
        }

        if (isHoodOpen)
        {
            ForcedTutorialManager.Instance?.NotifyHoodOpened();
        }
        else
        {
            ForcedTutorialManager.Instance?.NotifyHoodClosed();
        }
    }

    public void ForceClose()
    {
        if (!isHoodOpen) return;
        isHoodOpen = false;

        if (hoodHinge != null) hoodHinge.ForceClose();

        ForcedTutorialManager.Instance?.NotifyHoodClosed();
    }

    public string GetInteractPrompt()
    {
        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        return isHoodOpen 
            ? (isVietnamese ? "[Chuột Trái] Đóng nắp Capo" : "[Left Click] Close Hood") 
            : (isVietnamese ? "[Chuột Trái] Mở nắp Capo" : "[Left Click] Open Hood");
    }

    /* 
     * Đổi đồng loạt Layer cho object hiện tại và các object con
     */
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