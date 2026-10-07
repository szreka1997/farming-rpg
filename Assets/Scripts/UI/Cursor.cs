using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    #region Variables

    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite transparentCursorSprite = null;
    [SerializeField] private GridCursor gridCursor = null;

    private Canvas canvas;
    private Camera mainCamera;

    private bool cursorIsEnabled = false;
    public bool CursorIsEnabled { 
        get => cursorIsEnabled; 
        set => cursorIsEnabled = value; 
    }

    private bool cursorPositionIsValid = false;
    public bool CursorPositionIsValid {
        get => cursorPositionIsValid; 
        set => cursorPositionIsValid = value; 
    }

    private ItemType selectedItemType;
    public ItemType SelectedItemType { 
        get => selectedItemType; 
        set => selectedItemType = value; 
    }

    private float itemUseRadius = 0f;
    public float ItemUseRadius { 
        get => itemUseRadius; 
        set => itemUseRadius = value;
    }

    #endregion



    #region Unity Basic Methods

    private void Start() {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update() {
        if(CursorIsEnabled) {
            DisplayCursor();
        }
    }

	#endregion



    #region Public Methods

    public void EnableCursor() {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    public void DisableCursor() {
        cursorImage.color = new Color(1f, 1f, 1f, 0f);
        CursorIsEnabled = false;
    }

    public Vector3 GetWorldPositionForCursor() {
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        return worldPosition;
    }

    public Vector2 GetRectTransformPositionForCursor() {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        return RectTransformUtility.PixelAdjustPoint(screenPosition, cursorRectTransform, canvas);
    }

	#endregion


    #region Private Methods


    private void DisplayCursor() {
        Vector3 cursorWorldPosition = GetWorldPositionForCursor();

        // Set cursor sprite
        SetCursorValidity(cursorWorldPosition, Player.Instance.GetPlayerCentrePosition());

        // Get rect transform position for cursor
        cursorRectTransform.position = GetRectTransformPositionForCursor();
    }

    private void SetCursorValidity(Vector3 cursorPosition, Vector3 playerPosition) {
        SetCursorToValid();

        // Check use radius corners
        if(cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f) ||
           cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f) ||
           cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f) ||
           cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)) {

            SetCursorToInvalid();
            return;
        }

        // Check item use radius is valid
        if(Mathf.Abs(cursorPosition.x - playerPosition.x) > ItemUseRadius || Mathf.Abs(cursorPosition.y - playerPosition.y) > ItemUseRadius) {
            SetCursorToInvalid();
            return;
        }

        // Get selected item details
        SO_ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.Player);

        if (itemDetails == null) {
            SetCursorToInvalid();
            return;
        }

        // Determine cursor validity based on inventory item selected and what object the cursor is over
        switch (itemDetails.itemType) {
            case ItemType.WateringTool:
            case ItemType.BreakingTool:
            case ItemType.ChoppingTool:
            case ItemType.HoeingTool:
            case ItemType.ReapingTool:
            case ItemType.CollectingTool:
                if(!SetCursorValidityTool(cursorPosition, playerPosition, itemDetails)) {
                    SetCursorToInvalid();
                    return;
                }
                break;

            default:
                break;
        }
    }

    private void SetCursorToValid() {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;

        gridCursor.DisableCursor();
    }

    private void SetCursorToInvalid() {
        cursorImage.sprite = transparentCursorSprite;
        CursorPositionIsValid = false;

        gridCursor.EnableCursor();
    }

    /// <summary>
    /// Sets the cursor as either valid or invalid for the tool for the target. Returns true if valid or false if invalid
    /// </summary>
    private bool SetCursorValidityTool(Vector3 cursorPosition, Vector3 playerPosition, SO_ItemDetails itemDetails) {
        switch (itemDetails.itemType) {
            case ItemType.ReapingTool:
                return SetCursorValidityReapingTool(cursorPosition, playerPosition, itemDetails);

            default:
                return false;
        }
    }

    private bool SetCursorValidityReapingTool(Vector3 cursorPosition, Vector3 playerPosition, SO_ItemDetails equippedItemDetails) {
        List<Item> itemList = new List<Item>();

        if(HelperMethods.GetComponentsAtCursorLocation<Item>(out itemList, cursorPosition)) {
            if (itemList.Count != 0) {
                foreach (Item item in itemList) {
                    if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.ReapableScenary) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

	#endregion
   

}
