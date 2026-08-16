using UnityEngine;

public class NPCOffroadUpgrade : MonoBehaviour
{
    [Header("=== THOẠI LẦN ĐẦU GẶP MẶT ===")]
    [SerializeField]
    private DialogueLine[] upgradeFirstTimeDialogues = new DialogueLine[] {
        new DialogueLine { 
            text = "Chào cậu! Chiếc xe tải này nhìn rỉ sét quá nhỉ?" 
        },
        new DialogueLine { 
            text = "Nếu cậu muốn đi vào khu vực Đầm Lầy Sương Mù, lốp thường sẽ bị trơn trượt đấy." 
        },
        new DialogueLine { 
            text = "Để ta xem xem có thể nâng cấp bộ lốp gai off-road nào cho cậu không!" 
        }
    };

    [Header("=== THOẠI CÁC LẦN SAU QUAY LẠI (1 CÂU) ===")]
    [SerializeField]
    private DialogueLine[] upgradeReturningDialogues = new DialogueLine[] {
        new DialogueLine { text = "Muốn độ thêm món gì cho xe nữa à?" },
        new DialogueLine { text = "Xe cộ dạo này chạy êm chứ? Cần nâng cấp gì cứ bảo ta!" },
        new DialogueLine { text = "Để ta kiểm tra lại dàn gầm và lốp xe cho cậu nhé!" }
    };

    private bool _hasMetPlayer = false; // Cờ ghi nhớ đã gặp lần đầu chưa

    public void HandleUpgradeInteraction(string npcName, System.Action onComplete)
    {
        // Chọn danh sách thoại: Lần đầu nói hết, lần sau bốc ngẫu nhiên 1 câu
        DialogueLine[] linesToPlay;

        if (!_hasMetPlayer)
        {
            _hasMetPlayer = true;
            linesToPlay = upgradeFirstTimeDialogues;
        }
        else
        {
            linesToPlay = GetRandomReturningLine();
        }

        // 1. Chạy hội thoại kèm file Voice AI của thợ nâng cấp xe
        DialogueManager.Instance.StartDialogueWithVoice(npcName, linesToPlay, () => {

            // 2. Hội thoại kết thúc thì mở giao diện Gara
            if (GarageZone.Instance != null)
            {
                GarageZone.Instance.OpenGarage(() => {
                    // 3. Khi người chơi tắt giao diện Gara, hoàn tất tương tác đưa NPC về Idle
                    onComplete?.Invoke();
                });
            }
            else
            {
                onComplete?.Invoke();
            }
        });
    }

    private DialogueLine[] GetRandomReturningLine()
    {
        if (upgradeReturningDialogues != null && upgradeReturningDialogues.Length > 0)
        {
            int randomIndex = Random.Range(0, upgradeReturningDialogues.Length);
            return new DialogueLine[] { upgradeReturningDialogues[randomIndex] };
        }

        if (upgradeFirstTimeDialogues != null && upgradeFirstTimeDialogues.Length > 0)
        {
            return new DialogueLine[] { upgradeFirstTimeDialogues[0] };
        }

        return new DialogueLine[] { new DialogueLine { text = "Cần nâng cấp xe à?" } };
    }
}