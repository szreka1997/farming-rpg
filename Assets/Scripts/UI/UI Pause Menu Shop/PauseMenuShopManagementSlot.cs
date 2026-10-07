using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuShopManagementSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, 
                                                          IPointerEnterHandler, IPointerExitHandler,
                                                          IPointerClickHandler
{
    #region Variables

    [SerializeField] private PauseMenuShopManagement shopManagement = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;
    [SerializeField] private int slotNumber = 0;

    [HideInInspector] public SO_ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [HideInInspector] public int price;
    [HideInInspector] public GameObject draggedItem;

    public Image shopManagementSlotImage;
    public Image moneyImage;
    public TextMeshProUGUI itemQuantityText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI priceText;
    public GameObject greyedOutImageGO;
    public Sprite moneySprite;

    private Canvas parentCanvas;
    private UIManager uiManager;

    #endregion


    #region Unity Basic Methods

    private void Awake() {
        parentCanvas = GetComponentInParent<Canvas>();
        uiManager    = GetComponentInParent<UIManager>();
    }

    #endregion


    #region Interface Implementations

    public void OnBeginDrag(PointerEventData eventData) {
        if (itemQuantity != 0) {
            // Instatiate gameobject as dragged item
            draggedItem = Instantiate(shopManagement.inventoryManagementDraggedItemPrefab, shopManagement.transform);

            // Get image for dragged item
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = shopManagementSlotImage.sprite;
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
               (eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>() != null ||
                eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagement>() != null)) {

                ProcessBuying(1);
            }
        }
    }



    public void OnPointerEnter(PointerEventData eventData) {
        // Populate text box with item details
        if (itemQuantity != 0) {
            if(price <= uiManager.playerPurse.GetBalance() &&InventoryManager.Instance.CanAddItem(InventoryLocation.Player, itemDetails.itemCode, 1)) {
                FindObjectOfType<PlayerUIController>().SetCursor(CursorType.Shop);
            }

            // Instantiate inventory text box
            shopManagement.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            shopManagement.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);
            shopManagement.currentlyActiveSlot = slotNumber;

            UIInventoryTextbox inventoryTextBox = shopManagement.inventoryTextBoxGameobject.GetComponent<UIInventoryTextbox>();

            // Set item type description
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            // Populate text box
            inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", 
                                            itemDetails.itemLongDescription, "Requires Level 1", $"Buying Price:  {price:N0}", "", "");

            if(price > uiManager.playerPurse.GetBalance()) {
                inventoryTextBox.SetPriceTextColor(shopManagement.noMoneyTextColor);
            } 
            else {
                inventoryTextBox.SetPriceTextColor(shopManagement.defaultTextColor);
            }

            inventoryTextBox.SetMoney1Image(moneySprite);
            inventoryTextBox.SetMoney2Image(shopManagement.transparent16x16);

            // Set text box position
            if (slotNumber > 7) {
                shopManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
                shopManagement.inventoryTextBoxGameobject.transform.position =
                            new Vector3(transform.position.x + 60f, transform.position.y + 10f, transform.position.z);
            }
            else {
                shopManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
                shopManagement.inventoryTextBoxGameobject.transform.position =
                            new Vector3(transform.position.x + 60f, transform.position.y - 60f, transform.position.z);
            }
        } else {
            FindObjectOfType<PlayerUIController>().SetCursor(CursorType.None);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        shopManagement.DestroyInventoryTextBoxGameobject();
        shopManagement.currentlyActiveSlot = -1;
        FindObjectOfType<PlayerUIController>().SetCursor(CursorType.None);
    }



    public void OnPointerClick(PointerEventData eventData) {
        if(eventData.button == PointerEventData.InputButton.Right) {
            // Get object drag is over
            if(eventData.pointerCurrentRaycast.gameObject != null && 
               eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuShopManagementSlot>() != null &&
               itemDetails != null) {

                ProcessBuying(1);

                if(itemQuantity == 0) {
                    FindObjectOfType<PlayerUIController>().SetCursor(CursorType.None);
                }
            
            }
        } 
        else if(eventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift)) {
            if (itemQuantity == 0)                                                                                           { return; }
            if (uiManager.playerPurse.GetBalance() < price)                                                                  { return; }
            if (InventoryManager.Instance.InventorySlotSpaceLeft(InventoryLocation.Player) == 0 && !itemDetails.isStackable) { return; }
            if (InventoryManager.Instance.InventorySlotSpaceLeft(InventoryLocation.Player) == 0 && 
                !InventoryManager.Instance.CanAddItem(InventoryLocation.Player, itemDetails.itemCode, 0))                    { return; }

            uiManager.blockCanvas.SetActive(true);
            uiManager.InputFieldOn = true;

            UIShopInputField shopInputField = FindObjectOfType<UIShopInputField>();
            shopInputField.InitializeInputFieldForBuying();

            uiManager.slotNumber = slotNumber;
            uiManager.isBuying = true;

            shopInputField.ShopInputFieldSetup(price, itemQuantity, itemDetails.itemCode);

            if (slotNumber > 7) {
                uiManager.inputFieldGameobject.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
                uiManager.inputFieldGameobject.transform.position =
                            new Vector3(transform.position.x + 60, transform.position.y + 10f, transform.position.z);
            }
            else {
                uiManager.inputFieldGameobject.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
                uiManager.inputFieldGameobject.transform.position =
                            new Vector3(transform.position.x + 60f, transform.position.y - 60f, transform.position.z);
            }
        }
    }

    #endregion


    public void ProcessBuying(int itemCount) {
        if(itemQuantity != 0 && InventoryManager.Instance.CanAddItem(InventoryLocation.Player, itemDetails.itemCode, itemCount)) {
            uiManager.currentShop.SetTotalPrice(price * itemCount);

            if(uiManager.currentShop.TransactionTotal() > uiManager.playerPurse.GetBalance()) { return; }

            uiManager.playerPurse.UpdateBalance(-uiManager.currentShop.TransactionTotal());
            InventoryManager.Instance.AddItem(InventoryLocation.Player, itemDetails.itemCode, itemCount);

            uiManager.currentShop.RemoveItem(itemDetails.itemCode, itemCount);

            EventHandler.CallShopInventoryUpdatedEvent();
            EventHandler.CallInventoryUpdatedEvent(InventoryLocation.Player, InventoryManager.Instance.inventoryItemLists[(int) InventoryLocation.Player]);

            if(itemQuantity == 0) {
                shopManagement.DestroyInventoryTextBoxGameobject();
            }
        }
    }


}
