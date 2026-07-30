using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class VehicleInput : MonoBehaviour
{
    private CarInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public bool IsBraking { get; private set; }
    public bool IsPushing { get; private set; }
    public bool IsUIOpen { get; set; }
    public bool MapTriggered { get; private set; }

    private void Awake()
    {
        inputActions = new CarInputActions();
    }

    private void OnEnable()
    {
        inputActions.Gameplay.Enable();

       
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Disable();
    }
    private void Update()
    {
        if (inputActions != null)
        {
            // Phải đọc nút Map ở ngoài để biến này được reset lại (false) ở frame tiếp theo
            MapTriggered = inputActions.Gameplay.Map.WasPressedThisFrame();
        }

        if (IsUIOpen)
        {
            MoveInput = Vector2.zero;
            IsBraking = true;
            return;
        }

        if (inputActions != null)
        {
            MoveInput = inputActions.Gameplay.Drive.ReadValue<Vector2>();
            IsBraking = inputActions.Gameplay.Brake.IsPressed();
        }
    }
}