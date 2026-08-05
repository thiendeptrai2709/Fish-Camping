using UnityEngine;

public class MapButtonRefresher : MonoBehaviour
{
    private void OnEnable()
    {
        MapButtonVisibility[] buttons = GetComponentsInChildren<MapButtonVisibility>(true);
        foreach (var btn in buttons)
        {
            btn.CheckVisibility();
        }
    }
}