using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapInteractionManager : MonoBehaviour
{
    public bool IsMapExpanded => activeMap != null;

    [SerializeField] private GameObject goButtonObject;
    [SerializeField] private GameObject gameplayCorePrefab;
    private string targetSceneName;
    private string targetSpawnID;

    private RectTransform activeMap;
    private Vector2 originalPosition;
    private Vector2 originalSizeDelta;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Coroutine animationCoroutine;

    public void ExpandMap(RectTransform mapTransform, string sceneName, string spawnID)
    {
        if (activeMap == mapTransform)
        {
            RestoreMap();
            return;
        }

        if (activeMap != null)
        {
            RestoreMapInstantly();
        }

        activeMap = mapTransform;
        originalPosition = mapTransform.anchoredPosition;
        originalSizeDelta = mapTransform.sizeDelta;
        originalAnchorMin = mapTransform.anchorMin;
        originalAnchorMax = mapTransform.anchorMax;

        targetSceneName = sceneName;
        targetSpawnID = spawnID;
        if (goButtonObject != null) goButtonObject.SetActive(false);

        mapTransform.SetAsLastSibling();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateMap(mapTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, true));
    }

    private void RestoreMap()
    {
        if (activeMap != null)
        {
            if (goButtonObject != null) goButtonObject.SetActive(false);

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateMap(activeMap, originalPosition, originalSizeDelta, originalAnchorMin, originalAnchorMax, false));
            activeMap = null;
        }
    }

    public void RestoreMapInstantly()
    {
        if (goButtonObject != null) goButtonObject.SetActive(false);

        if (activeMap != null)
        {
            activeMap.anchoredPosition = originalPosition;
            activeMap.sizeDelta = originalSizeDelta;
            activeMap.anchorMin = originalAnchorMin;
            activeMap.anchorMax = originalAnchorMax;
            activeMap = null;
        }
    }
    public void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(targetSceneName, targetSpawnID, gameplayCorePrefab);
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }

    private IEnumerator AnimateMap(RectTransform target, Vector2 targetPos, Vector2 targetSize, Vector2 anchorMin, Vector2 anchorMax, bool showGoButton)
    {
        Vector2 startPos = target.anchoredPosition;
        Vector2 startSize = target.sizeDelta;
        Vector2 startAnchorMin = target.anchorMin;
        Vector2 startAnchorMax = target.anchorMax;

        float time = 0;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            target.anchorMin = Vector2.Lerp(startAnchorMin, anchorMin, t);
            target.anchorMax = Vector2.Lerp(startAnchorMax, anchorMax, t);
            target.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            target.sizeDelta = Vector2.Lerp(startSize, targetSize, t);

            yield return null;
        }

        if (showGoButton && goButtonObject != null)
        {
            goButtonObject.SetActive(true);
            goButtonObject.transform.SetAsLastSibling();
        }
    }
}