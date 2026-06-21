using UnityEngine;

public class VehicleAudio : MonoBehaviour
{
    [Header("Link Hệ thống")]
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private VehicleInput vehicleInput;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource engineSource;
    [SerializeField] private AudioSource brakeSource;

    [Header("Cài đặt Động cơ")]
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 2.5f;
    [SerializeField] private float speedToPitchRatio = 80f;

    private bool hasPlayedBrake = false;
    private void Update()
    {
        if (vehicleController == null || vehicleInput == null) return;

        HandleEngineAudio();
        HandleBrakeAudio();
    }

    // Xử lý tăng giảm cao độ (Pitch) của tiếng máy tùy theo tốc độ thực tế và thao tác đạp ga
    private void HandleEngineAudio()
    {
        float currentSpeed = Mathf.Abs(vehicleController.GetCurrentSpeedKmh());
        float throttleInput = Mathf.Abs(vehicleInput.MoveInput.y);

        float targetPitch = minPitch + (currentSpeed / speedToPitchRatio) * (maxPitch - minPitch) + (throttleInput * 0.3f);
        engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, Time.deltaTime * 5f);

        // Chỉ phát tiếng khi xe đang chạy hoặc đang đạp ga
        if (currentSpeed > 1f || throttleInput > 0.1f)
        {
            if (!engineSource.isPlaying) engineSource.Play();
            engineSource.volume = Mathf.Lerp(engineSource.volume, 1f, Time.deltaTime * 5f);
        }
        else
        {
            // Xe đứng im thì giảm dần âm lượng rồi ngắt hẳn cho nhẹ tai
            engineSource.volume = Mathf.Lerp(engineSource.volume, 0f, Time.deltaTime * 5f);
            if (engineSource.volume < 0.05f && engineSource.isPlaying)
            {
                engineSource.Pause();
            }
        }
    }

    // Xử lý tiếng rít phanh khi xe đang chạy trên 10km/h mà người chơi thực hiện thao tác phanh
    private void HandleBrakeAudio()
    {
        float currentSpeed = Mathf.Abs(vehicleController.GetCurrentSpeedKmh());

        if (vehicleInput.IsBraking && currentSpeed > 5f)
        {
            // Dùng cờ hasPlayedBrake để bắt nó chỉ được kêu đúng 1 lần dù m có giữ rịt phím phanh bao lâu đi nữa
            if (!hasPlayedBrake)
            {
                brakeSource.Play();
                hasPlayedBrake = true;
            }
        }
        else
        {
            if (brakeSource.isPlaying)
            {
                brakeSource.Stop();
            }
            // Khi nhả phanh hoặc xe đã dừng hẳn thì reset cờ để chuẩn bị cho cú phanh tiếp theo
            hasPlayedBrake = false;
        }
    }
}