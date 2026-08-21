using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings; // Thêm thư viện Localization

[System.Serializable]
public class QuestData
{
    [Header("--- THÔNG TIN NHIỆM VỤ ---")]
    public string questId = "Quest1";               // ID riêng (Quest1, Quest2...)
    public string questName = "NV1";                // Mã Key hoặc Tên hiển thị
    [TextArea(2, 3)]
    public string questDescription = "QP1";         // Mã Key hoặc Nội dung mô tả
    public string targetItem = "Cá Hồ Cam Đốm";     // Mã Key hoặc Tên cá/vật phẩm
    public QuestType questType = QuestType.CatchFish;
    public int requiredGrade = 0;                   // 0: Bất kỳ, 1: Đồng, 2: Bạc, 3: Vàng
    public float requiredMinSize = 0f;              // Chiều dài tối thiểu nếu có
    public int targetAmount = 1;                    // Số lượng
    public int rewardGold = 1500;                   // Tiền thưởng

    [Header("--- HỘI THOẠI & VOICE AI RIÊNG CHO NHIỆM VỤ NÀY ---")]
    [Tooltip("Thoại lúc đầu giao nhiệm vụ (chạy hết tất cả các câu)")]
    public DialogueLine[] offerDialogues;

    [Tooltip("Thoại nhắc nhở khi đang làm (chọn ngẫu nhiên 1 câu mỗi khi quay lại)")]
    public DialogueLine[] progressDialogues = new DialogueLine[] {
        new DialogueLine { text = "Cậu vẫn đang làm nhiệm vụ đúng không? Cố lên nhé!" },
        new DialogueLine { text = "Tiến độ tới đâu rồi? Nhớ mang đủ đồ về cho ta nhé!" }
    };

    [Tooltip("Thoại khen thưởng khi hoàn thành (chạy hết các câu trả thưởng)")]
    public DialogueLine[] completeDialogues;
}

public class QuestGiver : MonoBehaviour
{
    [Header("Danh Sách Nhiệm Vụ Nối Tiếp (NV1 -> NV2 -> ...)")]
    public List<QuestData> questList = GetDefaultQuestList();

    [Header("Thoại khi ĐÃ HOÀN THÀNH HẾT TẤT CẢ Nhiệm vụ (1 câu ngẫu nhiên)")]
    public DialogueLine[] finalReturningDialogues = new DialogueLine[] {
        new DialogueLine { text = "Cảm ơn cậu nhé! Nhờ có cậu mà mọi việc êm xuôi rồi." },
        new DialogueLine { text = "Hôm nay thời tiết đẹp thật đấy, nghỉ ngơi chút đi cậu!" },
        new DialogueLine { text = "Dạo này khỏe chứ? Ta vẫn nhớ công sức cậu giúp ta đấy!" }
    };

    private void Reset()
    {
        questList = GetDefaultQuestList();
    }

    private void OnValidate()
    {
        if (questList == null || questList.Count == 0)
        {
            questList = GetDefaultQuestList();
        }
    }

    [ContextMenu("Tải Lại 6 Nhiệm Vụ Mặc Định")]
    public void ReloadDefaultQuests()
    {
        questList = GetDefaultQuestList();
    }

    private void Awake()
    {
        if (questList == null || questList.Count == 0)
        {
            questList = GetDefaultQuestList();
        }
    }

