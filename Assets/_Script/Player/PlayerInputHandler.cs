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
    public bool IsUIOpen { get; set; }

    private CarInputActions inputActions;

    private void Awake()
    {
        inputActions = new CarInputActions();
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Enable();
            Debug.Log("<color=green>[PlayerInputHandler] Đã Enable Input Actions thành công!</color>");
        }
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Disable();
        }
    }

    private void Update()
    {
        if (inputActions == null) return;

        // Nếu đang mở UI, khóa di chuyển và xoay camera
        if (IsUIOpen)
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            IsSprinting = false;
            InteractTriggered = false;
            IsInteractHeld = false;
            return;
        }

        // Đọc các giá trị di chuyển
        MoveInput = inputActions.Player.Move.ReadValue<Vector2>();
        LookInput = inputActions.Player.Look.ReadValue<Vector2>();
        IsSprinting = inputActions.Player.Sprint.IsPressed();

        // Đọc tín hiệu phím E / Interact
        InteractTriggered = inputActions.Player.Interact.WasPressedThisFrame();
        IsInteractHeld = inputActions.Player.Interact.IsPressed();

        // IN LOG TEST BẤM E (Khi bạn bấm E, dòng này BẮT BUỘC phải hiện ở Console)
        if (InteractTriggered)
        {
            Debug.Log("<color=yellow>[PlayerInputHandler] NHẬN TÍN HIỆU BẤM PHÍM INTERACT (E)!</color>");
        }

        // Đọc các tín hiệu phím khác
        BuildTriggered = inputActions.Player.Build.WasPressedThisFrame();
        MinigameTriggered = inputActions.Player.MinigameAction.WasPressedThisFrame();
        IsMinigameHeld = inputActions.Player.MinigameAction.IsPressed();
        BackpackTriggered = inputActions.Player.Backpack.WasPressedThisFrame();

        float rotateY = inputActions.Player.RotateItem.ReadValue<Vector2>().y;
        RotateItemTriggered = Mathf.Abs(rotateY) > 0.1f;
        RotateItemValue = rotateY;

        ScreenshotTriggered = inputActions.Player.Screenshot.WasPressedThisFrame();
        JournalTriggered = inputActions.Player.Journal.WasPressedThisFrame();
    }
}