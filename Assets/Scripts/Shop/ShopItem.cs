using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem
{
    private int itemCode;
    private int quantity;
    private bool stackable;   
    private int price;
    private int quantityInTransaction;   // Shift + click-nél


    public ShopItem(int itemCode, int quantity, bool stackable, int price, int quantityInTransaction) {
        this.itemCode              = itemCode;
        this.quantity              = quantity;
        this.stackable             = stackable;
        this.price                 = price;
        this.quantityInTransaction = quantityInTransaction; 
    }


    #region Public Getters

    public int GetItemCode() {
        return itemCode;
    }

    public int GetQuantity() {
        return quantity;
    }

    public bool GetStackable() {
        return stackable;
    }

    public int GetPrice() {
        return price;
    }

    public int GetQuantityInTransaction() {
        return quantityInTransaction;
    }

    #endregion


}
