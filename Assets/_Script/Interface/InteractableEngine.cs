using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractableEngine : MonoBehaviour, IInteractable
{
    [Header("Liên kết chức năng")]
    [SerializeField] private EngineRepairMinigame engineRepairMinigame;

    private int normalLayer;
    private int outlineLayer;

    private void Awake()
    {
        // Setup layer để làm Outline
        normalLayer = LayerMask.NameToLayer("Interactable");
        outlineLayer = LayerMask.NameToLayer("Outlined");

        SetLayerRecursively(gameObject, normalLayer);
    }

    public void OnFocus()
    {
        SetLayerRecursively(gameObject, outlineLayer);
    }

    public void OnLoseFocus()
    {
        SetLayerRecursively(gameObject, normalLayer);
    }

    public void Interact()
    {
        if (engineRepairMinigame != null && !engineRepairMinigame.IsEngineOut)
        {
            engineRepairMinigame.TryToggleRepairMode();
        }
    }

    public string GetInteractPrompt()
    {
        if (engineRepairMinigame != null && engineRepairMinigame.IsEngineOut)
        {
            // Khi máy đang ở ngoài, ta trả về chuỗi rỗng để ẩn dòng chữ gợi ý đi
            // vì bây giờ người chơi sẽ dùng chuột để bấm các nút trên bảng Menu.
            return "";
        }
        return "[Chuột Trái] Kiểm tra động cơ";
    }
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}