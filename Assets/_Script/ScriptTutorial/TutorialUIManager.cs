using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using TMPro;

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager Instance;

    [Header("Kéo cái Panel Background to vào đây")]
    public GameObject tutorialPanel;

    [Header("Tốc độ hiệu ứng (Càng to càng nhanh)")]
    public float tocDoHieuUng = 15f;

    private bool dangChayHieuUng = false;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

        // Mới vào game là ép tắt bảng luôn
        TatBangMacDinh();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(UnityEngine.Localization.Locale locale)
    {
        if (tutorialPanel != null && tutorialPanel.activeSelf)
        {
            TranslatePanelTexts();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Vừa chuyển sang Map mới cũng ép tắt bảng luôn
        TatBangMacDinh();
    }

    // --- ĐÃ SỬA: Hàm này giờ chỉ có nhiệm vụ giấu cái bảng đi ---
    private void TatBangMacDinh()
    {
        if (tutorialPanel != null)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            dangChayHieuUng = false;

            // Ép tàng hình và thu nhỏ về 0
            tutorialPanel.SetActive(false);
            tutorialPanel.transform.localScale = Vector3.zero;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (tutorialPanel != null && !dangChayHieuUng)
            {
                if (currentCoroutine != null) StopCoroutine(currentCoroutine);

                if (tutorialPanel.activeSelf)
                {
                    currentCoroutine = StartCoroutine(HieuUngDongBang());
                }
                else
                {
                    currentCoroutine = StartCoroutine(HieuUngMoBang());
                }
            }
        }
    }

    private void TranslatePanelTexts()
    {
        if (tutorialPanel == null) return;

        bool isVietnamese = LocalizationSettings.SelectedLocale != null &&
                            LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        var texts = tutorialPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            if (t == null) continue;
            string key = t.gameObject.name;
            try
            {
                var table = LocalizationSettings.StringDatabase.GetTable("Game Text");
                if (table != null)
                {
                    var entry = table.GetEntry(key) ?? table.GetEntry(t.text);
                    if (entry != null)
                    {
                        t.text = entry.GetLocalizedString();
                    }
                }
            }
            catch { }
        }
    }

    private IEnumerator HieuUngMoBang()
    {
        dangChayHieuUng = true;
        TranslatePanelTexts();
        tutorialPanel.SetActive(true);
        tutorialPanel.transform.localScale = Vector3.zero;

        Vector3 kichThuocGoc = Vector3.one;

        while (Vector3.Distance(tutorialPanel.transform.localScale, kichThuocGoc) > 0.01f)
        {
            tutorialPanel.transform.localScale = Vector3.Lerp(tutorialPanel.transform.localScale, kichThuocGoc, Time.unscaledDeltaTime * tocDoHieuUng);
            yield return null;
        }

        tutorialPanel.transform.localScale = kichThuocGoc;
        dangChayHieuUng = false;
    }

    private IEnumerator HieuUngDongBang()
    {
        dangChayHieuUng = true;
        Vector3 kichThuocThuNho = Vector3.zero;

        while (Vector3.Distance(tutorialPanel.transform.localScale, kichThuocThuNho) > 0.01f)
        {
            tutorialPanel.transform.localScale = Vector3.Lerp(tutorialPanel.transform.localScale, kichThuocThuNho, Time.unscaledDeltaTime * tocDoHieuUng);
            yield return null;
        }

        tutorialPanel.transform.localScale = kichThuocThuNho;
        tutorialPanel.SetActive(false);
        dangChayHieuUng = false;
    }
}