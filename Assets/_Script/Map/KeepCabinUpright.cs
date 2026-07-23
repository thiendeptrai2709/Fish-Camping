using UnityEngine;

public class KeepCabinUpright : MonoBehaviour
{
    [Header("Cấu hình Đung Đưa (Fake Vật Lý)")]
    [SerializeField] private float swingAngle = 5f;    // Độ đung đưa (độ)
    [SerializeField] private float swingSpeed = 2f;    // Tốc độ lắc

    private Quaternion initialWorldRotation;
    private float randomOffset;

    void Start()
    {
        // 1. Lưu lại chính xác góc quay ban đầu trong World Space của từng buồng 
        initialWorldRotation = transform.rotation;

        // 2. Tạo độ trễ ngẫu nhiên cho hiệu ứng đung đưa
        randomOffset = Random.Range(0f, 100f);
    }

    void LateUpdate()
    {
        // 3. Tính độ lắc đung đưa nhẹ
        float swing = Mathf.Sin((Time.time + randomOffset) * swingSpeed) * swingAngle;

        // 4. Giữ nguyên góc World chuẩn ban đầu + cộng thêm độ lắc swing
        transform.rotation = initialWorldRotation * Quaternion.Euler(0f, 0f, swing);
    }
}