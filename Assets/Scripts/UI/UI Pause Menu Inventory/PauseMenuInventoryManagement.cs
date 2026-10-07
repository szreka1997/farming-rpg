using System.Collections.Generic;
using UnityEngine;

public class PauseMenuInventoryManagement : MonoBehaviour
{
    #region Variables

    [SerializeField] private PauseMenuInventoryManagementSlot[] inventoryManagementSlotArray = null;
    [SerializeField] public Sprite transparent16x16 = null;
    
    [SerializeField] public Sprite moneySprite;
    [SerializeField] public GameObject inventoryManagementDraggedItemPrefab;

    [HideInInspector] public GameObject inventoryTextBoxGameobject;

    #endregion



    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.InventoryUpdatedEvent += PopulatePlayerInventory;

        // Populate player inventory
        if (InventoryManager.Instance != null) {
            PopulatePlayerInventory(InventoryLocation.Player, InventoryManager.Instance.inventoryItemLists[(int)InventoryLocation.Player]);
        }
    }

    private void OnDisable() {
        EventHandler.InventoryUpdatedEvent -= PopulatePlayerInventory;

        DestroyInventoryTextBoxGameobject();
    }

    #endregion


    #region Event Subscriber Methods

    private void PopulatePlayerInventory(InventoryLocation inventoryLocation, List<InventoryItem> playerInventoryList) {
        if (inventoryLocation == InventoryLocation.Player) {
            InitialiseInventoryManagementSlots();

            // loop through all player inventory items
            for (int i = 0; i < playerInventoryList.Count; i++) {
                // Get inventory item details
                inventoryManagementSlotArray[i].itemDetails = InventoryManager.Instance.GetItemDetails(playerInventoryList[i].itemCode);
                inventoryManagementSlotArray[i].itemQuantity = playerInventoryList[i].itemQuantity;

                if (inventoryManagementSlotArray[i].itemDetails != null) {
                    // update inventory management slot with image and quantity
                    inventoryManagementSlotArray[i].inventoryManagementSlotImage.sprite = inventoryManagementSlotArray[i].itemDetails.itemSprite;
                    
                    if(InventoryManager.Instance.GetItemDetails(inventoryManagementSlotArray[i].itemDetails.itemCode).isStackable) {
                        inventoryManagementSlotArray[i].textMeshProUGUI.text = inventoryManagementSlotArray[i].itemQuantity.ToString();
                    } 
                    else {
                        inventoryManagementSlotArray[i].textMeshProUGUI.text = "";
                    }

                    inventoryManagementSlotArray[i].price = (int)(inventoryManagementSlotArray[i].itemDetails.buyingPrice * Settings.sellingValuePercentage);
                }
            }
        }
    }

    #endregion



    public void DestroyInventoryTextBoxGameobject() {
        UIInventoryTextbox[] textboxArray = FindObjectsOfType<UIInventoryTextbox>();
        
        foreach(UIInventoryTextbox texbox in textboxArray) {
            Destroy(texbox.gameObject);
        }
    }

    public void DestroyCurrentlyDraggedItems() {
        // loop through all player inventory items
        for (int i = 0; i < InventoryManager.Instance.inventoryItemLists[(int)InventoryLocation.Player].Count; i++) {
            if (inventoryManagementSlotArray[i].draggedItem != null) {
                Destroy(inventoryManagementSlotArray[i].draggedItem);
            }

        }
    }

    public void ProcessSelling(int slotNumber, int quantityInTransaction) {
        inventoryManagementSlotArray[slotNumber].ProcessSelling(quantityInTransaction);
    }


    private void InitialiseInventoryManagementSlots() {
        // Clear inventory slots
        for (int i = 0; i < Settings.playerMaximumInventoryCapacity; i++) {
            inventoryManagementSlotArray[i].greyedOutImageGO.SetActive(false);
            inventoryManagementSlotArray[i].itemDetails = null;
            inventoryManagementSlotArray[i].itemQuantity = 0;
            inventoryManagementSlotArray[i].inventoryManagementSlotImage.sprite = transparent16x16;
            inventoryManagementSlotArray[i].textMeshProUGUI.text = "";
        }

        // Grey out unavailable slots
        for (int i = InventoryManager.Instance.inventoryListCapacityIntArray[(int)InventoryLocation.Player]; 
                 i < Settings.playerMaximumInventoryCapacity; i++) {

            inventoryManagementSlotArray[i].greyedOutImageGO.SetActive(true);
        }
    }

}