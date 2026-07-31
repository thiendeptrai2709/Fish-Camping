using UnityEngine;
using System.Collections;

public class LeafZoneController : MonoBehaviour
{
    [Header("Target VFX")]
    public ParticleSystem leafParticle;

    [Header("Random Timing Settings")]
    [Tooltip("Thời gian nghỉ giữa các đợt lá rơi (giây)")]
    public float minInterval = 3f;
    public float maxInterval = 8f;

    [Tooltip("Thời gian lá rơi trong 1 đợt (giây)")]
    public float minDuration = 3f;
    public float maxDuration = 6f;

    private int activeObjectsCount = 0;
    private Coroutine leafCoroutine;

    private void Start()
    {
        if (leafParticle != null)
        {
            leafParticle.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        activeObjectsCount++;

        // Bất kỳ object nào bước vào -> Bật vòng lặp ngẫu nhiên ngay lập tức
        if (activeObjectsCount == 1 && leafCoroutine == null)
        {
            leafCoroutine = StartCoroutine(RandomLeafRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        activeObjectsCount--;
        if (activeObjectsCount < 0) activeObjectsCount = 0;

        // Khi không còn ai trong Zone -> Tắt hẳn hệ thống lá rơi
        if (activeObjectsCount == 0 && leafCoroutine != null)
        {
            StopCoroutine(leafCoroutine);
            leafCoroutine = null;

            if (leafParticle != null)
            {
                leafParticle.Stop();
            }
        }
    }

    private IEnumerator RandomLeafRoutine()
    {
        while (activeObjectsCount > 0)
        {
            // 1. Vừa vào Zone -> Đợt lá ĐẦU TIÊN rơi NGAY LẬP TỨC (không chờ)
            if (leafParticle != null)
            {
                leafParticle.Play();
            }

            // 2. Cho lá rơi trong một khoảng thời gian ngẫu nhiên
            float duration = Random.Range(minDuration, maxDuration);
            yield return new WaitForSeconds(duration);

            // 3. Tắt lá rơi để tạo quãng nghỉ
            if (leafParticle != null)
            {
                leafParticle.Stop();
            }

            // 4. Cho gió nghỉ một khoảng thời gian ngẫu nhiên trước khi sang đợt tiếp theo
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
}