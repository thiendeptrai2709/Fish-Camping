using System;
using System.Collections;
using UnityEngine;

public class ForcedSleepController : MonoBehaviour
{
    public CharacterStatsManager statsManager;
    public float maxEnergyPenaltyStep = 20f;
    public float lowestMaxEnergyLimit = 20f;
    public float normalMaxEnergy = 100f;
    public float maxSleepPenaltyStep = 20f;
    public float lowestMaxSleepLimit = 20f;
    public float normalMaxSleep = 100f;
    public float forcedSleepDuration = 4f;

    public event Action OnForcedSleepStart;
    public event Action OnForcedSleepEnd;

    private bool isSleeping = false;

    public bool IsSleeping()
    {
        return isSleeping;
    }

    public void TriggerForcedSleep()
    {
        if (isSleeping) return;
        StartCoroutine(ForcedSleepRoutine());
    }

    public void ResetAllPenalties()
    {
        CharacterStat energyStat = statsManager.GetStat(StatType.Energy);
        if (energyStat != null)
        {
            energyStat.maxValue = normalMaxEnergy;
        }

        CharacterStat sleepStat = statsManager.GetStat(StatType.Sleep);
        if (sleepStat != null)
        {
            sleepStat.maxValue = normalMaxSleep;
        }
    }

    private IEnumerator ForcedSleepRoutine()
    {
        isSleeping = true;
        OnForcedSleepStart?.Invoke();

        CharacterStat energyStat = statsManager.GetStat(StatType.Energy);
        if (energyStat != null)
        {
            energyStat.maxValue = Mathf.Max(lowestMaxEnergyLimit, energyStat.maxValue - maxEnergyPenaltyStep);
        }

        CharacterStat sleepStat = statsManager.GetStat(StatType.Sleep);
        if (sleepStat != null)
        {
            sleepStat.maxValue = Mathf.Max(lowestMaxSleepLimit, sleepStat.maxValue - maxSleepPenaltyStep);
        }

        yield return new WaitForSeconds(forcedSleepDuration);

        if (energyStat != null)
        {
            statsManager.ModifyStat(StatType.Energy, energyStat.maxValue);
        }
        if (sleepStat != null)
        {
            statsManager.ModifyStat(StatType.Sleep, sleepStat.maxValue);
        }

        isSleeping = false;
        OnForcedSleepEnd?.Invoke();
    }
}