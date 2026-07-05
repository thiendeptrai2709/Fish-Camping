using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FishingLineVisual : MonoBehaviour
{
    [SerializeField] private int linePoints = 15;
    [SerializeField] private float defaultSag = 0.3f;
    [SerializeField] private float flyingSag = 0.05f;
    [SerializeField] private float waveFrequency = 3f;
    [SerializeField] private float waveAmplitude = 0.02f;

    private LineRenderer lineRenderer;
    private Transform startPoint;
    private Transform endPoint;
    private bool isFlying = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints;
        lineRenderer.useWorldSpace = true;
    }

    public void Setup(Transform tipSocket, Transform bobber)
    {
        startPoint = tipSocket;
        endPoint = bobber;
        lineRenderer.enabled = true;
        lineRenderer.useWorldSpace = true;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    public void SetFlyingState(bool flying)
    {
        isFlying = flying;
    }

    public void ClearLine()
    {
        lineRenderer.enabled = false;
        startPoint = null;
        endPoint = null;
    }

    private void LateUpdate()
    {
        if (startPoint == null || endPoint == null || !lineRenderer.enabled) return;

        Vector3 p0 = startPoint.position;
        Vector3 p2 = endPoint.position;
        Vector3 midPoint = (p0 + p2) * 0.5f;

        /*
         * Tính toán điểm uốn (Control Point) cho đường cong Bezier 3 điểm.
         * Khi phao đang bay trên không, dây bị kéo căng nên độ chùng (sag) rất nhỏ.
         * Khi phao rớt xuống nước, trọng lực kéo dây chùng xuống dưới tạo đường cong tự nhiên,
         * kết hợp với hàm Sin để tạo độ dao động nhẹ như đang trôi trên sóng nước.
         */
        float currentSag = isFlying ? flyingSag : defaultSag;
        float distance = Vector3.Distance(p0, p2);
        Vector3 p1 = midPoint + Vector3.down * (currentSag * (distance * 0.3f));

        if (!isFlying)
        {
            p1.y += Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        }

        for (int i = 0; i < linePoints; i++)
        {
            float t = i / (float)(linePoints - 1);
            Vector3 pointPosition = CalculateQuadraticBezierPoint(t, p0, p1, p2);
            lineRenderer.SetPosition(i, pointPosition);
        }
    }

    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawSphere(startPoint.position, 0.05f);
            Gizmos.DrawSphere(endPoint.position, 0.05f);
        }
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 point = uu * p0;
        point += 2f * u * t * p1;
        point += tt * p2;
        return point;
    }
}