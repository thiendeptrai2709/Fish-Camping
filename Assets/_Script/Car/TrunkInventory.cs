using UnityEngine;

public class TrunkInventory : MonoBehaviour
{
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
                if (interactableTrunk != null)
                    interactableTrunk.ForceClose();
                else
                    ForceCloseUI();
            }
        }
    }

    public void ToggleTrunkAndUI()
    {
        if (trunkInventoryPanel != null)
        {
            trunkInventoryPanel.SetActive(!trunkInventoryPanel.activeSelf);
        }
    }
    public void ForceCloseUI()
    {
        if (trunkInventoryPanel != null)
        {
            trunkInventoryPanel.SetActive(false);
        }
    }
}