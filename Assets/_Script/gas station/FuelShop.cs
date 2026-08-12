using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class FuelShop : MonoBehaviour
{
    [Header("=== KẾT NỐI HỆ THỐNG ===")]
    public InventoryGridData trunkGrid;
    public ItemShapeSO fuelCanItem;

    [Header("=== GIAO DIỆN UI TÚI ĐỒ ===")]
    public GameObject itemUIPrefab;

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
        // Luôn luôn cho phép quét lại nếu phát hiện bị mất kết nối (phòng hờ khi đổi Scene)
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
        // Cho phép cả người chơi đi bộ VÀ ngồi trên xe đều có thể mua
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
        // Ẩn chữ nếu đang đi bộ mà bấm leo lên xe (Collider tắt)
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
        // Cứu cánh nếu mất đường dẫn Cốp xe khi đổi map
        if (trunkGrid == null && TrunkMinigameUI.Instance != null)
        {
            trunkGrid = TrunkMinigameUI.Instance.GetGridData();
        }

        if (trunkGrid == null || fuelCanItem == null || itemUIPrefab == null) return;

        if (MoneyManager.Instance != null && !MoneyManager.Instance.CoDuTien(pricePerCan))
        {
            ShowNotification("Không đủ tiền!", Color.red);
            if (audioSource != null && buyFailSound != null) audioSource.PlayOneShot(buyFailSound);
            return;
        }

        bool isPlaced = false;
        int width = trunkGrid.GetGridWidth();
        int height = trunkGrid.GetGridHeight();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (trunkGrid.CanPlaceItem(x, y, fuelCanItem, false))
                {
                    GameObject newItemObj = Instantiate(itemUIPrefab);
                    InventoryItemUI itemUI = newItemObj.GetComponent<InventoryItemUI>();

                    BackpackMinigameUI backpackUI = Object.FindFirstObjectByType<BackpackMinigameUI>(FindObjectsInactive.Include);
                    itemUI.Setup(fuelCanItem, backpackUI, x, y, false);
                    itemUI.currentOwner = InventoryItemUI.GridOwner.Trunk;

                    if (TrunkMinigameUI.Instance != null) TrunkMinigameUI.Instance.PlaceItemDirectlyToGrid(itemUI, x, y, false);

                    isPlaced = true;
                    break;
                }
            }
            if (isPlaced) break;
        }

        if (isPlaced)
        {
            if (MoneyManager.Instance != null) MoneyManager.Instance.TruTien(pricePerCan);
            ShowNotification("Đã mua 1 Can Xăng (-" + pricePerCan + "K)", Color.green);
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