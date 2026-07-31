using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapRegion : MonoBehaviour
{
    [SerializeField] private MapInteractionManager interactionManager;
    [SerializeField] private string sceneName;
    [SerializeField] private string spawnID;
    private RectTransform rectTransform;

    private Button button;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        interactionManager.ExpandMap(rectTransform, sceneName, spawnID);
    }
}