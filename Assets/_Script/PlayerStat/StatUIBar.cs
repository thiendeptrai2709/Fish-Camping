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
            statSlider.value = Mathf.Lerp(statSlider.value, targetPercentage, Time.deltaTime * lerpSpeed);
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