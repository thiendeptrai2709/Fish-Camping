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

    [Header("Các bộ phận cần tự động đóng")]
    [SerializeField] private InteractableHood interactableHood;
    [SerializeField] private InteractableTrunk interactableTrunk;
    [SerializeField] private EngineRepairMinigame engineRepair;

    [Header("Sự kiện ra vào xe")]
    public UnityEngine.Events.UnityEvent OnEnteredVehicle;
    public UnityEngine.Events.UnityEvent OnExitedVehicle;

    private CarInputActions inputActions;
    private Transform currentExitPoint;
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

        // Lắng nghe sự kiện bấm phím E để xuống xe
        inputActions.Gameplay.ExitVehicle.performed += _ => TryExitVehicle();

        // Mặc định khi mới vào game: Người chưa lên xe thì TẮT điều khiển và TẮT camera xe
        if (vehicleInput != null) vehicleInput.enabled = false;
        if (carCamera != null) carCamera.SetActive(false);
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
    private void TogglePlayerPhysics(bool state)
    {
        if (playerMovement != null) playerMovement.enabled = state;
        if (playerInputHandler != null) playerInputHandler.enabled = state;
        if (playerInteraction != null) playerInteraction.enabled = state;

        if (playerCollider != null) playerCollider.enabled = state;
        if (playerRigidbody != null) playerRigidbody.isKinematic = !state;
    }
    public void EnterVehicle(Transform doorExitPoint)
    {
        if (isInCar) return;

        if (engineRepair != null && engineRepair.IsEngineOut) engineRepair.ExitRepairMode();
        if (interactableHood != null) interactableHood.ForceClose();
        if (interactableTrunk != null) interactableTrunk.ForceClose();

        currentExitPoint = doorExitPoint;
        isInCar = true;

        if (playerUI != null) playerUI.SetActive(false);

        TogglePlayerPhysics(false);

        // Snap ngay lập tức vào ghế lái
        playerObject.transform.SetParent(driverSeatPoint);
        playerObject.transform.localPosition = Vector3.zero;
        playerObject.transform.localRotation = Quaternion.identity;

        if (playerAnimation != null)
        {
            playerAnimation.SetDrivingState(true);
        }

        playerCamera.SetActive(false);
        carCamera.SetActive(true);
        vehicleInput.enabled = true;

        OnEnteredVehicle?.Invoke();
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

        // Snap ngay lập tức ra ngoài cửa xe
        playerObject.transform.SetParent(null);
        playerObject.transform.position = currentExitPoint.position;
        playerObject.transform.rotation = currentExitPoint.rotation;

        TogglePlayerPhysics(true);

        carCamera.SetActive(false);
        playerCamera.SetActive(true);
        if (playerUI != null) playerUI.SetActive(true);

        OnExitedVehicle?.Invoke();
    }
}