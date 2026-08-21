using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum PerspectiveMode
{
    FirstPerson,
    ThirdPerson
}

/// <summary>
/// DualCameraController:
/// - Chuyển đổi FPP <-> TPP siêu nhanh, mượt mà và liền mạch như PUBG (0.16s blend).
/// - FPP: FPPEyeTarget là con trực tiếp của playerTransform.
///   - Khi ĐI BỘ: Chiều cao fppStandOffset = (0, 1.58, 0.22).
///   - Khi NGỒI TRONG XE: Chiều cao fppCarOffset = (0, 0.62, 0.20).
///   - Mouse X xoay thân nhân vật, Mouse Y xoay ngẩng/cúi tâm mắt.
/// - TPP: Sử dụng CameraTargetPlayer (khi đi bộ) và CameraTargetCar (khi trên xe).
/// </summary>
public class DualCameraController : MonoBehaviour
{
    public static DualCameraController Instance { get; private set; }

    [Header("--- Player References ---")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("--- Cinemachine Cameras ---")]
    [SerializeField] private CinemachineCamera tppPlayerCamera;   // CameraTargetPlayer (Cinemachine)
    [SerializeField] private CinemachineCamera tppCarCamera;      // CameraTargetCar (Cinemachine)
    [SerializeField] private CinemachineCamera fppCamera;         // CameraFPP (Cinemachine)

    [Header("--- PUBG-Style Instant Transition ---")]
    [Tooltip("Chuyển đổi góc nhìn FPP <-> TPP tức thì không có độ trễ (0 giây chuẩn PUBG)")]
    [SerializeField] private float transitionDuration = 0f;
    [SerializeField] private CinemachineBlendDefinition.Styles transitionStyle = CinemachineBlendDefinition.Styles.Cut;

    [Header("--- FPP Eye Target Offsets ---")]
    [Tooltip("Tọa độ tâm mắt FPP khi ĐI BỘ (X giữa, Y tầm mắt đứng, Z nhô ra trước)")]
    [SerializeField] private Vector3 fppStandOffset = new Vector3(0f, 1.58f, 0.22f);

    [Tooltip("Tọa độ tâm mắt FPP khi NGỒI TRONG XE (X giữa, Y tầm mắt ngồi ghế lái, Z nhô ra trước)")]
    [SerializeField] private Vector3 fppCarOffset = new Vector3(0f, 0.62f, 0.20f);

    [SerializeField] private Transform fppEyeTarget;

    [Header("--- FPP Mouse Look ---")]
    [SerializeField] private float fppMouseSensitivity = 1.8f;
    [SerializeField] private float fppTiltMin = -75f;
    [SerializeField] private float fppTiltMax = 75f;
    [SerializeField] private float fppFov = 75f;

    [Header("--- Priority Values ---")]
    [SerializeField] private int priorityHigh = 20;
    [SerializeField] private int priorityLow = 0;

    [Header("--- Perspective Settings ---")]
    [SerializeField] private PerspectiveMode currentMode = PerspectiveMode.ThirdPerson;
    [SerializeField] private KeyCode togglePerspectiveKey = KeyCode.Y;

    [Header("--- Mesh Visibility ---")]
    [SerializeField] private Renderer[] headRenderers;
    [SerializeField] private Renderer[] allPlayerRenderers;
    [SerializeField] private ShadowCastingMode fppShadowMode = ShadowCastingMode.ShadowsOnly;
    [SerializeField] private ShadowCastingMode tppShadowMode = ShadowCastingMode.On;

    public PerspectiveMode CurrentMode => currentMode;
    public bool IsInVehicle => isInVehicle;
    public Vector3 CurrentFppOffset => isInVehicle ? fppCarOffset : fppStandOffset;

    private bool isInVehicle = false;
    private float lastToggleTime = -1f;
    private const float TOGGLE_COOLDOWN = 0.1f;

    // FPP on-foot pitch
    private float fppPitch = 0f;

