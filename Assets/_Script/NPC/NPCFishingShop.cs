using UnityEngine;

public class NPCFishingShop : MonoBehaviour
{
    [Header("Dialogue Config")]
    [SerializeField]
    [TextArea(2, 5)]
    private string[] shopWelcomeDialogues = new string[] {
        "Chào cậu bạn trẻ! Cần một vài mồi câu bén hay cần xịn để ra hồ à?",
        "Hoặc nếu câu được con cá nào tươi ngon, cứ đem hết lại đây ta thu mua giá tốt cho nhé!"
    };

    public void HandleShopInteraction(string npcName, System.Action onComplete)
    {
        // 1. Kiểm tra an toàn tránh crash nếu thiếu DialogueManager
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(npcName, shopWelcomeDialogues, () => {

                // 2. SỬA DÒNG 19: Đổi từ ShopUIManager sang Shop_Tab_Manager + Bọc check Null
                if (Shop_Tab_Manager.Instance != null)
                {
                    Shop_Tab_Manager.Instance.OpenShop(() => {
                        // 3. Khi tắt Shop -> Trả NPC về trạng thái Idle
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    Debug.LogError("[NPCFishingShop] Không tìm thấy Shop_Tab_Manager.Instance trong Scene! Hãy kiểm tra GameObject Shop_Tab_Manager.");
                    onComplete?.Invoke();
                }
            });
        }
        else
        {
            Debug.LogError("[NPCFishingShop] Thiếu DialogueManager trong Scene!");
            onComplete?.Invoke();
        }
    }
}