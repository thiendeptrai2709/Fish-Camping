using UnityEngine;

public class NavigationArrow : MonoBehaviour
{
    public static NavigationArrow Instance { get; private set; }

    [Header("Cài đặt Mũi tên")]
    public float heightOffset = 3f; // Độ cao so với mặt đất/nóc xe
    public float rotationSpeed = 10f; // Tốc độ xoay mượt của mũi tên
    public float stopDistance = 5f; // Khoảng cách tự tắt mũi tên khi đến nơi
    public GameObject arrowVisual; // Khối 3D hình mũi tên

    private Transform playerTransform;
    private bool isActive = false;
    private Vector3 targetPosition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (arrowVisual != null) arrowVisual.SetActive(false);
    }

    public void StartNavigation(Vector3 targetPos)
    {
        targetPosition = targetPos;
        isActive = true;

        if (arrowVisual != null) arrowVisual.SetActive(true);
    }

    public void StopNavigation()
    {
        isActive = false;

        if (arrowVisual != null) arrowVisual.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        // Tự động tìm người chơi (chỉ tìm 1 lần hoặc khi người chơi bị mất/đổi map)
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else return;
        }

        // 1. Cập nhật vị trí mũi tên bay lơ lửng trên đầu
        transform.position = playerTransform.position + Vector3.up * heightOffset;

        // 2. Xoay mũi tên về phía mục tiêu (bỏ qua độ cao Y để mũi tên luôn song song mặt đất)
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // 3. Tự động tắt nếu đã đến đích
        float distanceToTarget = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(targetPosition.x, 0, targetPosition.z)
        );

        if (distanceToTarget <= stopDistance)
        {
            StopNavigation();
        }
    }
}