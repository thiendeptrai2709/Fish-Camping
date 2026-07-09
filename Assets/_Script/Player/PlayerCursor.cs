using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    [SerializeField] private bool lockCursorAtStart = true;

    private bool isCurrentLocked;

    private void Start()
    {
        SetCursorState(lockCursorAtStart);
    }

    // Đặt trạng thái chuột
    public void SetCursorState(bool isLocked)
    {
        isCurrentLocked = isLocked;
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            SetCursorState(isCurrentLocked);
        }
    }
}