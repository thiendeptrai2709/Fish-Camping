using UnityEngine;

public class FishingZone : MonoBehaviour
{
    [Header("--- FISHING ZONE INFO ---")]
    public string zoneName = "Hồ Nước Ngọt";

    [Tooltip("Danh sách các loại cá có thể câu được ở vùng nước này")]
    public FishSO[] zoneFishes;

    // Hàm trả về ngẫu nhiên 1 con cá trong danh sách (hỗ trợ tăng tỷ lệ cá hiếm theo mồi câu)
    public FishSO GetRandomFish(int rarityBonus = 0)
    {
        if (zoneFishes == null || zoneFishes.Length == 0) return null;

        // Nếu có mồi xịn (rarityBonus > 0), tính trọng số ưu tiên cá Rare và Legendary
        if (rarityBonus > 0)
        {
            float totalWeight = 0f;
            float[] weights = new float[zoneFishes.Length];

            for (int i = 0; i < zoneFishes.Length; i++)
            {
                var fish = zoneFishes[i];
                if (fish == null) continue;

                float baseWeight = 10f;
                switch (fish.rarity)
                {
                    case FishRarity.Common:
                        baseWeight = Mathf.Max(2f, 10f - rarityBonus * 1.5f);
                        break;
                    case FishRarity.Uncommon:
                        baseWeight = 10f + rarityBonus * 1.2f;
                        break;
                    case FishRarity.Rare:
                        baseWeight = 4f + rarityBonus * 3.5f;
                        break;
                    case FishRarity.Legendary:
                        baseWeight = 1f + rarityBonus * 5.0f;
                        break;
                }
                weights[i] = baseWeight;
                totalWeight += baseWeight;
            }

            if (totalWeight > 0f)
            {
                float roll = Random.Range(0f, totalWeight);
                float accum = 0f;
                for (int i = 0; i < zoneFishes.Length; i++)
                {
                    accum += weights[i];
                    if (roll <= accum)
                    {
                        return zoneFishes[i];
                    }
                }
            }
        }

        int randomIndex = Random.Range(0, zoneFishes.Length);
        return zoneFishes[randomIndex];
    }
}