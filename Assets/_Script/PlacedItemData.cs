using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlacedItemData
{
    public string itemID; // Mã định danh món đồ (VD: "Lantern", "Tent", "Chair")
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ;

    public PlacedItemData(string id, Vector3 pos, Vector3 rot)
    {
        itemID = id;
        posX = pos.x; posY = pos.y; posZ = pos.z;
        rotX = rot.x; rotY = rot.y; rotZ = rot.z;
    }

    public Vector3 GetPosition() => new Vector3(posX, posY, posZ);
    public Quaternion GetRotation() => Quaternion.Euler(rotX, rotY, rotZ);
}

[Serializable]
public class MapPlacementSaveData
{
    public List<PlacedItemData> items = new List<PlacedItemData>();
}