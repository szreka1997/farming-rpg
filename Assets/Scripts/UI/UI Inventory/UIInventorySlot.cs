using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// These interfaces enable drag behavior to be detected
// IBeginDragHandler - respond to drag event as they begin
// IDragHandler - as an item is being draged
// IEndDragHandler - as the player relased the mouse button
// IPointerEnterHandler - get triggered when pointer enters the game object
// IPointerExitHandler  - get triggered when pointer exits the game object
// IPointerClickHandler - respond to mouse click
public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler,
                                              IPointerClickHandler
{
    #region Variables

    [SerializeField] private UIInventoryBar inventoryBar = null;
    [SerializeField] private GameObject itemPrefab = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;
    [SerializeField] private int slotNumber = 0;
    [SerializeField] public Sprite moneySprite;
    [SerializeField] public Sprite transparent16x16;

    [HideInInspector] public SO_ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [HideInInspector] public bool isSelected = false;

    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI itemQuantityText;
    public  GameObject draggedItem;

    private Camera mainCamera;
    private Transform parentItem;
    private Canvas parentCanvas;
    private GridCursor gridCursor;
    private Cursor cursor;

    #endregion



    #region Unity Basic Methods

    private void Awake() {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start() {
        mainCamera = Camera.main;
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();        
    }

    private void OnEnable() {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
        EventHandler.DropSelectedItemEvent += DropSelectedItemAtMousePosition;
        EventHandler.RemoveSelectedItemFromInventoryEvent += RemoveSelectedItemFromInventory;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
        EventHandler.DropSelectedItemEvent -= DropSelectedItemAtMousePosition;
        EventHandler.RemoveSelectedItemFromInventoryEvent -= RemoveSelectedItemFromInventory;
    }

    #endregion



    #region Unity Event Related Methods

    // When an item starts to be dragged from the inventory bar, we gonna execute this code
    public void OnBeginDrag(PointerEventData eventData) {
        if (itemDetails != null) {
            Player.Instance.DisablePlayerInputAndResetMovement();

            // Instatiate gameobject as dragged item
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItemPrefab, inventoryBar.transform);

            // Get image for dragged item
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventorySlotImage.sprite;

            SetSelectedItem();
        }
    }

    // Move the dragged object to our mouse position
    public void OnDrag(PointerEventData eventData) {
        if (draggedItem != null) {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    // When the user lifts the left mouse key
    public void OnEndDrag(PointerEventData eventData) {
        if (draggedItem != null) {
            // Destroy game object as dragged item
            Destroy(draggedItem);

            // If drag ends over inventory bar, get item drag is over and swap them
            if(eventData.pointerCurrentRaycast.gameObject != null &&  
                    eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null) {

                // get the slot number where the drag ended
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;

                // Swap inventory items in inventory list
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.Player, slotNumber, toSlotNumber);

                // Destroy inventory text box
                DestroyInventoryTextBox();

                ClearSelectedItem();
            } else {
                // else attempt to drop the item if it can be dropped
                if (itemDetails.canBeDropped) {
                    DropSelectedItemAtMousePosition();
                }
            }

            Player.Instance.EnablePlayerInput();
        }
    }

    // As the cursor hovers over the UIInvertorySlot
    public void OnPointerEnter(PointerEventData eventData) {
        // Populate text box with item details
        if (itemQuantity != 0) {
            // Instantiate inventory text box
            inventoryBar.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryBar.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            UIInventoryTextbox inventoryTextBox = inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextbox>();

            // Set item type description
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            if(itemQuantity > 1) {
                // Populate text box
                inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", 
                                                itemDetails.itemLongDescription, "Requires Level 1", 
                                                "Sell Price: " + itemDetails.buyingPrice * itemQuantity,
                                                "    (per unit: " + itemDetails.buyingPrice, ")");

                inventoryTextBox.SetMoney1Image(moneySprite);
                inventoryTextBox.SetMoney2Image(moneySprite);
            } else {
                // Populate text box
                inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", 
                                                itemDetails.itemLongDescription, "Requires Level 1", 
                                                "Sell Price: " + itemDetails.buyingPrice * itemQuantity,
                                                "", "");

                inventoryTextBox.SetMoney1Image(moneySprite);
                inventoryTextBox.SetMoney2Image(transparent16x16);
            } 
              

            // Set text box position according to inventory bar position
            if (inventoryBar.IsInventoryBarPositionBottom) {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = 
                        new Vector3(transform.position.x, transform.position.y + 30f, transform.position.z);
            } else {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = 
                        new Vector3(transform.position.x, transform.position.y - 30f, transform.position.z);
            }
        }
    }

    // As the cursor exits the UIInvertorySlot
    public void OnPointerExit(PointerEventData eventData) {
        DestroyInventoryTextBox();
    }

    public void OnPointerClick(PointerEventData eventData) {
        // if left click
        if(eventData.button == PointerEventData.InputButton.Left) {
            // if inventory slot currently selected then deselect
            if (isSelected) {
                ClearSelectedItem();
            } else {
                if (itemQuantity > 0) {
                    SetSelectedItem();
                }
            }
        }
    }

    #endregion



    #region Public Methods

    public void DestroyInventoryTextBox() {
        if (inventoryBar.inventoryTextBoxGameobject != null) {
            Destroy(inventoryBar.inventoryTextBoxGameobject);
        }
    }

    public void SceneLoaded() {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

    public void ClearSelectedItem() {
        inventoryBar.ClearHighlightOnInventorySlots();
        isSelected = false;
        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.Player);

        ClearCursors();
        Player.Instance.ClearCarriedItem();
    }

    #endregion
    


    #region Private Methods

    /// <summary>
    /// Drops the item (if selected) at the current mouse position
    /// </summary>
    private void DropSelectedItemAtMousePosition() {
        if (itemDetails != null && isSelected) {
            if (gridCursor.CursorPositionIsValid) {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint
                        (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
                        
                // Create item from prefab at mouse position
                GameObject itemGameObjectInstance = Instantiate(itemPrefab, 
                                                                new Vector3(worldPosition.x, worldPosition.y - Settings.gridCellSize / 2f, 0f), 
                                                                Quaternion.identity, 
                                                                parentItem);
                                                                
                Item item = itemGameObjectInstance.GetComponent<Item>();
                item.ItemCode = itemDetails.itemCode;

                // Remove item from players inventory
                InventoryManager.Instance.RemoveItem(InventoryLocation.Player, item.ItemCode, 1, -1);

                // If no more of item then clear selected
                if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.Player, item.ItemCode) == -1) {
                    ClearSelectedItem();
                }
            }
        }
    }

    private void RemoveSelectedItemFromInventory() {
        if (itemDetails != null && isSelected) {
            int itemCode = itemDetails.itemCode;

            // Remove item from players inventory
            InventoryManager.Instance.RemoveItem(InventoryLocation.Player, itemCode, 1, -1);

            // If no more of item then clear selected
            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.Player, itemCode) == -1) {
                ClearSelectedItem();
            }
        }
    }

    /// <summary>
    /// Sets this inventory slot item to be selected
    /// </summary>
    private void SetSelectedItem() {
        inventoryBar.ClearHighlightOnInventorySlots();
        isSelected = true;
        inventoryBar.SetHighlightedInventorySlots();

        // Set use radius for cursors
        gridCursor.ItemUseGridRadius = itemDetails.itemsUseGridRadius;
        cursor.ItemUseRadius = itemDetails.itemUseRadius;

        // If item requires a grid cursor then enable cursor
        if (itemDetails.itemsUseGridRadius > 0) {
            gridCursor.EnableCursor();
        } else {
            gridCursor.DisableCursor();
        }

        // If item requires a cursor then enable cursor
        if (itemDetails.itemUseRadius > 0f) {
            cursor.EnableCursor();
        } else {
            cursor.DisableCursor();
        }

        // Set item type
        gridCursor.SelectedItemType = itemDetails.itemType;
        cursor.SelectedItemType = itemDetails.itemType;

        // Set item selected in inventory
        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.Player, itemDetails.itemCode);

        if (itemDetails.canBeCarried == true) {
            Player.Instance.ShowCarriedItem(itemDetails.itemCode);
        } else {
            Player.Instance.ClearCarriedItem();
        }
    }

    private void ClearCursors() {
        gridCursor.DisableCursor();
        cursor.DisableCursor();

        gridCursor.SelectedItemType = ItemType.None;
        cursor.SelectedItemType = ItemType.None;
    }

    #endregion


}
