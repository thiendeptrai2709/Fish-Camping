using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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

        // Mới vào game là ép tắt bảng luôn
        TatBangMacDinh();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

    private IEnumerator HieuUngMoBang()
    {
        dangChayHieuUng = true;
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