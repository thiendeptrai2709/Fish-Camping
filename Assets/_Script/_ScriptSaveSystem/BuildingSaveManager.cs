using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PlacedItemSaveData
{
    public string itemID;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ;

    public PlacedItemSaveData(string id, Vector3 pos, Vector3 rot)
    {
        itemID = id;
        posX = pos.x; posY = pos.y; posZ = pos.z;
        rotX = rot.x; rotY = rot.y; rotZ = rot.z;
    }

    public Vector3 GetPosition() => new Vector3(posX, posY, posZ);
    public Quaternion GetRotation() => Quaternion.Euler(rotX, rotY, rotZ);
}

[Serializable]
public class MapBuildingSaveWrapper
{
    public List<PlacedItemSaveData> items = new List<PlacedItemSaveData>();
}

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance { get; private set; }

    [Header("Danh sách BuildableItemSO trong Game")]
    [SerializeField] private List<BuildableItemSO> allBuildableItems = new List<BuildableItemSO>();

    private Dictionary<string, BuildableItemSO> itemLookup = new Dictionary<string, BuildableItemSO>();
    private List<PlacedItemSaveData> currentScenePlacedItems = new List<PlacedItemSaveData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLookup();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeLookup()
    {
        itemLookup.Clear();

        if (allBuildableItems == null || allBuildableItems.Count == 0)
        {
            BuildableItemSO[] found = Resources.FindObjectsOfTypeAll<BuildableItemSO>();
            if (found != null && found.Length > 0)
            {
                allBuildableItems = new List<BuildableItemSO>(found);
            }
        }

        foreach (var item in allBuildableItems)
        {
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.itemName) && !itemLookup.ContainsKey(item.itemName))
                {
                    itemLookup.Add(item.itemName, item);
                }
                if (!string.IsNullOrEmpty(item.name) && !itemLookup.ContainsKey(item.name))
                {
                    itemLookup.Add(item.name, item);
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScenePlacedItems.Clear();
        InitializeLookup();
        LoadSceneBuildings();
    }

    private string GetSaveFilePath()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return Path.Combine(Application.persistentDataPath, $"{sceneName}_placed_buildings.json");
    }

    public void SavePlacedItem(BuildableItemSO item, Vector3 position, Quaternion rotation)
    {
        if (item == null) return;

        string idToSave = !string.IsNullOrEmpty(item.itemName) ? item.itemName : item.name;
        PlacedItemSaveData newItem = new PlacedItemSaveData(idToSave, position, rotation.eulerAngles);
        currentScenePlacedItems.Add(newItem);

        WriteSaveToFile();
        GameDatabaseManager.Instance?.SaveAndSyncToCloud();
    }

    private void WriteSaveToFile()
    {
        string path = GetSaveFilePath();
        MapBuildingSaveWrapper wrapper = new MapBuildingSaveWrapper { items = currentScenePlacedItems };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(path, json);
    }

    public void LoadSceneBuildings()
    {
        string path = GetSaveFilePath();
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        MapBuildingSaveWrapper wrapper = JsonUtility.FromJson<MapBuildingSaveWrapper>(json);

        if (wrapper == null || wrapper.items == null) return;

        currentScenePlacedItems = wrapper.items;

        foreach (var data in currentScenePlacedItems)
        {
            if (itemLookup.TryGetValue(data.itemID, out BuildableItemSO itemSO))
            {
                if (itemSO.prefab != null)
                {
                    Instantiate(itemSO.prefab, data.GetPosition(), data.GetRotation());
                }
            }
        }
    }
}