using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
public class TireRepairMinigame : MonoBehaviour
{
    public enum TireState { Normal, Viewing, Removed, Swapped }


    [Header("Tham chiếu Transform")]
    [SerializeField] private Transform tireVisualMesh;
    [SerializeField] private Transform inspectPoint; // Điểm lôi lốp ra ngoài
    [SerializeField] private Transform dropPoint;    // Điểm vứt lốp xuống đất

    [SerializeField] private CinemachineCamera inspectCam;
    [SerializeField] private int activePriority = 10;

    [Header("Cài đặt")]
    [SerializeField] private float floatSpeed = 4f;

    [Header("Sự kiện")]
    public UnityEvent OnFinishedRepair;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private TireState currentState = TireState.Normal;
    private Coroutine moveCoroutine;
    private bool isAnimating = false;

    private void Awake()
    {
        originalLocalPos = tireVisualMesh.localPosition;
        originalLocalRot = tireVisualMesh.localRotation;
    }

    public void Interact()
    {
        if (isAnimating) return; // Khóa chặt không cho bấm láo khi lốp đang bay

        switch (currentState)
        {
            case TireState.Normal:
                // Bấm lần 1: Chỉ chuyển Camera, lốp nằm im
                currentState = TireState.Viewing;
                if (inspectCam != null) inspectCam.Priority = activePriority;
                break;

            case TireState.Viewing:
                // Bấm lần 2: Bắt đầu lôi lốp hỏng ra
                currentState = TireState.Removed;
                moveCoroutine = StartCoroutine(AnimateTireTo(inspectPoint.position, inspectPoint.rotation));
                break;

            case TireState.Removed:
                // Bấm lần 3: Vứt lốp xuống, lấy lốp mới lên
                currentState = TireState.Swapped;
                moveCoroutine = StartCoroutine(AnimateFakeSwap());
                break;

            case TireState.Swapped:
                // Bấm lần 4: Lắp lốp vào xe (Camera sẽ tự trả về khi chạy xong)
                currentState = TireState.Normal;
                Vector3 dockWorldPos = tireVisualMesh.parent.TransformPoint(originalLocalPos);
                Quaternion dockWorldRot = tireVisualMesh.parent.rotation * originalLocalRot;
                moveCoroutine = StartCoroutine(AnimateTireTo(dockWorldPos, dockWorldRot, true));
                break;
        }
    }

    private IEnumerator AnimateTireTo(Vector3 targetPos, Quaternion targetRot, bool isDocking = false)
    {
        isAnimating = true;
        Vector3 startPos = tireVisualMesh.position;
        Quaternion startRot = tireVisualMesh.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * floatSpeed;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            tireVisualMesh.position = Vector3.Lerp(startPos, targetPos, ease);
            tireVisualMesh.rotation = Quaternion.Slerp(startRot, targetRot, ease);
            yield return null;
        }

        tireVisualMesh.position = targetPos;
        tireVisualMesh.rotation = targetRot;

        if (isDocking)
        {
            tireVisualMesh.localPosition = originalLocalPos;
            tireVisualMesh.localRotation = originalLocalRot;
            if (inspectCam != null) inspectCam.Priority = 1; // Tự động nhả Camera về gốc khi lốp đã vào khớp
            OnFinishedRepair?.Invoke();
        }

        isAnimating = false;
    }

    private IEnumerator AnimateFakeSwap()
    {
        isAnimating = true;
        yield return StartCoroutine(AnimateTireTo(dropPoint.position, dropPoint.rotation));

        isAnimating = true; // Khóa lại ngay lập tức vì AnimateTireTo ở trên vừa nhả khóa
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(AnimateTireTo(inspectPoint.position, inspectPoint.rotation));
        isAnimating = false; // Mở khóa hoàn toàn khi xong chuỗi fake swap
    }
}