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

        // 1. MỞ GIAO DIỆN BALO TRƯỚC TIÊN
        // (Bắt buộc phải bật trước để đảm bảo Balo kịp Awake() và không bị null)
        if (BackpackController.Instance != null)
        {
            BackpackController.Instance.OpenForCooking(true);
        }

        if (cookingPanel != null)
        {
            cookingPanel.SetActive(true);
        }

        // Hiện chuột, Ẩn tâm ngắm
        if (playerCursor != null) playerCursor.SetCursorState(false);
        if (crosshairUI != null) crosshairUI.SetActive(false);

        // 2. DỌN DẸP ĐỒ CŨ NẾU CÓ
        if (spawnedResultItem != null)
        {
            Destroy(spawnedResultItem.gameObject);
            spawnedResultItem = null;
        }

        // 3. SINH RA MÓN ĂN VÀ TRUYỀN DỮ LIỆU KÉO THẢ
        if (currentRack.State == CookingRack.CookingState.Finished)
        {
            if (startCookButton != null) startCookButton.gameObject.SetActive(false);

            ItemShapeSO finalFood = currentRack.GetCookedFood();

            if (finalFood != null && itemUIPrefab != null && cookingSlots != null && cookingSlots.Length > 0 && BackpackMinigameUI.Instance != null)
            {
                // DỌN SẠCH CÁI ẢNH NGUYÊN LIỆU CŨ TRONG SLOT ĐỂ TRÁNH CHẶN CHUỘT
                cookingSlots[0].ClearSlot();

                GameObject itemObj = Instantiate(itemUIPrefab, cookingSlots[0].transform);
                spawnedResultItem = itemObj.GetComponent<InventoryItemUI>();

                // THÊM 2 DÒNG NÀY ĐỂ KẾT NỐI VỚI HỆ THỐNG KÉO THẢ
                spawnedResultItem.Setup(finalFood, BackpackMinigameUI.Instance, 0, 0, false);
                // Ép nó nhận diện Controller của Balo, nếu không sẽ bị Null khiến nó đéo cho kéo

                spawnedResultItem.SetFromCooking(true);
                spawnedResultItem.UpdateVisualSize();

                // HỌC TẬP HOTBAR: Ép tâm về 0.5 để khớp với Slot_1 của Inspector
                RectTransform rect = itemObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            if (startCookButton != null) startCookButton.gameObject.SetActive(true);
        }
    }

    public void CloseCookingUI()
    {
        currentRack = null;
        if (cookingPanel != null)
        {
            cookingPanel.SetActive(false);
        }

        if (playerCursor != null) playerCursor.SetCursorState(true);
        if (crosshairUI != null) crosshairUI.SetActive(true);

        if (BackpackController.Instance != null)
        {
            BackpackController.Instance.OpenForCooking(false);
        }
    }

    private void OnStartCookClicked()
    {
        if (currentRack != null)
        {
            System.Collections.Generic.List<ItemShapeSO> ingredients = new System.Collections.Generic.List<ItemShapeSO>();
            if (cookingSlots != null)
            {
                foreach (var slot in cookingSlots)
                {
                    if (slot.CurrentItem != null)
                    {
                        ingredients.Add(slot.CurrentItem);
                    }
                }
            }

            bool started = currentRack.TryStartCooking(ingredients);
            if (started)
            {
                foreach (var slot in cookingSlots)
                {
                    slot.ClearSlot();
                }
                CloseCookingUI();
            }
        }
    }

    public void OnFoodCollectedSuccessfully()
    {
        if (currentRack != null)
        {
            currentRack.ClearCookedFood();
        }
        spawnedResultItem = null;
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