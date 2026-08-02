using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public bool IsDialogueActive => dialogueCanvas != null && dialogueCanvas.activeInHierarchy;


    [Header("UI Elements")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.04f; // Tốc độ chạy từng chữ

    private Queue<string> _sentences; // Hàng đợi chứa các câu thoại
    private bool _isTyping;
    private string _currentSentence;
    private System.Action _onDialogueComplete; // Hành động chạy sau khi hết thoại (Ví dụ: mở Shop)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _sentences = new Queue<string>();
        dialogueCanvas.SetActive(false); // Đảm bảo UI ẩn khi vào game
    }

    // Hàm gọi từ NPC để bắt đầu nói chuyện
    public void StartDialogue(string npcName, string[] dialogues, System.Action onComplete = null)
    {
        dialogueCanvas.SetActive(true);
        nameText.text = npcName;
        _onDialogueComplete = onComplete;

        _sentences.Clear();

        // Nạp tất cả các câu thoại của NPC vào hàng đợi
        foreach (string sentence in dialogues)
        {
            _sentences.Enqueue(sentence);
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

        // Nếu đã hết câu thoại trong hàng đợi thì kết thúc hội thoại
        if (_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        _currentSentence = _sentences.Dequeue();
        StartCoroutine(TypeSentence(_currentSentence));
    }

    // Hiệu ứng đánh máy chữ chạy từ từ (ASMR)
    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        _isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            // Ở ĐÂY BẠN CÓ THỂ CHÈN TIẾNG "TÍT TÍT" NHẸ CỦA HỘI THOẠI
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
        Debug.Log("Kết thúc hội thoại.");

        // Nếu có sự kiện cài cắm phía sau (như mở UI Shop), kích hoạt nó ngay
        _onDialogueComplete?.Invoke();
    }

    public void ForceCloseDialogue()
    {
        StopAllCoroutines();
        _sentences.Clear();
        dialogueCanvas.SetActive(false);
        _isTyping = false;
        _onDialogueComplete = null;
    }
}