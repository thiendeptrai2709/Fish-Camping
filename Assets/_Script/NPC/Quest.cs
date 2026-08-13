using System;

public enum QuestState
{
    NotStarted,  // Chưa nhận
    InProgress,  // Đang làm
    CanClaim,    // Đã hoàn thành, chờ nhận thưởng
    Claimed      // Đã xong hoàn toàn
}

[System.Serializable]
public class Quest
{
    public string id;
    public string title;
    public string description;
    public string targetItem;

    public int currentAmount;
    public int targetAmount;

    public int goldReward;
    public QuestState state = QuestState.NotStarted;
}