using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>, ISaveable
{
    #region Variables
    [SerializeField] private SO_ItemList itemList = null;

    // The index of the array is the inventory list (from the InventoryLocation enum), and the value is the capacity of that inventory list
    //     [0] - capacity of player inventory list
    //     [1] - capacity of chest inventory list
    [HideInInspector] public int[] inventoryListCapacityIntArray; 

    // We need array, because there is more inventory location
    //     [0] - player inventory list
    //     [1] - chest inventory list
    public List<InventoryItem>[] inventoryItemLists;

    //                <Key ,     Value    >
    private Dictionary<int, SO_ItemDetails> itemDetailsDictionary;

    // The index of the array is the inventory list, and the value is the item code
    private int[] selectedInventoryItem; 

    private UIInventoryBar inventoryBar;

    #endregion


    #region Properties

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID      { get => _iSaveableUniqueID;  set => _iSaveableUniqueID = value;  }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave;  set => _gameObjectSave = value;  }

    #endregion


    #region Unity Basic Methods

    protected override void Awake() {
        base.Awake();
        
        CreateInventoryItemLists();
        CreateItemDetailsDictionary();

        // Initailise selected inventory item array
        selectedInventoryItem = new int[(int)InventoryLocation.Count];

        for(int i = 0; i < selectedInventoryItem.Length; i++) {
            // -1 - we havent got an item selected
            selectedInventoryItem[i] = -1;
        }

        // Get unique ID for gameobject and create save data object
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start() {
        inventoryBar = FindObjectOfType<UIInventoryBar>();
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        ISaveableRegister();
    }

    private void OnDisable() {
        ISaveableDeregister();
    }

    #endregion


    #region Interface Implementation

    public void ISaveableRegister() {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister() {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableStoreScene(string sceneName) {
        // Nothing required her since the inventory manager is on a persistent scene;
    }

    public void ISaveableRestoreScene(string sceneName) {
        // Nothing required here since the inventory manager is on a persistent scene;
    }

    public GameObjectSave ISaveableSave() {
        // Create new scene save
        SceneSave sceneSave = new SceneSave();

        // Remove any existing scene save for persistent scene for this gameobject
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // Add inventory lists array to persistent scene save
        sceneSave.listInvItemArray = inventoryItemLists;

        // Add  inventory list capacity array to persistent scene save
        sceneSave.intArrayDictionary = new Dictionary<string, int[]>();
        sceneSave.intArrayDictionary.Add("inventoryListCapacityArray", inventoryListCapacityIntArray);

        // Add scene save for gameobject
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave) {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave)) {
            GameObjectSave = gameObjectSave;

            // Need to find inventory lists - start by trying to locate saveScene for game object
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave)) {
                // list inv items array exists for persistent scene
                if (sceneSave.listInvItemArray != null) {
                    inventoryItemLists = sceneSave.listInvItemArray;

                    //  Send events that inventory has been updated
                    for (int i = 0; i < (int)InventoryLocation.Count; i++) {
                        EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryItemLists[i]);
                    }

                    // Clear any items player was carrying
                    Player.Instance.ClearCarriedItem();

                    // Clear any highlights on inventory bar
                    inventoryBar.ClearHighlightOnInventorySlots();
                }

                // int array dictionary exists for scene
                if (sceneSave.intArrayDictionary != null && 
                    sceneSave.intArrayDictionary.TryGetValue("inventoryListCapacityArray", out int[] inventoryCapacityArray)) {

                    inventoryListCapacityIntArray = inventoryCapacityArray;
                }
            }

        }
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Returns the itemDetails (from the SO_ItemList) for the itemCode, or null if the code doesn't exist
    /// </summary>
    public SO_ItemDetails GetItemDetails(int itemCode) {
        SO_ItemDetails itemDetails;

        if(itemDetailsDictionary.TryGetValue(itemCode, out itemDetails)) {
            return itemDetails;
        } else {
            return null;
        }
    }

    public bool CanAddItem(InventoryLocation inventoryLocation, int itemCode, int itemCount) {
        if (FindItemInInventory(inventoryLocation, itemCode) == -1) {
            if(inventoryItemLists[(int) inventoryLocation].Count >= inventoryListCapacityIntArray[(int)inventoryLocation]) {
                return false;
            }
        }
        else {
            if(!GetItemDetails(itemCode).isStackable) {
                if(inventoryItemLists[(int) inventoryLocation].Count + itemCount > inventoryListCapacityIntArray[(int)inventoryLocation]) {
                    return false;
                }
            }
        }

        return true;
    }

    public int InventorySlotSpaceLeft(InventoryLocation inventoryLocation) {
        return (inventoryListCapacityIntArray[(int)inventoryLocation] - inventoryItemLists[(int) inventoryLocation].Count);
    }

    /// <summary>
    /// Add an item to the inventoryItemList for the inventoryLocation and then destroy the gameObjectToDelete
    /// </summary>
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete) {
        AddItem(inventoryLocation, item.ItemCode, 1);
        Destroy(gameObjectToDelete);
    }
    
    public void AddItem(InventoryLocation inventoryLocation, int itemCode, int itemCount) {
        if (itemCount <= 0)                                      { return; }
        if (!CanAddItem(inventoryLocation, itemCode, itemCount)) { return; }

        List<InventoryItem> localInventoryItemList = inventoryItemLists[(int)inventoryLocation];

        // Check if inventory already contains the item
        int position = FindItemInInventory(inventoryLocation, itemCode);

        if(GetItemDetails(itemCode).isStackable) {
            if (position != -1) {
                InventoryItem inventoryItem = new InventoryItem();
                int quantity = localInventoryItemList[position].itemQuantity + itemCount;

                inventoryItem.itemQuantity = quantity;
                inventoryItem.itemCode = itemCode;
                localInventoryItemList[position] = inventoryItem;
            }
            else {
                InventoryItem inventoryItem = new InventoryItem();

                inventoryItem.itemCode = itemCode;
                inventoryItem.itemQuantity = itemCount;
                localInventoryItemList.Add(inventoryItem);
            }
        }
        else {
            for(int i = 0; i < itemCount; i++) {
                InventoryItem inventoryItem = new InventoryItem();

                inventoryItem.itemCode = itemCode;
                inventoryItem.itemQuantity = 1;
                localInventoryItemList.Add(inventoryItem);
            } 
        }

        

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryItemLists[(int)inventoryLocation]);
    }

    /// <summary>
    /// Find if an itemCode is already in the inventory. Returns the item position in the inventory list, or -1 if the item is not in the inventory
    /// </summary>
    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode) {
        List<InventoryItem> localInventoryItemList = inventoryItemLists[(int)inventoryLocation];

        for (int i = 0; i < localInventoryItemList.Count; i++) {
            if (localInventoryItemList[i].itemCode == itemCode) {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Swap item at fromItemIndex with item at toItemIndex in inventoryLocation inventoryItemList
    /// </summary>
    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItemIndex, int toItemIndex) {
        // if fromItem index and toItemIndex are within the bounds of the list, not the same, and greater than or equal to zero
        if (fromItemIndex < inventoryItemLists[(int)inventoryLocation].Count && toItemIndex < inventoryItemLists[(int)inventoryLocation].Count
                && fromItemIndex != toItemIndex && fromItemIndex >= 0 && toItemIndex >= 0) {

            InventoryItem fromInventoryItem = inventoryItemLists[(int)inventoryLocation][fromItemIndex];
            InventoryItem toInventoryItem   = inventoryItemLists[(int)inventoryLocation][toItemIndex];

            inventoryItemLists[(int)inventoryLocation][toItemIndex] = fromInventoryItem;
            inventoryItemLists[(int)inventoryLocation][fromItemIndex] = toInventoryItem;

            //  Send event that inventory has been updated
            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryItemLists[(int)inventoryLocation]);
        }
    }

    /// <summary>
    /// Get the item type description for an item type - returns the item type description as a string for a given ItemType
    /// </summary>
    public string GetItemTypeDescription(ItemType itemType) {
        string itemTypeDescription;
        switch (itemType) {
            case ItemType.BreakingTool:
                itemTypeDescription = Settings.BreakingTool;
                break;

            case ItemType.ChoppingTool:
                itemTypeDescription = Settings.ChoppingTool;
                break;

            case ItemType.HoeingTool:
                itemTypeDescription = Settings.HoeingTool;
                break;

            case ItemType.ReapingTool:
                itemTypeDescription = Settings.ReapingTool;
                break;

            case ItemType.WateringTool:
                itemTypeDescription = Settings.WateringTool;
                break;

            case ItemType.CollectingTool:
                itemTypeDescription = Settings.CollectingTool;
                break;

            default:
                itemTypeDescription = itemType.ToString();
                break;
        }

        return itemTypeDescription;
    }

    /// <summary>
    /// If position == -1 then no particular position, it will remove the first itemCode item in the list
    /// </summary>
    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode, int itemCount, int position) {
        List<InventoryItem> inventoryList = inventoryItemLists[(int)inventoryLocation];

        int itemPosition = -1;

        // Check if inventory already contains the item
        if(position == -1) {
            itemPosition = FindItemInInventory(inventoryLocation, itemCode);
        }
        else {
            itemPosition = position;
        }

        if (itemPosition != -1) {
            InventoryItem inventoryItem = new InventoryItem();
            int quantity = inventoryList[itemPosition].itemQuantity - itemCount;

            if (quantity > 0) {
                inventoryItem.itemCode = itemCode;
                inventoryItem.itemQuantity = quantity;
                inventoryList[itemPosition] = inventoryItem;
            } else {
                inventoryList.RemoveAt(itemPosition);
            }
        }

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryItemLists[(int)inventoryLocation]);

    }

    /// <summary>
    /// Set the selected inventory item for inventoryLocation to itemCode
    /// </summary>
    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode) {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }

    /// <summary>
    /// Clear the selected inventory item for inventoryLocation
    /// </summary>
    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation) {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }

    /// <summary>
    /// Returns the itemDetails (from the SO_ItemList) for the currently selected item in the inventoryLocation , or null if an item isn't selected
    /// </summary>
    public SO_ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation) {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);

        if (itemCode == -1) {
            return null;
        } else {
            return GetItemDetails(itemCode);
        }
    }

    #endregion



    #region Private Methods

    /// <summary>
    /// Populates the itemDetailsDictionary from the scriptable object items list 
    /// </summary>
    private void CreateItemDetailsDictionary() {
        itemDetailsDictionary = new Dictionary<int, SO_ItemDetails>();

        foreach(SO_ItemDetails itemDetails in itemList.itemDetails) {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    /// <summary>
    /// Initialize the inventoryLists and inventoryCapacityIntArray arrays
    /// </summary>
    private void CreateInventoryItemLists() {
        inventoryItemLists = new List<InventoryItem>[(int)InventoryLocation.Count];

        for (int i = 0; i < (int)InventoryLocation.Count; i++) {
            inventoryItemLists[i] = new List<InventoryItem>();
        }

        // Initialise inventory list capacity array
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.Count];

        // Initialise player inventory list capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.Player] = Settings.playerInitialInventoryCapacity;
    }

    /// <summary>
    /// Get the selected item for inventoryLocation - returns itemCode or -1 if nothing is selected
    /// </summary>
    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation) {
        return selectedInventoryItem[(int)inventoryLocation];
    }

    #endregion



    /// <summary>
    /// Print to the console the inventory state
    /// </summary>
    /* private void DebugPrintInventoryList(List<InventoryItem> inventoryItemList) {
        foreach (InventoryItem inventoryItem in inventoryItemList) {
            Debug.Log("Item Description:" + GetItemDetails(inventoryItem.itemCode).itemDescription 
                    + "    Item Quantity: " + inventoryItem.itemQuantity);
        }
        
        Debug.Log("******************************************************************************");
    }
    */









}
