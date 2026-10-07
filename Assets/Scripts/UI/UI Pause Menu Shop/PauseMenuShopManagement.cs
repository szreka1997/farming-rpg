using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuShopManagement : MonoBehaviour
{
    #region Variables

    [SerializeField] private PauseMenuShopManagementSlot[] shopManagementSlotArray = null;
    [SerializeField] public Sprite transparent16x16 = null;
    [SerializeField] private Sprite moneySprite;

    [SerializeField] public GameObject inventoryManagementDraggedItemPrefab;
    [SerializeField] public Color noMoneyTextColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] public Color defaultTextColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] public Color noMoneyImageColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] public Color defaultImageColor = new Color(1f, 1f, 1f, 1f);

    [HideInInspector] public GameObject inventoryTextBoxGameobject;

    private UIManager uiManager;
    public int currentlyActiveSlot = -1;

    #endregion


    #region Unity basic Methods

    private void Awake() {
        uiManager = GetComponentInParent<UIManager>();
    }

    private void Start() {
        EventHandler.ShopInventoryUpdatedEvent += PopulateShop;
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        //FindObjectOfType<PlayerUIController>().SetCursor(CursorType.None);
    }

    private void OnDisable() {
        DestroyInventoryTextBoxGameobject();
    }

    #endregion


    #region  Subscriber Methods

    private void PopulateShop() {
        if(uiManager.currentShop != null) {
            int i = 0;
            InitialiseShopManagementSlots();

            // loop through all player inventory items
            foreach (ShopItem shopItem in uiManager.currentShop.GetFilteredItems()) {
                if(i < shopManagementSlotArray.Length) {
                    shopManagementSlotArray[i].greyedOutImageGO.SetActive(shopItem.GetQuantity() == 0);

                    shopManagementSlotArray[i].itemDetails  = InventoryManager.Instance.GetItemDetails(shopItem.GetItemCode());
                    shopManagementSlotArray[i].itemQuantity = shopItem.GetQuantity();
                    shopManagementSlotArray[i].price        = shopItem.GetPrice();

                    if (shopManagementSlotArray[i].itemDetails != null) {
                        shopManagementSlotArray[i].shopManagementSlotImage.sprite = shopManagementSlotArray[i].itemDetails.itemSprite;
                        shopManagementSlotArray[i].moneyImage.sprite              = moneySprite;
                        shopManagementSlotArray[i].itemQuantityText.text    = shopManagementSlotArray[i].itemQuantity.ToString();

                        shopManagementSlotArray[i].itemDescriptionText.text = shopManagementSlotArray[i].itemDetails.itemDescription;
                        shopManagementSlotArray[i].priceText.text           = $"{shopItem.GetPrice():N0}  ";

                        if(!InventoryManager.Instance.CanAddItem(InventoryLocation.Player, shopManagementSlotArray[i].itemDetails.itemCode, 1) &&
                                currentlyActiveSlot == i) {

                            FindObjectOfType<PlayerUIController>().SetCursor(CursorType.None);
                        }

                        if(shopItem.GetPrice() > uiManager.playerPurse.GetBalance()) {
                            shopManagementSlotArray[i].priceText.color = noMoneyTextColor;

                            if(shopItem.GetQuantity() != 0) { 
                                shopManagementSlotArray[i].greyedOutImageGO.SetActive(true);
                                shopManagementSlotArray[i].greyedOutImageGO.GetComponent<Image>().color = noMoneyImageColor;
                                if(currentlyActiveSlot == i) {
                                    FindObjectOfType<PlayerUIController>().SetCursor(CursorType.None);
                                }
                            } 
                        } 
                        else {
                            shopManagementSlotArray[i].priceText.color = defaultTextColor;
                            shopManagementSlotArray[i].greyedOutImageGO.GetComponent<Image>().color = defaultImageColor;
                            shopManagementSlotArray[i].greyedOutImageGO.SetActive(shopItem.GetQuantity() == 0);
                        }
                    }
                }

                i++;
            }
        }
    }


    #endregion

    public void ActiveShopChanged(){
        if(uiManager.currentShop != null) {
            PopulateShop();
        }
    }


    public void DestroyInventoryTextBoxGameobject() {
        UIInventoryTextbox[] textboxArray = FindObjectsOfType<UIInventoryTextbox>();
        
        foreach(UIInventoryTextbox texbox in textboxArray) {
            Destroy(texbox.gameObject);
        }
    }

    public void DestroyCurrentlyDraggedItems() {
        for (int i = 0; i < shopManagementSlotArray.Length; i++) {
            if (shopManagementSlotArray[i].draggedItem != null) {
                Destroy(shopManagementSlotArray[i].draggedItem);
            }
        }
    }

    public void ProcessBuying(int slotNumber, int quantityInTransaction) {
        shopManagementSlotArray[slotNumber].ProcessBuying(quantityInTransaction);
    }
   

    private void InitialiseShopManagementSlots() {
        for (int i = 0; i < shopManagementSlotArray.Length; i++) {
            shopManagementSlotArray[i].greyedOutImageGO.SetActive(true);
            shopManagementSlotArray[i].greyedOutImageGO.GetComponent<Image>().color = defaultImageColor;
            shopManagementSlotArray[i].shopManagementSlotImage.sprite = transparent16x16;
            shopManagementSlotArray[i].moneyImage.sprite              = transparent16x16;

            shopManagementSlotArray[i].itemDetails  = null;
            shopManagementSlotArray[i].itemQuantity = 0;

            shopManagementSlotArray[i].itemQuantityText.text    = "";
            shopManagementSlotArray[i].priceText.text           = "";
            shopManagementSlotArray[i].itemDescriptionText.text = "";
        }
    }


}
