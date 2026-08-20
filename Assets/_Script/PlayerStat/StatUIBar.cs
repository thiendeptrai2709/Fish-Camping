using UnityEngine;
using UnityEngine.UI;

public class StatUIBar : MonoBehaviour
{
    public StatType statType;
    public Slider statSlider;
    public Image fillImage;
    public float lerpSpeed = 10f;

    private float targetPercentage = 1f;
    private float initialMaxValue = 100f;

    private void Update()
    {
        if (statSlider != null)
        {
            float current = statSlider.value;
            if (Mathf.Abs(current - targetPercentage) > 0.001f)
            {
                statSlider.value = Mathf.Lerp(current, targetPercentage, Time.deltaTime * lerpSpeed);
            }
            else if (current != targetPercentage)
            {
                statSlider.value = targetPercentage;
            }
        }
    }

    public void Initialize(float currentValue, float maxValue)
    {
        if (maxValue > 0)
        {
            initialMaxValue = maxValue;
            targetPercentage = currentValue / initialMaxValue;
            if (statSlider != null)
            {
                statSlider.value = targetPercentage;
            }
        }
    }

    public void UpdateStat(float currentValue, float maxValue)
    {
        if (initialMaxValue > 0)
        {
            targetPercentage = currentValue / initialMaxValue;
        }
    }
}