using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractableTrunk : MonoBehaviour, IInteractable
{
    [Header("Liên kết chức năng")]
    [SerializeField] private ProceduralHinge trunkHinge;
    [SerializeField] private TrunkInventory trunkInventory;

    private int normalLayer;
    private int outlineLayer;
    private bool isTrunkOpen = false;

    private void Awake()
    {
        // Setup layer để làm Outline (giống hệt cửa xe)
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        gameObject.layer = normalLayer;
    }

    public void OnFocus()
    {
        SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    public void Interact()
    {
        isTrunkOpen = !isTrunkOpen;

        // 1. Chạy animation mở/đóng nắp rương
        if (trunkHinge != null)
        {
            trunkHinge.Toggle();
        }

        // 2. Bật/tắt Giao diện chứa đồ
        if (trunkInventory != null)
        {
            trunkInventory.ToggleTrunkAndUI();
        }

        // 3. Thông báo cho Tutorial Manager
        if (isTrunkOpen)
        {
            ForcedTutorialManager.Instance?.NotifyTrunkOpened();
        }
        else
        {
            ForcedTutorialManager.Instance?.NotifyTrunkClosed();
        }
    }
    public void ForceClose()
    {
        if (!isTrunkOpen) return;
        isTrunkOpen = false;

        if (trunkHinge != null) trunkHinge.ForceClose();
        if (trunkInventory != null) trunkInventory.ForceCloseUI();

        ForcedTutorialManager.Instance?.NotifyTrunkClosed();
    }
    public string GetInteractPrompt()
    {
        bool isVietnamese = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale != null &&
                            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("vi");

        return isTrunkOpen 
            ? (isVietnamese ? "[Chuột Trái] Đóng cốp" : "[Left Click] Close Trunk") 
            : (isVietnamese ? "[Chuột Trái] Mở cốp xe" : "[Left Click] Open Trunk");
    }
}