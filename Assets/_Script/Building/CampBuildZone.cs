using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CampBuildZone : MonoBehaviour
{
    public static CampBuildZone Instance { get; private set; }

    private BoxCollider zoneCollider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        zoneCollider = GetComponent<BoxCollider>();
        zoneCollider.isTrigger = true;
    }

    public bool IsInsideBuildZone(Vector3 worldPosition)
    {
        if (zoneCollider == null) return true;
        return zoneCollider.bounds.Contains(worldPosition);
    }

    private void OnDrawGizmos()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position + col.center, col.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + col.center, col.size);
        }
    }
}