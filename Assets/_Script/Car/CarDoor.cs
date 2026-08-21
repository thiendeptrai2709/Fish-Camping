using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CarDoor : MonoBehaviour, IInteractable
{
    [Header("Liên kết Hệ Thống")]
    [SerializeField] private VehicleEnterExit vehicleSystem;
    [SerializeField] private Transform exitPoint; // Cái Empty Object đứng cạnh cửa

    public Transform ExitPoint => exitPoint; // Cho phép script khác lấy vị trí cửa xe


    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        // Sử dụng lại logic tráo Layer y hệt như TestInteractableCube
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
        if (this == null) return;
        SetLayerRecursively(gameObject, normalLayer);
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

    public void Interact()
    {
        OnLoseFocus();

        if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanEnterVehicle())
        {
            Debug.LogWarning("<color=yellow>[CarDoor] Chưa thể lên xe lúc này! Hãy hoàn thành nhiệm vụ hiện tại trước.</color>");
            return;
        }

        if (vehicleSystem == null)
        {
            vehicleSystem = GetComponentInParent<VehicleEnterExit>();
            if (vehicleSystem == null)
            {
                vehicleSystem = Object.FindFirstObjectByType<VehicleEnterExit>(FindObjectsInactive.Include);
            }
        }

        if (vehicleSystem != null)
        {
            Transform point = exitPoint != null ? exitPoint : transform;
            vehicleSystem.EnterVehicle(point);
            ForcedTutorialManager.Instance?.NotifyEnteredVehicle();
        }
        else
        {
            Debug.LogWarning("[CarDoor] Không tìm thấy VehicleEnterExit trên xe!");
        }
    }

    public string GetInteractPrompt()
    {
        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        if (ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanEnterVehicle())
        {
            return isVietnamese ? "Cần hoàn thành nhiệm vụ trước khi lên xe" : "Complete mission before entering vehicle";
        }
        return isVietnamese ? "[Chuột Trái] Lên xe" : "[Left Click] Enter Vehicle";
    }
}