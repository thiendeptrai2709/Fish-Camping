using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Cài đặt Âm thanh UI")]
    public AudioSource uiAudioSource; // Nguồn phát âm thanh
    public AudioClip clickSound;      // File âm thanh (tiếng Tít/Click)

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Bỏ comment dòng này nếu muốn loa tổng xuyên qua các Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hàm này để các nút bấm gọi tới
    public void PhatAmThanhClick()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            // PlayOneShot giúp âm thanh click nhiều lần không bị đè hay ngắt quãng
            uiAudioSource.PlayOneShot(clickSound);
        }
    }
}