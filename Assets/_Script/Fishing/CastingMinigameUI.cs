using UnityEngine;
using UnityEngine.UI;

public class CastingMinigameUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform indicatorArrow;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private float pointerSpeed = 1.5f;

    [Header("Randomized Zones Configuration")]
    [SerializeField] private CastingZoneUI[] castingZones;

    [System.Serializable]
    public class CastingZoneUI
    {
        public string zoneName;
        public int zoneIndex;
        public RectTransform visualRect;
        public float minHeightRatio = 0.15f;
        public float maxHeightRatio = 0.35f;

        [HideInInspector] public float actualRatio;
        [HideInInspector] public float topThreshold;
        [HideInInspector] public float bottomThreshold;
    }

    private bool isPlaying = false;
    private float currentProgress = 0f;
    private int movingDirection = 1;

    private void Awake()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPlaying) return;

        currentProgress += movingDirection * pointerSpeed * Time.deltaTime;

        if (currentProgress >= 1f)
        {
            currentProgress = 1f;
            movingDirection = -1; // Chạm đáy -> Bật ngược lên trên
        }
        else if (currentProgress <= 0f)
        {
            currentProgress = 0f;
            movingDirection = 1; // Chạm đỉnh -> Chạy tiếp xuống dưới
        }

        UpdateIndicatorPosition();
    }

    private void UpdateIndicatorPosition()
    {
        if (indicatorArrow == null || barBackground == null) return;

        // Tính chiều cao tổng của thanh
        float height = barBackground.rect.height;

        // Chuyển progress (0 -> 1) thành tọa độ Y từ TRÊN (top) xuống DƯỚI (bottom)
        // Hệ tọa độ AnchoredPosition với Pivot ở giữa (0.5, 0.5) thì Top là +height/2, Bottom là -height/2
        float targetY = (0.5f - currentProgress) * height;

        indicatorArrow.anchoredPosition = new Vector2(indicatorArrow.anchoredPosition.x, targetY);
    }
    public void StartMinigame()
    {
        currentProgress = 0f;
        movingDirection = 1;
        isPlaying = true;
        if (panelRoot != null) panelRoot.SetActive(true);

        RandomizeAndStretchZones();
    }

    public void StopMinigame(out int zoneIndex, out float powerRatio)
    {
        isPlaying = false;
        if (panelRoot != null) panelRoot.SetActive(false);

        powerRatio = currentProgress;
        zoneIndex = 0;

        if (castingZones != null)
        {
            for (int i = 0; i < castingZones.Length; i++)
            {
                if (currentProgress >= castingZones[i].topThreshold && currentProgress <= castingZones[i].bottomThreshold)
                {
                    zoneIndex = castingZones[i].zoneIndex;
                    break;
                }
            }
        }
    }

    private void RandomizeAndStretchZones()
    {
        if (castingZones == null || castingZones.Length == 0) return;

        // Tạo tỷ lệ ngẫu nhiên cho từng zone theo giới hạn Min/Max
        float totalRatio = 0f;
        for (int i = 0; i < castingZones.Length; i++)
        {
            castingZones[i].actualRatio = Random.Range(castingZones[i].minHeightRatio, castingZones[i].maxHeightRatio);
            totalRatio += castingZones[i].actualRatio;
        }

        // Chuẩn hóa tổng các tỷ lệ về đúng bằng 1.0 (100% chiều cao thanh background)
        for (int i = 0; i < castingZones.Length; i++)
        {
            castingZones[i].actualRatio /= totalRatio;
        }

        // Tráo đổi ngẫu nhiên vị trí các zone trong mảng (Shuffle)
        for (int i = 0; i < castingZones.Length; i++)
        {
            CastingZoneUI temp = castingZones[i];
            int randomIndex = Random.Range(i, castingZones.Length);
            castingZones[i] = castingZones[randomIndex];
            castingZones[randomIndex] = temp;
        }

        // Cập nhật tọa độ ngưỡng logic và kéo giãn trực quan RectTransform UI
        float currentTop = 0f;
        for (int i = 0; i < castingZones.Length; i++)
        {
            castingZones[i].topThreshold = currentTop;
            castingZones[i].bottomThreshold = currentTop + castingZones[i].actualRatio;
            currentTop = castingZones[i].bottomThreshold;

            if (castingZones[i].visualRect != null)
            {
                castingZones[i].visualRect.anchorMin = new Vector2(0f, 1f - castingZones[i].bottomThreshold);
                castingZones[i].visualRect.anchorMax = new Vector2(1f, 1f - castingZones[i].topThreshold);
                castingZones[i].visualRect.offsetMin = Vector2.zero;
                castingZones[i].visualRect.offsetMax = Vector2.zero;
            }
        }
    }
}