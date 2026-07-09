using UnityEngine;

public class TimeBasedStatDecay : MonoBehaviour
{
    public CharacterStatsManager statsManager;
    public float hungerDecayRate = 1f;
    public float thirstDecayRate = 1.5f;
    public float decayInterval = 1f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= decayInterval)
        {
            timer -= decayInterval;
            ApplyDecay();
        }
    }

    private void ApplyDecay()
    {
        statsManager.ModifyStat(StatType.Hunger, -hungerDecayRate);
        statsManager.ModifyStat(StatType.Thirst, -thirstDecayRate);
    }
}