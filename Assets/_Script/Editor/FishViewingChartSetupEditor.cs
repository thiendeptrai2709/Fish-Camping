#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class FishViewingChartSetupEditor : MonoBehaviour
{
    [MenuItem("Tools/Fishing/Thiết Lập Giao Diện Bảng Cá 3D (FishViewingChart)", false, 15)]
    [MenuItem("GameObject/UI/Thiết Lập FishViewingChart Thành Bảng Xem Cá 3D", false, 25)]
    public static void SetupFishViewingChart()
    {
        // 1. Tìm GameObject FishViewingChart
        GameObject chartObj = GameObject.Find("FishViewingChart");
        if (chartObj == null && Selection.activeGameObject != null && Selection.activeGameObject.name.Contains("FishViewingChart"))
        {
            chartObj = Selection.activeGameObject;
        }

        if (chartObj == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[FishViewingChartSetupEditor] Không tìm thấy Canvas trong Scene!");
                return;
            }
            chartObj = new GameObject("FishViewingChart", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            chartObj.transform.SetParent(canvas.transform, false);
            RectTransform rtChart = chartObj.GetComponent<RectTransform>();
            rtChart.sizeDelta = new Vector2(1200, 720);
        }

        // Đảm bảo có CanvasGroup & FishInspectionUI
        CanvasGroup cg = chartObj.GetComponent<CanvasGroup>();
        if (cg == null) cg = chartObj.AddComponent<CanvasGroup>();

        FishInspectionUI inspectionUI = chartObj.GetComponent<FishInspectionUI>();
        if (inspectionUI == null) inspectionUI = chartObj.AddComponent<FishInspectionUI>();

        RectTransform rootRt = chartObj.GetComponent<RectTransform>();

        // Load Sprites từ Asset nếu có
        Sprite btnGreenSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Asset/Hyper_Casual_UI/Sprites/Buttons/Claim Reward.png");
        Sprite btnBlueSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Asset/Hyper_Casual_UI/Sprites/Buttons/Confirm.png");
        Sprite iconCoin = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Asset/Hyper_Casual_UI/Sprites/Icons/Coins (1).png");

        // 2. TẠO CỘT TRÁI (Left Card - Thông Số Cá)
        Transform leftCardTrans = chartObj.transform.Find("Left_StatsCard");
        GameObject leftCard;
        if (leftCardTrans == null)
        {
            leftCard = new GameObject("Left_StatsCard", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            leftCard.transform.SetParent(chartObj.transform, false);
            RectTransform rtLeft = leftCard.GetComponent<RectTransform>();
            rtLeft.anchorMin = new Vector2(0f, 0f);
            rtLeft.anchorMax = new Vector2(0.48f, 1f);
            rtLeft.pivot = new Vector2(0f, 0.5f);
            rtLeft.anchoredPosition = new Vector2(50, 0);
            rtLeft.sizeDelta = new Vector2(-100, -90);

            Image imgCard = leftCard.GetComponent<Image>();
            imgCard.color = new Color(0.08f, 0.11f, 0.16f, 0.88f); // Nền kính mờ sang trọng

            VerticalLayoutGroup vlgLeft = leftCard.GetComponent<VerticalLayoutGroup>();
            vlgLeft.padding = new RectOffset(28, 28, 28, 24);
            vlgLeft.spacing = 12;
            vlgLeft.childControlWidth = true;
            vlgLeft.childControlHeight = false;
            vlgLeft.childForceExpandWidth = true;
            vlgLeft.childForceExpandHeight = false;
        }
        else
        {
            leftCard = leftCardTrans.gameObject;
        }

        // Header Tiêu đề
        TextMeshProUGUI txtHeader = CreateOrGetText(leftCard.transform, "Txt_TitleHeader", "BẮT ĐƯỢC CÁ THÀNH CÔNG!", 26, FontStyles.Bold, new Color(1f, 0.88f, 0.35f), TextAlignmentOptions.Left);
        
        // Tên con cá
        TextMeshProUGUI txtFishName = CreateOrGetText(leftCard.transform, "Txt_FishName", "Tên Loài Cá", 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);

        // Khối Badge (Độ hiếm + Phẩm cấp)
        Transform badgeRowTrans = leftCard.transform.Find("Row_Badges");
        GameObject badgeRow;
        if (badgeRowTrans == null)
        {
            badgeRow = new GameObject("Row_Badges", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            badgeRow.transform.SetParent(leftCard.transform, false);
            RectTransform rtRow = badgeRow.GetComponent<RectTransform>();
            rtRow.sizeDelta = new Vector2(0, 34);
            HorizontalLayoutGroup hlg = badgeRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
        }
        else
        {
            badgeRow = badgeRowTrans.gameObject;
        }

        TextMeshProUGUI txtRarity = CreateOrGetText(badgeRow.transform, "Txt_RarityBadge", "[ HIẾM ]", 17, FontStyles.Bold, new Color(0.28f, 0.75f, 1f), TextAlignmentOptions.Center);
        TextMeshProUGUI txtGrade = CreateOrGetText(badgeRow.transform, "Txt_GradeBadge", "Hạng Vàng", 17, FontStyles.Bold, new Color(1f, 0.86f, 0.22f), TextAlignmentOptions.Center);

        // Huy hiệu Kỷ Lục Mới
        Transform newRecordTrans = leftCard.transform.Find("Badge_NewRecord");
        GameObject newRecordObj;
        if (newRecordTrans == null)
        {
            newRecordObj = new GameObject("Badge_NewRecord", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            newRecordObj.transform.SetParent(leftCard.transform, false);
            LayoutElement le = newRecordObj.GetComponent<LayoutElement>();
            le.preferredHeight = 32;
            Image img = newRecordObj.GetComponent<Image>();
            img.color = new Color(0.9f, 0.5f, 0.1f, 0.9f);

            TextMeshProUGUI txtRec = CreateOrGetText(newRecordObj.transform, "Txt_NewRecord", "KỶ LỰC MỚI ĐƯỢC THIẾT LẬP!", 15, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            newRecordObj.SetActive(false); // Mặc định ẩn
        }
        else
        {
            newRecordObj = newRecordTrans.gameObject;
        }

        // Divider
        Transform divTrans = leftCard.transform.Find("Divider");
        if (divTrans == null)
        {
            GameObject div = new GameObject("Divider", typeof(Image), typeof(LayoutElement));
            div.transform.SetParent(leftCard.transform, false);
            div.GetComponent<Image>().color = new Color(0.4f, 0.5f, 0.65f, 0.35f);
            div.GetComponent<LayoutElement>().preferredHeight = 2;
        }

        // Chi tiết thông số
        TextMeshProUGUI txtLength = CreateOrGetText(leftCard.transform, "Txt_Length", "Chiều dài: <b><color=#FFEE77>48.5 cm</color></b>", 19, FontStyles.Normal, Color.white, TextAlignmentOptions.Left);
        TextMeshProUGUI txtWeight = CreateOrGetText(leftCard.transform, "Txt_Weight", "Cân nặng: <b><color=#FFEE77>3.45 kg</color></b>", 19, FontStyles.Normal, Color.white, TextAlignmentOptions.Left);
        TextMeshProUGUI txtPrice = CreateOrGetText(leftCard.transform, "Txt_Price", "Giá bán: <b><color=#55FF66>$140</color></b>", 19, FontStyles.Normal, Color.white, TextAlignmentOptions.Left);
        TextMeshProUGUI txtLocation = CreateOrGetText(leftCard.transform, "Txt_Location", "Khu vực: <b><color=#88DDFF>Hồ Pine Lake</color></b>", 17, FontStyles.Normal, Color.white, TextAlignmentOptions.Left);

        // Dòng cảnh báo Balo đầy
        TextMeshProUGUI txtWarning = CreateOrGetText(leftCard.transform, "Txt_BackpackWarning", "Balo đã đầy! Hãy dọn chỗ trống trong Balo hoặc chọn Thả Cá.", 15, FontStyles.Bold, new Color(1f, 0.45f, 0.2f), TextAlignmentOptions.Left);
        txtWarning.gameObject.SetActive(false);

        // Khối 2 Nút Hành Động (Góc Dưới Trái)
        Transform buttonRowTrans = leftCard.transform.Find("Row_ActionButtons");
        GameObject buttonRow;
        if (buttonRowTrans == null)
        {
            buttonRow = new GameObject("Row_ActionButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            buttonRow.transform.SetParent(leftCard.transform, false);
            LayoutElement le = buttonRow.GetComponent<LayoutElement>();
            le.preferredHeight = 56;
            le.minHeight = 56;

            HorizontalLayoutGroup hlg = buttonRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
        }
        else
        {
            buttonRow = buttonRowTrans.gameObject;
        }

        Button btnKeep = CreateActionButton(buttonRow.transform, "Btn_KeepFish", "CẤT VÀO BALO", new Color(0.18f, 0.68f, 0.38f), btnGreenSprite);
        Button btnRelease = CreateActionButton(buttonRow.transform, "Btn_ReleaseFish", "THẢ CÁ VỀ HỒ", new Color(0.24f, 0.52f, 0.85f), btnBlueSprite);

        // 3. TẠO CỘT PHẢI (Right Card - 3D Fish Viewport)
        Transform rightCardTrans = chartObj.transform.Find("Right_3DPreviewCard");
        GameObject rightCard;
        RawImage rawImg;
        if (rightCardTrans == null)
        {
            rightCard = new GameObject("Right_3DPreviewCard", typeof(RectTransform), typeof(Image));
            rightCard.transform.SetParent(chartObj.transform, false);
            RectTransform rtRight = rightCard.GetComponent<RectTransform>();
            rtRight.anchorMin = new Vector2(0.51f, 0f);
            rtRight.anchorMax = new Vector2(1f, 1f);
            rtRight.pivot = new Vector2(1f, 0.5f);
            rtRight.anchoredPosition = new Vector2(-50, 0);
            rtRight.sizeDelta = new Vector2(-100, -90);

            Image imgRight = rightCard.GetComponent<Image>();
            imgRight.color = new Color(0.06f, 0.08f, 0.12f, 0.75f); // Khung viền mờ cho 3D model

            // RawImage hiển thị camera 3D
            GameObject rawImgObj = new GameObject("RawImage_Fish3D", typeof(RawImage));
            rawImgObj.transform.SetParent(rightCard.transform, false);
            RectTransform rtRaw = rawImgObj.GetComponent<RectTransform>();
            rtRaw.anchorMin = Vector2.zero;
            rtRaw.anchorMax = Vector2.one;
            rtRaw.sizeDelta = Vector2.zero;
            rawImg = rawImgObj.GetComponent<RawImage>();

            // Chữ hướng dẫn tương tác chuột
            TextMeshProUGUI txtHint = CreateOrGetText(rightCard.transform, "Txt_DragHint", "Rê chuột trái để xoay 360 độ", 15, FontStyles.Bold, new Color(1f, 0.92f, 0.65f, 0.85f), TextAlignmentOptions.Center);
            RectTransform rtHint = txtHint.GetComponent<RectTransform>();
            rtHint.anchorMin = new Vector2(0f, 0f);
            rtHint.anchorMax = new Vector2(1f, 0f);
            rtHint.pivot = new Vector2(0.5f, 0f);
            rtHint.anchoredPosition = new Vector2(0, 16);
            rtHint.sizeDelta = new Vector2(0, 28);
        }
        else
        {
            rightCard = rightCardTrans.gameObject;
            rawImg = rightCard.GetComponentInChildren<RawImage>(true);
        }

        // 4. Gán Serialized Fields vào FishInspectionUI
        SerializedObject so = new SerializedObject(inspectionUI);
        so.FindProperty("panelRoot").objectReferenceValue = chartObj;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("panelRect").objectReferenceValue = rootRt;
        so.FindProperty("txtTitleHeader").objectReferenceValue = txtHeader;
        so.FindProperty("txtFishName").objectReferenceValue = txtFishName;
        so.FindProperty("txtRarityBadge").objectReferenceValue = txtRarity;
        so.FindProperty("txtGradeBadge").objectReferenceValue = txtGrade;
        so.FindProperty("newRecordBadge").objectReferenceValue = newRecordObj;
        so.FindProperty("txtLength").objectReferenceValue = txtLength;
        so.FindProperty("txtWeight").objectReferenceValue = txtWeight;
        so.FindProperty("txtPrice").objectReferenceValue = txtPrice;
        so.FindProperty("txtLocation").objectReferenceValue = txtLocation;
        so.FindProperty("txtBackpackWarning").objectReferenceValue = txtWarning;
        so.FindProperty("btnKeepFish").objectReferenceValue = btnKeep;
        so.FindProperty("btnReleaseFish").objectReferenceValue = btnRelease;
        so.FindProperty("fishRenderImage").objectReferenceValue = rawImg;
        so.FindProperty("dragAreaRect").objectReferenceValue = rightCard.GetComponent<RectTransform>();
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(chartObj, "Setup Luxury FishViewingChart");
        Selection.activeGameObject = chartObj;

        Debug.Log("✨ [FishViewingChartSetupEditor] Đã thiết lập hoàn tất giao diện Bảng Xem Cá 3D đẳng cấp trong FishViewingChart!");
    }

    private static TextMeshProUGUI CreateOrGetText(Transform parent, string name, string defaultText, float fontSize, FontStyles style, Color color, TextAlignmentOptions align)
    {
        Transform t = parent.Find(name);
        GameObject obj;
        if (t == null)
        {
            obj = new GameObject(name, typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
        }
        else
        {
            obj = t.gameObject;
        }

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        return tmp;
    }

    private static Button CreateActionButton(Transform parent, string name, string label, Color btnColor, Sprite btnSprite = null)
    {
        Transform t = parent.Find(name);
        GameObject btnObj;
        if (t == null)
        {
            btnObj = new GameObject(name, typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            Image img = btnObj.GetComponent<Image>();
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Simple;
                img.color = Color.white;
            }
            else
            {
                img.color = btnColor;
            }

            GameObject txtObj = new GameObject("Txt_Label", typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 17;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
        else
        {
            btnObj = t.gameObject;
        }

        return btnObj.GetComponent<Button>();
    }
}
#endif
