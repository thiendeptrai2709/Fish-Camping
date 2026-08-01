using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroSequenceManager : MonoBehaviour
{
    [Header("=== 1. LOGO & WHITE PANEL SETTINGS ===")]
    public CanvasGroup whitePanelCanvasGroup;
    public float logoFadeInDuration = 1.5f;
    public float logoDisplayDuration = 2.0f;
    public float logoFadeOutDuration = 1.0f;

    [Header("=== 2. VIDEO INTRO SETTINGS ===")]
    public GameObject videoUIPanel;
    public VideoPlayer videoPlayer;
    public bool allowSkipVideo = true;

    [Header("=== 3. MÀN HÌNH CHỜ (PLAY BUTTON) ===")]
    [Tooltip("GameObject chứa Ảnh Nền (Background) lúc bấm Play")]
    public GameObject playBackgroundObject;
    [Tooltip("GameObject chứa Nút/Chữ PLAY")]
    public GameObject playButtonObject;

    [Header("=== 4. SCENE LOGIC (LOGIN) & FADE SETTINGS ===")]
    public string targetSceneName = "LoginScene";
    public CanvasGroup fadeTransitionPanel;
    public float fadeTransitionDuration = 1.0f;

    private bool isVideoFinished = false;
    private bool isPlayClicked = false;

    private IEnumerator Start()
    {
        // 0. TRẠNG THÁI BAN ĐẦU
        if (whitePanelCanvasGroup != null)
        {
            whitePanelCanvasGroup.gameObject.SetActive(true);
            whitePanelCanvasGroup.alpha = 0f;
        }
        if (videoUIPanel != null) videoUIPanel.SetActive(false);
        if (playBackgroundObject != null) playBackgroundObject.SetActive(false); // Ẩn nền chờ
        if (playButtonObject != null) playButtonObject.SetActive(false); // Ẩn nút PLAY
        if (fadeTransitionPanel != null)
        {
            fadeTransitionPanel.gameObject.SetActive(false);
            fadeTransitionPanel.alpha = 0f;
        }

        // ==========================================
        // GIAI ĐOẠN 1: LOGO TRÊN PANEL TRẮNG HIỆN DẦN
        // ==========================================
        if (whitePanelCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < logoFadeInDuration)
            {
                timer += Time.deltaTime;
                whitePanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / logoFadeInDuration);
                yield return null;
            }
            whitePanelCanvasGroup.alpha = 1f;

            yield return new WaitForSeconds(logoDisplayDuration);

            timer = 0f;
            while (timer < logoFadeOutDuration)
            {
                timer += Time.deltaTime;
                whitePanelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / logoFadeOutDuration);
                yield return null;
            }
            whitePanelCanvasGroup.alpha = 0f;
            whitePanelCanvasGroup.gameObject.SetActive(false);
        }

        // ==========================================
        // GIAI ĐOẠN 2: CHUYỂN CẢNH SANG VIDEO INTRO
        // ==========================================
        if (videoPlayer != null && videoUIPanel != null)
        {
            videoUIPanel.SetActive(true);
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Play();

            while (!isVideoFinished)
            {
                if (allowSkipVideo && (Input.anyKeyDown || Input.GetKeyDown(KeyCode.Space)))
                {
                    break;
                }
                yield return null;
            }

            videoPlayer.Stop();
            // Đã có background mới nên ta TẮT luôn panel video cho nhẹ máy
            videoUIPanel.SetActive(false);
        }

        // ==========================================
        // GIAI ĐOẠN 3: HIỆN BACKGROUND VÀ CHỮ "PLAY"
        // ==========================================
        if (playBackgroundObject != null) playBackgroundObject.SetActive(true); // Bật ảnh nền

        if (playButtonObject != null)
        {
            playButtonObject.SetActive(true); // Bật chữ PLAY

            while (!isPlayClicked)
            {
                yield return null;
            }

            playButtonObject.SetActive(false); // Ẩn chữ đi khi đã bấm
        }

        // ==========================================
        // GIAI ĐOẠN 4: FADE TO BLACK VÀ LOAD SCENE
        // ==========================================
        if (fadeTransitionPanel != null)
        {
            fadeTransitionPanel.gameObject.SetActive(true);
            float timer = 0f;
            while (timer < fadeTransitionDuration)
            {
                timer += Time.deltaTime;
                fadeTransitionPanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeTransitionDuration);
                yield return null;
            }
            fadeTransitionPanel.alpha = 1f;
        }

        SceneManager.LoadSceneAsync(targetSceneName);
    }

    public void OnPlayButtonClicked()
    {
        isPlayClicked = true;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        isVideoFinished = true;
    }
}