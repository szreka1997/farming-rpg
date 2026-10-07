using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shop : MonoBehaviour, IRaycastable
{
    #region Variables

    [SerializeField] private StockItemConfig[] stockItemConfigArray = null;
    [SerializeField] private string shopName;

    private List<ShopItem> shopItemList;
    private UIManager uIManager;
    private int totalPrice = 0;

    #endregion



    public void SelectFilter (ItemCategory category) {}
    public ItemCategory GetFilter() { return ItemCategory.None; }
    public bool CanTransact() { return true; }

    


    #region Unity Basic Methods

    void Awake() {
        uIManager = FindObjectOfType<UIManager>();

        shopItemList = new List<ShopItem>();

        foreach(StockItemConfig stockItemConfig in stockItemConfigArray) {
            int price = (int) (stockItemConfig.item.buyingPrice * (1 - stockItemConfig.buyingDiscountPercentage / 100.0f));

            ShopItem shopItem = new ShopItem(stockItemConfig.item.itemCode, stockItemConfig.initialStock, true, price, 0);

            shopItemList.Add(shopItem);
        }
    }

    #endregion


    #region Interface Inplementations

    public CursorType GetCursorType() {
        if((uIManager.PauseMenuOn || uIManager.InventoryMenuOn || uIManager.ShopMenuOn)) {
            return CursorType.None;
        } 
        else {
            return CursorType.Shop;
        }
    }

    public bool HandleRaycast(PlayerUIController callingController) {
        if (!(uIManager.PauseMenuOn || uIManager.InventoryMenuOn || uIManager.ShopMenuOn)) {
            if(Input.GetMouseButtonDown(1)) {
                callingController.GetComponent<Shopper>().SetActiveShop(this);
            }

            return true;
        }

        return false;
    }

    #endregion


    #region Public Methods

    public string GetShopName() {
        return shopName;
    }

    public List<ShopItem> GetFilteredItems() { 
        List<ShopItem> localShopItemList = new List<ShopItem>();

        foreach(ShopItem shopItem in shopItemList) {
            localShopItemList.Add(new ShopItem(shopItem.GetItemCode(), 
                                               shopItem.GetQuantity(), 
                                               shopItem.GetStackable(), 
                                               shopItem.GetPrice(), 
                                               shopItem.GetQuantityInTransaction()));
        }

        return localShopItemList;
    }

    public void SetTotalPrice(int price) {
        totalPrice = price;
    }

    public int TransactionTotal() { 
        return totalPrice;
    }

 

    /// <summary>
    /// Find if an itemCode is already in the inventory. Returns the item position in the inventory list, or -1 if the item is not in the inventory
    /// </summary>
    public int FindItemInShop(int itemCode) {
        for (int i = 0; i < shopItemList.Count; i++) {
            if (shopItemList[i].GetItemCode() == itemCode) {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Remove an item from the inventory, and create a game object at the position it was dropped
    /// </summary>
    public void RemoveItem(int itemCode, int itemCount) {
        // Check if inventory already contains the item
        int itemPosition = FindItemInShop(itemCode);

        if (itemPosition != -1) {
            RemoveItemAtPosition(itemCode, itemPosition, itemCount);
        }

        //  Send event that inventory has been updated
        EventHandler.CallShopInventoryUpdatedEvent();
    }

    #endregion


    #region Private Methods

    private void RemoveItemAtPosition(int itemCode, int position, int itemCount) {
        int quantity = shopItemList[position].GetQuantity() - itemCount;

        if (quantity < 0) {
            quantity = 0;
        }

        ShopItem shopItem = new ShopItem(shopItemList[position].GetItemCode(), quantity, shopItemList[position].GetStackable(),
                                         shopItemList[position].GetPrice(), shopItemList[position].GetQuantityInTransaction());

        shopItemList[position] = shopItem;
    }

    #endregion




}
