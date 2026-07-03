using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool InteractTriggered { get; private set; }
    public bool MinigameTriggered { get; private set; } // Dùng cho QTE (bấm 1 lần)
    public bool IsMinigameHeld { get; private set; }
    public bool BackpackTriggered { get; private set; }
    public bool RotateItemTriggered { get; private set; }
    public bool IsUIOpen { get; set; }
    private CarInputActions inputActions;

    private void Awake()
    {
        inputActions = new CarInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        MoveInput = inputActions.Player.Move.ReadValue<Vector2>();
        LookInput = IsUIOpen ? Vector2.zero : inputActions.Player.Look.ReadValue<Vector2>();
        IsSprinting = inputActions.Player.Sprint.IsPressed();
        InteractTriggered = inputActions.Player.Interact.WasPressedThisFrame();

        MinigameTriggered = inputActions.Player.MinigameAction.WasPressedThisFrame();
        IsMinigameHeld = inputActions.Player.MinigameAction.IsPressed();
        BackpackTriggered = inputActions.Player.Backpack.WasPressedThisFrame();
        RotateItemTriggered = Mathf.Abs(inputActions.Player.RotateItem.ReadValue<Vector2>().y) > 0.1f;
    }
}