using UnityEngine;
using UnityEngine.UI;

public class CookingUIManager : MonoBehaviour
{
    public static CookingUIManager Instance { get; private set; }

    [SerializeField] private GameObject cookingPanel;
    [SerializeField] private Button startCookButton;
    [SerializeField] private Button closeButton;

    [Header("Result Setup")]
    [SerializeField] private GameObject itemUIPrefab;

    [Header("Player References")]
    [SerializeField] private PlayerCursor playerCursor;
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private MonoBehaviour freeLookCamera;

    private CookingRack currentRack;
    [SerializeField] private CookingSlotUI[] cookingSlots;
    private InventoryItemUI spawnedResultItem;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (cookingPanel != null) cookingPanel.SetActive(false);

        if (startCookButton != null)
        {
            startCookButton.onClick.AddListener(OnStartCookClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCookingUI);
        }
    }
    public bool IsOpen()
    {
        return cookingPanel != null && cookingPanel.activeSelf;
    }
    public void OpenCookingUI(CookingRack rack)
    {
        currentRack = rack;

        if (BackpackController.Instance != null)
        {
            BackpackController.Instance.OpenForCooking(true);
        }

        if (cookingPanel != null)
        {
            cookingPanel.SetActive(true);
        }

        // Tự động tìm lại PlayerCursor nếu bị mất do đổi scene
        if (playerCursor == null)
        {
            playerCursor = PlayerCursor.Instance ?? Object.FindFirstObjectByType<PlayerCursor>();
        }

        // Hiện chuột, Ẩn tâm ngắm
        if (playerCursor != null) playerCursor.SetCursorState(false);

        if (crosshairUI == null)
        {
            var pInteraction = Object.FindFirstObjectByType<PlayerInteraction>();
            if (pInteraction != null) crosshairUI = pInteraction.gameObject;
        }
        if (crosshairUI != null) crosshairUI.SetActive(false);

        if (freeLookCamera != null)
        {
            MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
            if (cm3Input != null) cm3Input.enabled = false;

            MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
            if (cm2Input != null) cm2Input.enabled = false;
        }

        // 2. DỌN DẸP ĐỒ CŨ NẾU CÓ
        if (spawnedResultItem != null)
        {
            Destroy(spawnedResultItem.gameObject);
            spawnedResultItem = null;
        }

        if (cookingSlots != null)
        {
            foreach (var slot in cookingSlots)
            {
                if (slot != null) slot.ClearSlot();
            }
        }

        // 3. SINH RA MÓN ĂN VÀ TRUYỀN DỮ LIỆU KÉO THẢ
        if (currentRack != null && currentRack.State == CookingRack.CookingState.Finished)
        {
            if (startCookButton != null) startCookButton.gameObject.SetActive(false);

            ItemShapeSO finalFood = currentRack.GetCookedFood();

            if (finalFood != null && cookingSlots != null && cookingSlots.Length > 0 && BackpackMinigameUI.Instance != null)
            {
                // DỌN SẠCH CÁI ẢNH NGUYÊN LIỆU CŨ TRONG SLOT ĐỂ TRÁNH CHẶN CHUỘT
                cookingSlots[0].ClearSlot();

                GameObject prefabToUse = itemUIPrefab;
                if (prefabToUse == null)
                {
                    // Fallback prefab nếu bị rỗng do đổi scene
                    var anyItemUI = Object.FindFirstObjectByType<InventoryItemUI>(FindObjectsInactive.Include);
                    if (anyItemUI != null) prefabToUse = anyItemUI.gameObject;
                }

                if (prefabToUse != null)
                {
                    GameObject itemObj = Instantiate(prefabToUse, cookingSlots[0].transform);
                    spawnedResultItem = itemObj.GetComponent<InventoryItemUI>();

                    // KẾT NỐI VỚI HỆ THỐNG KÉO THẢ CỦA BALO
                    spawnedResultItem.Setup(finalFood, BackpackMinigameUI.Instance, 0, 0, false);
                    spawnedResultItem.SetFromCooking(true);
                    spawnedResultItem.UpdateVisualSize();

                    RectTransform rect = itemObj.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }
        else
        {
            UpdateCookButtonState();
        }
    }

    public void UpdateCookButtonState()
    {
        if (currentRack == null)
        {
            currentRack = Object.FindFirstObjectByType<CookingRack>();
        }

        if (startCookButton == null) return;

        if (currentRack != null && (currentRack.State == CookingRack.CookingState.Finished || currentRack.State == CookingRack.CookingState.Cooking))
        {
            startCookButton.gameObject.SetActive(false);
            return;
        }

        // Luôn hiển thị và kích hoạt nút nấu ăn khi ở trạng thái Idle
        startCookButton.gameObject.SetActive(true);
        startCookButton.interactable = true;
    }

    public void CloseCookingUI()
    {
        // Hoàn trả các nguyên liệu thô chưa nấu trong slot về lại Balo của người chơi khi ở trạng thái Idle
        if (currentRack != null && currentRack.State == CookingRack.CookingState.Idle && cookingSlots != null)
        {
            foreach (var slot in cookingSlots)
            {
                if (slot != null && slot.CurrentItem != null)
                {
                    ItemShapeSO itemToReturn = slot.CurrentItem;
                    slot.ClearSlot(); // Xóa slot ngay lập tức trước khi Add để tránh lặp lại

                    if (BackpackMinigameUI.Instance != null)
                    {
                        BackpackMinigameUI.Instance.TryAutoAddItem(itemToReturn);
                        BackpackMinigameUI.Instance.SaveBackpack();
                    }
                }
            }
        }

        if (spawnedResultItem != null)
        {
            // Chỉ Destroy nếu nó vẫn còn là con của ô nấu (chưa được đưa vào Balo)
            if (cookingSlots != null && cookingSlots.Length > 0 && spawnedResultItem.transform.IsChildOf(cookingSlots[0].transform))
            {
                Destroy(spawnedResultItem.gameObject);
            }
            spawnedResultItem = null;
        }

        currentRack = null;
        if (cookingPanel != null)
        {
            cookingPanel.SetActive(false);
        }

        if (playerCursor == null)
        {
            playerCursor = PlayerCursor.Instance ?? Object.FindFirstObjectByType<PlayerCursor>();
        }

        if (playerCursor != null) playerCursor.SetCursorState(true);
        if (crosshairUI != null) crosshairUI.SetActive(true);

        if (freeLookCamera != null)
        {
            MonoBehaviour cm3Input = freeLookCamera.GetComponent("CinemachineInputAxisController") as MonoBehaviour;
            if (cm3Input != null) cm3Input.enabled = true;

            MonoBehaviour cm2Input = freeLookCamera.GetComponent("CinemachineInputProvider") as MonoBehaviour;
            if (cm2Input != null) cm2Input.enabled = true;
        }

        if (BackpackController.Instance != null)
        {
            BackpackController.Instance.OpenForCooking(false);
        }
    }

    private void OnStartCookClicked()
    {
        if (currentRack == null)
        {
            currentRack = Object.FindFirstObjectByType<CookingRack>();
        }

        if (currentRack == null)
        {
            Debug.LogWarning("<color=yellow>[Cooking UI] Không tìm thấy giá treo nấu ăn!</color>");
            return;
        }

        System.Collections.Generic.List<ItemShapeSO> ingredients = new System.Collections.Generic.List<ItemShapeSO>();
        if (cookingSlots != null)
        {
            foreach (var slot in cookingSlots)
            {
                if (slot != null && slot.CurrentItem != null)
                {
                    ingredients.Add(slot.CurrentItem);
                }
            }
        }

        if (ingredients.Count == 0)
        {
            Debug.Log("<color=yellow>[Cooking UI] Hãy kéo cá từ Balo vào ô nấu trước khi bấm!</color>");
            return;
        }

        bool started = currentRack.TryStartCooking(ingredients);
        if (started)
        {
            ForcedTutorialManager.Instance?.NotifyCookFish();
            if (cookingSlots != null)
            {
                foreach (var slot in cookingSlots)
                {
                    if (slot != null) slot.ClearSlot();
                }
            }
            CloseCookingUI();
        }
        else
        {
            Debug.LogWarning("<color=red>[Cooking UI] Không thể nấu món này, hãy kiểm tra lửa trại và nguyên liệu!</color>");
        }
    }

    public void OnFoodCollectedSuccessfully()
    {
        if (currentRack != null)
        {
            currentRack.ClearCookedFood();
        }
        else
        {
            CookingRack[] allRacks = Object.FindObjectsByType<CookingRack>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var r in allRacks)
            {
                if (r != null && r.State == CookingRack.CookingState.Finished)
                {
                    r.ClearCookedFood();
                }
            }
        }

        // Không Destroy spawnedResultItem vì item này đã được chuyển làm con của Balo
        spawnedResultItem = null;

        if (cookingSlots != null)
        {
            foreach (var slot in cookingSlots)
            {
                if (slot != null) slot.ClearSlot();
            }
        }

        if (BackpackMinigameUI.Instance != null)
        {
            BackpackMinigameUI.Instance.SaveBackpack();
        }

        CloseCookingUI();
    }

    public void ReturnFoodToSlot(InventoryItemUI itemUI)
    {
        if (cookingSlots != null && cookingSlots.Length > 0)
        {
            itemUI.transform.SetParent(cookingSlots[0].transform);
            RectTransform rect = itemUI.GetComponent<RectTransform>();

            // Bay ngược về giữ đúng góc (0,1)
            rect.anchoredPosition = Vector2.zero;
        }
    }
    public GameObject GetItemPrefab() => itemUIPrefab;
}