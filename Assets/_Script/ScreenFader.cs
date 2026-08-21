using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    private static ScreenFader _instance;
    public static ScreenFader Instance
    {
        get
        {
            if (_instance == null)
            {
                EnsureFader();
            }
            return _instance;
        }
    }

    [SerializeField] private Image fadeImage;
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void EnsureFader()
    {
        if (_instance == null)
        {
            _instance = UnityEngine.Object.FindFirstObjectByType<ScreenFader>();
            if (_instance == null)
            {
                GameObject go = new GameObject("[ScreenFader]");
                _instance = go.AddComponent<ScreenFader>();
                DontDestroyOnLoad(go);
            }
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            BuildUIIfNeeded();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Ở các màn hình ngoài menu (Intro, Login, Register, Setting), luôn đảm bảo không bị đen màn hình
        if (!PlayerLocationSaveManager.IsGameplayScene(scene.name))
        {
            SetImmediateAlpha(0f);
        }
    }

    private void BuildUIIfNeeded()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Trên cùng mọi UI
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        if (fadeCanvasGroup == null)
        {
            fadeCanvasGroup = GetComponent<CanvasGroup>();
            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (fadeImage == null)
        {
            Transform imgTrans = transform.Find("FadeBlack");
            if (imgTrans != null)
            {
                fadeImage = imgTrans.GetComponent<Image>();
            }
            else
            {
                GameObject imgObj = new GameObject("FadeBlack");
                imgObj.transform.SetParent(transform, false);
                RectTransform rt = imgObj.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;

                fadeImage = imgObj.AddComponent<Image>();
                fadeImage.color = new Color(0.04f, 0.04f, 0.05f, 1f); // Đen điện ảnh
            }
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f; // MẶC ĐỊNH LUÔN TRONG SUỐT (0f)
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void SetImmediateAlpha(float alpha)
    {
        BuildUIIfNeeded();
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = alpha;
            fadeCanvasGroup.blocksRaycasts = (alpha > 0.01f);
        }
    }

    public IEnumerator FadeOut(float duration = 0.35f)
    {
        BuildUIIfNeeded();
        if (fadeCanvasGroup == null) yield break;
        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;
        float startAlpha = fadeCanvasGroup.alpha;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, timer / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn(float duration = 0.45f)
    {
        BuildUIIfNeeded();
        if (fadeCanvasGroup == null) yield break;

        float timer = 0f;
        float startAlpha = fadeCanvasGroup.alpha;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}