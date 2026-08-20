using UnityEngine;

public class GameplayCoreManager : MonoBehaviour
{
    public static GameplayCoreManager Instance { get; private set; }

    private void Awake()
    {
        // Nếu chưa có Core nào tồn tại thì giữ lại cái này
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Đảm bảo nó không bị nằm trong object nào khác
            DontDestroyOnLoad(gameObject);
        }
        // Nếu đã có Core khác bay từ map cũ sang, lập tức tự hủy bản thân
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static void EnsureGameplayCoreExists()
    {
        if (Instance == null && GameObject.FindGameObjectWithTag("Player") == null)
        {
            GameObject prefab = Resources.Load<GameObject>("GameplayCorePrefab");
            if (prefab != null)
            {
                GameObject core = Instantiate(prefab);
                core.name = "GameplayCorePrefab";
            }
            else
            {
                Debug.LogWarning("[GameplayCoreManager] Không tìm thấy GameplayCorePrefab trong thư mục Resources!");
            }
        }
    }
}