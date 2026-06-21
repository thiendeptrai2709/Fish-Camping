using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

public class EngineRepairMinigame : MonoBehaviour
{
    [Header("Link Vận hành")]
    [SerializeField] private VehicleInput vehicleInput;
    [SerializeField] private ProceduralHinge hoodHinge;
    [SerializeField] private Transform engineVisualMesh;
    [SerializeField] private Transform inspectPoint;

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
        originalLocalPos = engineVisualMesh.localPosition;
        originalLocalRot = engineVisualMesh.localRotation;
    }

    private void OnEnable()
    {
        if (vehicleInput != null)
            vehicleInput.OnInspectEngineEvent += TryToggleRepairMode;
    }

    private void OnDisable()
    {
        if (vehicleInput != null)
            vehicleInput.OnInspectEngineEvent -= TryToggleRepairMode;
    }

    private void TryToggleRepairMode()
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
        isRepairing = true;
        isAnimating = true;

        if (hoodHinge != null)
            hoodHinge.IsLocked = true;

        if (inspectCam != null)
            inspectCam.Priority = activePriority;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateEngineTo(inspectPoint.position, inspectPoint.rotation, true));

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

        Vector3 dockWorldPos = engineVisualMesh.parent.TransformPoint(originalLocalPos);
        Quaternion dockWorldRot = engineVisualMesh.parent.rotation * originalLocalRot;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateEngineTo(dockWorldPos, dockWorldRot, false));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnFinishedRepair?.Invoke();
    }

    private IEnumerator AnimateEngineTo(Vector3 targetPos, Quaternion targetRot, bool isEntering)
    {
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
            
            if (hoodHinge != null)
                hoodHinge.IsLocked = false;

        }
        isAnimating = false;
    }
}