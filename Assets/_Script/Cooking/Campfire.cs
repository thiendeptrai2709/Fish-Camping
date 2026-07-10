using UnityEngine;

public class Campfire : MonoBehaviour
{
    [SerializeField] private GameObject fireVFX;

    private void Start()
    {
        // Đảm bảo lửa luôn tắt khi vừa load game hoặc vừa xây xong
        SetFireActive(false);
    }

    public void SetFireActive(bool isActive)
    {
        if (fireVFX != null)
        {
            fireVFX.SetActive(isActive);
        }
    }
}