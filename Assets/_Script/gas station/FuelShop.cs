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

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        FindUI();
        if (interactUI != null) interactUI.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);
    }

    private void FindUI()
    {
        if (interactUI == null || notificationText == null)
        {
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
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<CarFuel>() != null)
        {
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
            if (interactUI != null) interactUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerNearby && playerCollider != null && !playerCollider.enabled && playerCollider.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactUI != null) interactUI.SetActive(false);
        }

        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            TryBuyFuelCan();
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
        }
        else
        {
            ShowNotification("Cốp xe đã đầy!", Color.yellow);
            if (audioSource != null && buyFailSound != null) audioSource.PlayOneShot(buyFailSound);
        }
    }

    private void ShowNotification(string message, Color color)
    {
        if (notificationText == null) return;
        StopAllCoroutines();
        StartCoroutine(FadeOutText(message, color));
    }

    private IEnumerator FadeOutText(string message, Color color)
    {
        notificationText.text = message;
        notificationText.color = color;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        float duration = 1f;
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
    }
}