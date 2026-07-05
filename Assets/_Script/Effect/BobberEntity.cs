using UnityEngine;

public class BobberEntity : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float flightDuration;
    private float arcHeight;

    private float elapsedTime;
    private bool isFlying;
    private bool isBiting;
    private Vector3 landedPosition;
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
        if (isFlying)
        {
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
        else if (isBiting)
        {
            float sinkOffset = -0.15f + Mathf.Sin(Time.time * 30f) * 0.1f;
            float shakeX = Mathf.Cos(Time.time * 25f) * 0.08f;
            float shakeZ = Mathf.Sin(Time.time * 20f) * 0.08f;
            transform.position = landedPosition + new Vector3(shakeX, sinkOffset, shakeZ);
        }
    }

    private void OnLand()
    {
        landedPosition = transform.position;
        if (connectedLine != null)
        {
            connectedLine.SetFlyingState(false);
        }
    }

    public void StartBiting()
    {
        isBiting = true;
    }
}