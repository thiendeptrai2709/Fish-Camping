using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class VehicleInput : MonoBehaviour
{
    private CarInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public bool IsBraking { get; private set; }
    public bool IsPushing { get; private set; }

    public event Action OnToggleTrunkEvent;
    public event Action OnToggleHoodEvent;
    public event Action OnInspectEngineEvent;
    public event Action OnToggleStatsUIEvent;
    public event Action OnQuickRepairEvent;
    public event Action OnQTEHitEvent;
    public event Action OnRefillCoolantEvent;
    public event Action OnInteractTire1;
    public event Action OnInteractTire2;
    public event Action OnInteractTire3;
    public event Action OnInteractTire4;
    private void Awake()
    {
        inputActions = new CarInputActions();
    }

    private void OnEnable()
    {
        inputActions.Gameplay.Enable();

        inputActions.Gameplay.ToggleTrunk.performed += _ => OnToggleTrunkEvent?.Invoke();
        inputActions.Gameplay.ToggleHood.performed += _ => OnToggleHoodEvent?.Invoke();
        inputActions.Gameplay.InspectEngine.performed += _ => OnInspectEngineEvent?.Invoke();
        inputActions.Gameplay.ToggleStatsUI.performed += _ => OnToggleStatsUIEvent?.Invoke();
        inputActions.Gameplay.QuickRepair.performed += _ => OnQuickRepairEvent?.Invoke();
        inputActions.Gameplay.QTEHit.performed += _ =>
        {
            OnQTEHitEvent?.Invoke();
            IsPushing = true;
        };
        inputActions.Gameplay.QTEHit.canceled += _ => IsPushing = false; inputActions.Gameplay.RefillCoolant.performed += _ => OnRefillCoolantEvent?.Invoke();
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Disable();
    }
    private void Update()
    {
        // Đọc liên tục trạng thái của trục di chuyển và phanh mỗi khung hình
        if (inputActions != null)
        {
            MoveInput = inputActions.Gameplay.Drive.ReadValue<Vector2>();
            IsBraking = inputActions.Gameplay.Brake.IsPressed();

            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame) OnInteractTire1?.Invoke();
                if (Keyboard.current.digit2Key.wasPressedThisFrame) OnInteractTire2?.Invoke();
                if (Keyboard.current.digit3Key.wasPressedThisFrame) OnInteractTire3?.Invoke();
                if (Keyboard.current.digit4Key.wasPressedThisFrame) OnInteractTire4?.Invoke();
            }
        }
    }
}