using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CarDoor : MonoBehaviour, IInteractable
{
    [Header("Liên kết Hệ Thống")]
    [SerializeField] private VehicleEnterExit vehicleSystem;
    [SerializeField] private Transform exitPoint; // Cái Empty Object đứng cạnh cửa


    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        // Sử dụng lại logic tráo Layer y hệt như TestInteractableCube
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        gameObject.layer = normalLayer;
    }

    public void OnFocus()
    {
        gameObject.layer = outlineLayer;
    }

    public void OnLoseFocus()
    {
        gameObject.layer = normalLayer;
    }

    public void Interact()
    {
        OnLoseFocus();

        if (vehicleSystem != null && exitPoint != null)
        {
            vehicleSystem.EnterVehicle(exitPoint);
        }
    }

    public string GetInteractPrompt()
    {
        return "[Chuột Trái] Lên xe";
    }
}