    // FPP in-car yaw & pitch
    private float fppCarYaw = 0f;
    private float fppCarPitch = 0f;

    private CinemachineBrain mainBrain;
    private CinemachineHardLockToTarget fppHardLock;
    private CinemachineInputAxisController tppPlayerInputCtrl;
    private CinemachineOrbitalFollow tppOrbitalFollow;
    private Coroutine meshVisibilityCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(this); return; }

        AutoFindReferences();
        EnsureFPPEyeAndCamera();
        ConfigureBrainBlend();
    }

    private void Start()
    {
        AutoFindReferences();
        EnsureFPPEyeAndCamera();
        ConfigureBrainBlend();
        ApplyPerspectiveMode(currentMode);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AutoFindReferences();
        EnsureFPPEyeAndCamera();
        ConfigureBrainBlend();
        ApplyPerspectiveMode(currentMode);
    }

    private void ConfigureBrainBlend()
    {
        if (mainBrain == null && Camera.main != null)
        {
            mainBrain = Camera.main.GetComponent<CinemachineBrain>();
        }

        if (mainBrain != null)
        {
            mainBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
        }
    }

    private void Update()
    {
        if (inputHandler != null && inputHandler.IsUIOpen) return;
        bool isDialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;
        if (isDialogueActive) return;

        // Phím nóng Y chuyển đổi góc nhìn nhanh
        bool yPressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            yPressed = Keyboard.current.yKey.wasPressedThisFrame;
        }
#endif
        if (!yPressed)
        {
            yPressed = Input.GetKeyDown(togglePerspectiveKey);
        }

        if (yPressed && (Time.unscaledTime - lastToggleTime > TOGGLE_COOLDOWN))
        {
            lastToggleTime = Time.unscaledTime;
            TogglePerspective();
        }

        // Xử lý xoay chuột chuẩn FPS khi ở chế độ FPP
        if (currentMode == PerspectiveMode.FirstPerson)
        {
            HandleFPPMouseLook();
        }
    }

    private void HandleFPPMouseLook()
    {
        Vector2 mouseDelta = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue() * 0.1f;
        }
