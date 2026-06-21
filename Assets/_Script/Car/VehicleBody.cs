using UnityEngine;

public class VehicleBody : MonoBehaviour
{
    [SerializeField] private VehicleInput vehicleInput;
    [SerializeField] private ProceduralHinge trunkHinge;
    [SerializeField] private ProceduralHinge hoodHinge;

    private void OnEnable()
    {
        if (vehicleInput == null) return;
        vehicleInput.OnToggleTrunkEvent += trunkHinge.Toggle;
        vehicleInput.OnToggleHoodEvent += hoodHinge.Toggle;
    }

    private void OnDisable()
    {
        if (vehicleInput == null) return;
        vehicleInput.OnToggleTrunkEvent -= trunkHinge.Toggle;
        vehicleInput.OnToggleHoodEvent -= hoodHinge.Toggle;
    }
}