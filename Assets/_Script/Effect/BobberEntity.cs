using UnityEngine;

public class BobberEntity : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float flightDuration;
    private float arcHeight;
    private float elapsedTime;
    private bool isFlying;
    private FishingLineVisual connectedLine;

    public void Cast(Vector3 startPoint, Vector3 targetPoint, float duration, float height, FishingLineVisual lineVisual = null)
    {
        startPosition = startPoint;
        targetPosition = targetPoint;
        flightDuration = duration;
        arcHeight = height;
        elapsedTime = 0f;
        isFlying = true;
        connectedLine = lineVisual;
        transform.position = startPosition;

        if (connectedLine != null)
        {
            connectedLine.SetFlyingState(true);
        }
    }

    private void Update()
    {
        if (!isFlying) return;

        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / flightDuration);

        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
        currentPos.y += 4f * arcHeight * progress * (1f - progress);

        transform.position = currentPos;

        if (progress >= 1f)
        {
            isFlying = false;
            OnLand();
        }
    }

    private void OnLand()
    {
        if (connectedLine != null)
        {
            connectedLine.SetFlyingState(false);
        }
    }
}