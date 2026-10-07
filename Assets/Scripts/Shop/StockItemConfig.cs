using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StockItemConfig
{
    public SO_ItemDetails item;
    public int initialStock;

    [Range(0, 100)] 
    public int buyingDiscountPercentage;
}
