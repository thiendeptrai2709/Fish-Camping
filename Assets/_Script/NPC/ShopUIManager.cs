using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemContainer; // Nơi chứa danh sách vật phẩm tự động sinh ra
    [SerializeField] private GameObject itemPrefab;    // Ô giao diện vật phẩm mẫu để nhân bản

    [Header("UI Info Detail")]
    [SerializeField] private TextMeshProUGUI selectedItemName;
    [SerializeField] private TextMeshProUGUI selectedItemDesc;
    [SerializeField] private TextMeshProUGUI selectedItemPrice;
    [SerializeField] private Button actionButton; // Nút "Mua" hoặc "Bán"

    [Header("Shop Data Config")]
    [SerializeField] private List<ShopItemSO> itemsToSell; // Danh sách hàng hóa NPC Thuyền trưởng bán

    private ShopItemSO _selectedItem;
    private System.Action _onCloseCallback;
    private bool _isSellTab = false; // False = Đang ở Tab Mua, True = Đang ở Tab Bán Cá

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        shopPanel.SetActive(false);
    }

    public void OpenShop(System.Action onClose)
    {
        shopPanel.SetActive(true);
        _onCloseCallback = onClose;
        SwitchToBuyTab(); // Mặc định mở Tab mua đồ câu trước
    }

    // TAB MUA ĐỒ CÂU
    public void SwitchToBuyTab()
    {
        _isSellTab = false;
        GenerateShopList(itemsToSell);
    }

    // TAB BÁN CÁ
    public void SwitchToSellTab()
    {
        _isSellTab = true;

        // GIẢ LẬP: Tạo danh sách cá trong hòm đồ của Player để bán
        // Sau này bạn kết nối thực tế: List<ShopItemSO> playerFish = PlayerInventory.GetOnlyFish();
        List<ShopItemSO> mockPlayerFish = GetMockPlayerFish();

        GenerateShopList(mockPlayerFish);
    }

    private void GenerateShopList(List<ShopItemSO> itemList)
    {
        // Xóa danh sách cũ hiển thị trên UI tránh trùng lặp
        foreach (Transform child in itemContainer) Destroy(child.gameObject);

        if (itemList.Count == 0)
        {
            ClearSelectionUI("Hòm đồ trống!");
            return;
        }

        // Tự động sinh ra các ô vật phẩm dựa theo dữ liệu đầu vào
        foreach (ShopItemSO item in itemList)
        {
            GameObject obj = Instantiate(itemPrefab, itemContainer);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = $"{item.itemName} ({item.price}G)";

            // Bấm vào ô nào thì hiện thông tin chi tiết ô đó
            obj.GetComponent<Button>().onClick.AddListener(() => SelectItem(item));
        }

        SelectItem(itemList[0]);
    }

    private void SelectItem(ShopItemSO item)
    {
        _selectedItem = item;
        selectedItemName.text = item.itemName;
        selectedItemDesc.text = item.description;
        selectedItemPrice.text = _isSellTab ? $"Giá bán: {item.price}G" : $"Giá mua: {item.price}G";

        actionButton.GetComponentInChildren<TextMeshProUGUI>().text = _isSellTab ? "BÁN" : "MUA";
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(ExecuteTransaction);
    }

    private void ExecuteTransaction()
    {
        if (_selectedItem == null) return;

        if (!_isSellTab)
        {
            // LOGIC MUA ĐỒ:
            // if (PlayerWallet.Gold >= _selectedItem.price && PlayerLevel >= _selectedItem.requiredLevel)
            Debug.Log($"[Cửa hàng] Bạn đã mua thành công {_selectedItem.itemName} trừ {_selectedItem.price}G!");
            // PlayerInventory.Add(_selectedItem);
        }
        else
        {
            // LOGIC BÁN CÁ:
            Debug.Log($"[Cửa hàng] Bạn đã bán {_selectedItem.itemName} nhận về {_selectedItem.price}G!");
            // PlayerWallet.AddGold(_selectedItem.price);
            // PlayerInventory.Remove(_selectedItem);
            SwitchToSellTab(); // Làm tươi lại danh sách cá sau khi bán
        }
    }

    private void ClearSelectionUI(string message)
    {
        selectedItemName.text = message;
        selectedItemDesc.text = "";
        selectedItemPrice.text = "";
        actionButton.onClick.RemoveAllListeners();
    }

    private List<ShopItemSO> GetMockPlayerFish()
    {
        // Tạo nhanh dữ liệu cá giả lập để bạn test tính năng bán cá
        List<ShopItemSO> fishList = new List<ShopItemSO>();
        ShopItemSO caVuoc = ScriptableObject.CreateInstance<ShopItemSO>();
        caVuoc.itemName = "Cá Vược Hồ Thông"; caVuoc.price = 80; caVuoc.description = "Một con cá vược béo ú thích ăn mồi bột.";
        fishList.Add(caVuoc);
        return fishList;
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        _onCloseCallback?.Invoke();
    }
}