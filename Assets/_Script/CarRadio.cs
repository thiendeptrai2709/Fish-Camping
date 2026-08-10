using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class CarRadio : MonoBehaviour
{
    [Header("=== GIAO DIỆN RADIO ===")]
    public Image radioIcon;
    [Range(0f, 1f)] public float doMoKhiTat = 0.5f;

    [Header("=== CÀI ĐẶT NHẠC ===")]
    public AudioClip[] musicTracks;

    [Header("=== ÂM LƯỢNG ===")]
    [Range(0f, 1f)] public float currentVolume = 0.8f; // Âm lượng mặc định 80%
    public float volumeStep = 0.1f; // Mỗi lần bấm tăng/giảm 10%

    private AudioSource audioSource;
    private bool isRadioOn = false;
    private int currentTrackIndex = 0;
    private bool isInCar = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = currentVolume;
        audioSource.playOnAwake = false;

        if (radioIcon != null)
        {
            radioIcon.gameObject.SetActive(false);
            SetIconAlpha(doMoKhiTat);
        }
    }

    void Update()
    {
        if (!isInCar) return;

        if (Keyboard.current != null)
        {
            // Bấm L bật/tắt
            if (Keyboard.current.lKey.wasPressedThisFrame) ToggleRadio();

            // Bấm K chuyển bài
            if (isRadioOn && Keyboard.current.kKey.wasPressedThisFrame) NextTrack();

            // Bấm phím ] để tăng âm lượng
            if (isRadioOn && Keyboard.current.rightBracketKey.wasPressedThisFrame)
            {
                ChangeVolume(volumeStep);
            }

            // Bấm phím [ để giảm âm lượng
            if (isRadioOn && Keyboard.current.leftBracketKey.wasPressedThisFrame)
            {
                ChangeVolume(-volumeStep);
            }
        }
    }

    private void ChangeVolume(float amount)
    {
        currentVolume = Mathf.Clamp01(currentVolume + amount);
        audioSource.volume = currentVolume;
    }

    private void ToggleRadio()
    {
        if (musicTracks == null || musicTracks.Length == 0) return;

        isRadioOn = !isRadioOn;

        if (isRadioOn)
        {
            if (audioSource.clip == null) audioSource.clip = musicTracks[currentTrackIndex];
            audioSource.Play();
            SetIconAlpha(1f);
        }
        else
        {
            audioSource.Pause();
            SetIconAlpha(doMoKhiTat);
        }
    }

    private void NextTrack()
    {
        if (musicTracks == null || musicTracks.Length == 0) return;
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        audioSource.clip = musicTracks[currentTrackIndex];
        audioSource.Play();
    }

    private void SetIconAlpha(float alphaValue)
    {
        if (radioIcon != null)
        {
            Color c = radioIcon.color;
            c.a = alphaValue;
            radioIcon.color = c;
        }
    }
    public void PlayerEnteredCar()
    {
        isInCar = true;
        if (radioIcon != null) radioIcon.gameObject.SetActive(true);
    }
    public void PlayerExitedCar()
    {
        isInCar = false;
        if (radioIcon != null) radioIcon.gameObject.SetActive(false);
        if (isRadioOn) ToggleRadio();
    }
}