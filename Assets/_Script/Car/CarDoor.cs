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
        return "[Chuột Trái] Lên xe";
    }
}