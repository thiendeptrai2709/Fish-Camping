using UnityEngine;

public class ActivityEnergyController : MonoBehaviour
{
    public CharacterStatsManager statsManager;

    private void Awake()
    {
        if (statsManager == null)
        {
            statsManager = GetComponent<CharacterStatsManager>() ?? CharacterStatsManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<CharacterStatsManager>();
        }
    }

    public bool TryConsumeEnergy(float amount)
    {
        if (statsManager == null)
        {
            statsManager = GetComponent<CharacterStatsManager>() ?? CharacterStatsManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<CharacterStatsManager>();
        }
        if (statsManager == null) return true;

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