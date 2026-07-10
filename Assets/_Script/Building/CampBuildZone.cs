using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class CampBuildZone : MonoBehaviour
{
    public static CampBuildZone Instance { get; private set; }

    [SerializeField] private GameObject borderPrefab;
    [SerializeField] private float borderThickness = 0.5f;
    [SerializeField] private float textureAspectRatio = 4f;
    [SerializeField] private float cornerGap = 0.5f;
    [SerializeField] private float yOffset = 0f;

    private BoxCollider zoneCollider;
    private GameObject[] borders = new GameObject[4];

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

        if (borderPrefab != null)
        {
            GenerateBorders();
        }

        ToggleZoneVisual(false);
    }

    private void GenerateBorders()
    {
        if (zoneCollider == null || borderPrefab == null) return;

        Vector3 centerPos = transform.position + zoneCollider.center;
        float height = 1f; // Chiều cao của bức tường cảnh báo
        centerPos.y += (height / 2f) + yOffset;

        float width = zoneCollider.size.x;
        float length = zoneCollider.size.z;
        float t = borderThickness;

        float topBottomLength = width - (cornerGap * 2f);
        borders[0] = CreateBorderLine(centerPos + new Vector3(0, 0, (length / 2f) - (t / 2f)), new Vector3(topBottomLength, height, t), Quaternion.Euler(0f, 0f, 0f));
        borders[1] = CreateBorderLine(centerPos + new Vector3(0, 0, -(length / 2f) + (t / 2f)), new Vector3(topBottomLength, height, t), Quaternion.Euler(0f, 0f, 0f));

        float sideLength = length - (cornerGap * 2f);
        borders[2] = CreateBorderLine(centerPos + new Vector3((width / 2f) - (t / 2f), 0, 0), new Vector3(sideLength, height, t), Quaternion.Euler(0f, 90f, 0f));
        borders[3] = CreateBorderLine(centerPos + new Vector3(-(width / 2f) + (t / 2f), 0, 0), new Vector3(sideLength, height, t), Quaternion.Euler(0f, 90f, 0f));
    }
    private GameObject CreateBorderLine(Vector3 pos, Vector3 scale, Quaternion rotation)
    {
        GameObject line = Instantiate(borderPrefab, pos, rotation, transform);
        line.transform.localScale = scale;

        MeshRenderer renderer = line.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            // Do trục X luôn là chiều dài của mặt, tính toán Tiling trở nên cực kỳ ổn định
            float tileX = scale.x / textureAspectRatio;
            renderer.material.mainTextureScale = new Vector2(tileX, 1f);
        }

        return line;
    }
    public void ToggleZoneVisual(bool isVisible)
    {
        for (int i = 0; i < borders.Length; i++)
        {
            if (borders[i] != null)
            {
                borders[i].SetActive(isVisible);
            }
        }
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