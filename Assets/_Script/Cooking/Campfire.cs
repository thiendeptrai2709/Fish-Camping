using UnityEngine;

public class Campfire : MonoBehaviour
{
    [SerializeField] private GameObject fireVFX;

    [Header("Âm thanh Lửa (Audio)")]
    [SerializeField] private AudioSource fireAudioSource;
    [SerializeField] private AudioClip fireLoopSound;

    private void Awake()
    {
        if (fireAudioSource == null)
        {
            fireAudioSource = GetComponent<AudioSource>();
        }

        // Tự động cấu hình AudioSource nếu có gán clip
        if (fireAudioSource != null && fireLoopSound != null)
        {
            fireAudioSource.clip = fireLoopSound;
            fireAudioSource.loop = true;
            fireAudioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        SetFireActive(false);
    }

    public void SetFireActive(bool isActive)
    {
        if (fireVFX != null)
        {
            fireVFX.SetActive(isActive);
        }

        if (fireAudioSource != null)
        {
            if (isActive)
            {
                if (!fireAudioSource.isPlaying) fireAudioSource.Play();
            }
            else
            {
                if (fireAudioSource.isPlaying) fireAudioSource.Stop();
            }
        }
    }
}