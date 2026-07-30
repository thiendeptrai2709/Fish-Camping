using UnityEngine;

public class TrunkInventory : MonoBehaviour
{
    // Thêm biến tĩnh để Balo có thể gọi đóng cốp
    public static TrunkInventory CurrentOpenTrunk { get; private set; }

    [Header("Link UI")]
    [SerializeField] private GameObject trunkInventoryPanel;

    [Header("Link Cốp & Người Chơi")]
    [SerializeField] private InteractableTrunk interactableTrunk;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float maxDistance = 3.5f;

    private void Update()
    {
        if (trunkInventoryPanel != null && trunkInventoryPanel.activeSelf)
        {
            if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > maxDistance)
            {
                ForceCloseAll();
            }
        }
    }

    public void ToggleTrunkAndUI()
    {
        if (trunkInventoryPanel != null)
        {
            bool isActive = !trunkInventoryPanel.activeSelf;
            trunkInventoryPanel.SetActive(isActive);

            // Cập nhật trạng thái cốp đang mở
            if (isActive) CurrentOpenTrunk = this;
            else if (CurrentOpenTrunk == this) CurrentOpenTrunk = null;

            if (BackpackController.Instance != null)
            {
                BackpackController.Instance.OpenForCooking(isActive);
            }
        }
    }

    public void ForceCloseAll()
    {
        if (interactableTrunk != null)
        {
            interactableTrunk.ForceClose(); // Hàm này tự động gọi ForceCloseUI() bên trong nó
        }
        else
        {
            ForceCloseUI();
        }
    }

    public void ForceCloseUI()
    {
        if (trunkInventoryPanel != null)
        {
            trunkInventoryPanel.SetActive(false);
            if (CurrentOpenTrunk == this) CurrentOpenTrunk = null;

            // Rất quan trọng: Báo cho Balo đóng theo để tránh kẹt giao diện
            if (BackpackController.Instance != null)
            {
                BackpackController.Instance.OpenForCooking(false);
            }
        }
    }
}