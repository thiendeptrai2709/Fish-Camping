using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleEnterExit : MonoBehaviour
{
    [Header("Dữ liệu Người Chơi")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject playerCamera; // TPC_Camera của người chơi
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private Collider playerCollider; // Tắt collider nhân vật
    [SerializeField] private Rigidbody playerRigidbody; // (Tùy chọn) Bật isKinematic để ko bị trôi

    // Cache lại các script của player để tự động tắt/bật
    private PlayerMovement playerMovement;
    private PlayerInputHandler playerInputHandler;
    private PlayerInteraction playerInteraction;// (Tùy chọn) Bật isKinematic để ko bị trôi

    [Header("Dữ liệu Xe")]
    [SerializeField] private Transform driverSeatPoint; // Vị trí ghế lái trong xe
    [SerializeField] private VehicleInput vehicleInput; // Script đọc Input của xe bạn đã viết
    [SerializeField] private VehicleController vehicleController; // Thêm tham chiếu đến Controller để lấy tốc độ
    [SerializeField] private GameObject carCamera; // Cinemachine Camera riêng của xe
    [SerializeField] private GameObject playerUI;
    [SerializeField] private CarRadio myCarRadio;

    [Header("Các bộ phận cần tự động đóng")]
    [SerializeField] private InteractableHood interactableHood;
    [SerializeField] private InteractableTrunk interactableTrunk;
    [SerializeField] private EngineRepairMinigame engineRepair;
    [SerializeField] private VehicleStats vehicleStats;
    [SerializeField] private CarFuel carFuel;


    [Header("Sự kiện ra vào xe")]
    public UnityEngine.Events.UnityEvent OnEnteredVehicle;
    public UnityEngine.Events.UnityEvent OnExitedVehicle;

    private CarInputActions inputActions;
    private Transform currentExitPoint;
    public bool IsInCar => isInCar;
    private bool isInCar = false;

    private void Awake()
    {
        inputActions = new CarInputActions();

        // Tự động lấy các script từ Player mà không cần kéo thả tay
        if (playerObject != null)
        {
            playerMovement = playerObject.GetComponent<PlayerMovement>();
            playerInputHandler = playerObject.GetComponent<PlayerInputHandler>();
            playerInteraction = playerObject.GetComponent<PlayerInteraction>();
        }

        // Mặc định khi mới vào game: Người chưa lên xe thì TẮT điều khiển và TẮT camera xe
        if (vehicleInput != null) vehicleInput.enabled = false;
        if (carCamera != null) carCamera.SetActive(false);
    }

    private void OnEnable()
    {
        inputActions.Enable();
        // Đăng ký lắng nghe sự kiện
        inputActions.Gameplay.ExitVehicle.performed += OnExitVehiclePerformed;
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh bị rò rỉ bộ nhớ hoặc gọi nhầm khi đổi Scene
        inputActions.Gameplay.ExitVehicle.performed -= OnExitVehiclePerformed;
        inputActions.Disable();
    }

    private void OnExitVehiclePerformed(InputAction.CallbackContext context)
    {
        TryExitVehicle();
    }
    private void TogglePlayerPhysics(bool state)
    {
        if (playerMovement != null) playerMovement.enabled = state;
        if (playerInputHandler != null) playerInputHandler.enabled = state;
        if (playerInteraction != null) playerInteraction.enabled = state;

        if (playerCollider != null) playerCollider.enabled = state;
        if (playerRigidbody != null) playerRigidbody.isKinematic = !state;
    }
    public void ForceEnterVehicleAndMove(Transform targetSpawn)
    {
        StartCoroutine(DelayPhysicsRoutine(targetSpawn));
    }

    private System.Collections.IEnumerator DelayPhysicsRoutine(Transform targetSpawn)
    {
        // Lấy trực tiếp Rigidbody của xe, KHÔNG dùng chữ transform.root nữa
        Rigidbody carRb = GetComponent<Rigidbody>();
        if (carRb != null)
        {
            carRb.isKinematic = true;
            carRb.linearVelocity = Vector3.zero;
            carRb.angularVelocity = Vector3.zero;
        }

        transform.position = targetSpawn.position;
        transform.rotation = targetSpawn.rotation;
        Physics.SyncTransforms();

        if (!isInCar)
        {
            // Tự động tìm cửa xe để lấy đúng điểm xuống xe thay vì dùng targetSpawn
            CarDoor carDoor = GetComponentInChildren<CarDoor>();
            Transform correctExitPoint = (carDoor != null) ? carDoor.ExitPoint : null;

            EnterVehicle(correctExitPoint);
        }

        // Chờ 0.5s để địa hình load xong Collider rồi mới nhả trọng lực cho xe
        yield return new WaitForSeconds(0.5f);

        if (carRb != null)
        {
            carRb.isKinematic = false;
        }
    }
    public void EnterVehicle(Transform doorExitPoint)
    {
        if (isInCar) return;

        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            if (playerMovement == null) playerMovement = playerObject.GetComponent<PlayerMovement>();
            if (playerInputHandler == null) playerInputHandler = playerObject.GetComponent<PlayerInputHandler>();
            if (playerInteraction == null) playerInteraction = playerObject.GetComponent<PlayerInteraction>();
            if (playerAnimation == null) playerAnimation = playerObject.GetComponent<PlayerAnimation>();
            if (playerCollider == null) playerCollider = playerObject.GetComponent<Collider>();
            if (playerRigidbody == null) playerRigidbody = playerObject.GetComponent<Rigidbody>();
        }

        if (driverSeatPoint == null)
            driverSeatPoint = transform;

        if (engineRepair != null && engineRepair.IsEngineOut) engineRepair.ExitRepairMode();
        if (interactableHood != null) interactableHood.ForceClose();
        if (interactableTrunk != null) interactableTrunk.ForceClose();
        if (vehicleStats != null) vehicleStats.CloseOverviewPanel();

        currentExitPoint = doorExitPoint;
        isInCar = true;

        if (playerUI != null) playerUI.SetActive(false);

        TogglePlayerPhysics(false);

        // Snap ngay lập tức vào ghế lái
        if (playerObject != null)
        {
            playerObject.transform.SetParent(driverSeatPoint);
            playerObject.transform.localPosition = Vector3.zero;
            playerObject.transform.localRotation = Quaternion.identity;
        }

        if (playerAnimation != null)
        {
            playerAnimation.SetDrivingState(true);
        }

        playerCamera.SetActive(false);
        carCamera.SetActive(true);
        vehicleInput.enabled = true;
        vehicleInput.IsUIOpen = false;

        OnEnteredVehicle?.Invoke();
        if (myCarRadio != null) myCarRadio.PlayerEnteredCar();
        if (carFuel != null) carFuel.PlayerEnterCar();
    }

    private void TryExitVehicle()
    {
        if (!isInCar) return;

        if (vehicleController != null && Mathf.Abs(vehicleController.GetCurrentSpeedKmh()) > 1f)
        {
            Debug.Log("Xe phải dừng hẳn mới có thể xuống!");
            return;
        }

        isInCar = false;
        vehicleInput.enabled = false;

        if (playerAnimation != null)
        {
            playerAnimation.SetDrivingState(false);
        }

        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerObject.transform.SetParent(null);

            // Kiểm tra xem điểm thoát hiểm có còn tồn tại (hoặc có bị đổi scene xóa mất không)
            if (currentExitPoint != null)
            {
                playerObject.transform.position = currentExitPoint.position;
                playerObject.transform.rotation = currentExitPoint.rotation;
            }
            else
            {
                // Nếu không có, đặt nhân vật đứng ngay cạnh xe để tránh lỗi
                playerObject.transform.position = transform.position + transform.right * 2f;
            }
        }

        TogglePlayerPhysics(true);

        carCamera.SetActive(false);
        playerCamera.SetActive(true);
        if (playerUI != null) playerUI.SetActive(true);

        OnExitedVehicle?.Invoke();
        ForcedTutorialManager.Instance?.NotifyExitedVehicle();
        if (myCarRadio != null) myCarRadio.PlayerExitedCar();
        if (carFuel != null) carFuel.PlayerExitCar();
    }

    public void ForceExitVehicle()
    {
        if (!isInCar) return;

        isInCar = false;
        if (vehicleInput != null) vehicleInput.enabled = false;

        if (playerAnimation != null)
        {
            playerAnimation.SetDrivingState(false);
        }

        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerObject.transform.SetParent(null);
            if (currentExitPoint != null)
            {
                playerObject.transform.position = currentExitPoint.position;
                playerObject.transform.rotation = currentExitPoint.rotation;
            }
            else
            {
                playerObject.transform.position = transform.position + transform.right * 2f;
            }
        }

        TogglePlayerPhysics(true);

        if (carCamera != null) carCamera.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);
        if (playerUI != null) playerUI.SetActive(true);

        OnExitedVehicle?.Invoke();
        if (myCarRadio != null) myCarRadio.PlayerExitedCar();
        if (carFuel != null) carFuel.PlayerExitCar();
    }
}