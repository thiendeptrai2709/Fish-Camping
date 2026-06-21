using UnityEngine;
using System;

public class VehicleInput : MonoBehaviour
{
    private CarInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public bool IsBraking { get; private set; }
    public event Action OnToggleTrunkEvent;
    public event Action OnToggleHoodEvent;
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
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Disable();
    }
}