using System;

public enum QuestState
{
    NotStarted,  // Chưa nhận
    InProgress,  // Đang làm
    CanClaim,    // Đã hoàn thành, chờ nhận thưởng
    Claimed      // Đã xong hoàn toàn
}

public enum QuestType
{
    CatchFish,            // Câu loài cá cụ thể hoặc cá bất kỳ
    CatchGrade,           // Câu cá đạt phẩm chất (Đồng, Bạc, Vàng)
    CatchRecordSize,      // Câu cá đạt chiều dài/cân nặng
    CookFish,             // Nấu chín cá tại bếp dã ngoại
    UpgradeCar,           // Nâng cấp xe/lốp tại gara
    Refuel,               // Đổ xăng tại trạm xăng
    DriveDistance,        // Lái xe di chuyển quãng đường
    DiscoverFishSpecies,  // Ghi nhận loài cá mới vào Sổ Tay
    GenericItem           // Thu thập/Giao nộp item
}

[System.Serializable]
public class Quest
{
    public string id;
    public string title;
    public string description;
    public string targetItem;

    public QuestType questType = QuestType.CatchFish;
    public bool isDaily = false;
    public int requiredGrade = 0; // 0: Any, 1: Bronze, 2: Silver, 3: Gold
    public float requiredMinSize = 0f;

    public int currentAmount;
    public int targetAmount;

    public int goldReward;
    public QuestState state = QuestState.NotStarted;
}