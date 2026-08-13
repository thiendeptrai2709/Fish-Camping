using UnityEngine;

public class QuestUIBinder : MonoBehaviour
{
    [Header("Kéo UI của Scene này vào đây")]
    public GameObject questPanel;
    public Transform questContentParent;

    private void Start()
    {
        // Vì QuestCanvas luôn BẬT nên hàm Start() chắc chắn sẽ chạy ngay khi Load Scene mới!
        RegisterToManager();
    }

    public void RegisterToManager()
    {
        if (QuestManager.Instance != null && questPanel != null && questContentParent != null)
        {
            QuestManager.Instance.RegisterUI(questPanel, questContentParent);
        }
    }
}