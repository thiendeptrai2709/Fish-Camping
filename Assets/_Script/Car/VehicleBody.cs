using UnityEngine;

public class VehicleBody : MonoBehaviour, IInteractable
{
    [SerializeField] private VehicleStats vehicleStats;

    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        if (vehicleStats == null)
        {
            vehicleStats = GetComponent<VehicleStats>() ?? GetComponentInChildren<VehicleStats>() ?? Object.FindFirstObjectByType<VehicleStats>(FindObjectsInactive.Include);
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
            vehicleStats = GetComponent<VehicleStats>() ?? GetComponentInChildren<VehicleStats>() ?? Object.FindFirstObjectByType<VehicleStats>(FindObjectsInactive.Include);
        }

        if (vehicleStats != null)
        {
            vehicleStats.ToggleOverviewPanel();
        }

        ForcedTutorialManager.Instance?.NotifyInspectCar();
    }

    public string GetInteractPrompt()
    {
        return "[Chuột Trái] Xem thông số xe";
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