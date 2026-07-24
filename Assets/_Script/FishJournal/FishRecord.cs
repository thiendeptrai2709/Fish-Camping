[System.Serializable]
public class FishRecord
{
    public string fishID;
    public bool isUnlocked;
    public int totalCaught;
    public float maxLength;
    public float maxWeight;

    public FishRecord(string id)
    {
        fishID = id;
        isUnlocked = false;
        totalCaught = 0;
        maxLength = 0f;
        maxWeight = 0f;
    }
}