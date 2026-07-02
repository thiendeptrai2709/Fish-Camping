using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class VehicleInput : MonoBehaviour
{
    private CarInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public bool IsBraking { get; private set; }
    public bool IsPushing { get; private set; }

   
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
            MoveInput = inputActions.Gameplay.Drive.ReadValue<Vector2>();
            IsBraking = inputActions.Gameplay.Brake.IsPressed();
        }
    }
}