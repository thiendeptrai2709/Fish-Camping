#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class MapRequirementPopupCreator : MonoBehaviour
{
    [MenuItem("Tools/Map/Tạo Bảng Nhiệm Vụ Mở Map Trong Hierarchy", false, 10)]
    [MenuItem("GameObject/UI/Tạo Bảng Nhiệm Vụ Mở Map (Popup)", false, 20)]
    public static void CreatePopupInHierarchy()
    {
        // 1. Tìm Canvas phù hợp
        Canvas targetCanvas = Object.FindFirstObjectByType<Canvas>();
        if (targetCanvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasObj.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = canvasObj.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // Ưu tiên đặt dưới panel UI cha nếu có
        Transform parentTransform = targetCanvas.transform;
        Transform mapPanel = targetCanvas.transform.Find("MapPanel");
        if (mapPanel != null && mapPanel.parent != null)
        {
            parentTransform = mapPanel.parent;
        }

        // 2. Load Sprite Background
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Asset/Hyper_Casual_UI/Sprites/Panel_Sprites/PREMIUM OFFER! popup.png");

        // 3. Tạo Root GameObject
        GameObject root = new GameObject("MapRequirementPopup_Root", typeof(RectTransform), typeof(CanvasGroup), typeof(MapRequirementPopupUI));
        root.transform.SetParent(parentTransform, false);
        RectTransform rtRoot = root.GetComponent<RectTransform>();
        rtRoot.anchorMin = Vector2.zero;
        rtRoot.anchorMax = Vector2.one;
        rtRoot.sizeDelta = Vector2.zero;

        CanvasGroup cg = root.GetComponent<CanvasGroup>();
        MapRequirementPopupUI popupUI = root.GetComponent<MapRequirementPopupUI>();

        // 4. Background Dimmer (Màn đen mờ phía sau)
        GameObject bgDim = new GameObject("BG_Dimmer", typeof(Image), typeof(Button));
        bgDim.transform.SetParent(root.transform, false);
        RectTransform rtDim = bgDim.GetComponent<RectTransform>();
        rtDim.anchorMin = Vector2.zero;
        rtDim.anchorMax = Vector2.one;
        rtDim.sizeDelta = Vector2.zero;
        Image imgDim = bgDim.GetComponent<Image>();
        imgDim.color = new Color(0f, 0f, 0f, 0.75f);
        Button btnDim = bgDim.GetComponent<Button>();

        // 5. Panel Box Chính (Khung Popup)
        GameObject box = new GameObject("Panel_Box", typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        box.transform.SetParent(root.transform, false);
        RectTransform rtBox = box.GetComponent<RectTransform>();
        rtBox.anchorMin = new Vector2(0.5f, 0.5f);
        rtBox.anchorMax = new Vector2(0.5f, 0.5f);
        rtBox.pivot = new Vector2(0.5f, 0.5f);
        rtBox.sizeDelta = new Vector2(720, 0);

        Image imgBox = box.GetComponent<Image>();
        if (bgSprite != null)
        {
            imgBox.sprite = bgSprite;
            imgBox.type = Image.Type.Simple;
            imgBox.color = Color.white;
        }
        else
        {
            imgBox.color = new Color(0.11f, 0.13f, 0.16f, 0.98f);
        }

        VerticalLayoutGroup vlgBox = box.GetComponent<VerticalLayoutGroup>();
        vlgBox.padding = new RectOffset(45, 45, 45, 40);
        vlgBox.spacing = 16;
        vlgBox.childControlWidth = true;
        vlgBox.childControlHeight = true;
        vlgBox.childForceExpandWidth = true;
        vlgBox.childForceExpandHeight = false;

        ContentSizeFitter csf = box.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 6. Header
        GameObject header = new GameObject("Header", typeof(VerticalLayoutGroup));
        header.transform.SetParent(box.transform, false);
        VerticalLayoutGroup vlgHeader = header.GetComponent<VerticalLayoutGroup>();
        vlgHeader.spacing = 6;
        vlgHeader.childControlWidth = true;
        vlgHeader.childControlHeight = true;

        GameObject titleObj = new GameObject("Txt_MapTitle", typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI txtTitle = titleObj.GetComponent<TextMeshProUGUI>();
        txtTitle.text = "🔒 ĐẦM LẦY (SWAMP)";
        txtTitle.fontSize = 28;
        txtTitle.fontStyle = FontStyles.Bold;
        txtTitle.alignment = TextAlignmentOptions.Center;
        txtTitle.color = new Color(1f, 0.85f, 0.4f);

        GameObject subObj = new GameObject("Txt_Subtitle", typeof(TextMeshProUGUI));
        subObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI txtSub = subObj.GetComponent<TextMeshProUGUI>();
        txtSub.text = "Khu vực này hiện đang bị khóa. Hãy hoàn thành các điều kiện bên dưới để mở đường:";
        txtSub.fontSize = 16;
        txtSub.alignment = TextAlignmentOptions.Center;
        txtSub.color = new Color(0.85f, 0.85f, 0.9f);

        // Divider
        GameObject div = new GameObject("Divider", typeof(Image), typeof(LayoutElement));
        div.transform.SetParent(box.transform, false);
        div.GetComponent<Image>().color = new Color(0.35f, 0.4f, 0.5f, 0.5f);
        div.GetComponent<LayoutElement>().preferredHeight = 2;

        // 7. Card 1: Sổ Tay Cá
        GameObject card1 = new GameObject("Card_FishJournal", typeof(Image), typeof(VerticalLayoutGroup));
        card1.transform.SetParent(box.transform, false);
        card1.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.2f, 0.85f);
        VerticalLayoutGroup vlgCard1 = card1.GetComponent<VerticalLayoutGroup>();
        vlgCard1.padding = new RectOffset(20, 20, 16, 16);
        vlgCard1.spacing = 8;
        vlgCard1.childControlWidth = true;
        vlgCard1.childControlHeight = true;

        GameObject fishStatusObj = new GameObject("Txt_FishStatus", typeof(TextMeshProUGUI));
        fishStatusObj.transform.SetParent(card1.transform, false);
        TextMeshProUGUI txtFishStatus = fishStatusObj.GetComponent<TextMeshProUGUI>();
        txtFishStatus.text = "[❌ CHƯA ĐẠT]";
        txtFishStatus.fontSize = 20;
        txtFishStatus.fontStyle = FontStyles.Bold;
        txtFishStatus.color = new Color(1f, 0.35f, 0.35f);

        GameObject fishDetailsObj = new GameObject("Txt_FishDetails", typeof(TextMeshProUGUI));
        fishDetailsObj.transform.SetParent(card1.transform, false);
        TextMeshProUGUI txtFishDetails = fishDetailsObj.GetComponent<TextMeshProUGUI>();
        txtFishDetails.text = "Sổ tay cá (Hồ Pine Lake): <b>2 / 4</b> loài cá đã ghi nhận";
        txtFishDetails.fontSize = 17;
        txtFishDetails.color = Color.white;

        // Slider Progress Bar
        GameObject sliderObj = new GameObject("Slider_FishProgress", typeof(Slider));
        sliderObj.transform.SetParent(card1.transform, false);
        Slider sliderFish = sliderObj.GetComponent<Slider>();
        sliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 14);

        GameObject bgSlider = new GameObject("Background", typeof(Image));
        bgSlider.transform.SetParent(sliderObj.transform, false);
        RectTransform rtBgSlider = bgSlider.GetComponent<RectTransform>();
        rtBgSlider.anchorMin = Vector2.zero;
        rtBgSlider.anchorMax = Vector2.one;
        rtBgSlider.sizeDelta = Vector2.zero;
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
        sliderFish.fillRect = rtFill;
        sliderFish.maxValue = 4;
        sliderFish.value = 2;

        GameObject fishHintObj = new GameObject("Txt_FishHint", typeof(TextMeshProUGUI));
        fishHintObj.transform.SetParent(card1.transform, false);
        TextMeshProUGUI txtFishHint = fishHintObj.GetComponent<TextMeshProUGUI>();
        txtFishHint.text = "💡 Gợi ý: Hãy câu thêm 2 loài cá tại Hồ Pine Lake!";
        txtFishHint.fontSize = 14;
        txtFishHint.color = new Color(0.75f, 0.85f, 0.95f);

        // 8. Card 2: Lốp Xe Garage
        GameObject card2 = new GameObject("Card_TireGarage", typeof(Image), typeof(VerticalLayoutGroup));
        card2.transform.SetParent(box.transform, false);
        card2.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.2f, 0.85f);
        VerticalLayoutGroup vlgCard2 = card2.GetComponent<VerticalLayoutGroup>();
        vlgCard2.padding = new RectOffset(20, 20, 16, 16);
        vlgCard2.spacing = 8;
        vlgCard2.childControlWidth = true;
        vlgCard2.childControlHeight = true;

        GameObject tireStatusObj = new GameObject("Txt_TireStatus", typeof(TextMeshProUGUI));
        tireStatusObj.transform.SetParent(card2.transform, false);
        TextMeshProUGUI txtTireStatus = tireStatusObj.GetComponent<TextMeshProUGUI>();
        txtTireStatus.text = "[❌ CHƯA ĐẠT]";
        txtTireStatus.fontSize = 20;
        txtTireStatus.fontStyle = FontStyles.Bold;
        txtTireStatus.color = new Color(1f, 0.35f, 0.35f);

        GameObject tireDetailsObj = new GameObject("Txt_TireDetails", typeof(TextMeshProUGUI));
        tireDetailsObj.transform.SetParent(card2.transform, false);
        TextMeshProUGUI txtTireDetails = tireDetailsObj.GetComponent<TextMeshProUGUI>();
        txtTireDetails.text = "Đang trang bị: <b>Lốp Đô Thị</b>\nYêu cầu: <b>Lốp Bùn Thường / Chuyên Dụng</b>";
        txtTireDetails.fontSize = 17;
        txtTireDetails.color = Color.white;

        GameObject tireHintObj = new GameObject("Txt_TireHint", typeof(TextMeshProUGUI));
        tireHintObj.transform.SetParent(card2.transform, false);
        TextMeshProUGUI txtTireHint = tireHintObj.GetComponent<TextMeshProUGUI>();
        txtTireHint.text = "💡 Gợi ý: Hãy lái xe đến Garage ở Thị Trấn để nâng cấp lốp xe.";
        txtTireHint.fontSize = 14;
        txtTireHint.color = new Color(0.75f, 0.85f, 0.95f);

        // 9. Nút Đóng / Đã hiểu
        GameObject btnObj = new GameObject("Btn_Close", typeof(Image), typeof(Button), typeof(LayoutElement));
        btnObj.transform.SetParent(box.transform, false);
        LayoutElement leBtn = btnObj.GetComponent<LayoutElement>();
        leBtn.preferredHeight = 52;
        Image imgBtn = btnObj.GetComponent<Image>();
        imgBtn.color = new Color(0.18f, 0.65f, 0.35f);
        Button btnClose = btnObj.GetComponent<Button>();

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

        // 10. Gán References vào Script Serialized Object
        SerializedObject so = new SerializedObject(popupUI);
        so.FindProperty("popupRoot").objectReferenceValue = root;
        so.FindProperty("panelBox").objectReferenceValue = rtBox;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("popupBackgroundSprite").objectReferenceValue = bgSprite;
        so.FindProperty("txtMapTitle").objectReferenceValue = txtTitle;
        so.FindProperty("txtSubtitle").objectReferenceValue = txtSub;
        so.FindProperty("txtFishStatus").objectReferenceValue = txtFishStatus;
        so.FindProperty("txtFishDetails").objectReferenceValue = txtFishDetails;
        so.FindProperty("sliderFishProgress").objectReferenceValue = sliderFish;
        so.FindProperty("txtFishHint").objectReferenceValue = txtFishHint;
        so.FindProperty("txtTireStatus").objectReferenceValue = txtTireStatus;
        so.FindProperty("txtTireDetails").objectReferenceValue = txtTireDetails;
        so.FindProperty("txtTireHint").objectReferenceValue = txtTireHint;
        so.FindProperty("btnClose").objectReferenceValue = btnClose;
        so.ApplyModifiedProperties();

        // Gán sự kiện cho các nút
        btnDim.onClick.AddListener(popupUI.Hide);
        btnClose.onClick.AddListener(popupUI.Hide);

        // Đăng ký Undo và chọn Object vừa tạo trong Hierarchy
        Undo.RegisterCreatedObjectUndo(root, "Create Map Requirement Popup");
        Selection.activeGameObject = root;

        Debug.Log("✨ [MapRequirementPopupCreator] Đã tạo thành công Bảng Nhiệm Vụ Mở Map vào Hierarchy!");
    }
}
#endif
