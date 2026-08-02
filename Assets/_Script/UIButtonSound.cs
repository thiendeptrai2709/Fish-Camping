using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // Tự động yêu cầu object phải có UI Button
public class UIButtonSound : MonoBehaviour
{
    private void Start()
    {
        Button nutBam = GetComponent<Button>();

        if (nutBam != null)
        {
            // Tự động chèn lệnh gọi âm thanh vào sự kiện click của nút
            nutBam.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PhatAmThanhClick();
                }
            });
        }
    }
}