using UnityEngine;

[CreateAssetMenu(fileName = "NewTire", menuName = "FishAndCamping/Tire Data")]
public class TireData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string tireName;
    public Sprite tireIcon;
    public int price;
    [TextArea]
    public string description;

    [Header("Chỉ số độ bám (Ma sát) trên 4 Map")]
    [Range(0f, 1f)] public float cityFriction;   // Map Phố
    [Range(0f, 1f)] public float forestFriction; // Map Rừng thông
    [Range(0f, 1f)] public float swampFriction;  // Map Đầm lầy
    [Range(0f, 1f)] public float sandFriction;   // Map Cát biển
}