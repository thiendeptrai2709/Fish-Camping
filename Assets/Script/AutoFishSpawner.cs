using System.Collections.Generic;
using UnityEngine;

public class AutoFishSpawner : MonoBehaviour
{
    [Header("Hệ Thống Liên Kết")]
    public DayNightSystem dayNightSystem;

    [Header("Cấu Hình Điểm Sinh Cá (Spawn Points)")]
    public List<Transform> spawnPoints; // Danh sách các điểm cố định trên map

    [Header("Cấu Hình Sinh Cá")]
    public List<FishData> fishList;
    public int totalMaxFishInWorld = 30;  // Tổng số lượng cá tối đa của TẤT CẢ các loài trong Map này
    public float spawnInterval = 5f;

    [Header("Cấu Hình Độ Sâu (Tùy chỉnh quanh điểm spawn)")]
    public float minDepthOffset = 0f;     // Độ lệch sâu tối thiểu so với điểm mốc
    public float maxDepthOffset = 2f;     // Độ lệch sâu tối đa so với điểm mốc

    private List<GameObject> spawnedFishes = new List<GameObject>();
    private Dictionary<GameObject, FishData> fishToDataMap = new Dictionary<GameObject, FishData>();
    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("Chưa gán điểm spawn nào cho AutoFishSpawner!", this);
        }
    }

    void Update()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;

        RefreshFishCounter();

        if (Time.time >= nextSpawnTime)
        {
            nextSpawnTime = Time.time + spawnInterval;
            TrySpawnRandomFish();
        }
    }

    void RefreshFishCounter()
    {
        spawnedFishes.RemoveAll(fish => fish == null);

        foreach (var fish in fishList)
        {
            fish.currentCount = 0;
        }

        List<GameObject> deadFishes = new List<GameObject>();
        foreach (var pair in fishToDataMap)
        {
            if (pair.Key != null)
            {
                pair.Value.currentCount++;
            }
            else
            {
                deadFishes.Add(pair.Key);
            }
        }
        foreach (var dead in deadFishes) fishToDataMap.Remove(dead);
    }

    void TrySpawnRandomFish()
    {
        if (spawnedFishes.Count >= totalMaxFishInWorld) return;

        float currentTime = dayNightSystem.currentTime;
        List<FishData> validFishesForNow = new List<FishData>();

        foreach (var fish in fishList)
        {
            bool isRightTime = IsFishTimeValid(fish, currentTime);
            bool isUnderLimit = fish.currentCount < fish.maxSimultaneous;

            if (isRightTime && isUnderLimit)
            {
                validFishesForNow.Add(fish);
            }
        }

        if (validFishesForNow.Count == 0) return;

        FishData selectedFish = validFishesForNow[Random.Range(0, validFishesForNow.Count)];

        if (Random.Range(0f, 100f) <= selectedFish.spawnChance)
        {
            ExecuteSpawn(selectedFish);
        }
    }

    bool IsFishTimeValid(FishData fish, float time)
    {
        bool isMorning = time >= 0.2f && time < 0.5f;
        bool isAfternoon = time >= 0.5f && time < 0.7f;
        bool isEvening = time >= 0.7f && time < 0.8f;
        bool isNight = time >= 0.8f || time < 0.2f;

        if (fish.spawnMorning && isMorning) return true;
        if (fish.spawnAfternoon && isAfternoon) return true;
        if (fish.spawnEvening && isEvening) return true;
        if (fish.spawnNight && isNight) return true;

        return false;
    }

    void ExecuteSpawn(FishData fishData)
    {
        if (fishData.fishPrefab == null || spawnPoints.Count == 0) return;

        // Chọn ngẫu nhiên một điểm trong danh sách điểm spawn đã thiết lập
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        if (randomPoint == null) return;

        // Tính toán vị trí dựa trên điểm mốc cộng thêm độ sâu ngẫu nhiên
        Vector3 basePos = randomPoint.position;
        float randomOffsetY = Random.Range(minDepthOffset, maxDepthOffset);

        Vector3 spawnPosition = new Vector3(basePos.x, basePos.y - randomOffsetY, basePos.z);

        GameObject newFish = Instantiate(fishData.fishPrefab, spawnPosition, Quaternion.identity);
        newFish.transform.SetParent(this.transform);

        spawnedFishes.Add(newFish);
        fishToDataMap.Add(newFish, fishData);
        fishData.currentCount++;
    }
}