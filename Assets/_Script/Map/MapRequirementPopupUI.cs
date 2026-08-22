using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MapRequirementPopupUI : MonoBehaviour
{
    public static MapRequirementPopupUI Instance { get; private set; }

    [Header("UI References (Tự động khởi tạo nếu để trống)")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private RectTransform panelBox;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Sprite popupBackgroundSprite;
    [SerializeField] private TextMeshProUGUI txtMapTitle;
    [SerializeField] private TextMeshProUGUI txtSubtitle;

    [Header("Item 1: Sổ Tay Cá")]
    [SerializeField] private TextMeshProUGUI txtFishStatus;
    [SerializeField] private TextMeshProUGUI txtFishDetails;
    [SerializeField] private Slider sliderFishProgress;
    [SerializeField] private TextMeshProUGUI txtFishHint;

    [Header("Item 2: Lốp Xe Garage")]
    [SerializeField] private TextMeshProUGUI txtTireStatus;
    [SerializeField] private TextMeshProUGUI txtTireDetails;
    [SerializeField] private TextMeshProUGUI txtTireHint;

    [Header("Nút bấm")]
    [SerializeField] private Button btnClose;

    private Coroutine animCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadBackgroundSprite();

        if (popupRoot == null)
        {
            BuildUIRuntime();
        }
        else
        {
            ApplyBackgroundSprite();
            popupRoot.SetActive(false);
        }

        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(Hide);
        }
    }

    private void LoadBackgroundSprite()
    {
        if (popupBackgroundSprite == null)
        {
#if UNITY_EDITOR
            popupBackgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Asset/Hyper_Casual_UI/Sprites/Panel_Sprites/PREMIUM OFFER! popup.png");
#endif
        }
    }

    private void ApplyBackgroundSprite()
    {
        if (panelBox != null && popupBackgroundSprite != null)
        {
            Image img = panelBox.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = popupBackgroundSprite;
                img.type = Image.Type.Simple;
                img.color = Color.white;
            }
        }
    }

    [ContextMenu("Tự Động Tìm Tham Chiếu (Auto Find References)")]
    public void AutoFindReferencesInHierarchy()
    {
        if (popupRoot == null) popupRoot = gameObject;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (panelBox == null)
        {
            Transform boxTrans = transform.Find("Panel_Box");
            if (boxTrans != null) panelBox = boxTrans.GetComponent<RectTransform>();
        }

        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            string n = t.gameObject.name.ToLower();
            if (txtMapTitle == null && (n.Contains("title") || n.Contains("maptitle"))) txtMapTitle = t;
            else if (txtSubtitle == null && (n.Contains("subtitle") || n.Contains("sub"))) txtSubtitle = t;
            else if (txtFishStatus == null && n.Contains("fishstatus")) txtFishStatus = t;
            else if (txtFishDetails == null && n.Contains("fishdetails")) txtFishDetails = t;
            else if (txtFishHint == null && n.Contains("fishhint")) txtFishHint = t;
            else if (txtTireStatus == null && n.Contains("tirestatus")) txtTireStatus = t;
            else if (txtTireDetails == null && n.Contains("tiredetails")) txtTireDetails = t;
            else if (txtTireHint == null && n.Contains("tirehint")) txtTireHint = t;
        }

        if (sliderFishProgress == null) sliderFishProgress = GetComponentInChildren<Slider>(true);
        if (btnClose == null)
        {
            Button[] btns = GetComponentsInChildren<Button>(true);
            foreach (var b in btns)
            {
                if (b.gameObject.name.ToLower().Contains("close") || b.gameObject.name.ToLower().Contains("btn"))
                {
                    btnClose = b;
                    break;
                }
            }
        }
        LoadBackgroundSprite();
        ApplyBackgroundSprite();
    }

    /// <summary>
    /// Hiển thị Popup Checklist điều kiện mở map
    /// </summary>
    public void Show(
        string mapDisplayName,
        bool isTutorialLocked,
        string tutorialHint,
        bool fishConditionMet,
        int currentFishCount,
        int targetFishCount,
        string fishMapName,
        bool tireConditionMet,
        string currentTireName,
        string requiredTireDesc,
        string tireHint)
    {
        if (popupRoot == null) BuildUIRuntime();

        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        // 1. Tên Bản Đồ & Trạng Thái Khóa
        if (txtMapTitle != null)
        {
            txtMapTitle.text = $"🔒 {mapDisplayName.ToUpper()}";
        }

        if (txtSubtitle != null)
        {
            txtSubtitle.text = isVietnamese 
                ? "Khu vực này hiện đang bị khóa. Hãy hoàn thành các điều kiện bên dưới để mở đường:" 
                : "This area is currently locked. Complete the requirements below to unlock:";
        }

        // Trường hợp bị khóa bởi Nhiệm vụ Tutorial Map 1
        if (isTutorialLocked)
        {
            if (txtFishStatus != null)
            {
                txtFishStatus.gameObject.SetActive(true);
                txtFishStatus.text = isVietnamese ? "[❌ CHƯA ĐẠT]" : "[❌ LOCKED]";
                txtFishStatus.color = new Color(1f, 0.35f, 0.35f);
            }
            if (txtFishDetails != null)
            {
                txtFishDetails.gameObject.SetActive(true);
                txtFishDetails.text = isVietnamese ? "Cốt truyện: Hoàn thành toàn bộ nhiệm vụ tại Map 1" : "Story: Complete all quests in Map 1";
            }
            if (sliderFishProgress != null) sliderFishProgress.gameObject.SetActive(false);
            if (txtFishHint != null)
            {
                txtFishHint.gameObject.SetActive(true);
                txtFishHint.text = $"💡 {tutorialHint}";
            }

            if (txtTireStatus != null) txtTireStatus.gameObject.SetActive(false);
            if (txtTireDetails != null) txtTireDetails.gameObject.SetActive(false);
            if (txtTireHint != null) txtTireHint.gameObject.SetActive(false);
        }
        else
        {
            // 2. Cập nhật Mục 1: Sổ Tay Cá
            if (txtFishStatus != null)
            {
                txtFishStatus.gameObject.SetActive(true);
                txtFishStatus.text = fishConditionMet 
                    ? (isVietnamese ? "[✅ HOÀN THÀNH]" : "[✅ COMPLETED]") 
                    : (isVietnamese ? "[❌ CHƯA ĐẠT]" : "[❌ INCOMPLETE]");
                txtFishStatus.color = fishConditionMet ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.35f, 0.35f);
            }

            if (txtFishDetails != null)
            {
                txtFishDetails.gameObject.SetActive(true);
                txtFishDetails.text = isVietnamese 
                    ? $"Sổ tay cá ({fishMapName}): <b>{currentFishCount} / {targetFishCount}</b> loài cá đã ghi nhận"
                    : $"Fish Journal ({fishMapName}): <b>{currentFishCount} / {targetFishCount}</b> species recorded";
            }

            if (sliderFishProgress != null)
            {
                sliderFishProgress.gameObject.SetActive(true);
                sliderFishProgress.maxValue = targetFishCount;
                sliderFishProgress.value = Mathf.Clamp(currentFishCount, 0, targetFishCount);
            }

            if (txtFishHint != null)
            {
                txtFishHint.gameObject.SetActive(true);
                txtFishHint.text = fishConditionMet
                    ? (isVietnamese ? "✨ Đã hoàn thành bộ sưu tập cá cho khu vực này!" : "✨ Fish collection for this region is completed!")
                    : (isVietnamese 
                        ? $"💡 <b>Mẹo:</b> Thả cần ở nhiều điểm câu khác nhau quanh {fishMapName} để câu thêm {targetFishCount - currentFishCount} loài cá mới."
                        : $"💡 <b>Tip:</b> Fish at different spots around {fishMapName} to catch {targetFishCount - currentFishCount} more new species.");
            }

            // 3. Cập nhật Mục 2: Lốp Xe Garage
            if (txtTireStatus != null)
            {
                txtTireStatus.gameObject.SetActive(true);
                txtTireStatus.text = tireConditionMet 
                    ? (isVietnamese ? "[✅ HOÀN THÀNH]" : "[✅ COMPLETED]") 
                    : (isVietnamese ? "[❌ CHƯA ĐẠT]" : "[❌ INCOMPLETE]");
                txtTireStatus.color = tireConditionMet ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.35f, 0.35f);
            }

            if (txtTireDetails != null)
            {
                txtTireDetails.gameObject.SetActive(true);
                txtTireDetails.text = isVietnamese 
                    ? $"Yêu cầu: <b>{requiredTireDesc}</b>\n(Hiện tại: <color={(tireConditionMet ? "#55FF55" : "#FFAA33")}>{currentTireName}</color>)"
                    : $"Required: <b>{requiredTireDesc}</b>\n(Current: <color={(tireConditionMet ? "#55FF55" : "#FFAA33")}>{currentTireName}</color>)";
            }

            if (txtTireHint != null)
            {
                txtTireHint.gameObject.SetActive(true);
                txtTireHint.text = tireConditionMet
                    ? (isVietnamese ? "✨ Xe của bạn đã được trang bị lốp phù hợp với địa hình này!" : "✨ Your car is equipped with suitable tires for this terrain!")
                    : $"💡 {tireHint}";
            }
        }

        popupRoot.SetActive(true);
        popupRoot.transform.SetAsLastSibling();

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateOpen());
    }

    public void Hide()
    {
        if (UIButtonSoundManager.Instance != null)
        {
            UIButtonSoundManager.Instance.PlayClickSound();
        }

        if (popupRoot != null && popupRoot.activeSelf)
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(AnimateClose());
        }
    }

    private IEnumerator AnimateOpen()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (panelBox != null) panelBox.localScale = new Vector3(0.85f, 0.85f, 1f);

        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (canvasGroup != null) canvasGroup.alpha = smoothT;
            if (panelBox != null) panelBox.localScale = Vector3.LerpUnclamped(new Vector3(0.85f, 0.85f, 1f), Vector3.one, smoothT);
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (panelBox != null) panelBox.localScale = Vector3.one;
    }

    private IEnumerator AnimateClose()
    {
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (canvasGroup != null) canvasGroup.alpha = 1f - smoothT;
            if (panelBox != null) panelBox.localScale = Vector3.LerpUnclamped(Vector3.one, new Vector3(0.9f, 0.9f, 1f), smoothT);
            yield return null;
        }

        if (popupRoot != null) popupRoot.SetActive(false);
    }

    /// <summary>
    /// Tự động sinh giao diện Canvas Popup nếu Scene/Prefab chưa được gán thủ công
    /// </summary>
    private void BuildUIRuntime()
    {
        // 1. Tạo Canvas Root
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        GameObject root;
        if (parentCanvas == null)
        {
            root = new GameObject("MapRequirementPopup_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas c = root.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 999;
            CanvasScaler cs = root.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            DontDestroyOnLoad(root);
        }
        else
        {
            root = new GameObject("MapRequirementPopup_Root");
            root.transform.SetParent(parentCanvas.transform, false);
        }

        popupRoot = root;
        canvasGroup = popupRoot.AddComponent<CanvasGroup>();

        // 2. Background Dimmer (Mờ phía sau)
        GameObject bgDim = new GameObject("BG_Dimmer", typeof(Image), typeof(Button));
        bgDim.transform.SetParent(popupRoot.transform, false);
        RectTransform rtDim = bgDim.GetComponent<RectTransform>();
        rtDim.anchorMin = Vector2.zero;
        rtDim.anchorMax = Vector2.one;
        rtDim.sizeDelta = Vector2.zero;
        Image imgDim = bgDim.GetComponent<Image>();
        imgDim.color = new Color(0f, 0f, 0f, 0.75f);
        Button btnDim = bgDim.GetComponent<Button>();
        btnDim.onClick.AddListener(Hide);

        // 3. Panel Box Chính (Khung Popup)
        GameObject box = new GameObject("Panel_Box", typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        box.transform.SetParent(popupRoot.transform, false);
        panelBox = box.GetComponent<RectTransform>();
        panelBox.anchorMin = new Vector2(0.5f, 0.5f);
        panelBox.anchorMax = new Vector2(0.5f, 0.5f);
        panelBox.pivot = new Vector2(0.5f, 0.5f);
        panelBox.sizeDelta = new Vector2(720, 0);

        Image imgBox = box.GetComponent<Image>();
        LoadBackgroundSprite();
        if (popupBackgroundSprite != null)
        {
            imgBox.sprite = popupBackgroundSprite;
            imgBox.type = Image.Type.Simple;
            imgBox.color = Color.white;
        }
        else
        {
            imgBox.color = new Color(0.11f, 0.13f, 0.16f, 0.98f);
        }

        VerticalLayoutGroup vlg = box.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(45, 45, 45, 40);
        vlg.spacing = 16;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = box.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 4. Header: Tiêu đề Map & Subtitle
        GameObject headerObj = new GameObject("Header", typeof(VerticalLayoutGroup));
        headerObj.transform.SetParent(panelBox, false);
        VerticalLayoutGroup vlgHeader = headerObj.GetComponent<VerticalLayoutGroup>();
        vlgHeader.spacing = 6;
        vlgHeader.childControlWidth = true;
        vlgHeader.childControlHeight = true;

        GameObject titleObj = new GameObject("Txt_Title", typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(headerObj.transform, false);
        txtMapTitle = titleObj.GetComponent<TextMeshProUGUI>();
        txtMapTitle.fontSize = 28;
        txtMapTitle.fontStyle = FontStyles.Bold;
        txtMapTitle.alignment = TextAlignmentOptions.Center;
        txtMapTitle.color = new Color(1f, 0.85f, 0.4f);

        GameObject subObj = new GameObject("Txt_Subtitle", typeof(TextMeshProUGUI));
        subObj.transform.SetParent(headerObj.transform, false);
        txtSubtitle = subObj.GetComponent<TextMeshProUGUI>();
        txtSubtitle.fontSize = 16;
        txtSubtitle.alignment = TextAlignmentOptions.Center;
        txtSubtitle.color = new Color(0.8f, 0.8f, 0.85f);

        // Divider
        CreateDivider(panelBox, new Color(0.3f, 0.35f, 0.4f, 0.6f));

        // 5. Card 1: Sổ Tay Cá
        GameObject card1 = CreateCardBox(panelBox, "Card_FishJournal", new Color(0.12f, 0.15f, 0.2f, 0.85f));
        
        GameObject fishStatusObj = new GameObject("Txt_FishStatus", typeof(TextMeshProUGUI));
        fishStatusObj.transform.SetParent(card1.transform, false);
        txtFishStatus = fishStatusObj.GetComponent<TextMeshProUGUI>();
        txtFishStatus.fontSize = 20;
        txtFishStatus.fontStyle = FontStyles.Bold;

        GameObject fishDetailsObj = new GameObject("Txt_FishDetails", typeof(TextMeshProUGUI));
        fishDetailsObj.transform.SetParent(card1.transform, false);
        txtFishDetails = fishDetailsObj.GetComponent<TextMeshProUGUI>();
        txtFishDetails.fontSize = 17;
        txtFishDetails.color = Color.white;

        // Slider Progress Bar
        GameObject sliderObj = new GameObject("Slider_Fish", typeof(Slider));
        sliderObj.transform.SetParent(card1.transform, false);
        sliderFishProgress = sliderObj.GetComponent<Slider>();
        RectTransform rtSlider = sliderObj.GetComponent<RectTransform>();
        rtSlider.sizeDelta = new Vector2(0, 14);

        GameObject bgSlider = new GameObject("Background", typeof(Image));
        bgSlider.transform.SetParent(sliderObj.transform, false);
        RectTransform rtBg = bgSlider.GetComponent<RectTransform>();
        rtBg.anchorMin = Vector2.zero;
        rtBg.anchorMax = Vector2.one;
        rtBg.sizeDelta = Vector2.zero;
        bgSlider.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.11f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform rtFillArea = fillArea.GetComponent<RectTransform>();
        rtFillArea.anchorMin = Vector2.zero;
        rtFillArea.anchorMax = Vector2.one;
        rtFillArea.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform rtFill = fill.GetComponent<RectTransform>();
        rtFill.sizeDelta = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.25f, 0.75f, 1f);
        sliderFishProgress.fillRect = rtFill;

        GameObject fishHintObj = new GameObject("Txt_FishHint", typeof(TextMeshProUGUI));
        fishHintObj.transform.SetParent(card1.transform, false);
        txtFishHint = fishHintObj.GetComponent<TextMeshProUGUI>();
        txtFishHint.fontSize = 14;
        txtFishHint.color = new Color(0.75f, 0.85f, 0.95f);

        // 6. Card 2: Lốp Xe Garage
        GameObject card2 = CreateCardBox(panelBox, "Card_TireGarage", new Color(0.12f, 0.15f, 0.2f, 0.85f));

        GameObject tireStatusObj = new GameObject("Txt_TireStatus", typeof(TextMeshProUGUI));
        tireStatusObj.transform.SetParent(card2.transform, false);
        txtTireStatus = tireStatusObj.GetComponent<TextMeshProUGUI>();
        txtTireStatus.fontSize = 20;
        txtTireStatus.fontStyle = FontStyles.Bold;

        GameObject tireDetailsObj = new GameObject("Txt_TireDetails", typeof(TextMeshProUGUI));
        tireDetailsObj.transform.SetParent(card2.transform, false);
        txtTireDetails = tireDetailsObj.GetComponent<TextMeshProUGUI>();
        txtTireDetails.fontSize = 17;
        txtTireDetails.color = Color.white;

        GameObject tireHintObj = new GameObject("Txt_TireHint", typeof(TextMeshProUGUI));
        tireHintObj.transform.SetParent(card2.transform, false);
        txtTireHint = tireHintObj.GetComponent<TextMeshProUGUI>();
        txtTireHint.fontSize = 14;
        txtTireHint.color = new Color(1f, 0.88f, 0.65f);

        // 7. Footer: Nút Đóng Popup
        GameObject btnObj = new GameObject("Btn_ClosePopup", typeof(Image), typeof(Button), typeof(LayoutElement));
        btnObj.transform.SetParent(panelBox, false);
        LayoutElement leBtn = btnObj.GetComponent<LayoutElement>();
        leBtn.preferredHeight = 52;

        Image imgBtn = btnObj.GetComponent<Image>();
        imgBtn.color = new Color(0.22f, 0.55f, 0.85f);

        btnClose = btnObj.GetComponent<Button>();
        ColorBlock cb = btnClose.colors;
        cb.highlightedColor = new Color(0.3f, 0.65f, 1f);
        cb.pressedColor = new Color(0.15f, 0.45f, 0.75f);
        btnClose.colors = cb;
        btnClose.onClick.AddListener(Hide);

        GameObject btnTxtObj = new GameObject("Txt_Btn", typeof(TextMeshProUGUI));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        RectTransform rtBtnTxt = btnTxtObj.GetComponent<RectTransform>();
        rtBtnTxt.anchorMin = Vector2.zero;
        rtBtnTxt.anchorMax = Vector2.one;
        rtBtnTxt.sizeDelta = Vector2.zero;
        TextMeshProUGUI txtBtn = btnTxtObj.GetComponent<TextMeshProUGUI>();
        txtBtn.text = "ĐÃ HIỂU";
        txtBtn.fontSize = 18;
        txtBtn.fontStyle = FontStyles.Bold;
        txtBtn.alignment = TextAlignmentOptions.Center;
        txtBtn.color = Color.white;

        popupRoot.SetActive(false);
    }

    private GameObject CreateCardBox(Transform parent, string name, Color bgColor)
    {
        GameObject card = new GameObject(name, typeof(Image), typeof(VerticalLayoutGroup));
        card.transform.SetParent(parent, false);
        Image img = card.GetComponent<Image>();
        img.color = bgColor;

        VerticalLayoutGroup vlg = card.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 16, 16);
        vlg.spacing = 8;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        return card;
    }

    private void CreateDivider(Transform parent, Color color)
    {
        GameObject div = new GameObject("Divider", typeof(Image), typeof(LayoutElement));
        div.transform.SetParent(parent, false);
        div.GetComponent<Image>().color = color;
        LayoutElement le = div.GetComponent<LayoutElement>();
        le.preferredHeight = 2;
    }
}
