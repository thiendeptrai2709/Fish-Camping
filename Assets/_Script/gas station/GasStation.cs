using UnityEngine;
using UnityEngine.InputSystem;

public class GasStation : MonoBehaviour
{
    [Header("Cấu hình Cây xăng")]
    [SerializeField] private float fillSpeed = 25f;

    [Header("UI Cảnh báo / Tương tác")]
    [SerializeField] private GameObject interactUI;

    [Header("Âm thanh (Audio)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fullFuelSound;

    private CarFuel currentCarFuel;
    private bool isPlayerInZone = false;
    private bool isRefilling = false;

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        FindUI();
        if (interactUI != null) interactUI.SetActive(false);
    }

    // --- HÀM TÌM UI CHỐNG MẤT TRÍ NHỚ KHI ĐỔI MAP ---
    private void FindUI()
    {
        if (interactUI != null) return;
        TrunkMinigameUI trunk = Object.FindFirstObjectByType<TrunkMinigameUI>(FindObjectsInactive.Include);
        if (trunk != null)
        {
            Transform[] allT = trunk.transform.root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allT)
            {
                if (t.name == "GasPromptUI") interactUI = t.gameObject;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CarFuel car = other.GetComponentInParent<CarFuel>();
        if (car != null)
        {
            currentCarFuel = car;
            isPlayerInZone = true;
            FindUI(); // Quét lại 1 lần nữa lúc chạm cho an toàn 100%
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CarFuel car = other.GetComponentInParent<CarFuel>();
        if (car != null && car == currentCarFuel)
        {
            StopRefill();
            currentCarFuel = null;
            isPlayerInZone = false;
            if (interactUI != null) interactUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInZone && currentCarFuel != null && !isRefilling)
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (currentCarFuel.currentFuel < currentCarFuel.maxFuel)
                {
                    isRefilling = true;
                }
            }
        }

        if (isRefilling && currentCarFuel != null)
        {
            if (currentCarFuel.currentFuel < currentCarFuel.maxFuel)
            {
                currentCarFuel.AddFuel(fillSpeed * Time.deltaTime);
            }
            else
            {
                CompleteRefill();
            }
        }
    }

    private void CompleteRefill()
    {
        isRefilling = false;
        if (audioSource != null && fullFuelSound != null) audioSource.PlayOneShot(fullFuelSound);
        if (interactUI != null) interactUI.SetActive(false);
    }

    private void StopRefill()
    {
        isRefilling = false;
    }
}