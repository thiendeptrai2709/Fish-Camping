using UnityEngine;

public class TrunkInventory : MonoBehaviour
{
    [Header("Link UI")]
    [SerializeField] private GameObject trunkInventoryPanel;

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