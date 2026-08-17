using UnityEngine;

public class WindmillRotator : MonoBehaviour
{
    [Header("Cài Đặt Tốc Độ & Hướng Xoay")]
    [Tooltip("Tốc độ quay (độ/giây). Số âm sẽ quay ngược chiều kim đồng hồ")]
    [SerializeField] private float rotationSpeed = 30f;

    [Tooltip("Trục quay của cánh quạt (Mặc định thường là trục Z)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;

    [Tooltip("Dùng tọa độ Local để không bị ảnh hưởng bởi góc đặt của thân cối xay gió")]
    [SerializeField] private bool useLocalSpace = true;

    void Update()
    {
        float angle = rotationSpeed * Time.deltaTime;

        if (useLocalSpace)
        {
            transform.Rotate(rotationAxis * angle, Space.Self);
        }
        else
        {
            transform.Rotate(rotationAxis * angle, Space.World);
        }
    }
}