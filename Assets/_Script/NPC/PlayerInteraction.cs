using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Transform interactionPoint; // Điểm tâm để quét (thường đặt ở trước ngực/chân Player)
    [SerializeField] private float interactionRadius = 2f; // Bán kính quét tương tác (mét)
    [SerializeField] private LayerMask interactableLayer; // Chỉ quét các Object thuộc Layer này (đặt Layer là "Interactable")

    private IInteractable _currentInteractable;

    void Update()
    {
        CheckForInteractable();

        // Nếu phát hiện có vật thể tương tác và người chơi nhấn phím E
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Nếu khung hội thoại ĐANG BẬT, phím E sẽ dùng để tua chữ/đổi câu thoại tiếp theo
            if (DialogueCanvasIsActive())
            {
                DialogueManager.Instance.DisplayNextSentence();
            }
            // Nếu khung hội thoại ĐANG TẮT và có NPC ở gần, tiến hành bắt đầu nói chuyện
            else if (_currentInteractable != null)
            {
                _currentInteractable.Interact();
            }
        }
    }

    private void CheckForInteractable()
    {
        // Quét tất cả các Collider trong bán kính xung quanh điểm tương tác
        Collider[] colliders = Physics.OverlapSphere(interactionPoint.position, interactionRadius, interactableLayer);

        if (colliders.Length > 0)
        {
            // Lấy vật thể đầu tiên quét trúng có chứa Interface IInteractable
            IInteractable interactable = colliders[0].GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (_currentInteractable != interactable)
                {
                    _currentInteractable = interactable;
                    // Log ra màn hình câu lệnh nhắc nhở
                    Debug.Log(_currentInteractable.InteractionPrompt);
                }
                return;
            }
        }

        // Nếu không có vật thể nào trong vùng quét, reset tương tác
        _currentInteractable = null;
    }

    // Vẽ hình cầu trong không gian Scene để bạn dễ căn chỉnh bán kính quét bằng mắt
    private void OnDrawGizmosSelected()
    {
        if (interactionPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }
    private bool DialogueCanvasIsActive()
    {
        // Bạn có thể kéo trực tiếp DialogueCanvas vào script này hoặc check nhanh qua Instance
        return GameObject.Find("DialogueCanvas") != null && GameObject.Find("DialogueCanvas").activeInHierarchy;
    }
}