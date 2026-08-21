using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(VehicleInput))]
public class VehicleController : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] private float maxSpeedKmh = 80f;
    [SerializeField] private float motorTorque = 1500f;
    [SerializeField] private float brakeTorque = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float idleBrakeTorque = 400f;
    [SerializeField] private float lowSpeedPunch = 1.6f;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheel;
    [SerializeField] private WheelCollider frontRightWheel;
    [SerializeField] private WheelCollider rearLeftWheel;
    [SerializeField] private WheelCollider rearRightWheel;

    [Header("Wheel Transforms")]
    [SerializeField] private Transform frontLeftTransform;
    [SerializeField] private Transform frontRightTransform;
    [SerializeField] private Transform rearLeftTransform;
    [SerializeField] private Transform rearRightTransform;

    [SerializeField] private Transform centerOfMass;

    [Header("Steering Wheel")]
    [SerializeField] private Transform steeringWheel;
    [Tooltip("Trục xoay của vô lăng")]
    [SerializeField] private Vector3 steeringAxis = Vector3.forward;
    [Tooltip("Góc đánh lái tối đa của vô lăng (độ). 90 - 120 độ tạo cảm giác chân thực")]
    [SerializeField] private float maxSteeringWheelAngle = 100f;
    [Tooltip("Thời gian làm mượt đánh lái và trả lái (giây). Càng nhỏ càng nhanh, 0.14s êm ái chuẩn xe thật")]
    [SerializeField] private float steeringSmoothTime = 0.14f;
    [Tooltip("Độ dời tâm xoay thủ công nếu cần tinh chỉnh thêm")]
    [SerializeField] private Vector3 customPivotOffset = Vector3.zero;

    [Header("Hand Grips (IK)")]
    [Tooltip("Khoảng cách mở rộng của 2 tay (độ rộng bám vô lăng)")]
    [SerializeField] private float handGripWidth = 0.16f;
    [Tooltip("Độ cao của 2 tay trên vô lăng (Y)")]
    [SerializeField] private float handGripHeight = 0.04f;
    [Tooltip("Độ nông/sâu tiến lùi của tay trên vô lăng (Z)")]
    [SerializeField] private float handGripForward = -0.02f;
    [Tooltip("Góc nghiêng xoay bàn tay khi cầm vô lăng")]
    [SerializeField] private float handGripAngle = 25f;

    [SerializeField] private Transform leftHandGrip;
    [SerializeField] private Transform rightHandGrip;

    public Transform LeftHandGrip => leftHandGrip;
    public Transform RightHandGrip => rightHandGrip;

    private Transform steeringPivot;
    private Quaternion initialSteeringWheelRotation;
    private float currentSteeringWheelAngle = 0f;
    private float steeringWheelVelocity = 0f;

    private Rigidbody rb;
    private VehicleInput vehicleInput;
    private float currentSpeedKmh;

    private CarFuel carFuel;
    private CarLightController carLightController; // Thêm tham chiếu đến Đèn xe

    // Lưu trữ thông số ma sát gốc của 4 bánh xe để điều chỉnh theo độ bám của lốp
    private WheelFrictionCurve baseFwdFL, baseSideFL;
    private WheelFrictionCurve baseFwdFR, baseSideFR;
    private WheelFrictionCurve baseFwdRL, baseSideRL;
    private WheelFrictionCurve baseFwdRR, baseSideRR;
    private bool hasCachedBaseFriction = false;
    private float currentSteerBias = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        vehicleInput = GetComponent<VehicleInput>();
        carFuel = GetComponent<CarFuel>();
        carLightController = GetComponent<CarLightController>(); // Tự động tìm bộ điều khiển đèn

        if (vehicleInput == null)
        {
            vehicleInput = gameObject.AddComponent<VehicleInput>();
        }
        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
        }

        CacheBaseFriction();

        // Tự động tìm vô lăng và tạo tâm xoay hình học chuẩn xác
        SetupSteeringWheelPivot();
    }

    private void CacheBaseFriction()
    {
        if (hasCachedBaseFriction) return;
        if (frontLeftWheel != null) { baseFwdFL = frontLeftWheel.forwardFriction; baseSideFL = frontLeftWheel.sidewaysFriction; }
        if (frontRightWheel != null) { baseFwdFR = frontRightWheel.forwardFriction; baseSideFR = frontRightWheel.sidewaysFriction; }
        if (rearLeftWheel != null) { baseFwdRL = rearLeftWheel.forwardFriction; baseSideRL = rearLeftWheel.sidewaysFriction; }
        if (rearRightWheel != null) { baseFwdRR = rearRightWheel.forwardFriction; baseSideRR = rearRightWheel.sidewaysFriction; }
        hasCachedBaseFriction = true;
    }

    private void SetupSteeringWheelPivot()
    {
        if (steeringWheel == null)
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            foreach (var t in allChildren)
            {
                if (t.name.Equals("SteeringWheel", System.StringComparison.OrdinalIgnoreCase) ||
                    t.name.Equals("Steering_Wheel", System.StringComparison.OrdinalIgnoreCase))
                {
                    steeringWheel = t;
                    break;
                }
            }
        }

        if (steeringWheel != null)
        {
            MeshFilter mf = steeringWheel.GetComponent<MeshFilter>();
            Vector3 meshCenterOffset = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.bounds.center : Vector3.zero;
            Vector3 totalCenterOffset = meshCenterOffset + customPivotOffset;

            // Nếu tâm của Mesh bị lệch so với Transform gốc, tạo Pivot đặt đúng tâm hình học
            if (totalCenterOffset.sqrMagnitude > 0.00001f)
            {
                GameObject pivotObj = new GameObject("SteeringWheel_Pivot");
                pivotObj.transform.SetParent(steeringWheel.parent, false);
                pivotObj.transform.position = steeringWheel.TransformPoint(totalCenterOffset);
                pivotObj.transform.rotation = steeringWheel.rotation;

                steeringWheel.SetParent(pivotObj.transform, true);

                steeringPivot = pivotObj.transform;
            }
            else
            {
                steeringPivot = steeringWheel;
            }

            initialSteeringWheelRotation = steeringPivot.localRotation;

            // Tự động tạo 2 điểm bám tay (Grips) trên vô lăng cho IK
            if (leftHandGrip == null)
            {
                Transform existingLeft = steeringPivot.Find("LeftHandGrip");
                if (existingLeft != null) leftHandGrip = existingLeft;
                else
                {
                    GameObject leftObj = new GameObject("LeftHandGrip");
                    leftObj.transform.SetParent(steeringPivot, false);
                    leftHandGrip = leftObj.transform;
                }
            }

            if (rightHandGrip == null)
            {
                Transform existingRight = steeringPivot.Find("RightHandGrip");
                if (existingRight != null) rightHandGrip = existingRight;
                else
                {
                    GameObject rightObj = new GameObject("RightHandGrip");
                    rightObj.transform.SetParent(steeringPivot, false);
                    rightHandGrip = rightObj.transform;
                }
            }

            UpdateHandGripPositions();
        }
    }

    private void OnValidate()
    {
        UpdateHandGripPositions();
    }

    public void UpdateHandGripPositions()
    {
        if (leftHandGrip != null)
        {
            leftHandGrip.localPosition = new Vector3(-handGripWidth, handGripHeight, handGripForward);
            leftHandGrip.localRotation = Quaternion.Euler(0f, 0f, handGripAngle);
        }

        if (rightHandGrip != null)
        {
            rightHandGrip.localPosition = new Vector3(handGripWidth, handGripHeight, handGripForward);
            rightHandGrip.localRotation = Quaternion.Euler(0f, 0f, -handGripAngle);
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleSteering();
        UpdateWheelsVisual();
    }

    private void HandleMovement()
    {
        currentSpeedKmh = rb.linearVelocity.magnitude * 3.6f;

        if (currentSpeedKmh > maxSpeedKmh)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * (maxSpeedKmh / 3.6f);
        }

        float finalMotorTorque = motorTorque;
        if (currentSpeedKmh < 40f)
        {
            finalMotorTorque *= lowSpeedPunch;
        }

        float torque = 0f;
        float currentBrake = 0f;

        bool hasFuel = (carFuel == null || carFuel.HasFuel());

        if (hasFuel)
        {
            if (currentSpeedKmh < maxSpeedKmh)
            {
                torque = vehicleInput.MoveInput.y * finalMotorTorque;
            }

            if (vehicleInput.IsBraking)
            {
                currentBrake = brakeTorque;
            }
            else if (Mathf.Abs(vehicleInput.MoveInput.y) < 0.01f)
            {
                currentBrake = idleBrakeTorque;
            }
        }
        else
        {
            torque = 0f;
            currentBrake = brakeTorque;
        }

        // CẬP NHẬT ĐÈN HẬU/PHANH: Bật sáng khi người lái chủ động đạp phanh (vehicleInput.IsBraking)
        if (carLightController != null)
        {
            carLightController.SetBraking(vehicleInput.IsBraking);
        }

        frontLeftWheel.motorTorque = torque;
        frontRightWheel.motorTorque = torque;

        frontLeftWheel.brakeTorque = currentBrake;
        frontRightWheel.brakeTorque = currentBrake;
        rearLeftWheel.brakeTorque = currentBrake;
        rearRightWheel.brakeTorque = currentBrake;
    }

    private void HandleSteering()
    {
        // Khi xe di chuyển, nếu có lốp bị xẹp sẽ có lực kéo lệch lái nhẹ (currentSteerBias)
        float steerAngle = vehicleInput.MoveInput.x * maxSteerAngle + currentSteerBias;
        frontLeftWheel.steerAngle = steerAngle;
        frontRightWheel.steerAngle = steerAngle;
    }

    private void UpdateWheelsVisual()
    {
        UpdateSingleWheel(frontLeftWheel, frontLeftTransform);
        UpdateSingleWheel(frontRightWheel, frontRightTransform);
        UpdateSingleWheel(rearLeftWheel, rearLeftTransform);
        UpdateSingleWheel(rearRightWheel, rearRightTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        if (wheelCollider == null || wheelTransform == null) return;
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void Update()
    {
        UpdateSteeringWheelVisual();
    }

    private void UpdateSteeringWheelVisual()
    {
        if (steeringPivot == null) return;

        float targetSteerAngle = (vehicleInput != null && vehicleInput.enabled) ? vehicleInput.MoveInput.x * maxSteeringWheelAngle : 0f;
        
        // Sử dụng SmoothDamp tạo quán tính bẻ lái và trả lái tự nhiên như vô lăng trợ lực thật
        currentSteeringWheelAngle = Mathf.SmoothDamp(currentSteeringWheelAngle, targetSteerAngle, ref steeringWheelVelocity, steeringSmoothTime);

        steeringPivot.localRotation = initialSteeringWheelRotation * Quaternion.AngleAxis(-currentSteeringWheelAngle, steeringAxis);
    }

    public float GetCurrentSpeedKmh()
    {
        return currentSpeedKmh;
    }

    public void ApplyUpgradedEngine(float newMaxSpeed, float newTorque)
    {
        maxSpeedKmh = newMaxSpeed;
        motorTorque = newTorque;
    }

    // --- HÀM MỚI: Áp dụng độ bám đường và lệch lái của từng bánh xe ---
    public void ApplyTirePhysics(float flGrip, float frGrip, float rlGrip, float rrGrip, float steerBias)
    {
        CacheBaseFriction();

        currentSteerBias = steerBias;

        UpdateSingleWheelFriction(frontLeftWheel, baseFwdFL, baseSideFL, flGrip);
        UpdateSingleWheelFriction(frontRightWheel, baseFwdFR, baseSideFR, frGrip);
        UpdateSingleWheelFriction(rearLeftWheel, baseFwdRL, baseSideRL, rlGrip);
        UpdateSingleWheelFriction(rearRightWheel, baseFwdRR, baseSideRR, rrGrip);
    }

    private void UpdateSingleWheelFriction(WheelCollider wc, WheelFrictionCurve baseFwd, WheelFrictionCurve baseSide, float gripMultiplier)
    {
        if (wc == null) return;

        WheelFrictionCurve fwd = baseFwd;
        fwd.stiffness = Mathf.Clamp(baseFwd.stiffness * gripMultiplier, 0.2f, 2.5f);
        wc.forwardFriction = fwd;

        WheelFrictionCurve side = baseSide;
        side.stiffness = Mathf.Clamp(baseSide.stiffness * gripMultiplier, 0.2f, 2.5f);
        wc.sidewaysFriction = side;
    }
}