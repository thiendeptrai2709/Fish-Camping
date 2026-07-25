using UnityEngine;

[CreateAssetMenu(fileName = "FishDatabase", menuName = "Inventory/Fish Database")]
public class FishDatabaseSO : ScriptableObject
{
    public FishSO[] allFishes;
}