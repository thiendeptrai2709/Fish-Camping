using UnityEngine;

public class ActivityEnergyController : MonoBehaviour
{
    public CharacterStatsManager statsManager;

    public bool TryConsumeEnergy(float amount)
    {
        float currentEnergy = statsManager.GetStatValue(StatType.Energy);
        if (currentEnergy > 0)
        {
            statsManager.ModifyStat(StatType.Energy, -amount);
            if (statsManager.GetStatValue(StatType.Energy) <= 0)
            {
                ForcedSleepController forcedSleep = GetComponent<ForcedSleepController>();
                if (forcedSleep != null)
                {
                    forcedSleep.TriggerForcedSleep();
                }
            }
            return true;
        }
        return false;
    }
}