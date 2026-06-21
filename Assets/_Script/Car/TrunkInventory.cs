using UnityEngine;

public class TrunkInventory : MonoBehaviour
{
    [Header("Link Hệ thống")]
    [SerializeField] private VehicleInput vehicleInput;

    [Header("Link UI")]
    [SerializeField] private GameObject trunkInventoryPanel;

    private void OnEnable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnToggleHoodEvent += ToggleTrunkAndUI;
        }
    }

    private void OnDisable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnToggleHoodEvent -= ToggleTrunkAndUI;
        }
    }

    private void ToggleTrunkAndUI()
    {

        if (trunkInventoryPanel != null)
        {
            trunkInventoryPanel.SetActive(!trunkInventoryPanel.activeSelf);
        }
        else
        {
        }
    }
}