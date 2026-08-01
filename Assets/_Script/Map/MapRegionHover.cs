using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MapRegionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MapInteractionManager interactionManager;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float duration = 0.1f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (interactionManager != null && interactionManager.IsMapExpanded && transform.localScale != originalScale)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            transform.localScale = originalScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (interactionManager != null && interactionManager.IsMapExpanded) return;

        transform.SetAsLastSibling();

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale * hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale));
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}