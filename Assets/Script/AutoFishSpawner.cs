using System.Collections.Generic;
using UnityEngine;

public class AutoFishSpawner : MonoBehaviour
{
    [Header("Hệ Thống Liên Kết")]
    public DayNightSystem dayNightSystem;
    public LayerMask waterLayer;
    public string targetWaterTag = "Untagged"; // Thẻ Tag để nhận diện đúng Map

    [Header("Cấu Hình Sinh Cá")]
    public List<FishData> fishList;
    public int totalMaxFishInWorld = 30;  // Tổng số lượng cá tối đa của TẤT CẢ các loài trong Map này
    public float spawnInterval = 5f;

    [Header("Cấu Hình Độ Sâu")]
    public float minDepth = 1f;           // Cách mặt nước tối thiểu bao nhiêu
    public float maxDepth = 6f;           // Sâu tối đa bao nhiêu

    private List<GameObject> spawnedFishes = new List<GameObject>();
    private Dictionary<GameObject, FishData> fishToDataMap = new Dictionary<GameObject, FishData>();
    private float nextSpawnTime;
    private Bounds waterBounds;
    private bool isWaterDetected = false;

    void Start()
    {
        TryDetectWater();
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (!isWaterDetected)
        {
            TryDetectWater();
            return;
        }

        RefreshFishCounter();

        if (Time.time >= nextSpawnTime)
        {
            nextSpawnTime = Time.time + spawnInterval;
            TrySpawnRandomFish();
        }
    }

    void TryDetectWater()
    {
        Collider[] allColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);

        foreach (var col in allColliders)
        {
            string layerName = LayerMask.LayerToName(col.gameObject.layer);


            if (layerName == "Water" && col.CompareTag(targetWaterTag))
            {
                waterBounds = col.bounds;
                isWaterDetected = true;

            }
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
        if (fishData.fishPrefab == null) return;

        float randomX = Random.Range(waterBounds.min.x, waterBounds.max.x);
        float randomZ = Random.Range(waterBounds.min.z, waterBounds.max.z);
        float waterSurfaceY = waterBounds.max.y;
        float randomY = Random.Range(waterSurfaceY - maxDepth, waterSurfaceY - minDepth);
        randomY = Mathf.Max(randomY, waterBounds.min.y); // Giữ cá không bị lún xuống dưới đáy bùn

        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);
        GameObject newFish = Instantiate(fishData.fishPrefab, spawnPosition, Quaternion.identity);
        newFish.transform.SetParent(this.transform);

        spawnedFishes.Add(newFish);
        fishToDataMap.Add(newFish, fishData);
        fishData.currentCount++;
    }
}