using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryBar : MonoBehaviour
{
    #region Variables

    [SerializeField] private Sprite blank16x16Sprite = null;
    [SerializeField] private UIInventorySlot[] inventorySlots = null;

    [HideInInspector] public GameObject inventoryTextBoxGameobject;

    public GameObject inventoryBarDraggedItemPrefab;

    private RectTransform rectTransform;
    private bool isInventoryBarPositionBottom = true;

    public bool IsInventoryBarPositionBottom { 
        get => isInventoryBarPositionBottom; 
        set => isInventoryBarPositionBottom = value; 
    }

    #endregion



    #region Unity Basic Methods

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        SwitchInventoryBarPosition();
    }

    #endregion



    #region Unity Event Related Methods

    private void OnEnable() {
        EventHandler.InventoryUpdatedEvent += InventoryUpdated;
    }

    private void OnDisable() {
        EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
    }

    #endregion



    #region Public Methods

    /// <summary>
    /// Clear all highlights from the inventory bar
    /// </summary>
    public void ClearHighlightOnInventorySlots() {
        if(inventorySlots.Length > 0) {
            for(int i = 0; i < inventorySlots.Length; i++) {
                if(inventorySlots[i].isSelected) {
                    inventorySlots[i].isSelected = false;
                    inventorySlots[i].inventorySlotHighlight.color = new Color(0f, 0f, 0f, 0f);

                    // Update inventory to show item as not selected
                    InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.Player);
                }
            }
        }
    }

    /// <summary>
    ///  Set the selected highlight if set on all inventory item positions
    /// </summary>
    public void SetHighlightedInventorySlots() {
        if(inventorySlots.Length > 0) {
            for(int i = 0; i < inventorySlots.Length; i++) {
                SetHighlightedInventorySlots(i);
            }
        }
    }

    /// <summary>
    ///  Set the selected highlight if set on an inventory item for a given slot item position
    /// </summary>
    public void SetHighlightedInventorySlots(int itemPosition) {
        if (inventorySlots.Length > 0 && inventorySlots[itemPosition].itemDetails != null) {
            if (inventorySlots[itemPosition].isSelected) {
                inventorySlots[itemPosition].inventorySlotHighlight.color = new Color(1f, 1f, 1f, 1f);

                // Update inventory to show item as selected
                InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.Player, inventorySlots[itemPosition].itemDetails.itemCode);
            }
        }
    }

    public void DestroyCurrentlyDraggedItems() {
        for (int i = 0; i < inventorySlots.Length; i++) {
            if (inventorySlots[i].draggedItem != null) {
                Destroy(inventorySlots[i].draggedItem);
            }
        }
    }

    public void ClearCurrentlySelectedItems() {
        for (int i = 0; i < inventorySlots.Length; i++) {
            inventorySlots[i].ClearSelectedItem();
        }
    }

    #endregion



    #region Private Methods

    /// <summary>
	/// Switch inventory bar position depending on player position
	/// </summarary>
    private void SwitchInventoryBarPosition() {
        Vector3 playerViewportPosition = Player.Instance.GetPlayerViewportPosition();

        if (playerViewportPosition.y > 0.3f && IsInventoryBarPositionBottom == false) {
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 2.5f);

            IsInventoryBarPositionBottom = true;

        } else if (playerViewportPosition.y <= 0.3f && IsInventoryBarPositionBottom == true) {
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            IsInventoryBarPositionBottom = false;
        }
    }

    private void ClearInventorySlots() {
        if (inventorySlots.Length > 0) {
            // Loop through inventory slots and update with blank sprite
            for (int i = 0; i < inventorySlots.Length; i++) {
                inventorySlots[i].inventorySlotImage.sprite = blank16x16Sprite;
                inventorySlots[i].itemQuantityText.text = "";
                inventorySlots[i].itemDetails = null;
                inventorySlots[i].itemQuantity = 0;
            }
        }
    }

    private void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryItem> inventoryItemList) {
        if(inventoryLocation == InventoryLocation.Player) {
            ClearInventorySlots();

            if (inventorySlots.Length > 0 && inventoryItemList.Count > 0) {
                // Loop through inventory slots and update with corresponding inventory list item
                for (int i = 0; i < inventorySlots.Length; i++) {
                    if (i < inventoryItemList.Count) {
                        int itemCode = inventoryItemList[i].itemCode;
                        SO_ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);

                        if (itemDetails != null) {
                            // Add images and details to inventory item slot
                            inventorySlots[i].inventorySlotImage.sprite = itemDetails.itemSprite;

                            if(InventoryManager.Instance.GetItemDetails(inventoryItemList[i].itemCode).isStackable) {
                                inventorySlots[i].itemQuantityText.text = inventoryItemList[i].itemQuantity.ToString();
                            } 
                            else {
                                inventorySlots[i].itemQuantityText.text = "";
                            }

                            inventorySlots[i].itemDetails = itemDetails;
                            inventorySlots[i].itemQuantity = inventoryItemList[i].itemQuantity;
                            SetHighlightedInventorySlots(i);
                        }
                    } else {
                        break;
                    }
                }
            }
        }
    }

    #endregion

}
