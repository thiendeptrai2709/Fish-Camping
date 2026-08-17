using UnityEngine;
using UnityEngine.InputSystem;

public class FullMapCameraController : MonoBehaviour
{
    public float panSpeed = 50f;

    [Header("Giới Hạn Bản Đồ (Chỉnh tay trên Inspector)")]
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;

    [Header("Cấu Hình Mượt")]
    public float smoothFocusSpeed = 5f;

    private Camera cam;
    private Coroutine smoothFocusCoroutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 startPos = player.transform.position;
            startPos.y = transform.position.y;
            startPos = GetClampedPosition(startPos);
            transform.position = startPos;
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            Vector3 move = new Vector3(-mouseDelta.x, 0, -mouseDelta.y) * panSpeed * Time.deltaTime;
            Vector3 newPos = transform.position + move;

            transform.position = GetClampedPosition(newPos);
        }
    }

    private Vector3 GetClampedPosition(Vector3 targetPos)
    {
        if (cam == null) cam = GetComponent<Camera>();

        // Chuẩn hóa aspect ratio chuẩn theo Render Texture hoặc màn hình Build
        float aspect = (cam.targetTexture != null)
            ? (float)cam.targetTexture.width / cam.targetTexture.height
            : cam.aspect;

        float orthoSize = cam.orthographicSize;
        float orthoWidth = orthoSize * aspect;

        float limitMinX = minX + orthoWidth;
        float limitMaxX = maxX - orthoWidth;
        float limitMinZ = minZ + orthoSize;
        float limitMaxZ = maxZ - orthoSize;

        // Tránh tình trạng bị kẹp dạt về mép khi Build ra màn hình Fullscreen
        if (limitMinX >= limitMaxX)
        {
            targetPos.x = (minX + maxX) / 2f;
        }
        else
        {
            targetPos.x = Mathf.Clamp(targetPos.x, limitMinX, limitMaxX);
        }

        if (limitMinZ >= limitMaxZ)
        {
            targetPos.z = (minZ + maxZ) / 2f;
        }
        else
        {
            targetPos.z = Mathf.Clamp(targetPos.z, limitMinZ, limitMaxZ);
        }

        return targetPos;
    }

    public void FocusOnPosition(Vector3 targetPosition)
    {
        if (smoothFocusCoroutine != null) StopCoroutine(smoothFocusCoroutine);

        Vector3 newPos = targetPosition;
        newPos.y = transform.position.y;
        transform.position = GetClampedPosition(newPos);
    }

    public void SmoothFocusOnPosition(Vector3 targetPosition, System.Action onComplete)
    {
        if (smoothFocusCoroutine != null) StopCoroutine(smoothFocusCoroutine);
        smoothFocusCoroutine = StartCoroutine(SmoothFocusRoutine(targetPosition, onComplete));
    }

    private System.Collections.IEnumerator SmoothFocusRoutine(Vector3 targetPosition, System.Action onComplete)
    {
        Vector3 targetPos = targetPosition;
        targetPos.y = transform.position.y;
        targetPos = GetClampedPosition(targetPos);

        while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                new Vector3(targetPos.x, 0, targetPos.z)) > 0.1f)
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                yield break;
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothFocusSpeed);
            yield return null;
        }

        transform.position = targetPos;
        onComplete?.Invoke();
    }
}