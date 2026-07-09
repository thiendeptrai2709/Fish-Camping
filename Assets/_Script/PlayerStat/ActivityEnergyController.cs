using UnityEngine;

public class ActivityEnergyController : MonoBehaviour
{
    public CharacterStatsManager statsManager;

    public bool TryConsumeEnergy(float amount)
    {
        float currentEnergy = statsManager.GetStatValue(StatType.Energy);
        if (currentEnergy >= amount)
        {
            statsManager.ModifyStat(StatType.Energy, -amount);
            return true;
        }
        return false;
    }
}