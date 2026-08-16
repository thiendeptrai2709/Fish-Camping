using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonSoundManager : MonoBehaviour
{
    public static UIButtonSoundManager Instance { get; private set; }

    [Header("--- AUDIO SETTINGS ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip defaultClickSound;
    [SerializeField] private AudioClip defaultHoverSound;

    private void Awake()
    {
        if (Instance != null && Instance != ExitInstance(this))
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D Sound
            }
        }

        // Tự động quét và gán âm thanh cho toàn bộ Button hiện có
        RegisterAllButtonsInScene();
    }

    public void RegisterAllButtonsInScene()
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in buttons)
        {
            // Bỏ qua các button thuộc Prefab chưa được Instantiate ra Scene
            if (btn.gameObject.scene.name == null) continue;

            RegisterButton(btn);
        }
    }

    public void RegisterButton(Button button)
    {
        // Gán sự kiện Click
        button.onClick.RemoveListener(() => PlayClickSound());
        button.onClick.AddListener(() => PlayClickSound());

        // Gán sự kiện Hover (rê chuột vào nút) nếu có âm thanh hover
        if (defaultHoverSound != null)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { PlayHoverSound(); });
            trigger.triggers.Add(entry);
        }
    }

    public void PlayClickSound(AudioClip customClip = null)
    {
        AudioClip clipToPlay = customClip != null ? customClip : defaultClickSound;
        if (audioSource != null && clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    public void PlayHoverSound()
    {
        if (audioSource != null && defaultHoverSound != null)
        {
            audioSource.PlayOneShot(defaultHoverSound);
        }
    }
    

    private bool ExitInstance(UIButtonSoundManager current) => Instance != current;
}