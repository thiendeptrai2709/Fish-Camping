using UnityEngine;

public class BobberEffectController : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterSplashParticle;
    [SerializeField] private GameObject alertIcon;
    [SerializeField] private float alertDuration = 1.5f;

    private void Awake()
    {
        if (alertIcon != null)
        {
            alertIcon.SetActive(false);
        }

        if (waterSplashParticle != null)
        {
            waterSplashParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void PlayBiteEffect()
    {
        if (waterSplashParticle != null)
        {
            waterSplashParticle.Play();
        }

        if (alertIcon != null)
        {
            alertIcon.SetActive(true);
            Invoke(nameof(HideAlertIcon), alertDuration);
        }
    }

    private void HideAlertIcon()
    {
        if (alertIcon != null)
        {
            alertIcon.SetActive(false);
        }
    }
}