using UnityEngine;

public class SleepDecayController : MonoBehaviour
{
    public CharacterStatsManager statsManager;
    public float dayDecayRate = 0.5f;
    public float nightDecayRate = 2.5f;
    public float decayInterval = 1f;
    public bool isNightTime = false;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= decayInterval)
        {
            timer -= decayInterval;
            ApplySleepDecay();
        }
    }

    public void SetNightTime(bool isNight)
    {
        isNightTime = isNight;
    }

    private void ApplySleepDecay()
    {
        float currentRate = isNightTime ? nightDecayRate : dayDecayRate;
        statsManager.ModifyStat(StatType.Sleep, -currentRate);
    }
}