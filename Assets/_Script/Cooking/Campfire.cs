using UnityEngine;

public class Campfire : MonoBehaviour
{
    [SerializeField] private GameObject fireVFX;

    [Header("Âm thanh Lửa (Audio)")]
    [SerializeField] private AudioSource fireAudioSource;
    [SerializeField] private AudioClip fireLoopSound;

    [Header("Tốc độ cháy của ngọn lửa")]
    [Tooltip("Điều chỉnh tốc độ cháy chậm lại cho tự nhiên và êm dịu (Mặc định 0.08f, cực chậm và bồng bềnh)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float fireBurnSpeed = 0.08f;

    private void Awake()
    {
        if (fireAudioSource == null)
        {
            fireAudioSource = GetComponent<AudioSource>();
        }

        // Tự động cấu hình AudioSource nếu có gán clip
        if (fireAudioSource != null && fireLoopSound != null)
        {
            fireAudioSource.clip = fireLoopSound;
            fireAudioSource.loop = true;
            fireAudioSource.playOnAwake = false;
        }

        ApplyBurnSpeed();
    }

    private void Start()
    {
        SetFireActive(false);
    }

    private void Update()
    {
        // Duy trì tốc độ cháy chậm rãi ổn định không bị tăng tốc
        if (fireVFX != null && fireVFX.activeSelf)
        {
            ApplyBurnSpeed();
        }
    }

    public void ApplyBurnSpeed()
    {
        if (fireBurnSpeed <= 0.01f) fireBurnSpeed = 0.2f;

        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var p in particles)
        {
            if (p == null) continue;
            var main = p.main;
            main.simulationSpeed = fireBurnSpeed;

            var vel = p.velocityOverLifetime;
            if (vel.enabled)
            {
                vel.speedModifier = fireBurnSpeed;
            }
        }
    }

    public void SetFireActive(bool isActive)
    {
        if (fireVFX == null)
        {
            // Tự động tìm VFX con nếu chưa gán thủ công
            Transform vfxTrans = transform.Find("Fire") ?? transform.Find("FireVFX") ?? transform.Find("Flame") ?? transform.Find("Fire_Yellow");
            if (vfxTrans != null) fireVFX = vfxTrans.gameObject;
        }

        if (fireVFX != null)
        {
            fireVFX.SetActive(isActive);
        }

        // Bắt đầu hoặc dừng tất cả ParticleSystem con với tốc độ cháy chậm rãi êm dịu
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var p in particles)
        {
            if (p == null) continue;
            var main = p.main;
            main.simulationSpeed = fireBurnSpeed;

            if (isActive)
            {
                p.gameObject.SetActive(true);
                p.Clear();
                p.Play();
            }
            else
            {
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        // Bật/tắt ánh sáng bập bùng của ngọn lửa
        Light[] lights = GetComponentsInChildren<Light>(true);
        foreach (var l in lights)
        {
            if (l != null) l.enabled = isActive;
        }

        if (fireAudioSource != null)
        {
            if (isActive)
            {
                if (!fireAudioSource.isPlaying) fireAudioSource.Play();
            }
            else
            {
                if (fireAudioSource.isPlaying) fireAudioSource.Stop();
            }
        }
    }
}