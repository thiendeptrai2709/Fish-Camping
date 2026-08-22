using UnityEngine;
using System.Collections.Generic;

public class CampingRadioController : MonoBehaviour
{
    public static CampingRadioController Instance { get; private set; }

    [Header("--- RADIO TRACKS & AUDIO ---")]
    [SerializeField] private AudioSource radioAudioSource;
    [SerializeField] private AudioClip[] radioPlaylists;
    [SerializeField] private float radioVolume = 0.5f;

    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private float comfortRegenTimer = 0f;

    private readonly string[] defaultStationNames = new string[]
    {
        "Kênh 1: Gió Núi Rì Rào (Acoustic Camp)",
        "Kênh 2: Đêm Trăng Bên Lửa Trại (Lo-Fi Chill)",
        "Kênh 3: Sóng Biển Hoàng Hôn (Ocean Waves)"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("CampingRadioController");
            obj.AddComponent<CampingRadioController>();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (radioAudioSource == null)
        {
            radioAudioSource = GetComponent<AudioSource>();
            if (radioAudioSource == null)
            {
                radioAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        radioAudioSource.loop = true;
        radioAudioSource.playOnAwake = false;
        radioAudioSource.volume = radioVolume;
    }

    private void Update()
    {
        // Phím tắt R để Bật / Tắt / Đổi kênh Radio
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleOrNextStation();
        }

        // Khi Radio đang phát: Tăng chỉ số Thoải mái (Comfort) và Thể lực (Energy) nhẹ nhàng
        if (isPlaying)
        {
            comfortRegenTimer += Time.deltaTime;
            if (comfortRegenTimer >= 2f)
            {
                comfortRegenTimer = 0f;
                if (CharacterStatsManager.Instance != null)
                {
                    CharacterStatsManager.Instance.ModifyStat(StatType.Comfort, 1.5f);
                    CharacterStatsManager.Instance.ModifyStat(StatType.Energy, 0.8f);
                }
            }
        }
    }

    public void ToggleOrNextStation()
    {
        if (!isPlaying)
        {
            PlayRadio();
        }
        else
        {
            // Đổi bài tiếp theo hoặc tắt nếu hết playlist
            currentTrackIndex++;
            int maxTracks = (radioPlaylists != null && radioPlaylists.Length > 0) ? radioPlaylists.Length : defaultStationNames.Length;
            
            if (currentTrackIndex >= maxTracks)
            {
                StopRadio();
            }
            else
            {
                PlayTrack(currentTrackIndex);
            }
        }
    }

    public void PlayRadio()
    {
        isPlaying = true;
        currentTrackIndex = 0;
        PlayTrack(currentTrackIndex);
    }

    public void StopRadio()
    {
        isPlaying = false;
        if (radioAudioSource != null) radioAudioSource.Stop();

        FishingController fc = FindFirstObjectByType<FishingController>();
        if (fc != null)
        {
            fc.ShowFishingFeedback("Đài Radio Dã Ngoại: Đã Tắt", new Color(0.8f, 0.8f, 0.8f));
        }
    }

    private void PlayTrack(int index)
    {
        isPlaying = true;
        string trackTitle = (index < defaultStationNames.Length) ? defaultStationNames[index] : $"Đài Radio Kênh {index + 1}";

        if (radioPlaylists != null && index < radioPlaylists.Length && radioPlaylists[index] != null)
        {
            radioAudioSource.clip = radioPlaylists[index];
            radioAudioSource.Play();
        }

        FishingController fc = FindFirstObjectByType<FishingController>();
        if (fc != null)
        {
            fc.ShowFishingFeedback(trackTitle, new Color(0.4f, 0.9f, 1f));
        }
    }

    public bool IsPlaying => isPlaying;
}
