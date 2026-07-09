using UnityEngine;
using UnityEngine.UI;

public class StatUIBar : MonoBehaviour
{
    public StatType statType;
    public Slider statSlider;
    public Image fillImage;
    public float lerpSpeed = 10f;

    private float targetPercentage = 1f;

    private void Update()
    {
        if (statSlider != null)
        {
            // Hiệu ứng thanh chạy mượt mà (Lerp) thay vì giật cục khi thay đổi giá trị
            statSlider.value = Mathf.Lerp(statSlider.value, targetPercentage, Time.deltaTime * lerpSpeed);
        }
    }

    public void Initialize(float currentValue, float maxValue)
    {
        if (maxValue > 0)
        {
            targetPercentage = currentValue / maxValue;
            if (statSlider != null)
            {
                statSlider.value = targetPercentage;
            }
        }
    }

    public void UpdateStat(float currentValue, float maxValue)
    {
        if (maxValue > 0)
        {
            targetPercentage = currentValue / maxValue;
        }
    }
}