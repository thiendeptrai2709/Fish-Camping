using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Gọi thư viện quản lý Scene

public class TutorialUIManager : MonoBehaviour
{
    // Cấp kim bài miễn tử (Singleton) để không bị nhân bản khi quay lại Map 1
    public static TutorialUIManager Instance;

    [Header("Kéo cái Panel Background to vào đây")]
    public GameObject tutorialPanel; //[cite: 4]

    [Header("Tốc độ hiệu ứng (Càng to càng nhanh)")]
    public float tocDoHieuUng = 15f; //[cite: 4]

    private bool dangChayHieuUng = false; //[cite: 4]

    private void Awake()
    {
        // Kiểm tra xem đã có bảng Tutorial nào tồn tại chưa
        if (Instance == null)
        {
            Instance = this;
            // Tách object này ra độc lập (nếu đang là con của object khác)
            transform.SetParent(null);
            // Cấp kim bài miễn tử khi chuyển Scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Nếu lỡ quay lại Map 1 mà đã có bảng rồi thì xóa bản sao đi
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Lắng nghe sự kiện mỗi khi load xong 1 Scene bất kỳ
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Chạy kiểm tra ngay khi vừa vào game
        KiemTraVaHienThiBang();
    }

    private void OnDestroy()
    {
        // Dọn dẹp bộ nhớ khi tắt game
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Hàm này sẽ tự động chạy mỗi khi bồ chuyển sang Map mới
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        KiemTraVaHienThiBang();
    }

    private void KiemTraVaHienThiBang()
    {
        if (tutorialPanel != null)
        {
            if (SceneManager.GetActiveScene().name == "Map_1_Town")
            {
                // Tự động bật lên khi ở Map 1
                tutorialPanel.SetActive(true);
                tutorialPanel.transform.localScale = Vector3.zero;
                StartCoroutine(HieuUngMoBang());
            }
            else
            {
                // Tự động giấu đi khi ở các Map khác
                tutorialPanel.SetActive(false);
                tutorialPanel.transform.localScale = Vector3.zero;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) //[cite: 4]
        {
            if (tutorialPanel != null && !dangChayHieuUng) //[cite: 4]
            {
                if (tutorialPanel.activeSelf) //[cite: 4]
                {
                    StartCoroutine(HieuUngDongBang()); //[cite: 4]
                }
                else //[cite: 4]
                {
                    StartCoroutine(HieuUngMoBang()); //[cite: 4]
                }
            }
        }
    }

    private IEnumerator HieuUngMoBang()
    {
        dangChayHieuUng = true; //[cite: 4]
        tutorialPanel.SetActive(true); //[cite: 4]
        tutorialPanel.transform.localScale = Vector3.zero; //[cite: 4]

        Vector3 kichThuocGoc = Vector3.one; //[cite: 4]

        while (Vector3.Distance(tutorialPanel.transform.localScale, kichThuocGoc) > 0.01f) //[cite: 4]
        {
            tutorialPanel.transform.localScale = Vector3.Lerp(tutorialPanel.transform.localScale, kichThuocGoc, Time.deltaTime * tocDoHieuUng); //[cite: 4]
            yield return null; //[cite: 4]
        }

        tutorialPanel.transform.localScale = kichThuocGoc; //[cite: 4]
        dangChayHieuUng = false; //[cite: 4]
    }

    private IEnumerator HieuUngDongBang()
    {
        dangChayHieuUng = true; //[cite: 4]
        Vector3 kichThuocThuNho = Vector3.zero; //[cite: 4]

        while (Vector3.Distance(tutorialPanel.transform.localScale, kichThuocThuNho) > 0.01f) //[cite: 4]
        {
            tutorialPanel.transform.localScale = Vector3.Lerp(tutorialPanel.transform.localScale, kichThuocThuNho, Time.deltaTime * tocDoHieuUng); //[cite: 4]
            yield return null; //[cite: 4]
        }

        tutorialPanel.transform.localScale = kichThuocThuNho; //[cite: 4]
        tutorialPanel.SetActive(false); //[cite: 4]
        dangChayHieuUng = false; //[cite: 4]
    }
}