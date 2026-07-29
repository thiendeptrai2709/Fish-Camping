using UnityEngine;

public class KeepCabinUpright : MonoBehaviour
{
    [Header("Cấu hình Đung Đưa")]
    [SerializeField] private float swingAngle = 3f;    // Độ đung đưa nhẹ
    [SerializeField] private float swingSpeed = 1.5f;  // Tốc độ đung đưa

    private Quaternion initialWorldRotation;
    private float randomOffset;

    void Start()
    {
        // Lưu lại chính xác hướng đứng ban đầu của buồng
        initialWorldRotation = transform.rotation;
        randomOffset = Random.Range(0f, 100f);
    }

    void LateUpdate()
    {
        // Tính nhịp đung đưa tự nhiên
        float swing = Mathf.Sin((Time.time + randomOffset) * swingSpeed) * swingAngle;

        // Giữ nguyên tư thế thẳng đứng ban đầu + lắc nhẹ
        transform.rotation = initialWorldRotation * Quaternion.Euler(0f, 0f, swing);
    }
}