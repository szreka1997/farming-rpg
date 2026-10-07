using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [ItemCodeDescription] [SerializeField] private int itemCode;

    private SpriteRenderer spriteRenderer;

    public int ItemCode {
        get {
            return itemCode;
        }
        set {
            itemCode = value;
        }
    }


    private void Awake() {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start() {
        if(ItemCode != 0) {
            Init(ItemCode);
        }
    }


    public void Init(int itemCodeParam) {
        if (itemCodeParam != 0) {
            ItemCode = itemCodeParam;

            SO_ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(ItemCode);
            spriteRenderer.sprite = itemDetails.itemSprite;

            // If item type is reapable then add nudgeable component
            if (itemDetails.itemType == ItemType.ReapableScenary) {
                gameObject.AddComponent<ItemNudge>();
            }
        }
    }


}
