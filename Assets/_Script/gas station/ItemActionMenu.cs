using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemActionMenu : MonoBehaviour
{
    private static ItemActionMenu _instance;
    public static ItemActionMenu Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<ItemActionMenu>(FindObjectsInactive.Include);
                if (_instance == null)
                {
                    Canvas targetCanvas = null;
                    if (BackpackMinigameUI.Instance != null)
                    {
                        targetCanvas = BackpackMinigameUI.Instance.GetComponentInParent<Canvas>();
                    }
                    if (targetCanvas == null)
                    {
                        targetCanvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
                    }

                    if (targetCanvas != null)
                    {
                        GameObject menuObj = new GameObject("ItemActionMenu");
                        menuObj.transform.SetParent(targetCanvas.transform, false);
                        _instance = menuObj.AddComponent<ItemActionMenu>();
                    }
                }
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    [Header("=== GIAO DIỆN MENU ===")]
    public GameObject menuPanel;
    public Button btnUse;
    public Button btnClose;
    public TextMeshProUGUI btnUseText;
    public TextMeshProUGUI btnCloseText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI notificationText;

    private InventoryItemUI currentItem;
    private bool listenersAdded = false;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        EnsureUIComponents();
        SetupListeners();
    }

    private void EnsureUIComponents()
    {
        if (menuPanel != null && btnUse != null && btnClose != null) return;

        Canvas targetCanvas = GetComponentInParent<Canvas>();
        if (targetCanvas == null)
        {
            if (BackpackMinigameUI.Instance != null)
            {
                targetCanvas = BackpackMinigameUI.Instance.GetComponentInParent<Canvas>();
            }
            if (targetCanvas == null)
            {
                targetCanvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            }
            if (targetCanvas != null)
            {
                transform.SetParent(targetCanvas.transform, false);
            }
        }

        if (menuPanel == null)
        {
            menuPanel = new GameObject("ItemActionMenuPanel");
            menuPanel.transform.SetParent(transform, false);

            Image bg = menuPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.12f, 0.16f, 0.95f);

            RectTransform rt = menuPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(280f, 180f);
            rt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = menuPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // 0. Tiêu đề
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(menuPanel.transform, false);
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Tác Vụ Vật Phẩm";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.85f, 0.4f, 1f);
            LayoutElement leTitle = titleObj.AddComponent<LayoutElement>();
            leTitle.minHeight = 30f;
            leTitle.preferredHeight = 30f;

            // 1. Nút Sử dụng / Ăn vật phẩm
            GameObject btnUseObj = new GameObject("BtnUse");
            btnUseObj.transform.SetParent(menuPanel.transform, false);
            Image btnUseImg = btnUseObj.AddComponent<Image>();
            btnUseImg.color = new Color(0.18f, 0.68f, 0.38f, 1f);
            btnUse = btnUseObj.AddComponent<Button>();
            LayoutElement leUse = btnUseObj.AddComponent<LayoutElement>();
            leUse.minHeight = 44f;
            leUse.preferredHeight = 44f;

            GameObject useTxtObj = new GameObject("Text");
            useTxtObj.transform.SetParent(btnUseObj.transform, false);
            btnUseText = useTxtObj.AddComponent<TextMeshProUGUI>();
            btnUseText.text = "Ăn Vật Phẩm";
            btnUseText.fontSize = 18;
            btnUseText.fontStyle = FontStyles.Bold;
            btnUseText.alignment = TextAlignmentOptions.Center;
            btnUseText.color = Color.white;
            RectTransform useRt = useTxtObj.GetComponent<RectTransform>();
            useRt.anchorMin = Vector2.zero;
            useRt.anchorMax = Vector2.one;
            useRt.sizeDelta = Vector2.zero;

            // 2. Nút Đóng
            GameObject btnCloseObj = new GameObject("BtnClose");
            btnCloseObj.transform.SetParent(menuPanel.transform, false);
            Image btnCloseImg = btnCloseObj.AddComponent<Image>();
            btnCloseImg.color = new Color(0.7f, 0.25f, 0.25f, 1f);
            btnClose = btnCloseObj.AddComponent<Button>();
            LayoutElement leClose = btnCloseObj.AddComponent<LayoutElement>();
            leClose.minHeight = 40f;
            leClose.preferredHeight = 40f;

            GameObject closeTxtObj = new GameObject("Text");
            closeTxtObj.transform.SetParent(btnCloseObj.transform, false);
            btnCloseText = closeTxtObj.AddComponent<TextMeshProUGUI>();
            btnCloseText.text = "Đóng";
            btnCloseText.fontSize = 17;
            btnCloseText.fontStyle = FontStyles.Bold;
            btnCloseText.alignment = TextAlignmentOptions.Center;
            btnCloseText.color = Color.white;
            RectTransform closeRt = closeTxtObj.GetComponent<RectTransform>();
            closeRt.anchorMin = Vector2.zero;
            closeRt.anchorMax = Vector2.one;
            closeRt.sizeDelta = Vector2.zero;
        }
    }

    private void SetupListeners()
    {
        if (listenersAdded) return;
        listenersAdded = true;

        if (btnUse != null)
        {
            btnUse.onClick.RemoveAllListeners();
            btnUse.onClick.AddListener(OnUseClicked);
        }
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(CloseMenu);
        }
    }

    private void SetButtonLabel(Button btn, string text)
    {
        if (btn == null) return;

        // Tắt tất cả LocalizeStringEvent để tránh bị Localization tự động ghi đè về "Đổ xăng"
        var locEvents = btn.GetComponentsInChildren<UnityEngine.Localization.Components.LocalizeStringEvent>(true);
        foreach (var loc in locEvents)
        {
            loc.enabled = false;
        }

        var tmps = btn.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmps)
        {
            tmp.text = text;
        }

        var texts = btn.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        foreach (var t in texts)
        {
            t.text = text;
        }
    }

    public void ShowMenu(InventoryItemUI item)
    {
        if (item == null || item.GetItemShape() == null) return;

        EnsureUIComponents();
        SetupListeners();
        currentItem = item;
        bool hasAction = false;

        ItemShapeSO shape = item.GetItemShape();
        string id = shape.itemID != null ? shape.itemID.ToLower() : "";
        string assetName = shape.name != null ? shape.name.ToLower() : "";
        string itemName = shape.itemName != null ? shape.itemName.ToLower() : "";

        if (titleText != null)
        {
            titleText.text = shape.itemName ?? "Vật Phẩm";
        }

        // 1. Kiểm tra Can Xăng
        bool isFuel = (id == "fuel_can_01" || id.Contains("fuel") || assetName.Contains("fuel") || itemName.Contains("xăng"));

        // 2. Kiểm tra Cá Sống (FishSO) -> CHƯA THỂ ĂN, PHẢI NƯỚNG CHÍN
        bool isRawFish = (shape is FishSO);

        // 3. Kiểm tra Món Ăn Đã Nấu Chín (FoodSO hoặc các món chế biến)
        bool isCookedFood = (shape is FoodSO) || 
                            (!isRawFish && (id.Contains("cooked") || id.Contains("food") || assetName.Contains("food") || 
                                           itemName.Contains("nướng") || itemName.Contains("thức ăn") || itemName.Contains("thịt nướng") || itemName.Contains("bánh")));

        if (isFuel)
        {
            SetButtonLabel(btnUse, "Đổ Xăng");
            if (btnUse != null) btnUse.gameObject.SetActive(true);
            hasAction = true;
        }
        else if (isCookedFood)
        {
            SetButtonLabel(btnUse, "Ăn Món Này");
            if (btnUse != null) btnUse.gameObject.SetActive(true);
            hasAction = true;
        }
        else if (isRawFish)
        {
            // Cá sống: Không có nút ăn trực tiếp
            if (btnUse != null) btnUse.gameObject.SetActive(false);
            ShowNotif("Cá sống chưa thể ăn! Hãy đặt lên Vỉ Nướng để nướng chín trước.");
            hasAction = true;
        }
        else
        {
            if (btnUse != null) btnUse.gameObject.SetActive(false);
        }

        SetButtonLabel(btnClose, "Đóng");
        if (btnClose != null)
        {
            btnClose.gameObject.SetActive(true);
        }

        // Hiện bảng nếu item có chức năng
        if (hasAction)
        {
            gameObject.SetActive(true);
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);

                RectTransform rect = menuPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                }
            }

            // Đưa lên lớp trên cùng để không bị che khuất
            transform.SetAsLastSibling();
        }
        else
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        gameObject.SetActive(false);
        currentItem = null;
    }

    private void OnUseClicked()
    {
        if (currentItem == null || currentItem.GetItemShape() == null) return;

        ItemShapeSO shape = currentItem.GetItemShape();
        string id = shape.itemID != null ? shape.itemID.ToLower() : "";
        string assetName = shape.name != null ? shape.name.ToLower() : "";
        string itemName = shape.itemName != null ? shape.itemName.ToLower() : "";

        bool isFuel = (id == "fuel_can_01" || id.Contains("fuel") || assetName.Contains("fuel") || itemName.Contains("xăng"));

        // Xử lý chức năng đổ xăng
        if (isFuel)
        {
            CarFuel carFuel = Object.FindFirstObjectByType<CarFuel>(FindObjectsInactive.Include);
            if (carFuel != null)
            {
                if (carFuel.currentFuel >= carFuel.maxFuel)
                {
                    ShowNotif("Xe đã đầy xăng, không thể đổ thêm!");
                    return;
                }

                carFuel.AddFuel(30f);
                ShowNotif("Đã bơm 30 Lít xăng!");

                if (currentItem.currentOwner == InventoryItemUI.GridOwner.Trunk && TrunkMinigameUI.Instance != null)
                {
                    if (TrunkMinigameUI.Instance.GetGridData() != null)
                    {
                        TrunkMinigameUI.Instance.GetGridData().ClearCells(currentItem.GetGridX(), currentItem.GetGridY(), currentItem.GetItemShape(), currentItem.IsRotated());
                    }
                    currentItem.transform.SetParent(null);
                    Destroy(currentItem.gameObject);
                    TrunkMinigameUI.Instance.SaveTrunk();
                }
                else if (BackpackMinigameUI.Instance != null)
                {
                    if (BackpackMinigameUI.Instance.GetGridData() != null)
                    {
                        BackpackMinigameUI.Instance.GetGridData().ClearCells(currentItem.GetGridX(), currentItem.GetGridY(), currentItem.GetItemShape(), currentItem.IsRotated());
                    }
                    currentItem.transform.SetParent(null);
                    Destroy(currentItem.gameObject);
                    BackpackMinigameUI.Instance.SaveBackpack();
                }

                CloseMenu();
            }
            else
            {
                ShowNotif("Không tìm thấy xe để đổ xăng!");
            }
        }
        // Xử lý chức năng Ăn uống
        else
        {
            currentItem.ConsumeItem();
            ShowNotif("Đã ăn xong, thể lực đã hồi phục!");
            ForcedTutorialManager.Instance?.NotifyEatFish();
            CloseMenu();
        }
    }

    private void ShowNotif(string msg)
    {
        if (notificationText != null)
        {
            StopAllCoroutines();
            StartCoroutine(NotifRoutine(msg));
        }
        else
        {
            Debug.Log(msg);
        }
    }

    private System.Collections.IEnumerator NotifRoutine(string msg)
    {
        notificationText.text = msg;
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        notificationText.gameObject.SetActive(false);
    }
}