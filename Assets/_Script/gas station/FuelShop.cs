using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class FuelShop : MonoBehaviour
{
    [Header("=== KẾT NỐI HỆ THỐNG ===")]
    public ItemShapeSO fuelCanItem;

    [Header("=== GIÁ TIỀN & ÂM THANH ===")]
    public int pricePerCan = 50;
    public AudioSource audioSource;
    public AudioClip buySuccessSound;
    public AudioClip buyFailSound;

    [Header("=== GIAO DIỆN UI THÔNG BÁO ===")]
    public GameObject interactUI;
    public TextMeshProUGUI notificationText;

    private bool isPlayerNearby = false;
    private Collider playerCollider;
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        FindUI();
        HideAllUI();
    }

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        FindUI();
        DisableLocalization(notificationText);
        HideAllUI();
    }

    private void OnEnable()
    {
        FindUI();
        HideAllUI();
    }

    private void OnDisable()
    {
        HideAllUI();
    }

    public void HideAllUI()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }

        if (interactUI != null) interactUI.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);
    }

    private void DisableLocalization(Component target)
    {
        if (target == null) return;
        var components = target.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp != null && comp.GetType().Name.Contains("LocalizeStringEvent"))
            {
                comp.enabled = false;
            }
        }
    }

    private void FindUI()
    {
        if (interactUI == null || notificationText == null)
        {
            // 1. Tìm trong GameplayCorePrefab (TrunkMinigameUI)
            TrunkMinigameUI trunk = Object.FindFirstObjectByType<TrunkMinigameUI>(FindObjectsInactive.Include);
            if (trunk != null)
            {
                Transform[] allT = trunk.transform.root.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allT)
                {
                    if (interactUI == null && t.name == "Gas") interactUI = t.gameObject;
                    if (notificationText == null && t.name == "dONE") notificationText = t.GetComponent<TextMeshProUGUI>();
                }
            }

            // 2. Nếu chưa thấy, tìm trong toàn bộ Scene (bao gồm cả Rongas)
            if (interactUI == null || notificationText == null)
            {
                var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var t in allTransforms)
                {
                    if (interactUI == null && t.name == "Gas") interactUI = t.gameObject;
                    if (notificationText == null && t.name == "dONE") notificationText = t.GetComponent<TextMeshProUGUI>();
                }
            }
        }
        DisableLocalization(notificationText);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ kích hoạt khi người chơi đi bộ lại gần (không ngồi trên xe)
        if (other.CompareTag("Player"))
        {
            VehicleEnterExit vehicle = Object.FindFirstObjectByType<VehicleEnterExit>(FindObjectsInactive.Include);
            if (vehicle != null && vehicle.IsInCar) return;

            playerCollider = other;
            isPlayerNearby = true;
            FindUI();
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<CarFuel>() != null)
        {
            isPlayerNearby = false;
            playerCollider = null;
            HideAllUI();
        }
    }

    private void Update()
    {
        if (isPlayerNearby)
        {
            VehicleEnterExit vehicle = Object.FindFirstObjectByType<VehicleEnterExit>(FindObjectsInactive.Include);
            bool isInCar = (vehicle != null && vehicle.IsInCar);

            if (isInCar || (playerCollider != null && !playerCollider.enabled))
            {
                isPlayerNearby = false;
                playerCollider = null;
                HideAllUI();
                return;
            }

            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                TryBuyFuelCan();
            }
        }
    }

    private void TryBuyFuelCan()
    {
        if (fuelCanItem == null)
        {
            Debug.LogWarning("[FuelShop] Chưa gán file ScriptableObject can xăng vào fuelCanItem!");
            return;
        }

        if (MoneyManager.Instance != null && !MoneyManager.Instance.CoDuTien(pricePerCan))
        {
            ShowNotification("Không đủ tiền!", Color.red);
            if (audioSource != null && buyFailSound != null) audioSource.PlayOneShot(buyFailSound);
            return;
        }

        // 1. Tìm TrunkMinigameUI (quét cả khi Panel đang bị ẩn/inactive)
        TrunkMinigameUI trunkUI = TrunkMinigameUI.Instance;
        if (trunkUI == null)
        {
            trunkUI = Object.FindFirstObjectByType<TrunkMinigameUI>(FindObjectsInactive.Include);
        }

        if (trunkUI == null)
        {
            Debug.LogError("[FuelShop] Không tìm thấy TrunkMinigameUI trong Scene!");
            return;
        }

        // 2. Thêm đồ trực tiếp vào Cốp
        bool isPlaced = trunkUI.AddItemToTrunk(fuelCanItem);

        if (isPlaced)
        {
            if (MoneyManager.Instance != null) MoneyManager.Instance.TruTien(pricePerCan);
            ShowNotification($"Đã mua 1 Can Xăng (-{pricePerCan}K)", Color.green);
            if (audioSource != null && buySuccessSound != null) audioSource.PlayOneShot(buySuccessSound);

            // Báo hoàn thành bước mua can xăng trong Tutorial
            ForcedTutorialManager.Instance?.NotifyGasCanisterBought();

            // Tự động cập nhật tiến độ nhiệm vụ
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.NotifyFuelRefilled();
            }
        }
        else
        {
            ShowNotification("Cốp xe đã đầy!", Color.yellow);
            if (audioSource != null && buyFailSound != null) audioSource.PlayOneShot(buyFailSound);
        }
    }

    private void ShowNotification(string message, Color color)
    {
        FindUI();
        if (notificationText == null) return;

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        notificationCoroutine = StartCoroutine(FadeOutText(message, color));
    }

    private IEnumerator FadeOutText(string message, Color color)
    {
        notificationText.gameObject.SetActive(true);
        DisableLocalization(notificationText);
        notificationText.text = message;
        notificationText.color = color;

        yield return new WaitForSeconds(1.5f);

        float duration = 0.8f;
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
            Color newColor = notificationText.color;
            newColor.a = alpha;
            notificationText.color = newColor;
            yield return null;
        }

        notificationText.gameObject.SetActive(false);
        notificationCoroutine = null;
    }
}