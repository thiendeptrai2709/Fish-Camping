using UnityEngine;
using System;

public class VehicleInput : MonoBehaviour
{
    private CarInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public bool IsBraking { get; private set; }

    public event Action OnToggleTrunkEvent;
    public event Action OnToggleHoodEvent;
    public event Action OnInspectEngineEvent;
    public event Action OnToggleStatsUIEvent;
    public event Action OnQuickRepairEvent;
    public event Action OnQTEHitEvent;
    private void Awake()
    {
        inputActions = new CarInputActions();
    }

    private void OnEnable()
    {
        inputActions.Gameplay.Enable();

        inputActions.Gameplay.Drive.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Gameplay.Drive.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Gameplay.Brake.performed += ctx => IsBraking = true;
        inputActions.Gameplay.Brake.canceled += ctx => IsBraking = false;

        inputActions.Gameplay.ToggleTrunk.performed += _ => OnToggleTrunkEvent?.Invoke();
        inputActions.Gameplay.ToggleHood.performed += _ => OnToggleHoodEvent?.Invoke();
        inputActions.Gameplay.InspectEngine.performed += _ => OnInspectEngineEvent?.Invoke();
        inputActions.Gameplay.ToggleStatsUI.performed += _ => OnToggleStatsUIEvent?.Invoke();
        inputActions.Gameplay.QuickRepair.performed += _ => OnQuickRepairEvent?.Invoke();
        inputActions.Gameplay.QTEHit.performed += _ => OnQTEHitEvent?.Invoke();
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Disable();
    }
}