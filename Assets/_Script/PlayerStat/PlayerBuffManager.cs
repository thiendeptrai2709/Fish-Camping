using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum BuffType
{
    AnglerLuck,     // Tăng tỷ lệ gặp cá hiếm (Rare / Legendary)
    ReelSpeed,      // Tăng tốc độ thu dây / kéo cá
    Endurance,      // Giảm tiêu hao thể lực khi hoạt động & câu cá
    LineTension     // Tăng độ bền và sức chịu tải dây cước trước cá lớn
}

[System.Serializable]
public class ActiveBuff
{
    public BuffType type;
    public string buffName;
    public float multiplier;
    public float remainingTime;
    public float totalDuration;
    public Color displayColor;
}

public class PlayerBuffManager : MonoBehaviour
{
    public static PlayerBuffManager Instance { get; private set; }

    public event Action<BuffType, float> OnBuffApplied;
    public event Action<BuffType> OnBuffExpired;

    private Dictionary<BuffType, ActiveBuff> activeBuffs = new Dictionary<BuffType, ActiveBuff>();

    [Header("--- UI TEXT REFERENCE ---")]
    [SerializeField] private TextMeshProUGUI skillText;
    private float findUITimer = 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("PlayerBuffManager");
            obj.AddComponent<PlayerBuffManager>();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindSkillTextUI();
    }

    private void Update()
    {
        if (activeBuffs.Count > 0)
        {
            List<BuffType> expiredBuffs = null;
            List<BuffType> keys = new List<BuffType>(activeBuffs.Keys);

            foreach (var key in keys)
            {
                ActiveBuff buff = activeBuffs[key];
                buff.remainingTime -= Time.unscaledDeltaTime;

                if (buff.remainingTime <= 0f)
                {
                    if (expiredBuffs == null) expiredBuffs = new List<BuffType>();
                    expiredBuffs.Add(key);
                }
            }

            if (expiredBuffs != null)
            {
                foreach (var key in expiredBuffs)
                {
                    activeBuffs.Remove(key);
                    OnBuffExpired?.Invoke(key);
                }
            }
        }

        UpdateSkillTextUI();
    }

    private void FindSkillTextUI()
    {
        if (skillText != null) return;

        // Quét tìm TextMeshProUGUI tên là "skill" trên UI
        TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in tmps)
        {
            if (t == null) continue;
            string tName = t.gameObject.name.ToLower();
            if (tName == "skill" || tName.Contains("skill"))
            {
                skillText = t;
                break;
            }
        }
    }

    private void UpdateSkillTextUI()
    {
        if (skillText == null)
        {
            findUITimer += Time.deltaTime;
            if (findUITimer >= 1f)
            {
                findUITimer = 0f;
                FindSkillTextUI();
            }
            return;
        }

        if (activeBuffs.Count == 0)
        {
            skillText.text = "";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var pair in activeBuffs)
        {
            ActiveBuff buff = pair.Value;
            int mins = Mathf.FloorToInt(buff.remainingTime / 60f);
            int secs = Mathf.FloorToInt(buff.remainingTime % 60f);
            string hexColor = ColorUtility.ToHtmlStringRGB(buff.displayColor);

            sb.AppendLine($"<color=#{hexColor}>{buff.buffName}</color>: {mins:D2}:{secs:D2}");
        }

        skillText.text = sb.ToString().TrimEnd();
    }

    public void ApplyBuff(BuffType type, string buffName, float duration, float multiplier, Color displayColor)
    {
        if (activeBuffs.TryGetValue(type, out ActiveBuff existingBuff))
        {
            existingBuff.remainingTime = Mathf.Max(existingBuff.remainingTime, duration);
            existingBuff.multiplier = Mathf.Max(existingBuff.multiplier, multiplier);
            existingBuff.buffName = buffName;
        }
        else
        {
            ActiveBuff newBuff = new ActiveBuff
            {
                type = type,
                buffName = buffName,
                multiplier = multiplier,
                remainingTime = duration,
                totalDuration = duration,
                displayColor = displayColor
            };
            activeBuffs[type] = newBuff;
        }

        OnBuffApplied?.Invoke(type, multiplier);

        // Hiển thị thông báo nhận Buff (không dùng icon)
        FishingController fc = FindFirstObjectByType<FishingController>();
        if (fc != null)
        {
            int mins = Mathf.CeilToInt(duration / 60f);
            fc.ShowFishingFeedback($"Nhận Buff: [{buffName}] ({mins} phút)!", displayColor);
        }

        UpdateSkillTextUI();
    }

    public bool HasBuff(BuffType type)
    {
        return activeBuffs.ContainsKey(type) && activeBuffs[type].remainingTime > 0f;
    }

    public float GetBuffMultiplier(BuffType type, float defaultValue = 1f)
    {
        if (activeBuffs.TryGetValue(type, out ActiveBuff buff) && buff.remainingTime > 0f)
        {
            return buff.multiplier;
        }
        return defaultValue;
    }

    public float GetBuffRemainingTime(BuffType type)
    {
        if (activeBuffs.TryGetValue(type, out ActiveBuff buff))
        {
            return buff.remainingTime;
        }
        return 0f;
    }

    /// <summary>
    /// Kích hoạt buff tương ứng với từng món ăn khi người chơi thưởng thức
    /// </summary>
    public void ApplyBuffFromFood(FoodSO foodData, string foodName)
    {
        if (foodData == null) return;
        string nameLower = (foodName + " " + foodData.name + " " + foodData.itemID).ToLower();

        if (nameLower.Contains("biển") || nameLower.Contains("ocean") || nameLower.Contains("mặn") || nameLower.Contains("rô biển"))
        {
            // Cá biển: Tăng sức chịu tải dây câu + Tốc độ kéo cước
            ApplyBuff(BuffType.LineTension, "Cước Thép Đại Dương", 300f, 1.4f, new Color(0.3f, 0.8f, 1f));
            ApplyBuff(BuffType.ReelSpeed, "Kéo Cước Siêu Tốc", 300f, 1.3f, new Color(0.2f, 1f, 0.7f));
        }
        else if (nameLower.Contains("đầm") || nameLower.Contains("swamp") || nameLower.Contains("lầy") || nameLower.Contains("trê") || nameLower.Contains("lóc"))
        {
            // Cá đầm lầy: Tăng Thể lực dẻo dai + May mắn câu cá
            ApplyBuff(BuffType.Endurance, "Thể Lực Dẻo Dai", 360f, 0.5f, new Color(1f, 0.8f, 0.2f));
            ApplyBuff(BuffType.AnglerLuck, "Mắt Đại Bàng Đầm Lầy", 300f, 1.25f, new Color(0.7f, 1f, 0.3f));
        }
        else
        {
            // Món nướng thông thường (Hồ Thông / Nước ngọt): May mắn câu cá + Hồi phục năng lượng
            ApplyBuff(BuffType.AnglerLuck, "May Mắn Thợ Câu", 240f, 1.2f, new Color(0.9f, 0.9f, 0.2f));
            ApplyBuff(BuffType.Endurance, "Sức Bền Cắm Trại", 240f, 0.7f, new Color(0.4f, 0.9f, 1f));
        }
    }
}
