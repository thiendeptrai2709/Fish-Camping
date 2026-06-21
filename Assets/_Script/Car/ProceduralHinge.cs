using System.Collections;
using UnityEngine;

// Script này không thèm biết ai bấm phím gì. Nó chỉ làm đúng 1 việc: "Bảo mở là mở, bảo đóng là sập".
public class ProceduralHinge : MonoBehaviour
{
    [SerializeField] private Vector3 openOffsetAngles = new Vector3(-65f, 0f, 0f);
    [SerializeField] private float speed = 4f;

    private Quaternion closedRotation;
    private Quaternion openedRotation;
    private bool isOpen = false;
    private Coroutine animCoroutine;

    private void Awake()
    {
        closedRotation = transform.localRotation;
        openedRotation = closedRotation * Quaternion.Euler(openOffsetAngles);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(Animate(isOpen ? openedRotation : closedRotation));
    }

    private IEnumerator Animate(Quaternion targetRot)
    {
        Quaternion startRot = transform.localRotation;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * speed;
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, time));
            yield return null;
        }
        transform.localRotation = targetRot;
    }
}