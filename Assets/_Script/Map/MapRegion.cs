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
        if (requireTireUpgrade)
        {
            int currentTire = PlayerPrefs.GetInt("EquippedTireIndex", 0);
            isLocked = currentTire < requiredTireIndex;
        }
        else
        {
            isLocked = false;
        }

        if (lockIcon != null)
        {
            lockIcon.SetActive(isLocked);
        }
    }

    private void OnClick()
    {
        if (isLocked)
        {
            Debug.Log($"Map {sceneName} bị khóa. Yêu cầu lốp xe chỉ số {requiredTireIndex}");
            return;
        }

        interactionManager.ExpandMap(rectTransform, sceneName, spawnID);
    }
}