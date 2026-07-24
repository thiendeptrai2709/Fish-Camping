[System.Serializable]
public class FishRecord
{
    public string fishID;
    public bool isUnlocked;
    public FishGrade highestGrade; // Lưu cấp độ cao nhất từng câu được
    public float maxLength;
    public float maxWeight;

    public FishRecord(string id)
    {
        fishID = id;
        isUnlocked = false;
        highestGrade = FishGrade.Normal; // Mặc định là Normal
        maxLength = 0f;
        maxWeight = 0f;
    }
}