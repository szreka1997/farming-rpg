using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Item/Item Details", fileName = "New so_Item Details")]
public class SO_ItemDetails : ScriptableObject
{
    public int itemCode;
    public ItemType itemType;
    public string itemDescription;
    public Sprite itemSprite;
    public string itemLongDescription;

    public int buyingPrice;

    public short itemsUseGridRadius;
    public float itemUseRadius;

    public bool isStackable;
    public bool isStartingItem;
    
    public bool canBePickedUp;
    public bool canBeDropped;
    public bool canBeEaten;
    public bool canBeCarried;
}
