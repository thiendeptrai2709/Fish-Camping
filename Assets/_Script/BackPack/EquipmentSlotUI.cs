using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


public class EquipmentSlotUI : MonoBehaviour, IDropHandler
{
    public event Action<ItemShapeSO> OnItemEquipped;
    public event Action OnItemRemoved;

    public enum SlotRequirement { OnlyFishingRod, OnlyBait, OnlyBobber, Universal }
    public enum SlotOrientation { KeepItemOrientation, ForceVertical, ForceHorizontal }

    [SerializeField] private SlotRequirement slotRequirement;
    [SerializeField] private BackpackMinigameUI minigameUI;
    [SerializeField] private Image placeholderIcon;
    [SerializeField] private CharacterHandVisual handVisual;

    [Header("Visual Auto-Resize Settings")]
    [SerializeField] private bool autoResizeSlotToFitItem = true;
    [SerializeField] private SlotOrientation slotOrientation = SlotOrientation.KeepItemOrientation;

    private InventoryItemUI equippedItem;
    private Vector2 originalSlotSize;

    public CharacterHandVisual GetHandVisual()
    {
        if (handVisual == null)
        {
            handVisual = FindFirstObjectByType<CharacterHandVisual>();
        }
        return handVisual;
    }

    private void Awake()
    {
        originalSlotSize = GetComponent<RectTransform>().sizeDelta;
        if (minigameUI == null) minigameUI = BackpackMinigameUI.Instance != null ? BackpackMinigameUI.Instance : FindFirstObjectByType<BackpackMinigameUI>();
        GetHandVisual();

        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
        }
    }

    public bool CanEquip(ItemShapeSO itemShape)
    {
        if (itemShape == null) return false;
        if (slotRequirement == SlotRequirement.OnlyFishingRod)
        {
            return itemShape is FishingRodSO || itemShape.name.ToLower().Contains("rod") || itemShape.name.ToLower().Contains("can") || (!string.IsNullOrEmpty(itemShape.itemName) && itemShape.itemName.ToLower().Contains("cần"));
        }
        if (slotRequirement == SlotRequirement.OnlyBait)
        {
            return itemShape is BaitSO || itemShape.name.ToLower().Contains("bait") || itemShape.name.ToLower().Contains("moi") || (!string.IsNullOrEmpty(itemShape.itemName) && itemShape.itemName.ToLower().Contains("mồi"));
        }
        if (slotRequirement == SlotRequirement.OnlyBobber)
        {
            return itemShape is BobberSO || itemShape.name.ToLower().Contains("bobber") || itemShape.name.ToLower().Contains("phao") || (!string.IsNullOrEmpty(itemShape.itemName) && itemShape.itemName.ToLower().Contains("phao"));
        }
        return true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;
        InventoryItemUI incomingItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (incomingItem == null) return;

        if (CanEquip(incomingItem.GetItemShape()))
        {
            if (minigameUI == null) minigameUI = BackpackMinigameUI.Instance != null ? BackpackMinigameUI.Instance : FindFirstObjectByType<BackpackMinigameUI>();

            if (equippedItem == null)
            {
                EquipItem(incomingItem);
            }
            else
            {
                if (incomingItem.IsEquipped())
                {
                    EquipmentSlotUI sourceSlot = incomingItem.GetCurrentSlot();
                    if (sourceSlot != null && sourceSlot.CanEquip(equippedItem.GetItemShape()))
                    {
                        InventoryItemUI tempOld = equippedItem;
                        EquipItem(incomingItem);
                        sourceSlot.EquipItemDirectly(tempOld);
                    }
                }
                else
                {
                    int oldX = incomingItem.GetGridX();
                    int oldY = incomingItem.GetGridY();
                    bool oldRot = incomingItem.IsRotated();

                    if (minigameUI != null && minigameUI.CanPlaceItemAt(oldX, oldY, equippedItem.GetItemShape(), oldRot))
                    {
                        InventoryItemUI tempOld = equippedItem;
                        EquipItem(incomingItem);
                        minigameUI.PlaceItemDirectlyToGrid(tempOld, oldX, oldY, oldRot);
                    }
                    else if (minigameUI != null && minigameUI.TryAutoFitItemToGrid(equippedItem))
                    {
                        EquipItem(incomingItem);
                    }
                    else
                    {
                        InventoryItemUI tempOld = equippedItem;
                        EquipItem(incomingItem);
                        if (minigameUI != null) minigameUI.TryAutoFitItemToGrid(tempOld);
                    }
                }
            }
        }
    }

    public void EquipItem(InventoryItemUI itemUI)
    {
        equippedItem = itemUI;
        itemUI.transform.SetParent(transform);
        itemUI.transform.SetAsLastSibling();

        if (slotOrientation == SlotOrientation.ForceHorizontal && !itemUI.IsRotated())
        {
            itemUI.ToggleRotate();
        }
        else if (slotOrientation == SlotOrientation.ForceVertical && itemUI.IsRotated())
        {
            itemUI.ToggleRotate();
        }

        RectTransform rect = itemUI.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        if (autoResizeSlotToFitItem)
        {
            GetComponent<RectTransform>().sizeDelta = rect.sizeDelta;
        }

        itemUI.SetEquippedState(true, this);
        itemUI.SetHandledBySlot(true);

        if (placeholderIcon != null) placeholderIcon.enabled = false;

        var hv = GetHandVisual();
        if (hv != null && itemUI.GetItemShape() != null)
        {
            hv.EquipItemVisual(itemUI.GetItemShape());
        }
        OnItemEquipped?.Invoke(itemUI.GetItemShape());

        if (slotRequirement == SlotRequirement.OnlyFishingRod)
        {
            ForcedTutorialManager.Instance?.NotifyRodEquipped();
        }
        else if (slotRequirement == SlotRequirement.OnlyBait || slotRequirement == SlotRequirement.OnlyBobber)
        {
            ForcedTutorialManager.Instance?.NotifyBaitAndBobberEquipped();
        }

        BackpackMinigameUI.Instance?.SaveBackpack();
    }

    public void EquipItemDirectly(InventoryItemUI itemUI)
    {
        if (itemUI == null) return;
        if (equippedItem != null && equippedItem != itemUI)
        {
            Destroy(equippedItem.gameObject);
        }
        equippedItem = itemUI;
        itemUI.transform.SetParent(transform);
        itemUI.transform.SetAsLastSibling();

        if (slotOrientation == SlotOrientation.ForceHorizontal && !itemUI.IsRotated())
        {
            itemUI.ToggleRotate();
        }
        else if (slotOrientation == SlotOrientation.ForceVertical && itemUI.IsRotated())
        {
            itemUI.ToggleRotate();
        }

        RectTransform rect = itemUI.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        if (autoResizeSlotToFitItem)
        {
            GetComponent<RectTransform>().sizeDelta = rect.sizeDelta;
        }

        itemUI.SetEquippedState(true, this);

        if (placeholderIcon != null) placeholderIcon.enabled = false;

        var hvDirect = GetHandVisual();
        if (hvDirect != null && itemUI.GetItemShape() != null)
        {
            hvDirect.EquipItemVisual(itemUI.GetItemShape());
        }
        OnItemEquipped?.Invoke(itemUI.GetItemShape());

        if (slotRequirement == SlotRequirement.OnlyFishingRod)
        {
            ForcedTutorialManager.Instance?.NotifyRodEquipped();
        }
        else if (slotRequirement == SlotRequirement.OnlyBait || slotRequirement == SlotRequirement.OnlyBobber)
        {
            ForcedTutorialManager.Instance?.NotifyBaitAndBobberEquipped();
        }
    }

    public void RemoveEquippedItem()
    {
        var hv = GetHandVisual();
        if (hv != null && equippedItem != null)
        {
            hv.RemoveItemVisual(equippedItem.GetItemShape());
        }

        equippedItem = null;
        if (placeholderIcon != null) placeholderIcon.enabled = true;

        if (autoResizeSlotToFitItem)
        {
            GetComponent<RectTransform>().sizeDelta = originalSlotSize;
        }
        OnItemRemoved?.Invoke();
        BackpackMinigameUI.Instance?.SaveBackpack();
    }
    public void ReturnItemToSlot(InventoryItemUI itemUI)
    {
        EquipItemDirectly(itemUI);
    }

    public InventoryItemUI GetEquippedItem() => equippedItem;
    public SlotRequirement GetSlotRequirement() => slotRequirement;
}