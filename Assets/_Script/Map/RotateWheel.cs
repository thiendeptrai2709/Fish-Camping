using UnityEngine;

public class RotateWheel : MonoBehaviour
{
    [Header("Tốc độ quay")]
    public float rotateSpeed = 15f;

    void Update()
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime, Space.Self);
    }
}