#endif
        if (mouseDelta == Vector2.zero)
        {
            mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        }

        float mouseX = mouseDelta.x * fppMouseSensitivity;
        float mouseY = mouseDelta.y * fppMouseSensitivity;

        if (!isInVehicle)
        {
            // 1. Đi bộ ngoài xe (Standard FPS):
            // - Mouse X xoay trực tiếp thân nhân vật theo trục Y (World)
            if (playerTransform != null && Mathf.Abs(mouseX) > 0.0001f)
            {
                playerTransform.Rotate(0f, mouseX, 0f, Space.World);
            }

            // - Mouse Y xoay góc ngẩng/cúi (Pitch) của FPPEyeTarget
            fppPitch = Mathf.Clamp(fppPitch - mouseY, fppTiltMin, fppTiltMax);

            if (fppEyeTarget != null)
            {
                fppEyeTarget.localPosition = CurrentFppOffset;
                fppEyeTarget.localRotation = Quaternion.Euler(fppPitch, 0f, 0f);
            }
        }
        else
        {
            // 2. Lái xe trong xe (In-Car Cockpit):
            // - Nhìn quanh tự do trong khoang lái (Pan & Tilt)
            fppCarYaw = Mathf.Clamp(fppCarYaw + mouseX, -110f, 110f);
            fppCarPitch = Mathf.Clamp(fppCarPitch - mouseY, -60f, 60f);

            if (fppEyeTarget != null)
            {
                fppEyeTarget.localPosition = CurrentFppOffset;
                fppEyeTarget.localRotation = Quaternion.Euler(fppCarPitch, fppCarYaw, 0f);
            }
        }
    }

    public void TogglePerspective()
    {
        SetPerspectiveMode(currentMode == PerspectiveMode.FirstPerson ? PerspectiveMode.ThirdPerson : PerspectiveMode.FirstPerson);
        ForcedTutorialManager.Instance?.NotifyPerspectiveToggled();
    }

    public void SetPerspectiveMode(PerspectiveMode newMode)
    {
        currentMode = newMode;
        ApplyPerspectiveMode(currentMode);
    }

    public void SetInVehicle(bool inVehicle)
    {
        isInVehicle = inVehicle;
        AutoFindReferences();
        EnsureFPPEyeAndCamera();
        ConfigureBrainBlend();
        ApplyPerspectiveMode(currentMode);
    }

    private void ApplyPerspectiveMode(PerspectiveMode mode)
    {
        bool isFpp = (mode == PerspectiveMode.FirstPerson);

        ConfigureBrainBlend();

        if (isFpp)
        {
            // Chuyển sang FPP: Đồng bộ góc nhìn chính xác từ TPP sang FPP (cắt tức thì 0s chuẩn PUBG)
            if (!isInVehicle)
            {
                if (Camera.main != null && playerTransform != null)
                {
                    float currentCamYaw = Camera.main.transform.eulerAngles.y;
                    float currentCamPitch = Camera.main.transform.eulerAngles.x;
                    if (currentCamPitch > 180f) currentCamPitch -= 360f;

                    playerTransform.rotation = Quaternion.Euler(0f, currentCamYaw, 0f);
                    fppPitch = Mathf.Clamp(currentCamPitch, fppTiltMin, fppTiltMax);

                    if (fppEyeTarget != null)
                    {
                        fppEyeTarget.localPosition = CurrentFppOffset;
                        fppEyeTarget.localRotation = Quaternion.Euler(fppPitch, 0f, 0f);
                    }

                    if (fppCamera != null)
                    {
                        fppCamera.ForceCameraPosition(fppEyeTarget.position, fppEyeTarget.rotation);
                    }
                }
            }
            else
            {
                fppCarYaw = 0f;
                fppCarPitch = 0f;
                if (fppEyeTarget != null)
                {
                    fppEyeTarget.localPosition = CurrentFppOffset;
                    fppEyeTarget.localRotation = Quaternion.identity;
                }
            }

            // FPP: CameraFPP ưu tiên cao nhất
            SetCamPriority(fppCamera, priorityHigh);
            SetCamPriority(tppPlayerCamera, priorityLow);
            SetCamPriority(tppCarCamera, priorityLow);

            // Tắt input controller của TPP
            if (tppPlayerInputCtrl != null) tppPlayerInputCtrl.enabled = false;
        }
        else // TPP
        {
            // Chuyển sang TPP: Đồng bộ camera TPP nằm CHÍNH XÁC ngay sau lưng nhân vật theo hướng FPP vừa nhìn
            if (!isInVehicle)
            {
                float currentYaw = playerTransform != null ? playerTransform.eulerAngles.y : 0f;
                if (currentYaw > 180f) currentYaw -= 360f;

                if (tppOrbitalFollow != null)
                {
                    var horizontal = tppOrbitalFollow.HorizontalAxis;
                    horizontal.Value = currentYaw; // Đồng bộ World Yaw chuẩn xác
                    tppOrbitalFollow.HorizontalAxis = horizontal;

                    var vertical = tppOrbitalFollow.VerticalAxis;
                    vertical.Value = Mathf.Clamp(fppPitch, vertical.Range.x, vertical.Range.y);
                    tppOrbitalFollow.VerticalAxis = vertical;
                }
            }

            // TPP: CameraFPP ưu tiên thấp
            SetCamPriority(fppCamera, priorityLow);

            if (!isInVehicle)
            {
                // Đi bộ: CameraTargetPlayer nhận quyền điều khiển
                SetCamPriority(tppPlayerCamera, priorityHigh);
                SetCamPriority(tppCarCamera, priorityLow);

                if (tppPlayerCamera != null && !tppPlayerCamera.gameObject.activeSelf)
                    tppPlayerCamera.gameObject.SetActive(true);

                if (tppPlayerInputCtrl != null) tppPlayerInputCtrl.enabled = true;
            }
            else
            {
                // Lái xe: CameraTargetCar nhận quyền điều khiển
                SetCamPriority(tppPlayerCamera, priorityLow);
                SetCamPriority(tppCarCamera, priorityHigh);

                if (tppCarCamera != null && !tppCarCamera.gameObject.activeSelf)
                    tppCarCamera.gameObject.SetActive(true);
            }
        }

        ApplyMeshVisibility(mode);
    }

    private void EnsureFPPEyeAndCamera()
    {
        if (playerTransform == null)
        {
            AutoFindReferences();
            if (playerTransform == null) return;
        }

        // 1. Tạo FPPEyeTarget là con trực tiếp của playerTransform
        if (fppEyeTarget == null)
        {
            Transform existingEye = playerTransform.Find("FPPEyeTarget");
            if (existingEye != null)
            {
                fppEyeTarget = existingEye;
            }
            else
            {
                GameObject eyeObj = new GameObject("FPPEyeTarget");
                eyeObj.transform.SetParent(playerTransform, false);
                eyeObj.transform.localPosition = CurrentFppOffset;
                eyeObj.transform.localRotation = Quaternion.identity;
                fppEyeTarget = eyeObj.transform;
            }
        }
        else
        {
            if (fppEyeTarget.parent != playerTransform)
            {
                fppEyeTarget.SetParent(playerTransform, false);
            }
            fppEyeTarget.localPosition = CurrentFppOffset;
        }

        // 2. Tạo CameraFPP là con của FPPEyeTarget (hoặc playerTransform)
        if (fppCamera == null)
        {
            Transform existingCam = playerTransform.Find("CameraFPP") ?? fppEyeTarget.Find("CameraFPP");
            if (existingCam != null)
            {
                fppCamera = existingCam.GetComponent<CinemachineCamera>();
            }
            else
            {
                GameObject camObj = new GameObject("CameraFPP");
                camObj.transform.SetParent(fppEyeTarget, false);
                camObj.transform.localPosition = Vector3.zero;
                camObj.transform.localRotation = Quaternion.identity;
                fppCamera = camObj.AddComponent<CinemachineCamera>();
            }
        }
        else
        {
            if (fppCamera.transform.parent != fppEyeTarget && fppCamera.transform.parent != playerTransform)
            {
                fppCamera.transform.SetParent(fppEyeTarget, false);
                fppCamera.transform.localPosition = Vector3.zero;
                fppCamera.transform.localRotation = Quaternion.identity;
            }
        }

        // 3. Cấu hình components cho CameraFPP
        if (fppCamera != null)
        {
            fppCamera.Target.TrackingTarget = fppEyeTarget;
            fppCamera.Target.LookAtTarget = null;

            var lens = fppCamera.Lens;
            lens.FieldOfView = fppFov;
            lens.NearClipPlane = 0.01f;
            fppCamera.Lens = lens;

            // Xóa PanTilt cũ nếu có để tránh xung đột xoay
            var oldPanTilt = fppCamera.GetComponent<CinemachinePanTilt>();
            if (oldPanTilt != null) Destroy(oldPanTilt);

            // HardLockToTarget: Khóa cứng vị trí và xoay theo FPPEyeTarget
            fppHardLock = fppCamera.GetComponent<CinemachineHardLockToTarget>();
            if (fppHardLock == null)
                fppHardLock = fppCamera.gameObject.AddComponent<CinemachineHardLockToTarget>();
        }
    }

    private void SetCamPriority(CinemachineCamera cam, int priority)
    {
        if (cam != null)
        {
            cam.Priority = new PrioritySettings { Enabled = true, Value = priority };
        }
    }

    private void AutoFindReferences()
    {
        if (playerTransform == null)
        {
            PlayerMovement pm = GetComponentInParent<PlayerMovement>() ?? FindFirstObjectByType<PlayerMovement>();
            playerTransform = pm != null ? pm.transform : transform.root;
        }

        if (inputHandler == null && playerTransform != null)
            inputHandler = playerTransform.GetComponent<PlayerInputHandler>();

        if (playerMovement == null && playerTransform != null)
            playerMovement = playerTransform.GetComponent<PlayerMovement>();

        if (mainBrain == null && Camera.main != null)
            mainBrain = Camera.main.GetComponent<CinemachineBrain>();

        CinemachineCamera[] allCMs = Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var cm in allCMs)
        {
            if (cm == null) continue;
            string n = cm.gameObject.name;

            if (n.Equals("CameraTargetPlayer", System.StringComparison.OrdinalIgnoreCase))
            {
                tppPlayerCamera = cm;
                tppPlayerInputCtrl = cm.GetComponent<CinemachineInputAxisController>();
                tppOrbitalFollow = cm.GetComponent<CinemachineOrbitalFollow>();
            }
            else if (n.Equals("CameraTargetCar", System.StringComparison.OrdinalIgnoreCase))
            {
                tppCarCamera = cm;
            }
            else if (n.Equals("CameraFPP", System.StringComparison.OrdinalIgnoreCase))
            {
                fppCamera = cm;
                fppHardLock = cm.GetComponent<CinemachineHardLockToTarget>();
            }
        }

        if ((allPlayerRenderers == null || allPlayerRenderers.Length == 0) && playerTransform != null)
            allPlayerRenderers = playerTransform.GetComponentsInChildren<Renderer>(true);

        if ((headRenderers == null || headRenderers.Length == 0) && allPlayerRenderers != null)
        {
            var list = new System.Collections.Generic.List<Renderer>();
            foreach (var r in allPlayerRenderers)
            {
                if (r == null) continue;
                string ln = r.gameObject.name.ToLower();
                if (ln.Contains("head") || ln.Contains("hair") || ln.Contains("face") || ln.Contains("hat") || ln.Contains("beard"))
                    list.Add(r);
            }
            headRenderers = list.ToArray();
        }
    }

    private void ApplyMeshVisibility(PerspectiveMode mode)
    {
        if (allPlayerRenderers == null || allPlayerRenderers.Length == 0)
            AutoFindReferences();

        if (mode == PerspectiveMode.FirstPerson)
        {
            // FPP: Ẩn phần đầu ngay lập tức
            if (headRenderers != null)
            {
                foreach (var r in headRenderers)
                {
                    if (r != null) r.shadowCastingMode = fppShadowMode;
                }
            }
        }
        else
        {
            // TPP: Hiện lại toàn bộ cơ thể và đầu tức thì
            if (allPlayerRenderers != null)
            {
                foreach (var r in allPlayerRenderers)
                {
                    if (r != null) r.shadowCastingMode = tppShadowMode;
                }
            }
        }
    }

    /// <summary>
    /// Dịch chuyển camera tức thì theo vị trí mới của nhân vật/xe, tránh hiệu ứng camera bay vút qua map
    /// </summary>
    public void WarpCameraToTarget()
    {
        AutoFindReferences();
        if (mainBrain == null && Camera.main != null)
        {
            mainBrain = Camera.main.GetComponent<CinemachineBrain>();
        }

        if (playerTransform != null)
        {
            if (tppPlayerCamera != null)
            {
                tppPlayerCamera.OnTargetObjectWarped(playerTransform, Vector3.zero);
            }
            if (fppCamera != null && fppEyeTarget != null)
            {
                fppCamera.OnTargetObjectWarped(fppEyeTarget, Vector3.zero);
            }
        }

        VehicleController vc = UnityEngine.Object.FindFirstObjectByType<VehicleController>(FindObjectsInactive.Include);
        if (vc != null && tppCarCamera != null)
        {
            tppCarCamera.OnTargetObjectWarped(vc.transform, Vector3.zero);
        }

        ApplyPerspectiveMode(currentMode);
    }
}
