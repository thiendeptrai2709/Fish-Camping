using UnityEngine;

[System.Serializable]
public struct UpgradeLevel
{
    public int level;
    public string upgradeName;
    public int cost;
    public string description;
}

[CreateAssetMenu(fileName = "NewUpgradeData", menuName = "Game/Vehicle Upgrade Data")]
public class VehicleUpgradeSO : ScriptableObject
{
    public string partName = "Lốp xe";
    public UpgradeLevel[] upgradeLevels;
}