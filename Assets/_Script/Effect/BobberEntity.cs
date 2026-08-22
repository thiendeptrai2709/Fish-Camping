using UnityEngine;
using System;

public class BobberEntity : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float flightDuration;
    private float arcHeight;

    private float elapsedTime;
    private bool isFlying;
    private bool isBiting;
    private bool isNibbling;
    private float nibbleTimer;
    private Vector3 landedPosition;
    private FishingLineVisual connectedLine;
    private BobberEffectController effectController;
    private Action onLandedCallback;

    private void Awake()
    {
        effectController = GetComponentInChildren<BobberEffectController>();
    }

    public void Cast(Vector3 startPoint, Vector3 targetPoint, float duration, float height, FishingLineVisual lineVisual = null, Action onLanded = null)
    {
        startPosition = startPoint;
        targetPosition = targetPoint;
        flightDuration = duration;
        arcHeight = height;
        elapsedTime = 0f;
        isFlying = true;
        isBiting = false;
        isNibbling = false;
        connectedLine = lineVisual;
        onLandedCallback = onLanded;
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
            // Phao chìm sâu và rung giật mạnh khi cắn câu
            float sinkOffset = -0.22f + Mathf.Sin(Time.time * 35f) * 0.08f;
            float shakeX = Mathf.Cos(Time.time * 28f) * 0.07f;
            float shakeZ = Mathf.Sin(Time.time * 24f) * 0.07f;
            transform.position = landedPosition + new Vector3(shakeX, sinkOffset, shakeZ);
        }
        else if (isNibbling)
        {
            nibbleTimer -= Time.deltaTime;
            // Nhấp nháy chìm nhẹ theo đường cong hình sin rồi nổi lại
            float t = Mathf.Clamp01(1f - (nibbleTimer / 0.35f));
            float nibbleOffset = -Mathf.Sin(t * Mathf.PI) * 0.09f;
            transform.position = landedPosition + new Vector3(0f, nibbleOffset, 0f);

            if (nibbleTimer <= 0f)
            {
                isNibbling = false;
                transform.position = landedPosition;
            }
        }
    }

    private void OnLand()
    {
        landedPosition = transform.position;
        if (connectedLine != null)
        {
            connectedLine.SetFlyingState(false);
        }
        onLandedCallback?.Invoke();
    }

    public void TriggerNibble(float duration = 0.35f)
    {
        if (isBiting || isFlying) return;
        isNibbling = true;
        nibbleTimer = duration;
    }

    public void StartBiting()
    {
        if (isFlying) return;
        isNibbling = false;
        isBiting = true;
        if (effectController != null)
        {
            effectController.PlayBiteEffect();
        }
    }

    public void StopBiting()
    {
        isBiting = false;
        isNibbling = false;
        transform.position = landedPosition;
    }
}