    public static List<QuestData> GetDefaultQuestList()
    {
        return new List<QuestData>
        {
            // NV1: Bữa Tiệc Hồ Thông (Map 2)
            new QuestData
            {
                questId = "Story_1",
                questName = "Bữa Tiệc Hồ Thông",
                questDescription = "Lái xe đến Hồ Thông (Map 2), câu 2 con cá tươi mang về cho làng.",
                targetItem = "Cá Tươi",
                questType = QuestType.CatchFish,
                targetAmount = 2,
                rewardGold = 500,
                offerDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Chào cậu! Cậu mới đến làng ta đúng không? Trông cậu tràn đầy năng lượng đấy!" },
                    new DialogueLine { text = "Làng ta đang chuẩn bị một bữa tiệc ấm cúng, cậu có thể lái xe đến Hồ Thông (Map 2) câu 2 con cá tươi mang về giúp ta không?" },
                    new DialogueLine { text = "Ta sẽ thưởng cho cậu 500G để sắm sửa dụng cụ câu cá!" }
                },
                progressDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Cậu đã đến Hồ Thông chưa? Nhớ câu đủ 2 con cá tươi mang về nhé!" },
                    new DialogueLine { text = "Mọi người trong làng đang rất mong chờ cá tươi của cậu đấy!" }
                },
                completeDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Tuyệt vời quá! Cá tươi roi rói, bữa tiệc tối nay chắc chắn sẽ rất vui vẻ!" },
                    new DialogueLine { text = "Đây là 500G tiền công của cậu. Cảm ơn cậu rất nhiều!" }
                }
            },

            // NV2: Hương Vị Lửa Trại (Nướng cá)
            new QuestData
            {
                questId = "Story_2",
                questName = "Hương Vị Cá Nướng",
                questDescription = "Nướng chín 1 đĩa Cá Nướng dã ngoại bên bếp lửa trại mang về cho Cậu chủ làng.",
                targetItem = "Cá Nướng",
                questType = QuestType.CookFish,
                targetAmount = 1,
                rewardGold = 800,
                offerDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Cá tươi thì ngon rồi, nhưng ta nghe nói món Cá Nướng than hoa dã ngoại bên bờ hồ mới là cực phẩm!" },
                    new DialogueLine { text = "Cậu hãy thử nhóm bếp lửa dã ngoại, nướng chín 1 con cá rồi mang về đây cho ta thưởng thức nhé!" },
                    new DialogueLine { text = "Phần thưởng lần này là 800G!" }
                },
                progressDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Mùi cá nướng thơm phức đâu rồi nhỉ? Nhớ dùng bếp dã ngoại để nướng cá chín nhé cậu!" }
                },
                completeDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Mùi thơm nức mũi! Thịt cá vừa ngọt vừa đậm đà, đúng là tay nghề đầu bếp cắm trại tài ba!" },
                    new DialogueLine { text = "Đây là 800G xứng đáng cho tài năng của cậu!" }
                }
            },

            // NV3: Chuẩn Bị Cho Chuyến Đi Xa (Gara / Đổ xăng)
            new QuestData
            {
                questId = "Story_3",
                questName = "Bảo Dưỡng Chuyến Đi Xa",
                questDescription = "Đến gặp Bác thợ máy nâng cấp lốp xe Gai Off-road hoặc đổ đầy bình xăng tại Trạm xăng.",
                targetItem = "Nâng cấp xe / Đổ xăng",
                questType = QuestType.UpgradeCar,
                targetAmount = 1,
                rewardGold = 1000,
                offerDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Sắp tới chúng ta cần thám hiểm vùng Đầm Lầy (Map 3), cung đường này rất gập ghềnh và lầy lội." },
                    new DialogueLine { text = "Cậu hãy ghé Gara của Bác thợ máy nâng cấp bộ lốp Gai Off-road hoặc ghé Trạm xăng nạp đầy nhiên liệu nhé!" },
                    new DialogueLine { text = "Hoàn thành khâu chuẩn bị, ta sẽ thưởng cho cậu 1.000G!" }
                },
                progressDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Đừng mạo hiểm đi xa khi xe chưa được bảo dưỡng và bình xăng chưa đầy nhé cậu!" }
                },
                completeDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Chiếc xe trông khỏe khoắn và sẵn sàng cho mọi địa hình hiểm trở rồi đấy!" },
                    new DialogueLine { text = "Nhận 1.000G này và chuẩn bị lên đường khám phá vùng Đầm Lầy nhé!" }
                }
            },

            // NV4: Thủy Quái Đầm Lầy (Map 3)
            new QuestData
            {
                questId = "Story_4",
                questName = "Đặc Sản Đầm Lầy",
                questDescription = "Lái xe vượt địa hình vào Đầm Lầy (Map 3) và câu 2 con cá đầm lầy.",
                targetItem = "Cá Đầm Lầy",
                questType = QuestType.CatchFish,
                targetAmount = 2,
                rewardGold = 1800,
                offerDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Vùng Đầm Lầy (Map 3) có hệ sinh thái độc đáo với những loài cá da trơn khổng lồ rất quý hiếm." },
                    new DialogueLine { text = "Cậu hãy lái xe vào đó, tìm vùng nước sâu và câu 2 con cá đầm lầy mang về nhé!" },
                    new DialogueLine { text = "Ta sẽ thưởng hậu hĩnh 1.800G cho chuyến thám hiểm này!" }
                },
                progressDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Vùng đầm lầy nhiều bùn lầy, hãy cẩn thận tay lái và kiên nhẫn khi câu cá nhé!" }
                },
                completeDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Thật kinh ngạc! Cậu đã vượt qua đầm lầy hiểm trở và mang về những con cá to thế này!" },
                    new DialogueLine { text = "1.800G này thuộc về cậu, cậu bạn dũng cảm!" }
                }
            },

            // NV5: Cá Vàng May Mắn (Gold Grade)
            new QuestData
            {
                questId = "Story_5",
                questName = "Cá Vàng May Mắn",
                questDescription = "Dùng Mồi câu xịn bắt được ít nhất 1 con cá đạt phẩm chất Vàng Kim (Gold Grade).",
                targetItem = "Cá Vàng Kim",
                questType = QuestType.CatchGrade,
                requiredGrade = 3, // Gold
                targetAmount = 1,
                rewardGold = 2500,
                offerDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Trong truyền thuyết làng ta, ai câu được một con cá Vàng Kim lấp lánh sẽ mang lại thịnh vượng cả đời." },
                    new DialogueLine { text = "Hãy đến Shop của Anh Cần thủ mua loại Mồi câu xịn nhất và thử vận may bắt 1 con cá phẩm chất Vàng Kim xem nào!" },
                    new DialogueLine { text = "Phần thưởng sẽ là 2.500G cực kỳ giá trị!" }
                },
                progressDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Cá Vàng Kim rất tinh khôn, nhớ dùng mồi câu tốt nhất và kéo cần thật khéo nhé!" }
                },
                completeDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Trời ơi! Ánh vàng lấp lánh tuyệt đẹp! Đây đúng là con cá Vàng Kim huyền thoại!" },
                    new DialogueLine { text = "Cậu chính là thợ câu tài ba nhất mà ta từng gặp! Nhận lấy 2.500G này nhé!" }
                }
            },

            // NV6: Chinh Phục Sóng Biển Map 4
            new QuestData
            {
                questId = "Story_6",
                questName = "Chinh Phục Đại Dương",
                questDescription = "Trang bị Cần câu 5 hoặc 6 cùng Mồi biển, câu 2 con cá biển lớn tại Bờ Biển (Map 4).",
                targetItem = "Cá Biển Lớn",
                questType = QuestType.CatchFish,
                targetAmount = 2,
                rewardGold = 3500,
                offerDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Thử thách đỉnh cao cuối cùng đã tới! Bờ Biển (Map 4) có những ngọn sóng dữ dội và những loài cá ngừ đại dương khổng lồ." },
                    new DialogueLine { text = "Hãy sắm Cần câu biển (Cấp 5 hoặc 6) và Mồi biển chuyên dụng để chinh phục đại dương, câu 2 con cá biển lớn về đây!" },
                    new DialogueLine { text = "Phần thưởng danh giá 3.500G và danh hiệu Cần Thủ Huyền Thoại của Làng đang chờ cậu!" }
                },
                progressDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Sóng biển ở Map 4 rất mạnh, chỉ có Cần câu 5 hoặc 6 cùng Phao biển chịu sóng mới câu được thôi đấy!" }
                },
                completeDialogues = new DialogueLine[]
                {
                    new DialogueLine { text = "Không thể tin được! Cậu đã chinh phục được cả những đợt sóng dữ dội của đại dương!" },
                    new DialogueLine { text = "Toàn bộ dân làng đều tự hào về cậu! Đây là 3.500G và danh hiệu Cần Thủ Bậc Thầy của Làng Chài!" }
                }
            }
        };
    }

    public void HandleQuestInteraction(string npcName, Animator animator, System.Action onComplete)
    {
        if (questList == null || questList.Count == 0)
        {
            questList = GetDefaultQuestList();
        }

        // 1. Tự động đóng Panel Quest nếu đang bật
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ClosePanel();
        }

        if (questList == null || questList.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // 2. Tìm nhiệm vụ đầu tiên chưa xong (chưa Claimed)
        QuestData currentQuestData = null;
        QuestState currentState = QuestState.NotStarted;

        foreach (var qData in questList)
        {
            QuestState state = QuestManager.Instance != null
                ? QuestManager.Instance.GetQuestState(qData.questId)
                : QuestState.NotStarted;

            if (state != QuestState.Claimed)
            {
                currentQuestData = qData;
                currentState = state;
                break;
            }
        }

        // 3. Nếu ĐÃ HOÀN THÀNH TẤT CẢ nhiệm vụ -> Chọn ngẫu nhiên DUY NHẤT 1 câu chào ngắn
        if (currentQuestData == null)
        {
            DialogueLine[] singleFinalLine = GetRandomDialogueLine(finalReturningDialogues, "Cảm ơn cậu đã giúp đỡ ta!");
            DialogueManager.Instance.StartDialogueWithVoice(npcName, singleFinalLine, onComplete);
            return;
        }

        // 4. Xử lý các trạng thái của nhiệm vụ hiện tại
        switch (currentState)
        {
            case QuestState.NotStarted:
                // LẦN ĐẦU NHẬN NV: Chạy toàn bộ câu thoại giao việc
                DialogueLine[] offerLines = (currentQuestData.offerDialogues != null && currentQuestData.offerDialogues.Length > 0)
                    ? currentQuestData.offerDialogues
                    : new DialogueLine[] { new DialogueLine { text = $"Ta có việc nhờ cậu: {GetLocalizedText(currentQuestData.questDescription)}" } };

                DialogueManager.Instance.StartDialogueWithVoice(npcName, offerLines, () => {
                    if (QuestManager.Instance != null)
                    {
                        // Tự động dịch Tên, Mô tả và Tên cá trước khi đưa vào hệ thống Quest
                        Quest newQuest = new Quest
                        {
                            id = currentQuestData.questId,
                            title = GetLocalizedText(currentQuestData.questName),
                            description = GetLocalizedText(currentQuestData.questDescription),
                            targetItem = GetLocalizedText(currentQuestData.targetItem),
                            questType = currentQuestData.questType,
                            requiredGrade = currentQuestData.requiredGrade,
                            requiredMinSize = currentQuestData.requiredMinSize,
                            currentAmount = 0,
                            targetAmount = currentQuestData.targetAmount,
                            goldReward = currentQuestData.rewardGold,
                            state = QuestState.InProgress
                        };
                        QuestManager.Instance.AddQuestToList(newQuest);
                    }

                    // Hoàn tất bước hướng dẫn tân thủ
                    ForcedTutorialManager.Instance?.NotifyQuestNPCTalked();

                    onComplete?.Invoke();
                });
                break;

            case QuestState.InProgress:
                // ĐANG LÀM DỞ QUAY LẠI: Chọn ngẫu nhiên DUY NHẤT 1 câu nhắc nhở ngắn
                DialogueLine[] singleProgressLine = GetRandomDialogueLine(currentQuestData.progressDialogues, "Cố gắng hoàn thành nhiệm vụ nhé!");
                DialogueManager.Instance.StartDialogueWithVoice(npcName, singleProgressLine, onComplete);
                break;

            case QuestState.CanClaim:
                // HOÀN THÀNH: Chạy toàn bộ câu trả thưởng và mở khóa nhiệm vụ tiếp theo
                DialogueLine[] completeLines = (currentQuestData.completeDialogues != null && currentQuestData.completeDialogues.Length > 0)
                    ? currentQuestData.completeDialogues
                    : new DialogueLine[] { new DialogueLine { text = "Tuyệt vời! Cảm ơn cậu đã hoàn thành công việc!" } };

                DialogueManager.Instance.StartDialogueWithVoice(npcName, completeLines, () => {
                    if (QuestManager.Instance != null)
                    {
                        QuestManager.Instance.ClaimReward(currentQuestData.questId);
                    }
                    onComplete?.Invoke();
                });
                break;
        }
    }

    // Hàm tiện ích bốc ngẫu nhiên 1 câu DialogueLine
    private DialogueLine[] GetRandomDialogueLine(DialogueLine[] sourceList, string defaultText)
    {
        if (sourceList != null && sourceList.Length > 0)
        {
            int randomIndex = Random.Range(0, sourceList.Length);
            return new DialogueLine[] { sourceList[randomIndex] };
        }
        return new DialogueLine[] { new DialogueLine { text = defaultText } };
    }

    // Hàm phụ trợ tra từ điển: Dò cả 2 bảng "Game Text" và "NPC Text"
    private string GetLocalizedText(string keyOrText)
    {
        if (string.IsNullOrEmpty(keyOrText)) return "";

        try
        {
            // 1. Tìm trong bảng "Game Text" trước
            var gameTable = LocalizationSettings.StringDatabase.GetTable("Game Text");
            if (gameTable != null)
            {
                var entry = gameTable.GetEntry(keyOrText);
                if (entry != null) return entry.GetLocalizedString();
            }

            // 2. Nếu không có, tìm tiếp trong bảng "NPC Text"
            var npcTable = LocalizationSettings.StringDatabase.GetTable("NPC Text");
            if (npcTable != null)
            {
                var entry = npcTable.GetEntry(keyOrText);
                if (entry != null) return entry.GetLocalizedString();
            }

            return keyOrText;
        }
        catch
        {
            return keyOrText;
        }
    }
}