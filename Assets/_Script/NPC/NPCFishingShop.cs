using UnityEngine;

public class NPCFishingShop : MonoBehaviour
{
    [Header("=== THOẠI LẦN ĐẦU GẶP MẶT ===")]
    [SerializeField]
    private DialogueLine[] shopFirstTimeDialogues = new DialogueLine[] {
        new DialogueLine { 
            text = "Chào cậu bạn trẻ! Cần một vài mồi câu bén hay cần xịn để ra hồ à?" 
        },
        new DialogueLine { 
            text = "Hoặc nếu câu được con cá nào tươi ngon, cứ đem hết lại đây ta thu mua giá tốt cho nhé!" 
        }
    };

    [Header("=== THOẠI CÁC LẦN SAU QUAY LẠI (1 CÂU) ===")]
    [SerializeField]
    private DialogueLine[] shopReturningDialogues = new DialogueLine[] {
        new DialogueLine { text = "Lại ghé tiệm câu cá à cậu bạn trẻ?" },
        new DialogueLine { text = "Hôm nay có chiến lợi phẩm nào tươi ngon không?" },
        new DialogueLine { text = "Xem qua chút mồi câu và dụng cụ mới về nhé!" }
    };

    private bool _hasMetPlayer = false; // Cờ ghi nhớ đã gặp lần đầu chưa

    public void HandleShopInteraction(string npcName, System.Action onComplete)
    {
        // Chọn danh sách thoại: Lần đầu nói hết, lần sau bốc 1 câu
        DialogueLine[] linesToPlay;

        if (!_hasMetPlayer)
        {
            _hasMetPlayer = true;
            linesToPlay = shopFirstTimeDialogues;
        }
        else
        {
            linesToPlay = GetRandomReturningLine();
        }

        // 1. Chạy thoại kèm Voice AI (nếu có DialogueManager)
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueWithVoice(npcName, linesToPlay, () => {
                OpenShopUI(onComplete);
            });
        }
        else
        {
            OpenShopUI(onComplete);
        }
    }

    private void OpenShopUI(System.Action onComplete)
    {
        if (ShopManager.Instance == null)
        {
            ShopManager.Instance = Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
        }

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.MoShop(() => {
                onComplete?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("[NPCFishingShop] Không tìm thấy ShopManager trong Scene!");
            onComplete?.Invoke();
        }
    }

    private DialogueLine[] GetRandomReturningLine()
    {
        if (shopReturningDialogues != null && shopReturningDialogues.Length > 0)
        {
            int randomIndex = Random.Range(0, shopReturningDialogues.Length);
            return new DialogueLine[] { shopReturningDialogues[randomIndex] };
        }

        if (shopFirstTimeDialogues != null && shopFirstTimeDialogues.Length > 0)
        {
            return new DialogueLine[] { shopFirstTimeDialogues[0] };
        }

        return new DialogueLine[] { new DialogueLine { text = "Cần mua gì nào?" } };
    }
}