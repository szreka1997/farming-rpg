using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuInventoryManagementSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, 
                                                               IPointerEnterHandler, IPointerExitHandler,
                                                               IPointerClickHandler
{
    #region Variables

    [SerializeField] private PauseMenuInventoryManagement inventoryManagement = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;
    [SerializeField] private int slotNumber = 0;
    
    [HideInInspector] public SO_ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;

    public Image inventoryManagementSlotImage;
    public TextMeshProUGUI textMeshProUGUI;
    public GameObject greyedOutImageGO;
    public GameObject draggedItem;
    public int price;

    private Canvas parentCanvas;
    private UIManager uiManager;
    private PlayerUIController playerUIController;

    #endregion


    #region Unity Basic Methods

    private void Awake() {
        parentCanvas = GetComponentInParent<Canvas>();
        uiManager = GetComponentInParent<UIManager>();
        playerUIController = FindObjectOfType<PlayerUIController>();
    }

    #endregion



    #region Interface Implementations

    public void OnBeginDrag(PointerEventData eventData) {
        if (itemQuantity != 0) {
            // Instatiate gameobject as dragged item
            draggedItem = Instantiate(inventoryManagement.inventoryManagementDraggedItemPrefab, inventoryManagement.transform);

            // Get image for dragged item
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventoryManagementSlotImage.sprite;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        // move game object as dragged item
        if (draggedItem != null) {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        // Destroy game object as dragged item
        if (draggedItem != null) {
            Destroy(draggedItem);

            // Get object drag is over
            if(eventData.pointerCurrentRaycast.gameObject != null && 
               eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>() != null) {

                // get the slot number where the drag ended
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>().slotNumber;

                // Swap inventory items in inventory list
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.Player, slotNumber, toSlotNumber);

                // Destroy inventory text box
                inventoryManagement.DestroyInventoryTextBoxGameobject();
            }

            if(eventData.pointerCurrentRaycast.gameObject != null && 
               (eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuShopManagementSlot>() != null ||
                eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuShopManagement>() != null)) {

                ProcessSelling(1);
            }
        }
    }



    public void OnPointerEnter(PointerEventData eventData) {
        // Populate text box with item details
        if (itemQuantity != 0) {
            if(uiManager.ShopMenuOn) {
                playerUIController.SetCursor(CursorType.Shop);
            } 
            else {
                playerUIController.SetCursor(CursorType.None);
            }

            // Instantiate inventory text box
            inventoryManagement.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryManagement.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            UIInventoryTextbox inventoryTextBox = inventoryManagement.inventoryTextBoxGameobject.GetComponent<UIInventoryTextbox>();

            // Set item type description
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            if(itemQuantity > 1) {
                // Populate text box
                inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", 
                                                itemDetails.itemLongDescription, "Requires Level 1", 
                                                "Sell Price: " + price * itemQuantity,
                                                "    (per unit: " + price, ")");

                inventoryTextBox.SetMoney1Image(inventoryManagement.moneySprite);
                inventoryTextBox.SetMoney2Image(inventoryManagement.moneySprite);
            } else {
                // Populate text box
                inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", 
                                                itemDetails.itemLongDescription, "Requires Level 1", 
                                                "Sell Price: " + price * itemQuantity,
                                                "", "");

                inventoryTextBox.SetMoney1Image(inventoryManagement.moneySprite);
                inventoryTextBox.SetMoney2Image(inventoryManagement.transparent16x16);
            } 

            // Set text box position
            if (slotNumber > 23) {
                inventoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
                inventoryManagement.inventoryTextBoxGameobject.transform.position = 
                            new Vector3(transform.position.x, transform.position.y + 10f, transform.position.z);
            }
            else {
                inventoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
                inventoryManagement.inventoryTextBoxGameobject.transform.position = 
                            new Vector3(transform.position.x, transform.position.y - 60f, transform.position.z);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        inventoryManagement.DestroyInventoryTextBoxGameobject();
        playerUIController.SetCursor(CursorType.None);
    }



    public void OnPointerClick(PointerEventData eventData) {
        if(eventData.button == PointerEventData.InputButton.Right) {
            if(uiManager.currentShop != null) {
                // Get object drag is over
                if(eventData.pointerCurrentRaycast.gameObject != null && 
                eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>() != null &&
                itemDetails != null) {

                    ProcessSelling(itemQuantity);

                    if(itemDetails == null) {
                        FindObjectOfType<PlayerUIController>().SetCursor(CursorType.None);
                    }
                
                }
            }
        }
        else if(eventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift)) {
            if (itemQuantity != 0 && itemDetails.isStackable) {
                uiManager.blockCanvas.SetActive(true);
                uiManager.InputFieldOn = true;

                UIShopInputField shopInputField = FindObjectOfType<UIShopInputField>();
                shopInputField.InitializeInputFieldForSelling();

                uiManager.slotNumber = slotNumber;
                uiManager.isBuying = false;

                shopInputField.ShopInputFieldSetup(price, itemQuantity, itemDetails.itemCode);

                if (slotNumber > 7) {
                    uiManager.inputFieldGameobject.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
                    uiManager.inputFieldGameobject.transform.position =
                                new Vector3(transform.position.x, transform.position.y + 10f, transform.position.z);
                }
                else {
                    uiManager.inputFieldGameobject.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
                    uiManager.inputFieldGameobject.transform.position =
                                new Vector3(transform.position.x, transform.position.y - 60f, transform.position.z);
                }
            }
        }
    }

    #endregion


    public void ProcessSelling(int itemCount) {
        if(uiManager.currentShop != null) {
            Debug.Log(itemDetails.itemDescription + "-bol " + itemCount + " db eladva.");

            InventoryManager.Instance.RemoveItem(InventoryLocation.Player, itemDetails.itemCode, itemCount, slotNumber);

            uiManager.currentShop.SetTotalPrice(price * itemCount);

            uiManager.playerPurse.UpdateBalance(uiManager.currentShop.TransactionTotal());

            EventHandler.CallShopInventoryUpdatedEvent();
            EventHandler.CallInventoryUpdatedEvent(InventoryLocation.Player, InventoryManager.Instance.inventoryItemLists[(int) InventoryLocation.Player]);

            // Destroy inventory text box
            inventoryManagement.DestroyInventoryTextBoxGameobject();
        }
    }


}
