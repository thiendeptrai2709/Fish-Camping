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

        if (vehicleStats == null)
        {
            vehicleStats = GetComponentInParent<VehicleStats>();
            if (vehicleStats == null)
            {
                vehicleStats = Object.FindFirstObjectByType<VehicleStats>(FindObjectsInactive.Include);
            }
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

        if (vehicleStats != null && vehicleStats.IsOverviewPanelOpen)
        {
            vehicleStats.CloseOverviewPanel();
        }
    }

    public void Interact()
    {
        if (vehicleStats == null)
        {
            vehicleStats = GetComponentInParent<VehicleStats>();
            if (vehicleStats == null)
            {
                vehicleStats = Object.FindFirstObjectByType<VehicleStats>(FindObjectsInactive.Include);
            }
        }

        if (vehicleStats != null)
        {
            vehicleStats.ToggleOverviewPanel();
        }

        ForcedTutorialManager.Instance?.NotifyInspectCar();
    }

    public string GetInteractPrompt()
    {
        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        return isVietnamese ? "[Chuột Trái] Xem thông số xe" : "[Left Click] View Vehicle Stats";
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