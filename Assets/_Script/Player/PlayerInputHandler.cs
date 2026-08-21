using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool InteractTriggered { get; private set; }
    public bool IsInteractHeld { get; private set; }
    public bool MinigameTriggered { get; private set; } // Dùng cho QTE (bấm 1 lần)
    public bool IsMinigameHeld { get; private set; }
    public bool BackpackTriggered { get; private set; }
    public bool RotateItemTriggered { get; private set; }
    public float RotateItemValue { get; private set; }
    public bool ScreenshotTriggered { get; private set; }
    public bool BuildTriggered { get; private set; } // Thêm nút B để mở UI xây dựng
    public bool JournalTriggered { get; private set; }
    public bool MapTriggered { get; private set; }
    public bool ExpandMapTriggered { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool TogglePerspectiveTriggered { get; private set; }
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
        IsInteractHeld = inputActions.Player.Interact.IsPressed();

        BuildTriggered = inputActions.Player.Build.WasPressedThisFrame(); // Đọc input phím B

        MinigameTriggered = inputActions.Player.MinigameAction.WasPressedThisFrame();
        IsMinigameHeld = inputActions.Player.MinigameAction.IsPressed();
        BackpackTriggered = inputActions.Player.Backpack.WasPressedThisFrame();
        float rotateY = inputActions.Player.RotateItem.ReadValue<Vector2>().y;
        RotateItemTriggered = Mathf.Abs(rotateY) > 0.1f;
        RotateItemValue = rotateY;
        ScreenshotTriggered = inputActions.Player.Screenshot.WasPressedThisFrame();
        JournalTriggered = inputActions.Player.Journal.WasPressedThisFrame();
        MapTriggered = inputActions.Player.Map.WasPressedThisFrame();
        ExpandMapTriggered = inputActions.Player.ExpandMap.WasPressedThisFrame();

        bool yPressed = false;
        bool spacePressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            yPressed = Keyboard.current.yKey.wasPressedThisFrame;
            spacePressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
#endif
        if (!yPressed)
        {
            yPressed = Input.GetKeyDown(KeyCode.Y);
        }
        if (!spacePressed)
        {
            spacePressed = Input.GetKeyDown(KeyCode.Space);
        }

        TogglePerspectiveTriggered = yPressed;
        JumpTriggered = spacePressed;
    }
}