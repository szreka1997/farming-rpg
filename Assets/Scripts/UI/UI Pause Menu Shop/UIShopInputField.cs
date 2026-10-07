using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopInputField : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] TextMeshProUGUI totalPriceText;

    private TMP_InputField inputField;
    private UIManager uiManager;
    private int price;
    private int totalPrice;   
    private int quantityInShop;
    private int inputNumber;
    private int itemCode;


    private void Awake() {
        inputField = GetComponent<TMP_InputField>();
        uiManager = GetComponentInParent<UIManager>();
    }

    private void Update() {
        if(gameObject.activeSelf && price != 0) {
            if(inputField.text == "") {
                totalPrice  = 0;
                inputNumber = 0;
            } 
            else if(Int32.TryParse(inputField.text, out inputNumber)) {
                if(inputNumber > quantityInShop) {
                    inputField.text = quantityInShop.ToString();
                    inputNumber = quantityInShop;
                }
                else if(inputNumber < 0) {
                    inputField.text = "0";
                    inputNumber = 0;
                }

                if (uiManager.isBuying &&
                    !InventoryManager.Instance.GetItemDetails(itemCode).isStackable && 
                    InventoryManager.Instance.InventorySlotSpaceLeft(InventoryLocation.Player) < inputNumber) {

                    inputNumber = InventoryManager.Instance.InventorySlotSpaceLeft(InventoryLocation.Player);
                    inputField.text = inputNumber.ToString();
                }
    
                totalPrice = price * inputNumber;

                if(totalPrice > uiManager.playerPurse.GetBalance() && uiManager.isBuying) {
                    inputNumber = (uiManager.playerPurse.GetBalance() / price);
                    totalPrice = price * inputNumber;
                    inputField.text = inputNumber.ToString();
                }
            }

            totalPriceText.text = "Total: " + totalPrice;
        }
    }


    #region Initialise Methods

    public void InitializeInputFieldForBuying() {
        inputField.text = "1";
        inputField.Select();
        buttonText.text = "Buy";

        price = 0;
        totalPrice = 0;
        quantityInShop = 0;
        inputNumber = 0;
        itemCode = 0;
    }

    public void InitializeInputFieldForSelling() {
        inputField.text = "1";
        inputField.Select();
        buttonText.text = "Sell";

        price = 0;
        totalPrice = 0;
        quantityInShop = 0;
        inputNumber = 0;
        itemCode = 0;
    }

    #endregion


    #region Getters / Setters
    
    public void ShopInputFieldSetup(int price, int quantityInShop, int itemCode) {
        this.price = price;
        this.quantityInShop = quantityInShop;
        this.itemCode = itemCode;
    }

    public int GetTotalPrice() {
        return totalPrice;
    }    

    public int GetEnteredNumber() {
        return inputNumber;
    }

    #endregion

}
