using UnityEngine;
using UnityEngine.InputSystem; // Dùng Input System mới

public class GasStation : MonoBehaviour
{
    [Header("Cấu hình Cây xăng")]
    [SerializeField] private float fillSpeed = 25f;     // Tốc độ bơm xăng (lít/giây)

    [Header("UI Cảnh báo / Tương tác")]
    [SerializeField] private GameObject interactUI;     // Canvas/Text hiện "[F] Đổ xăng"

    [Header("Âm thanh (Audio)")]
    [SerializeField] private AudioSource audioSource;   // Component AudioSource
    [SerializeField] private AudioClip fullFuelSound;   // Tiếng "tinh" báo đầy xăng

    private CarFuel currentCarFuel;                     // Xe đang trong vùng
    private bool isPlayerInZone = false;
    private bool isRefilling = false;                   // Trạng thái đang tự động bơm

    private void Start()
    {
        if (interactUI != null) interactUI.SetActive(false);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CarFuel car = other.GetComponentInParent<CarFuel>();
        if (car != null)
        {
            currentCarFuel = car;
            isPlayerInZone = true;
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CarFuel car = other.GetComponentInParent<CarFuel>();
        if (car != null && car == currentCarFuel)
        {
            // Nếu lái xe chạy ra khỏi vùng thì dừng bơm lập tức
            StopRefill();
            currentCarFuel = null;
            isPlayerInZone = false;
            if (interactUI != null) interactUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Nhấn F 1 lần để bật chế độ tự động bơm xăng
        if (isPlayerInZone && currentCarFuel != null && !isRefilling)
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                // Chỉ bơm nếu xăng chưa đầy
                if (currentCarFuel.currentFuel < currentCarFuel.maxFuel)
                {
                    isRefilling = true;
                }
            }
        }

        // Tự động cộng xăng liên tục khi đang trong trạng thái isRefilling
        if (isRefilling && currentCarFuel != null)
        {
            if (currentCarFuel.currentFuel < currentCarFuel.maxFuel)
            {
                currentCarFuel.AddFuel(fillSpeed * Time.deltaTime);
            }
            else
            {
                // Khi bình xăng đã ĐẦY:
                CompleteRefill();
            }
        }
    }

    private void CompleteRefill()
    {
        isRefilling = false;

        // Phát âm thanh báo đầy xăng (nếu có)
        if (audioSource != null && fullFuelSound != null)
        {
            audioSource.PlayOneShot(fullFuelSound);
        }

        // Tắt UI thông báo vì đã đầy bình
        if (interactUI != null) interactUI.SetActive(false);
    }

    private void StopRefill()
    {
        isRefilling = false;
    }
}