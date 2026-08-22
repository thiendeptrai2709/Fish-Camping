using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class FishInspectionUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public static FishInspectionUI Instance { get; private set; }

    [Header("=== GIAO DIỆN CHÍNH (FishViewingChart) ===")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRect;

    [Header("=== CỘT TRÁI: THÔNG SỐ CÁ ===")]
    [SerializeField] private TextMeshProUGUI txtTitleHeader;
    [SerializeField] private TextMeshProUGUI txtFishName;
    [SerializeField] private TextMeshProUGUI txtRarityBadge;
    [SerializeField] private TextMeshProUGUI txtGradeBadge;
    [SerializeField] private GameObject newRecordBadge;
    [SerializeField] private TextMeshProUGUI txtLength;
    [SerializeField] private TextMeshProUGUI txtWeight;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private TextMeshProUGUI txtLocation;
    [SerializeField] private TextMeshProUGUI txtBackpackWarning;

    [Header("=== NÚT HÀNH ĐỘNG (DƯỚI TRÁI) ===")]
    [SerializeField] private Button btnKeepFish;
    [SerializeField] private Button btnReleaseFish;

    [Header("=== CỘT PHẢI: 3D MODEL PREVIEW ===")]
    [SerializeField] private RawImage fishRenderImage;
    [SerializeField] private RectTransform dragAreaRect;
    [SerializeField] private Camera previewCamera;
    [SerializeField] private Transform previewStageHolder;
    [SerializeField] private Transform fishModelHolder;
    [SerializeField] private float rotationSpeed = 6.5f;
    [SerializeField] private float autoSpinSpeed = 18f;

    [Header("=== CAMERA CINEMACHINE (KHÓA XOAY KHI MỞ BẢNG) ===")]
    [SerializeField] private MonoBehaviour freeLookCamera;

    [Header("=== ÂM THANH ===")]
    [SerializeField] private AudioClip soundOpenPopup;
    [SerializeField] private AudioClip soundKeepFish;
    [SerializeField] private AudioClip soundReleaseFish;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private FishSO currentFishData;
    private float currentLength;
    private float currentWeight;
    private FishGrade currentGrade;
    private bool isCurrentNewRecord;

    private System.Action onKeepCallback;
    private System.Action onReleaseCallback;

    private RenderTexture previewRenderTexture;
    private GameObject spawnedModelInstance;
    private bool isDragging = false;
    private float targetRotationY = -30f;
    private float currentRotationY = -30f;
    private float targetRotationX = 8f;
    private float currentRotationX = 8f;
    private float floatTime = 0f;
    private Coroutine animCoroutine;
    private Coroutine warningCoroutine;

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

        GameObject oldStage = GameObject.Find("Fish3D_PreviewStage");
        if (oldStage != null)
        {
            Destroy(oldStage);
        }

        // Tự động tìm kiếm tham chiếu nếu chưa gán thủ công
        if (panelRoot == null)
        {
            AutoFindReferences();
        }

        Setup3DPreviewStage();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (btnKeepFish != null)
        {
            btnKeepFish.onClick.RemoveAllListeners();
            btnKeepFish.onClick.AddListener(OnKeepClicked);
        }

        if (btnReleaseFish != null)
        {
            btnReleaseFish.onClick.RemoveAllListeners();
            btnReleaseFish.onClick.AddListener(OnReleaseClicked);
        }
    }

    private void Update()
    {
        if (!IsOpen) return;

        // 1. Tự động xoay và đung đưa nhẹ khi không kéo chuột
        if (!isDragging)
        {
            targetRotationY += autoSpinSpeed * Time.unscaledDeltaTime;
        }

        floatTime += Time.unscaledDeltaTime * 2.2f;
        float gentleBobbing = Mathf.Sin(floatTime) * 0.06f;

        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, Time.unscaledDeltaTime * 14f);
        currentRotationX = Mathf.Lerp(currentRotationX, targetRotationX, Time.unscaledDeltaTime * 14f);

        if (fishModelHolder != null)
        {
            fishModelHolder.localRotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
            fishModelHolder.localPosition = new Vector3(0, gentleBobbing, 0);
        }
    }

    /// <summary>
    /// Hiển thị bảng chi tiết và mô hình 3D của cá vừa câu được
    /// </summary>
    public void ShowFish(
        FishSO fishData,
        float length,
        float weight,
        FishGrade grade,
        bool isNewRecord,
        System.Action onKeep,
        System.Action onRelease)
    {
        if (fishData == null) return;

        if (panelRoot == null)
        {
            AutoFindReferences();
        }

        currentFishData = fishData;
        currentLength = length;
        currentWeight = weight;
        currentGrade = grade;
        isCurrentNewRecord = isNewRecord;
        onKeepCallback = onKeep;
        onReleaseCallback = onRelease;

        // Reset cảnh báo balo
        if (txtBackpackWarning != null)
        {
            txtBackpackWarning.gameObject.SetActive(false);
        }

        // 1. Cập nhật thông tin text hiển thị
        UpdateUIContent();

        // 2. Nạp Model 3D vào khung Preview
        SpawnFish3DModel();

        // 3. Khóa an toàn toàn bộ Input và Camera tự do
        SetPlayerInputControl(false);

        // 4. Bật Panel & Chạy Animation mở
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(AnimateOpen());
        }

        // Âm thanh xuất hiện
        if (soundOpenPopup != null)
        {
            AudioSource.PlayClipAtPoint(soundOpenPopup, Camera.main != null ? Camera.main.transform.position : transform.position, 0.85f);
        }
    }

    private void UpdateUIContent()
    {
        if (currentFishData == null) return;

        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        // Tiêu đề Header
        if (txtTitleHeader != null)
        {
            txtTitleHeader.text = isVietnamese ? "BẮT ĐƯỢC CÁ THÀNH CÔNG!" : "CAUGHT A FISH!";
        }

        // Tên con cá (Lấy trực tiếp từ itemName của FishSO giống như Sổ tay cá và Balo)
        if (txtFishName != null)
        {
            txtFishName.text = currentFishData.itemName;
        }

        // Độ hiếm (Rarity)
        if (txtRarityBadge != null)
        {
            string rText = "";
            Color rColor = Color.white;
            switch (currentFishData.rarity)
            {
                case FishRarity.Common:
                    rText = isVietnamese ? "PHỔ BIẾN" : "COMMON";
                    rColor = new Color(0.75f, 0.85f, 0.95f);
                    break;
                case FishRarity.Uncommon:
                    rText = isVietnamese ? "ÍT GẶP" : "UNCOMMON";
                    rColor = new Color(0.35f, 0.92f, 0.45f);
                    break;
                case FishRarity.Rare:
                    rText = isVietnamese ? "HIẾM" : "RARE";
                    rColor = new Color(0.28f, 0.75f, 1f);
                    break;
                case FishRarity.Legendary:
                    rText = isVietnamese ? "HUYỀN THOẠI" : "LEGENDARY";
                    rColor = new Color(1f, 0.78f, 0.15f);
                    break;
            }
            txtRarityBadge.text = $"[ {rText} ]";
            txtRarityBadge.color = rColor;
        }

        // Phẩm cấp (Grade: Thường / Đồng / Bạc / Vàng)
        if (txtGradeBadge != null)
        {
            string gText = "";
            Color gColor = Color.white;
            switch (currentGrade)
            {
                case FishGrade.Normal:
                    gText = isVietnamese ? "Hạng Thường" : "Normal Grade";
                    gColor = new Color(0.85f, 0.85f, 0.85f);
                    break;
                case FishGrade.Bronze:
                    gText = isVietnamese ? "Hạng Đồng" : "Bronze Grade";
                    gColor = new Color(0.88f, 0.58f, 0.32f);
                    break;
                case FishGrade.Silver:
                    gText = isVietnamese ? "Hạng Bạc" : "Silver Grade";
                    gColor = new Color(0.82f, 0.88f, 0.96f);
                    break;
                case FishGrade.Gold:
                    gText = isVietnamese ? "Hạng Vàng" : "Gold Grade";
                    gColor = new Color(1f, 0.86f, 0.22f);
                    break;
            }
            txtGradeBadge.text = gText;
            txtGradeBadge.color = gColor;
        }

        // Huy hiệu Kỷ Lục Mới
        if (newRecordBadge != null)
        {
            newRecordBadge.SetActive(isCurrentNewRecord);
        }

        // Chiều dài
        if (txtLength != null)
        {
            txtLength.text = isVietnamese 
                ? $"Chiều dài: <b><color=#FFEE77>{currentLength:F1} cm</color></b>" 
                : $"Length: <b><color=#FFEE77>{currentLength:F1} cm</color></b>";
        }

        // Cân nặng
        if (txtWeight != null)
        {
            txtWeight.text = isVietnamese 
                ? $"Cân nặng: <b><color=#FFEE77>{currentWeight:F1} kg</color></b>" 
                : $"Weight: <b><color=#FFEE77>{currentWeight:F1} kg</color></b>";
        }

        // Giá trị ước tính
        if (txtPrice != null)
        {
            int calculatedPrice = Mathf.RoundToInt(currentFishData.basePrice * (1f + ((int)currentGrade * 0.35f)));
            txtPrice.text = isVietnamese 
                ? $"Giá bán: <b><color=#55FF66>${calculatedPrice}</color></b>" 
                : $"Value: <b><color=#55FF66>${calculatedPrice}</color></b>";
        }

        // Vùng/Hồ câu
        if (txtLocation != null)
        {
            string loc = !string.IsNullOrEmpty(currentFishData.mapName) ? currentFishData.mapName : (isVietnamese ? "Hồ Pine Lake" : "Pine Lake");
            txtLocation.text = isVietnamese 
                ? $"Khu vực: <b><color=#88DDFF>{loc}</color></b>" 
                : $"Habitat: <b><color=#88DDFF>{loc}</color></b>";
        }
    }

    private void SpawnFish3DModel()
    {
        if (fishModelHolder == null) return;

        // Xóa model cũ an toàn
        if (spawnedModelInstance != null)
        {
            Destroy(spawnedModelInstance);
            spawnedModelInstance = null;
        }

        if (currentFishData == null) return;

        GameObject prefabToSpawn = currentFishData.caughtFishPrefab != null 
            ? currentFishData.caughtFishPrefab 
            : currentFishData.equippedModelPrefab;

        if (prefabToSpawn == null) return;

        spawnedModelInstance = Instantiate(prefabToSpawn, fishModelHolder);
        spawnedModelInstance.transform.localPosition = Vector3.zero;
        spawnedModelInstance.transform.localRotation = Quaternion.identity;

        // Vô hiệu hóa toàn bộ Collider & Rigidbody trên mô hình preview để tránh xung đột vật lý
        foreach (var col in spawnedModelInstance.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }
        foreach (var rb in spawnedModelInstance.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
        }

        // Đảm bảo model hiển thị đúng layer của Preview Camera
        int previewLayer = previewCamera != null ? previewCamera.gameObject.layer : 0;
        SetLayerRecursively(spawnedModelInstance, previewLayer);

        // Chuẩn hóa kích thước & trọng tâm mô hình để vừa vặn khung hình 3D
        NormalizeModelBounds(spawnedModelInstance);

        // Reset góc xoay
        targetRotationY = -30f;
        currentRotationY = -30f;
        targetRotationX = 8f;
        currentRotationX = 8f;
        floatTime = 0f;
    }

    private void NormalizeModelBounds(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        bool hasValidBound = false;
        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;
            if (!hasValidBound)
            {
                bounds = r.bounds;
                hasValidBound = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (maxDim > 0.001f)
        {
            float targetSize = 2.2f;
            float scaleFactor = targetSize / maxDim;
            model.transform.localScale = Vector3.one * scaleFactor;

            // Căn giữa pivot vào tâm hình học của mô hình
            Vector3 centerOffset = bounds.center - model.transform.position;
            model.transform.localPosition = -centerOffset * scaleFactor;
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child != null) SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void Setup3DPreviewStage()
    {
        if (previewStageHolder != null && previewCamera != null && fishRenderImage != null) return;

        // 1. Tạo RenderTexture chất lượng cao (512x512, 24-bit depth, anti-aliased)
        if (previewRenderTexture == null)
        {
            previewRenderTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            previewRenderTexture.antiAliasing = 4;
            previewRenderTexture.Create();
        }

        if (fishRenderImage != null)
        {
            fishRenderImage.texture = previewRenderTexture;
        }

        // 2. Tạo Sân khấu 3D độc lập ngoài tầm nhìn của camera chính (tọa độ ngầm Y = -5000)
        if (previewStageHolder == null)
        {
            GameObject stageObj = new GameObject("Fish3D_PreviewStage");
            stageObj.transform.position = new Vector3(0, -5000, 0);
            previewStageHolder = stageObj.transform;
            DontDestroyOnLoad(stageObj);

            // Tạo Turntable Pivot
            GameObject pivotObj = new GameObject("Fish_TurntablePivot");
            pivotObj.transform.SetParent(previewStageHolder, false);
            pivotObj.transform.localPosition = Vector3.zero;
            fishModelHolder = pivotObj.transform;

            // Tạo Camera Preview
            GameObject camObj = new GameObject("PreviewCamera", typeof(Camera));
            camObj.transform.SetParent(previewStageHolder, false);
            camObj.transform.localPosition = new Vector3(0, 0, -3.4f);
            camObj.transform.localRotation = Quaternion.identity;

            previewCamera = camObj.GetComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0, 0, 0, 0); // Nền trong suốt
            previewCamera.targetTexture = previewRenderTexture;
            previewCamera.fieldOfView = 42f;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 20f;

            // 3. Hệ thống Ánh Sáng Cục Bộ (Point Lights trắng tự nhiên, chỉ sáng trong bán kính 6m tại Y = -5000)
            GameObject keyLightObj = new GameObject("PreviewLight_Key", typeof(Light));
            keyLightObj.transform.SetParent(previewStageHolder, false);
            keyLightObj.transform.localPosition = new Vector3(1.5f, 2f, -2f);
            Light keyLight = keyLightObj.GetComponent<Light>();
            keyLight.type = LightType.Point;
            keyLight.range = 6f;
            keyLight.intensity = 2.5f;
            keyLight.color = Color.white;

            GameObject fillLightObj = new GameObject("PreviewLight_Fill", typeof(Light));
            fillLightObj.transform.SetParent(previewStageHolder, false);
            fillLightObj.transform.localPosition = new Vector3(-1.5f, -1f, -1.5f);
            Light fillLight = fillLightObj.GetComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.range = 6f;
            fillLight.intensity = 1.5f;
            fillLight.color = Color.white;
        }

        if (fishRenderImage != null && fishRenderImage.texture == null)
        {
            fishRenderImage.texture = previewRenderTexture;
        }
    }

    #region Mouse Drag Interaction
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        targetRotationY -= eventData.delta.x * rotationSpeed * 0.15f;
        targetRotationX += eventData.delta.y * rotationSpeed * 0.15f;
        targetRotationX = Mathf.Clamp(targetRotationX, -45f, 45f);
    }
    #endregion

    #region Action Buttons
    public void OnKeepClicked()
    {
        if (currentFishData == null) return;

        // Thử cất cá vào Balo
        BackpackMinigameUI backpack = BackpackMinigameUI.Instance;
        if (backpack == null)
        {
            backpack = Object.FindFirstObjectByType<BackpackMinigameUI>(FindObjectsInactive.Include);
        }

        if (backpack != null)
        {
            bool added = backpack.TryAutoAddFish(currentFishData, currentLength, currentWeight, currentGrade);
            if (added)
            {
                string fName = currentFishData.itemName;

                // Cập nhật Quest
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.NotifyFishCaught(currentFishData, currentLength, currentWeight, currentGrade);
                }

                if (soundKeepFish != null)
                {
                    AudioSource.PlayClipAtPoint(soundKeepFish, Camera.main != null ? Camera.main.transform.position : transform.position, 0.9f);
                }

                onKeepCallback?.Invoke();
                Hide();
            }
            else
            {
                // Thông báo Balo đầy không làm mất cá
                ShowBackpackFullWarning();
            }
        }
        else
        {
            onKeepCallback?.Invoke();
            Hide();
        }
    }

    private void ShowBackpackFullWarning()
    {
        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        string warnText = isVietnamese 
            ? "Balo đã đầy! Hãy dọn chỗ trống trong Balo hoặc chọn Thả Cá." 
            : "Backpack is full! Clean space or choose Release Fish.";

        if (txtBackpackWarning != null)
        {
            txtBackpackWarning.text = warnText;
            txtBackpackWarning.gameObject.SetActive(true);
            if (warningCoroutine != null) StopCoroutine(warningCoroutine);
            warningCoroutine = StartCoroutine(AnimateWarningShake());
        }

        FishingController fc = Object.FindFirstObjectByType<FishingController>();
        if (fc != null)
        {
            fc.ShowFishingFeedback(warnText, new Color(1f, 0.45f, 0.2f));
        }
    }

    private IEnumerator AnimateWarningShake()
    {
        if (txtBackpackWarning == null) yield break;
        RectTransform rt = txtBackpackWarning.GetComponent<RectTransform>();
        Vector2 originalPos = rt.anchoredPosition;

        for (int i = 0; i < 6; i++)
        {
            float offsetX = (i % 2 == 0 ? 8f : -8f);
            rt.anchoredPosition = originalPos + new Vector2(offsetX, 0);
            yield return new WaitForSecondsRealtime(0.04f);
        }
        rt.anchoredPosition = originalPos;
    }

    public void OnReleaseClicked()
    {
        if (currentFishData == null) return;

        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        FishingController fc = Object.FindFirstObjectByType<FishingController>();
        if (fc != null)
        {
            fc.ShowFishingFeedback(
                isVietnamese ? "Bạn đã thả con cá về với hồ nước!" : "You released the fish back to the lake!",
                new Color(0.4f, 0.85f, 1f)
            );
        }

        if (soundReleaseFish != null)
        {
            AudioSource.PlayClipAtPoint(soundReleaseFish, Camera.main != null ? Camera.main.transform.position : transform.position, 0.9f);
        }

        onReleaseCallback?.Invoke();
        Hide();
    }

    public void Hide()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateClose());
    }

    private void SetPlayerInputControl(bool enableGameplayInput)
    {
        // 1. Khóa PlayerInputHandler
        PlayerInputHandler input = Object.FindFirstObjectByType<PlayerInputHandler>();
        if (input != null) input.IsUIOpen = !enableGameplayInput;

        // 2. Khóa di chuyển nhân vật
        PlayerMovement movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement != null) movement.enabled = enableGameplayInput;

        // 3. Hiện con trỏ chuột khi mở UI
        PlayerCursor cursor = Object.FindFirstObjectByType<PlayerCursor>();
        if (cursor != null) cursor.SetCursorState(enableGameplayInput);

        // 4. Khóa xoay Camera Cinemachine triệt để (quét toàn bộ component Input Cinemachine trong Scene)
        MonoBehaviour[] allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var comp in allComponents)
        {
            if (comp == null) continue;
            string typeName = comp.GetType().Name;
            if (typeName == "CinemachineInputAxisController" || typeName == "CinemachineInputProvider")
            {
                comp.enabled = enableGameplayInput;
            }
        }
    }

    private IEnumerator AnimateOpen()
    {
        if (canvasGroup == null && panelRoot != null) canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (panelRect == null && panelRoot != null) panelRect = panelRoot.GetComponent<RectTransform>();

        float elapsed = 0f;
        float duration = 0.22f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (canvasGroup != null) canvasGroup.alpha = smoothT;
            if (panelRect != null) panelRect.localScale = Vector3.LerpUnclamped(new Vector3(0.85f, 0.85f, 1f), Vector3.one, smoothT);
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (panelRect != null) panelRect.localScale = Vector3.one;
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
            if (panelRect != null) panelRect.localScale = Vector3.LerpUnclamped(Vector3.one, new Vector3(0.9f, 0.9f, 1f), smoothT);
            yield return null;
        }

        if (panelRoot != null) panelRoot.SetActive(false);

        if (spawnedModelInstance != null)
        {
            Destroy(spawnedModelInstance);
            spawnedModelInstance = null;
        }

        // Trả lại quyền điều khiển
        SetPlayerInputControl(true);
    }
    #endregion

    /// <summary>
    /// Tự động tìm kiếm các phần tử UI bên trong FishViewingChart
    /// </summary>
    [ContextMenu("Tự Động Tìm Tham Chiếu Trong Hierarchy")]
    public void AutoFindReferences()
    {
        // 1. Tìm GameObject FishViewingChart
        if (panelRoot == null)
        {
            GameObject chartObj = GameObject.Find("FishViewingChart");
            if (chartObj != null)
            {
                panelRoot = chartObj;
            }
            else
            {
                panelRoot = gameObject;
            }
        }

        if (panelRoot != null)
        {
            panelRect = panelRoot.GetComponent<RectTransform>();
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = panelRoot.AddComponent<CanvasGroup>();

            // Tìm các text
            TextMeshProUGUI[] allTexts = panelRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                string n = t.gameObject.name.ToLower();
                if (txtTitleHeader == null && (n.Contains("header") || n.Contains("title"))) txtTitleHeader = t;
                else if (txtFishName == null && (n.Contains("name") || n.Contains("ten"))) txtFishName = t;
                else if (txtRarityBadge == null && (n.Contains("rarity") || n.Contains("hiem"))) txtRarityBadge = t;
                else if (txtGradeBadge == null && (n.Contains("grade") || n.Contains("hang") || n.Contains("cap"))) txtGradeBadge = t;
                else if (txtLength == null && (n.Contains("length") || n.Contains("dai") || n.Contains("size"))) txtLength = t;
                else if (txtWeight == null && (n.Contains("weight") || n.Contains("nang") || n.Contains("kg"))) txtWeight = t;
                else if (txtPrice == null && (n.Contains("price") || n.Contains("gia") || n.Contains("coin"))) txtPrice = t;
                else if (txtLocation == null && (n.Contains("location") || n.Contains("map") || n.Contains("vung") || n.Contains("ho"))) txtLocation = t;
                else if (txtBackpackWarning == null && (n.Contains("warn") || n.Contains("full") || n.Contains("canhbao"))) txtBackpackWarning = t;
            }

            // Tìm các Button
            Button[] allBtns = panelRoot.GetComponentsInChildren<Button>(true);
            foreach (var b in allBtns)
            {
                string n = b.gameObject.name.ToLower();
                if (btnKeepFish == null && (n.Contains("keep") || n.Contains("nhan") || n.Contains("cat") || n.Contains("bag") || n.Contains("balo")))
                {
                    btnKeepFish = b;
                }
                else if (btnReleaseFish == null && (n.Contains("release") || n.Contains("tha") || n.Contains("bo")))
                {
                    btnReleaseFish = b;
                }
            }

            // Tìm RawImage hiển thị cá 3D
            if (fishRenderImage == null)
            {
                fishRenderImage = panelRoot.GetComponentInChildren<RawImage>(true);
            }
        }
    }

    private void OnDestroy()
    {
        if (previewRenderTexture != null)
        {
            previewRenderTexture.Release();
            Destroy(previewRenderTexture);
        }
        if (previewStageHolder != null)
        {
            Destroy(previewStageHolder.gameObject);
        }
    }
}
