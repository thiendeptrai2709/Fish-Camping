using UnityEngine;
using TMPro;

public class QTEMinigame : MonoBehaviour
{
    [Header("System Link")]
    [SerializeField] private VehicleInput vehicleInput;

    [Header("UI Elements")]
    [SerializeField] private GameObject qteCanvas;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private RectTransform targetZone;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Settings")]
    [SerializeField] private float speed = 600f;
    [SerializeField] private int targetHits = 3;

    private System.Action onWinCallback;
    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;
    private int currentHits = 0;
    private float pointerX = 0f;
    private float moveDir = 1f;
    private float barWidth = 600f;

    private void OnEnable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnQTEHitEvent += OnQTEKeyPress;
        }
    }

    private void OnDisable()
    {
        if (vehicleInput != null)
        {
            vehicleInput.OnQTEHitEvent -= OnQTEKeyPress;
        }
    }

    public void BeginQTE(string title, System.Action onWinAction)
    {
        onWinCallback = onWinAction;
        currentHits = 0;
        speed = 600f;
        titleText.text = $"{title}\n<size=24>(Bấm SPACE đúng vạch vàng {targetHits} lần)</size>";

        RandomizeTargetZone();

        qteCanvas.SetActive(true);
        isPlaying = true;
    }

    private void RandomizeTargetZone()
    {
        float safeLimit = (barWidth / 2f) - (targetZone.rect.width / 2f);
        float randomX = Random.Range(-safeLimit, safeLimit);
        targetZone.anchoredPosition = new Vector2(randomX, 0f);
    }

    private void Update()
    {
        if (!isPlaying) return;

        pointerX += moveDir * speed * Time.deltaTime;
        if (pointerX > barWidth / 2f || pointerX < -barWidth / 2f)
        {
            moveDir *= -1f;
            pointerX = Mathf.Clamp(pointerX, -barWidth / 2f, barWidth / 2f);
        }
        pointer.anchoredPosition = new Vector2(pointerX, 0f);
    }

    private void OnQTEKeyPress()
    {
        if (!isPlaying) return;
        VerifyHit();
    }

    private void VerifyHit()
    {
        float zoneMinX = targetZone.anchoredPosition.x - (targetZone.rect.width / 2f);
        float zoneMaxX = targetZone.anchoredPosition.x + (targetZone.rect.width / 2f);

        if (pointerX >= zoneMinX && pointerX <= zoneMaxX)
        {
            currentHits++;
            speed += 180f;

            if (currentHits >= targetHits)
            {
                isPlaying = false;
                qteCanvas.SetActive(false);
                onWinCallback?.Invoke();
            }
            else
            {
                titleText.text = $"<color=green>PERFECT!</color> Còn {targetHits - currentHits} phát nữa!";
                RandomizeTargetZone();
            }
        }
        else
        {
            currentHits = 0;
            speed = 600f;
            titleText.text = "<color=red>TRƯỢT!</color> Gà! Làm lại từ con số 0!";
            RandomizeTargetZone();
        }
    }
    public void ForceAbort()
    {
        if (!isPlaying) return;
        isPlaying = false;
        qteCanvas.SetActive(false);
    }
}
