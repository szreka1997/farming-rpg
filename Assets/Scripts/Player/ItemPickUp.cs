using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        Item item = other.GetComponent<Item>();

        if(item != null) {
            SO_ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(item.ItemCode);

            if (itemDetails.canBePickedUp) {
                if(InventoryManager.Instance.CanAddItem(InventoryLocation.Player, itemDetails.itemCode, 1)) {
                    InventoryManager.Instance.AddItem(InventoryLocation.Player, item, other.gameObject);
                    AudioManager.Instance.PlaySound(SoundName.EffectPickupSound);
                }
            }
        }
    }


    
}
