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
        // 1. Chạy hội thoại chào mời của Thuyền trưởng trước
        DialogueManager.Instance.StartDialogue(npcName, shopWelcomeDialogues, () => {

            // 2. SỬA LẠI Ở ĐÂY: Gọi đúng ShopManager và hàm MoShop()
            ShopManager.Instance.MoShop(() => {

                // 3. Khi người chơi tắt UI Shop, kết thúc tương tác đưa NPC về Idle
                onComplete?.Invoke();
            });
        });
    }
}