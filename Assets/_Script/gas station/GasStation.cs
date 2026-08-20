using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class GasStation : MonoBehaviour
{
    [Header("Cau hinh Cay xang")]
    [SerializeField] private float fillSpeed = 25f;
    [SerializeField] private float pricePerFuelUnit = 5f;
    [SerializeField] private float playerInteractionDistance = 6f;

    [Header("UI Canh bao / Tuong tac")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private float notifyDuration = 2f;

    [Header("Am thanh (Audio)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fullFuelSound;

    private CarFuel currentCarFuel;
    private bool isCarInZone = false;
    private bool isRefilling = false;
    private bool isRefillFinishedForThisEntry = false;
    private float unbilledFuelCost = 0f;
    private int totalCostThisSession = 0;

    private Transform playerTransform;
    private TextMeshProUGUI autoNotifyText;
    private Coroutine notifyCoroutine;

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        FindUI();
        CreateAutoNotificationUI();
        FindPlayer();
        UpdateUIState();
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void CreateAutoNotificationUI()
    {
        Canvas targetCanvas = Object.FindFirstObjectByType<Canvas>();
        if (targetCanvas == null)
        {
            GameObject canvasObj = new GameObject("AutoGasCanvas");
            targetCanvas = canvasObj.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        GameObject textObj = new GameObject("AutoGasNotifyText");
        textObj.transform.SetParent(targetCanvas.transform, false);

        autoNotifyText = textObj.AddComponent<TextMeshProUGUI>();
        autoNotifyText.alignment = TextAlignmentOptions.Center;
        autoNotifyText.fontSize = 28f;
        autoNotifyText.fontStyle = FontStyles.Bold;
        autoNotifyText.color = new Color(1f, 0.85f, 0.2f, 0f);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 120f);
        rect.sizeDelta = new Vector2(600f, 80f);

        textObj.SetActive(false);
    }

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
        FindUI();
        FindPlayer();

        CarFuel car = other.GetComponentInParent<CarFuel>();
        if (car != null)
        {
            currentCarFuel = car;
            isCarInZone = true;
            isRefillFinishedForThisEntry = false;

            // Khi xe đi vào cây xăng, lập tức chuyển sang nhiệm vụ đổ xăng cho xe
            ForcedTutorialManager.Instance?.NotifyGasStationFound();
        }

        UpdateUIState();
    }

    private void OnTriggerExit(Collider other)
    {
        CarFuel car = other.GetComponentInParent<CarFuel>();
        if (car != null && car == currentCarFuel)
        {
            StopRefill();
            isCarInZone = false;
            currentCarFuel = null;
            isRefillFinishedForThisEntry = false;
        }

        UpdateUIState();
    }

    private bool IsEligibleToRefill()
    {
        if (!isCarInZone || currentCarFuel == null || isRefillFinishedForThisEntry) return false;

        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            return true;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= playerInteractionDistance;
    }

    private void UpdateUIState()
    {
        if (interactUI != null)
        {
            interactUI.SetActive(IsEligibleToRefill() && !isRefilling);
        }
    }

    private void Update()
    {
        if (!IsEligibleToRefill())
        {
            if (isRefilling) StopRefill();
            UpdateUIState();
            return;
        }

        UpdateUIState();

        if (!isRefilling)
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (currentCarFuel.currentFuel < currentCarFuel.maxFuel)
                {
                    if (MoneyManager.Instance != null && !MoneyManager.Instance.CoDuTien(1))
                    {
                        ShowMessage("Khong du tien de do xang!");
                        return;
                    }
                    totalCostThisSession = 0;
                    isRefilling = true;
                    UpdateUIState();

                    // Lập tức chuyển sang nhiệm vụ tiếp theo khi người chơi bấm F đổ xăng
                    ForcedTutorialManager.Instance?.NotifyRefueled();
                }
                else
                {
                    isRefillFinishedForThisEntry = true;
                    UpdateUIState();
                    ShowMessage("Binh xang da day san!");

                    // Bình xăng đã đầy sẵn, bấm F cũng lập tức chuyển sang nhiệm vụ tiếp theo
                    ForcedTutorialManager.Instance?.NotifyRefueled();
                }
            }
        }

        if (isRefilling && currentCarFuel != null)
        {
            if (currentCarFuel.currentFuel < currentCarFuel.maxFuel)
            {
                float fuelToPump = fillSpeed * Time.deltaTime;
                float remainingFuelNeeded = currentCarFuel.maxFuel - currentCarFuel.currentFuel;
                fuelToPump = Mathf.Min(fuelToPump, remainingFuelNeeded);

                float cost = fuelToPump * pricePerFuelUnit;
                unbilledFuelCost += cost;

                if (unbilledFuelCost >= 1f)
                {
                    int amountToDeduct = Mathf.FloorToInt(unbilledFuelCost);
                    if (MoneyManager.Instance != null)
                    {
                        if (MoneyManager.Instance.CoDuTien(amountToDeduct))
                        {
                            MoneyManager.Instance.TruTien(amountToDeduct);
                            totalCostThisSession += amountToDeduct;
                            unbilledFuelCost -= amountToDeduct;
                        }
                        else
                        {
                            isRefillFinishedForThisEntry = true;
                            StopRefill();
                            ShowMessage($"Het tien! Da do het: {totalCostThisSession} Vang");
                            return;
                        }
                    }
                }

                currentCarFuel.AddFuel(fuelToPump);
            }
            else
            {
                CompleteRefill();
            }
        }
    }

    private void CompleteRefill()
    {
        DeductRemainingCost();
        isRefilling = false;
        isRefillFinishedForThisEntry = true;

        if (audioSource != null && fullFuelSound != null) audioSource.PlayOneShot(fullFuelSound);

        UpdateUIState();
        ShowMessage($"Day binh! Da do het: {totalCostThisSession} Vang");
    }

    private void StopRefill()
    {
        DeductRemainingCost();
        if (isRefilling && totalCostThisSession > 0)
        {
            ShowMessage($"Da dung! Da do het: {totalCostThisSession} Vang");
        }
        isRefilling = false;
        UpdateUIState();
    }

    private void DeductRemainingCost()
    {
        if (unbilledFuelCost >= 1f && MoneyManager.Instance != null)
        {
            int remaining = Mathf.FloorToInt(unbilledFuelCost);
            if (MoneyManager.Instance.CoDuTien(remaining))
            {
                MoneyManager.Instance.TruTien(remaining);
                totalCostThisSession += remaining;
            }
            unbilledFuelCost = 0f;
        }
    }

    private void ShowMessage(string message)
    {
        if (autoNotifyText == null) return;
        if (notifyCoroutine != null) StopCoroutine(notifyCoroutine);
        notifyCoroutine = StartCoroutine(FadeInOutRoutine(message));
    }

    private IEnumerator FadeInOutRoutine(string text)
    {
        autoNotifyText.text = text;
        autoNotifyText.gameObject.SetActive(true);

        Color c = autoNotifyText.color;

        float t = 0f;
        Vector3 initialScale = Vector3.one * 0.8f;
        Vector3 targetScale = Vector3.one;

        while (t < 0.25f)
        {
            t += Time.deltaTime;
            float progress = t / 0.25f;
            c.a = Mathf.Lerp(0f, 1f, progress);
            autoNotifyText.color = c;
            autoNotifyText.transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
            yield return null;
        }

        c.a = 1f;
        autoNotifyText.color = c;
        autoNotifyText.transform.localScale = targetScale;

        yield return new WaitForSeconds(notifyDuration);

        t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            float progress = t / 0.4f;
            c.a = Mathf.Lerp(1f, 0f, progress);
            autoNotifyText.color = c;
            yield return null;
        }

        c.a = 0f;
        autoNotifyText.color = c;
        autoNotifyText.gameObject.SetActive(false);
    }
}