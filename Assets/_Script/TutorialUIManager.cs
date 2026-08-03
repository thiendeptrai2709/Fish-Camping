using UnityEngine;
using System.Collections;

public class TutorialUIManager : MonoBehaviour
{
    [Header("Kéo cái Panel Background to vào đây")]
    public GameObject tutorialPanel;

    [Header("Tốc độ hiệu ứng (Càng to càng nhanh)")]
    public float tocDoHieuUng = 15f;

    // Biến chặn spam phím khi bảng đang chạy hiệu ứng
    private bool dangChayHieuUng = false;

    void Start()
    {
        // Vừa vào game là ẩn bảng và thu nhỏ về 0
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            tutorialPanel.transform.localScale = Vector3.zero;
        }
    }

    void Update()
    {
        // Đã đổi sang phím P để bật/tắt bảng
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Kiểm tra xem có đang bị kẹt hiệu ứng không
            if (tutorialPanel != null && !dangChayHieuUng)
            {
                // Nếu bảng đang bật -> thì gọi hiệu ứng Đóng
                if (tutorialPanel.activeSelf)
                {
                    StartCoroutine(HieuUngDongBang());
                }
                // Nếu bảng đang tắt -> thì gọi hiệu ứng Mở
                else
                {
                    StartCoroutine(HieuUngMoBang());
                }
            }
        }
    }

    // --- HIỆU ỨNG MỞ BẢNG ---
    private IEnumerator HieuUngMoBang()
    {
        dangChayHieuUng = true;

        tutorialPanel.SetActive(true);
        tutorialPanel.transform.localScale = Vector3.zero;

        Vector3 kichThuocGoc = Vector3.one;

        while (Vector3.Distance(tutorialPanel.transform.localScale, kichThuocGoc) > 0.01f)
        {
            tutorialPanel.transform.localScale = Vector3.Lerp(tutorialPanel.transform.localScale, kichThuocGoc, Time.deltaTime * tocDoHieuUng);
            yield return null;
        }

        tutorialPanel.transform.localScale = kichThuocGoc;
        dangChayHieuUng = false;
    }

    // --- HIỆU ỨNG ĐÓNG BẢNG ---
    private IEnumerator HieuUngDongBang()
    {
        dangChayHieuUng = true;

        Vector3 kichThuocThuNho = Vector3.zero;

        while (Vector3.Distance(tutorialPanel.transform.localScale, kichThuocThuNho) > 0.01f)
        {
            tutorialPanel.transform.localScale = Vector3.Lerp(tutorialPanel.transform.localScale, kichThuocThuNho, Time.deltaTime * tocDoHieuUng);
            yield return null;
        }

        tutorialPanel.transform.localScale = kichThuocThuNho;
        tutorialPanel.SetActive(false);
        dangChayHieuUng = false;
    }
}