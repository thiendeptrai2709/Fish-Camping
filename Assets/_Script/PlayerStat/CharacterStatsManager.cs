using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterStatsManager : MonoBehaviour
{
    public List<CharacterStat> initialStats = new List<CharacterStat>();
    private Dictionary<StatType, CharacterStat> statDictionary = new Dictionary<StatType, CharacterStat>();

    public event Action<StatType, float, float> OnStatChanged;
    public static CharacterStatsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        // ---------------------------

        foreach (var stat in initialStats)
        {
            stat.Initialize();
            statDictionary[stat.statType] = stat;
        }
    }

    public void ModifyStat(StatType type, float amount)
    {
        if (statDictionary.TryGetValue(type, out CharacterStat stat))
        {
            stat.Modify(amount);
            OnStatChanged?.Invoke(type, stat.currentValue, stat.maxValue);
        }
    }

    public float GetStatValue(StatType type)
    {
        if (statDictionary.TryGetValue(type, out CharacterStat stat))
        {
            return stat.currentValue;
        }
        return 0f;
    }

    public CharacterStat GetStat(StatType type)
    {
        statDictionary.TryGetValue(type, out CharacterStat stat);
        return stat;
    }
}