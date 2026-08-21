using System.Collections.Generic;
using UnityEngine;

public class CharacterStatsUIManager : MonoBehaviour
{
    public CharacterStatsManager statsManager;
    public List<StatUIBar> statUIBars = new List<StatUIBar>();

    private Dictionary<StatType, StatUIBar> uiBarDictionary = new Dictionary<StatType, StatUIBar>();

    private void Start()
    {
        if (statsManager == null)
        {
            statsManager = CharacterStatsManager.Instance ?? FindFirstObjectByType<CharacterStatsManager>();
        }

        // Setup từ điển UI và khởi tạo giá trị ban đầu cho các thanh
        foreach (var bar in statUIBars)
        {
            uiBarDictionary[bar.statType] = bar;
            if (statsManager != null)
            {
                CharacterStat stat = statsManager.GetStat(bar.statType);
                if (stat != null)
                {
                    bar.Initialize(stat.currentValue, stat.maxValue);
                }
            }
        }

        // Lắng nghe sự kiện từ Manager Core (không can thiệp vào logic của file Core cũ)
        if (statsManager != null)
        {
            statsManager.OnStatChanged += HandleStatChanged;
        }
    }

    private void OnDestroy()
    {
        // Hủy lắng nghe sự kiện khi bị phá hủy để tránh lỗi memory leak
        if (statsManager != null)
        {
            statsManager.OnStatChanged -= HandleStatChanged;
        }
    }

    private void HandleStatChanged(StatType type, float currentValue, float maxValue)
    {
        if (uiBarDictionary.TryGetValue(type, out StatUIBar uiBar))
        {
            uiBar.UpdateStat(currentValue, maxValue);
        }
    }
}