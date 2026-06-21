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

    private Rigidbody rb;
    private VehicleInput vehicleInput;
    private float currentSpeedKmh;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        vehicleInput = GetComponent<VehicleInput>();

        if (vehicleInput == null)
        {
            vehicleInput = gameObject.AddComponent<VehicleInput>();
        }
        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
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
        if (currentSpeedKmh < maxSpeedKmh)
        {
            torque = vehicleInput.MoveInput.y * finalMotorTorque;
        }

        float currentBrake = 0f;
        if (vehicleInput.IsBraking)
        {
            currentBrake = brakeTorque;
        }
        else if (Mathf.Abs(vehicleInput.MoveInput.y) < 0.01f)
        {
            currentBrake = idleBrakeTorque;
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
        float steerAngle = vehicleInput.MoveInput.x * maxSteerAngle;
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
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    public float GetCurrentSpeedKmh()
    {
        return currentSpeedKmh;
    }
}