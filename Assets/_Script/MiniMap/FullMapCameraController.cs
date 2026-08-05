using UnityEngine;
using UnityEngine.InputSystem;

public class FullMapCameraController : MonoBehaviour
{
    public float panSpeed = 50f;

    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;

    private Camera cam;

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
        float orthoSize = cam.orthographicSize;
        float orthoWidth = orthoSize * cam.aspect;

        float limitMinX = minX + orthoWidth;
        float limitMaxX = maxX - orthoWidth;
        float limitMinZ = minZ + orthoSize;
        float limitMaxZ = maxZ - orthoSize;

        if (limitMinX > limitMaxX)
        {
            limitMinX = limitMaxX = (minX + maxX) / 2f;
        }

        if (limitMinZ > limitMaxZ)
        {
            limitMinZ = limitMaxZ = (minZ + maxZ) / 2f;
        }

        targetPos.x = Mathf.Clamp(targetPos.x, limitMinX, limitMaxX);
        targetPos.z = Mathf.Clamp(targetPos.z, limitMinZ, limitMaxZ);

        return targetPos;
    }
    public void FocusOnPosition(Vector3 targetPosition)
    {
        Vector3 newPos = targetPosition;
        newPos.y = transform.position.y;
        transform.position = GetClampedPosition(newPos);
    }
}