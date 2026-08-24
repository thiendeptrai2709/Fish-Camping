using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

public class EngineRepairMinigame : MonoBehaviour
{
    [Header("Link Vận hành")]
    [SerializeField] private ProceduralHinge hoodHinge;
    [SerializeField] private Transform engineVisualMesh;
    [SerializeField] private Transform inspectPoint;

    [Header("Link Người Chơi")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private GameObject crosshairUI;

    [Header("Cinemachine Priority Override")]
    [SerializeField] private CinemachineCamera inspectCam;
    [SerializeField] private int activePriority = 10;

    [Header("Cài đặt bay")]
    [SerializeField] private float floatSpeed = 3f;

    [Header("Sự kiện gởi ra ngoài")]
    public UnityEvent OnStartedRepair;
    public UnityEvent OnFinishedRepair;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private bool isRepairing = false;
    private bool isAnimating = false;
    public bool IsEngineOut => isRepairing && !isAnimating;
    
    private Coroutine moveCoroutine;

    private void Awake()
    {
        if (engineVisualMesh != null)
        {
            originalLocalPos = engineVisualMesh.localPosition;
            originalLocalRot = engineVisualMesh.localRotation;
        }
        AutoFindReferences();
    }

    private void AutoFindReferences()
    {
        if (playerMovement == null) playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        if (playerInput == null) playerInput = Object.FindFirstObjectByType<PlayerInputHandler>();
        if (playerCursor == null) playerCursor = PlayerCursor.Instance ?? Object.FindFirstObjectByType<PlayerCursor>();
        if (crosshairUI == null)
        {
            var pInteraction = Object.FindFirstObjectByType<PlayerInteraction>();
            if (pInteraction != null) crosshairUI = pInteraction.gameObject;
        }
    }

    private void Update()
    {
        if (isRepairing && !isAnimating)
        {
            // Phím tắt ESC hoặc E để thoát nhanh chế độ sửa xe
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                ExitRepairMode();
                return;
            }

            // Luôn đảm bảo chuột hiển thị khi đang mở bảng sửa xe
            if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (playerCursor != null) playerCursor.SetCursorState(false);
            }
        }
    }

    public void TryToggleRepairMode()
    {
        if (hoodHinge == null || !hoodHinge.IsFullyOpen)
        {
            Debug.Log("Capo chưa mở hết 100%, khóa chuỗi hoạt động!");
            return;
        }

        if (isAnimating) return;

        if (!isRepairing)
            EnterRepairMode();
        else
            ExitRepairMode();
    }

    private void EnterRepairMode()
    {
        AutoFindReferences();
        isRepairing = true;
        isAnimating = true;

        if (hoodHinge != null)
            hoodHinge.IsLocked = true;

        if (inspectCam != null)
            inspectCam.Priority = activePriority;

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerInput != null) playerInput.IsUIOpen = true;
        if (crosshairUI != null) crosshairUI.SetActive(false);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        if (inspectPoint != null && engineVisualMesh != null)
        {
            moveCoroutine = StartCoroutine(AnimateEngineTo(inspectPoint.position, inspectPoint.rotation, true));
        }
        else
        {
            isAnimating = false;
        }

        if (playerCursor != null) playerCursor.SetCursorState(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnStartedRepair?.Invoke();
    }

    public void ExitRepairMode()
    {
        isRepairing = false;
        isAnimating = true;

        if (inspectCam != null)
            inspectCam.Priority = 1;

        if (playerMovement != null) playerMovement.enabled = true;
        if (playerInput != null) playerInput.IsUIOpen = false;
        if (crosshairUI != null) crosshairUI.SetActive(true);

        if (hoodHinge != null)
            hoodHinge.IsLocked = false;

        if (engineVisualMesh != null && engineVisualMesh.parent != null)
        {
            Vector3 dockWorldPos = engineVisualMesh.parent.TransformPoint(originalLocalPos);
            Quaternion dockWorldRot = engineVisualMesh.parent.rotation * originalLocalRot;

            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(AnimateEngineTo(dockWorldPos, dockWorldRot, false));
        }
        else
        {
            isAnimating = false;
        }

        if (playerCursor != null) playerCursor.SetCursorState(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnFinishedRepair?.Invoke();
    }

    private void OnDisable()
    {
        if (isRepairing)
        {
            isRepairing = false;
            isAnimating = false;
            if (playerMovement != null) playerMovement.enabled = true;
            if (playerInput != null) playerInput.IsUIOpen = false;
            if (playerCursor != null) playerCursor.SetCursorState(true);
        }
    }

    private IEnumerator AnimateEngineTo(Vector3 targetPos, Quaternion targetRot, bool isEntering)
    {
        if (engineVisualMesh == null)
        {
            isAnimating = false;
            yield break;
        }

        Vector3 startPos = engineVisualMesh.position;
        Quaternion startRot = engineVisualMesh.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * floatSpeed;
            float ease = Mathf.SmoothStep(0f, 1f, t);

            engineVisualMesh.position = Vector3.Lerp(startPos, targetPos, ease);
            engineVisualMesh.rotation = Quaternion.Slerp(startRot, targetRot, ease);
            yield return null;
        }

        engineVisualMesh.position = targetPos;
        engineVisualMesh.rotation = targetRot;

        if (!isEntering)
        {
            engineVisualMesh.localPosition = originalLocalPos;
            engineVisualMesh.localRotation = originalLocalRot;
        }
        isAnimating = false;
    }
}