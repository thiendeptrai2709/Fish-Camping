using UnityEngine;

/// <summary>
/// Vùng cảm biến (Trigger Zone) đặt tại NPC_MuaBan (Shop Đồ Câu).
/// Khi người chơi lái xe đến gần Shop ở nhiệm vụ Quest4_2_DriveToShop,
/// script sẽ tự động nhận diện xe đã đến nơi và chuyển sang nhiệm vụ Quest4_3_ExitVehicle (Bấm E để xuống xe).
/// </summary>
public class TutorialShopArrivalTriggerZone : MonoBehaviour
{
    [Header("=== CẤU HÌNH VÙNG CHECK XE ĐẾN SHOP ===")]
    [Tooltip("Tag của xe hoặc người chơi")]
    [SerializeField] private string carTag = "Car";
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Bán kính tự động kiểm tra nếu không dùng Collider vật lý")]
    [SerializeField] private float detectionRadius = 12f;

    [Tooltip("Tự động tắt vùng sau khi đã hoàn thành")]
    [SerializeField] private bool disableAfterTrigger = true;

    [Header("=== GIZMOS HIỂN THỊ TRONG SCENE ===")]
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.6f, 1f, 0.35f);

    private void Awake()
    {
        // Nếu có Collider thì đảm bảo là Trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsCarOrDrivingPlayer(other))
        {
            TriggerArrivalCompletion();
        }
    }

    private void Update()
    {
        // Kiểm tra khoảng cách dự phòng nếu không dùng Trigger Collider
        if (ForcedTutorialManager.Instance == null) return;
        if (ForcedTutorialManager.Instance.currentStage != TutorialStage.Quest4_2_DriveToShop) return;

        VehicleController vehicle = Object.FindFirstObjectByType<VehicleController>(FindObjectsInactive.Include);
        if (vehicle != null)
        {
            float dist = Vector3.Distance(transform.position, vehicle.transform.position);
            if (dist <= detectionRadius)
            {
                TriggerArrivalCompletion();
            }
        }
    }

    private bool IsCarOrDrivingPlayer(Collider other)
    {
        if (other.CompareTag(carTag)) return true;
        if (other.GetComponentInParent<VehicleController>() != null) return true;
        if (other.GetComponentInParent<VehicleEnterExit>() != null) return true;
        if (other.GetComponentInParent<CarFuel>() != null) return true;

        if (other.CompareTag(playerTag) || other.GetComponentInParent<PlayerMovement>() != null)
        {
            // Kiểm tra xem người chơi có đang trong xe không
            VehicleEnterExit vehicleEnterExit = Object.FindFirstObjectByType<VehicleEnterExit>(FindObjectsInactive.Include);
            if (vehicleEnterExit != null && vehicleEnterExit.IsInCar)
            {
                return true;
            }
        }

        return false;
    }

    public void TriggerArrivalCompletion()
    {
        if (ForcedTutorialManager.Instance != null)
        {
            if (ForcedTutorialManager.Instance.currentStage == TutorialStage.Quest4_2_DriveToShop)
            {
                Debug.Log("<color=green>[Tutorial] Xe đã đến Shop Đồ Câu (NPC_MuaBan)! Chuyển sang nhiệm vụ bấm E xuống xe.</color>");
                ForcedTutorialManager.Instance.NotifyDriveToShop();

                if (disableAfterTrigger)
                {
                    enabled = false;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        Gizmos.color = gizmoColor;

        if (col != null)
        {
            if (col is SphereCollider sphere)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
