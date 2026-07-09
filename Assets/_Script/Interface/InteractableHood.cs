using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractableHood : MonoBehaviour, IInteractable
{
    [SerializeField] private ProceduralHinge hoodHinge;

    private int normalLayer;
    private int outlineLayer;
    private bool isHoodOpen = false;

    private void Awake()
    {
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

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
    }
    public void ForceClose()
    {
        if (!isHoodOpen) return;
        isHoodOpen = false;

        if (hoodHinge != null) hoodHinge.ForceClose();
    }
    public string GetInteractPrompt()
    {
        return isHoodOpen ? "[Chuột Trái] Đóng nắp Capo" : "[Chuột Trái] Mở nắp Capo";
    }

    /* 
     * Đổi đồng loạt Layer cho object hiện tại và các object con
     */
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}