using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapRegion : MonoBehaviour
{
    [SerializeField] private MapInteractionManager interactionManager;
    [SerializeField] private string sceneName;
    [SerializeField] private string spawnID;

    [Header("Điều kiện mở khóa")]
    [SerializeField] private bool requireTireUpgrade;
    [SerializeField] private int requiredTireIndex;
    [SerializeField] private GameObject lockIcon;

    private RectTransform rectTransform;
    private Button button;
    private bool isLocked;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        UpdateLockState();
    }

    private void UpdateLockState()
    {
        isLocked = false;

        // 1. Khóa Map 2 (Pine Lake) nếu chưa hoàn thành toàn bộ chuỗi nhiệm vụ Map 1
        bool isMap2 = !string.IsNullOrEmpty(sceneName) && 
                      (sceneName.Contains("Map_2") || sceneName.Contains("PineLake") || sceneName.Contains("Map2"));

        if (isMap2 && ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanTravelToMap2())
        {
            isLocked = true;
        }

        // 2. Kiểm tra điều kiện nâng cấp lốp xe
        if (!isLocked && requireTireUpgrade)
        {
            int currentTire = PlayerPrefs.GetInt("EquippedTireIndex", 0);
            isLocked = currentTire < requiredTireIndex;
        }

        if (lockIcon != null)
        {
            lockIcon.SetActive(isLocked);
        }
    }

    private void OnClick()
    {
        UpdateLockState();

        if (isLocked)
        {
            bool isMap2 = !string.IsNullOrEmpty(sceneName) && 
                          (sceneName.Contains("Map_2") || sceneName.Contains("PineLake") || sceneName.Contains("Map2"));

            if (isMap2 && ForcedTutorialManager.Instance != null && !ForcedTutorialManager.Instance.CanTravelToMap2())
            {
                Debug.LogWarning("<color=yellow>[MapRegion] Map 2 (Pine Lake) đang bị khóa! Bạn cần hoàn thành tất cả nhiệm vụ ở Map 1 trước.</color>");
            }
            else
            {
                Debug.Log($"Map {sceneName} bị khóa. Yêu cầu lốp xe chỉ số {requiredTireIndex}");
            }
            return;
        }

        interactionManager.ExpandMap(rectTransform, sceneName, spawnID);
    }
}