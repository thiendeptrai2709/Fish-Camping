using UnityEngine;
using System;

public class SleepStatController : MonoBehaviour
{
    public CharacterStatsManager statsManager;
    public event Action OnSleepDeprived;
    public event Action OnSleepRestored;

    public void CheckSleepStatus()
    {
        CharacterStat sleepStat = statsManager.GetStat(StatType.Sleep);
        if (sleepStat != null)
        {
            if (sleepStat.IsEmpty())
            {
                OnSleepDeprived?.Invoke();
            }
            else if (sleepStat.IsFull())
            {
                OnSleepRestored?.Invoke();
            }
        }
    }

    public void Sleep(float restoreAmount)
    {
        statsManager.ModifyStat(StatType.Sleep, restoreAmount);
        CheckSleepStatus();
    }
}