using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractableTire : MonoBehaviour, IInteractable
{
    [SerializeField] private TireRepairMinigame tireRepairMinigame;

    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        SetLayerRecursively(gameObject, normalLayer);
    }

    public void OnFocus()
    {
        if (IsBlocked()) return; // Từ chối sáng viền Outline nếu đang có lốp khác được sửa
        SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }

    public void Interact()
    {
        if (IsBlocked()) return; // Từ chối bấm tương tác nếu đang có lốp khác được sửa

        if (tireRepairMinigame != null)
        {
            tireRepairMinigame.Interact();
        }
    }

    public string GetInteractPrompt()
    {
        if (IsBlocked()) return ""; // Ẩn câu nhắc UI nếu đang có lốp khác được sửa

        if (tireRepairMinigame != null)
        {
            return tireRepairMinigame.GetCurrentPrompt();
        }
        return "[Chuột Trái] Tương tác lốp xe";
    }

    /* Kiểm tra xem hệ thống có đang bị khóa bởi chiếc lốp khác hay không */
    private bool IsBlocked()
    {
        return TireRepairMinigame.ActiveTire != null && TireRepairMinigame.ActiveTire != tireRepairMinigame;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}