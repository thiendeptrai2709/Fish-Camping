using UnityEngine;
using System;

[Serializable]
public class CharacterStat
{
    public StatType statType;
    public float currentValue;
    public float maxValue;
    public float minValue;

    public void Initialize()
    {
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
    }

    public void Modify(float amount)
    {
        currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
    }

    public bool IsEmpty()
    {
        return currentValue <= minValue;
    }

    public bool IsFull()
    {
        return currentValue >= maxValue;
    }
}