using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

public class TireRepairMinigame : MonoBehaviour
{
    public enum TireState { Normal, Viewing, Swapped }

    public static TireRepairMinigame ActiveTire { get; private set; }

    [Header("Tham chiếu Transform")]
    // Đã XÓA biến tireVisualMesh để chống lỗi Gara xóa lốp
    [SerializeField] private Transform inspectPoint;
    [SerializeField] private Transform dropPoint;

    [SerializeField] private CinemachineCamera inspectCam;
    [SerializeField] private int activePriority = 10;

    [Header("Cài đặt")]
    [SerializeField] private float floatSpeed = 4f;

    [Header("Sự kiện")]
    public UnityEvent OnFinishedRepair;

    [Header("Sự kiện Âm thanh (Smart Audio)")]
    public UnityEvent OnPlayRemoveSound;
    public UnityEvent OnPlayDropSound;
    public UnityEvent OnPlayEquipSound;
    public UnityEvent OnPlayAttachSound;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private TireState currentState = TireState.Normal;
    private Coroutine moveCoroutine;
    private bool isAnimating = false;

    public bool IsActive => currentState != TireState.Normal || isAnimating;

    // --- HÀM MỚI: TỰ ĐỘNG TÌM LỐP XE HIỆN TẠI ---
    private Transform GetCurrentTireMesh()
    {

        if (transform.childCount > 0)
        {
            return transform.GetChild(0);
        }
        return null;
    }

    public void Interact()
    {
        if (isAnimating) return;

        Transform activeMesh = GetCurrentTireMesh();
        if (activeMesh == null) return; // Không có lốp thì nghỉ tương tác

        switch (currentState)
        {
            case TireState.Normal:
                // Cập nhật lại vị trí gốc mới nhất ngay lúc bấm (phòng hờ Gara vừa đổi lốp)
                originalLocalPos = activeMesh.localPosition;
                originalLocalRot = activeMesh.localRotation;

                currentState = TireState.Viewing;
                ActiveTire = this;
                if (inspectCam != null) inspectCam.Priority = activePriority;
                break;

            case TireState.Viewing:
                currentState = TireState.Swapped;
                moveCoroutine = StartCoroutine(AnimateRemoveAndSwap(activeMesh));
                break;

            case TireState.Swapped:
                currentState = TireState.Normal;
                OnPlayAttachSound?.Invoke();

                // Trả lốp về vị trí của Root
                Vector3 dockWorldPos = transform.TransformPoint(originalLocalPos);
                Quaternion dockWorldRot = transform.rotation * originalLocalRot;
                moveCoroutine = StartCoroutine(AnimateTireTo(activeMesh, dockWorldPos, dockWorldRot, true));
                break;
        }
    }

    private IEnumerator AnimateTireTo(Transform mesh, Vector3 targetPos, Quaternion targetRot, bool isDocking = false)
    {
        isAnimating = true;
        Vector3 startPos = mesh.position;
        Quaternion startRot = mesh.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * floatSpeed;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            mesh.position = Vector3.Lerp(startPos, targetPos, ease);
            mesh.rotation = Quaternion.Slerp(startRot, targetRot, ease);
            yield return null;
        }

        mesh.position = targetPos;
        mesh.rotation = targetRot;

        if (isDocking)
        {
            mesh.localPosition = originalLocalPos;
            mesh.localRotation = originalLocalRot;
            if (inspectCam != null) inspectCam.Priority = 1;
            ActiveTire = null;
            OnFinishedRepair?.Invoke();
        }

        isAnimating = false;
    }

    public string GetCurrentPrompt()
    {
        switch (currentState)
        {
            case TireState.Normal: return "[Chuột Trái] Kiểm tra lốp";
            case TireState.Viewing: return "[Chuột Trái] Tháo & Thay lốp mới";
            case TireState.Swapped: return "[Chuột Trái] Lắp lốp vào xe";
            default: return "";
        }
    }

    private IEnumerator AnimateRemoveAndSwap(Transform realTire)
    {
        isAnimating = true;
        OnPlayRemoveSound?.Invoke();

        Transform thrownTire = Instantiate(realTire.gameObject, realTire.position, realTire.rotation).transform;

        // Làm cũ lốp bằng code
        Vector3 origScale = thrownTire.localScale;
        thrownTire.localScale = new Vector3(origScale.x * 0.95f, origScale.y * 0.7f, origScale.z * 0.95f);

        MeshRenderer mr = thrownTire.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            foreach (Material mat in mr.materials)
            {
                mat.color = Color.Lerp(mat.color, new Color(0.2f, 0.2f, 0.2f, 1f), 0.7f);
            }
        }

        // Giấu lốp xịn trên xe đi
        realTire.gameObject.SetActive(false);

        // 2. Kéo lốp cũ ra điểm ngắm rồi vứt xuống đất
        yield return StartCoroutine(AnimateTireTo(thrownTire, inspectPoint.position, inspectPoint.rotation));
        yield return StartCoroutine(AnimateTireTo(thrownTire, dropPoint.position, dropPoint.rotation));
        OnPlayDropSound?.Invoke();

        // --- ĐÃ SỬA: XÓA LUÔN LỐP CŨ NGAY LẬP TỨC ---
        Destroy(thrownTire.gameObject);
        // --------------------------------------------

        yield return new WaitForSeconds(0.3f);

        realTire.position = dropPoint.position;
        realTire.rotation = dropPoint.rotation;
        realTire.gameObject.SetActive(true);

        OnPlayEquipSound?.Invoke();

        // Kéo lốp xịn lên điểm ngắm chờ lắp
        yield return StartCoroutine(AnimateTireTo(realTire, inspectPoint.position, inspectPoint.rotation));

        isAnimating = false;
    }
}