using UnityEngine;

public class MapTriggerZone : MonoBehaviour
{
    [SerializeField] private MapUIManager mapUIManager;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string carTag = "Car";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) || other.CompareTag(carTag))
        {
            if (mapUIManager != null)
            {
                mapUIManager.OpenMap();
            }
        }
    }
}