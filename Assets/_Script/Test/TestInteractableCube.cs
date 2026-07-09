using UnityEngine;

public class TestInteractableCube : MonoBehaviour, IInteractable
{
    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        // Nhận diện tên 2 Layer bạn đã set up trong Unity
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        // Mặc định lúc mới sinh ra là layer bình thường (không có viền)
        gameObject.layer = normalLayer;
    }

    public void OnFocus()
    {
        // Khi tia ngắm chạm vào -> Đổi sang Layer có viền
        gameObject.layer = outlineLayer;
    }

    public void OnLoseFocus()
    {
        // Khi quay mặt đi -> Trở về Layer bình thường
        gameObject.layer = normalLayer;
    }

    public void Interact()
    {
        Debug.Log("Đã mở rương chứa đồ của Richard!");
    }

    public string GetInteractPrompt()
    {
        return "[Chuột Trái] Khám phá rương";
    }
}