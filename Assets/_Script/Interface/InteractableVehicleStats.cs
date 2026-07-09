using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractableVehicleStats : MonoBehaviour, IInteractable
{
    [Header("Link đến script quản lý chỉ số")]
    [SerializeField] private VehicleStats vehicleStats;

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
        SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }

    public void Interact()
    {
        if (vehicleStats != null)
        {
            // Gọi hàm bật/tắt Canvas thông số đã có sẵn trong VehicleStats của mày
            vehicleStats.ToggleOverviewPanel();
        }
    }

    public string GetInteractPrompt()
    {
        return "[Chuột Trái] Xem thông số xe";
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