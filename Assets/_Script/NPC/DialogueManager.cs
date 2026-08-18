using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings; // Thêm thư viện tra cứu Localization
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public bool IsDialogueActive => dialogueCanvas != null && dialogueCanvas.activeInHierarchy;

    [Header("UI Elements")]
    [SerializeField] public GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Localization Config")]
    [SerializeField] private string tableName = "Game Text"; // Tên bảng Localization của bạn

    [Header("Audio Settings (Voice AI)")]
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.04f; // Tốc độ chạy từng chữ

    private Queue<DialogueLine> _voiceDialogueQueue; // Hàng đợi chứa struct câu thoại + voice
    private Queue<string> _sentences;                // Hàng đợi cho thoại chuỗi thường (tương thích ngược)
    private bool _isUsingVoiceQueue = false;

    private bool _isTyping;
    private string _currentSentence;
    private System.Action _onDialogueComplete;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _voiceDialogueQueue = new Queue<DialogueLine>();
        _sentences = new Queue<string>();

        if (voiceAudioSource == null)
            voiceAudioSource = GetComponent<AudioSource>();

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    // 1. HÀM MỚI: Nhận hội thoại kèm file âm thanh Voice AI từ NPCBase
    public void StartDialogueWithVoice(string npcName, DialogueLine[] dialogues, System.Action onComplete = null)
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ClosePanel();
        }

        if (dialogues == null || dialogues.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        dialogueCanvas.SetActive(true);
        nameText.text = GetLocalizedText(npcName); // Tự động dịch tên NPC nếu là Key
        _onDialogueComplete = onComplete;
        _isUsingVoiceQueue = true;

        _voiceDialogueQueue.Clear();
        foreach (var line in dialogues)
        {
            _voiceDialogueQueue.Enqueue(line);
        }

        DisplayNextSentence();
    }

    // 2. HÀM CŨ: Nhận hội thoại dạng string thông thường
    public void StartDialogue(string npcName, string[] dialogues, System.Action onComplete = null)
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ClosePanel();
        }

        if (dialogues == null || dialogues.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        dialogueCanvas.SetActive(true);
        nameText.text = GetLocalizedText(npcName); // Tự động dịch tên NPC nếu là Key
        _onDialogueComplete = onComplete;
        _isUsingVoiceQueue = false;

        _sentences.Clear();
        foreach (string sentence in dialogues)
        {
            // Tự động dịch câu thoại theo ngôn ngữ đang chọn trước khi đưa vào hàng đợi
            _sentences.Enqueue(GetLocalizedText(sentence));
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Nếu chữ đang chạy mà người chơi bấm tiếp, hiện luôn cả câu cho nhanh
        if (_isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = _currentSentence;
            _isTyping = false;
            return;
        }

        // Xử lý nhánh có Voice AI
        if (_isUsingVoiceQueue)
        {
            if (_voiceDialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            DialogueLine currentLine = _voiceDialogueQueue.Dequeue();
            // Tự động tra cứu bản dịch cho câu thoại Voice AI
            _currentSentence = GetLocalizedText(currentLine.text);

            // Dừng voice câu trước và phát voice AI câu hiện tại
            PlayVoiceClip(currentLine.voiceClip);
            StartCoroutine(TypeSentence(_currentSentence));
        }
        // Xử lý nhánh chuỗi thường
        else
        {
            if (_sentences.Count == 0)
            {
                EndDialogue();
                return;
            }

            _currentSentence = _sentences.Dequeue();
            StartCoroutine(TypeSentence(_currentSentence));
        }
    }

    // Hàm phụ trợ tra từ điển: Tự động dịch nếu là Key trong bảng, nếu không có Key thì giữ nguyên chữ gốc
    private string GetLocalizedText(string keyOrText)
    {
        if (string.IsNullOrEmpty(keyOrText)) return "";

        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (table != null)
            {
                var entry = table.GetEntry(keyOrText);
                if (entry != null)
                {
                    return entry.GetLocalizedString();
                }
            }
            return keyOrText;
        }
        catch
        {
            return keyOrText;
        }
    }

    private void PlayVoiceClip(AudioClip clip)
    {
        if (voiceAudioSource != null)
        {
            voiceAudioSource.Stop();
            if (clip != null)
            {
                voiceAudioSource.clip = clip;
                voiceAudioSource.Play();
            }
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        _isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
    }

    private void EndDialogue()
    {
        if (voiceAudioSource != null)
            voiceAudioSource.Stop();

        dialogueCanvas.SetActive(false);
        Debug.Log("Kết thúc hội thoại.");

        _onDialogueComplete?.Invoke();
    }

    public void ForceCloseDialogue()
    {
        StopAllCoroutines();
        if (voiceAudioSource != null)
            voiceAudioSource.Stop();

        _voiceDialogueQueue.Clear();
        _sentences.Clear();
        dialogueCanvas.SetActive(false);
        _isTyping = false;
        _onDialogueComplete = null;
    }
}