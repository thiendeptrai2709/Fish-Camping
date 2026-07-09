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
    private bool isBiting = false;
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
        if (flying) isBiting = false;
    }

    public void SetBitingState(bool biting)
    {
        isBiting = biting;
    }

    public void ClearLine()
    {
        lineRenderer.enabled = false;
        startPoint = null;
        endPoint = null;
        isBiting = false;
    }

    private void LateUpdate()
    {
        if (startPoint == null || endPoint == null || !lineRenderer.enabled) return;

        Vector3 p0 = startPoint.position;
        Vector3 p2 = endPoint.position;
        Vector3 midPoint = (p0 + p2) * 0.5f;

        float currentSag = isFlying ? flyingSag : defaultSag;
        if (isBiting) currentSag = -0.02f;

        float distance = Vector3.Distance(p0, p2);
        Vector3 p1 = midPoint + Vector3.down * (currentSag * (distance * 0.3f));

        if (!isFlying && !isBiting)
        {
            p1.y += Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        }
        else if (isBiting)
        {
            float jitterX = Mathf.Sin(Time.time * 35f) * 0.15f;
            float jitterY = Mathf.Cos(Time.time * 45f) * 0.1f;
            float jitterZ = Mathf.Sin(Time.time * 25f) * 0.15f;
            p1 += new Vector3(jitterX, jitterY, jitterZ);
            p2 += new Vector3(jitterX * 0.5f, -0.1f + jitterY * 0.5f, jitterZ * 0.5f);
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