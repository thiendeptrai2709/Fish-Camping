using UnityEngine;

/// <summary>
/// Vùng cảm biến (Trigger Zone) gắn vào xe tải hoặc khu vực quanh xe tải.
/// Khi người chơi bước vào vùng này ở Nhiệm vụ 2 (Quest1_2_FindOldTruck),
/// sẽ tự động hoàn thành nhiệm vụ và chuyển sang bước mở cốp xe.
/// </summary>
public class TutorialTruckTriggerZone : MonoBehaviour
{
    [Header("=== CẤU HÌNH VÙNG CẢM BIẾN XE TẢI ===")]
    [Tooltip("Tag của nhân vật người chơi")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Tự động tắt vùng cảm biến sau khi kích hoạt thành công")]
    [SerializeField] private bool disableAfterTrigger = true;

    [Tooltip("Object hiệu ứng hoặc vòng sáng chỉ dẫn (nếu có, sẽ tắt khi hoàn thành)")]
    [SerializeField] private GameObject visualIndicator;

    [Header("=== GIZMOS HIỂN THỊ TRONG SCENE ===")]
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.4f, 0.35f);

    private void Awake()
    {
        // Đảm bảo Collider trên GameObject này là Trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là Player không
        if (IsPlayer(other))
        {
            TriggerTutorialCompletion();
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag(playerTag)) return true;
        if (other.GetComponentInParent<PlayerMovement>() != null) return true;
        if (other.GetComponent<CharacterController>() != null && other.name.ToLower().Contains("player")) return true;
        return false;
    }

    public void TriggerTutorialCompletion()
    {
        if (ForcedTutorialManager.Instance != null)
        {
            if (ForcedTutorialManager.Instance.currentStage == TutorialStage.Quest1_2_FindOldTruck)
            {
                Debug.Log("<color=green>[Tutorial] Người chơi đã tìm thấy chiếc xe tải cũ! Hoàn thành Nhiệm vụ 2.</color>");
                ForcedTutorialManager.Instance.NotifyFindOldTruck();

                if (visualIndicator != null)
                {
                    visualIndicator.SetActive(false);
                }

                if (disableAfterTrigger)
                {
                    enabled = false;
                    Collider col = GetComponent<Collider>();
                    if (col != null && GetComponent<IInteractable>() == null)
                    {
                        col.enabled = false;
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = gizmoColor;

        if (col is SphereCollider sphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
