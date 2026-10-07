using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCursor : MonoBehaviour
{
    #region Variables

    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite redCursorSprite = null;
    [SerializeField] private SO_CropDetailsList so_CropDetailsList = null;

    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;

    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { 
        get => _cursorPositionIsValid; 
        set => _cursorPositionIsValid = value; 
    }

    private int _itemUseGridRadius = 0;
    public int ItemUseGridRadius { 
        get => _itemUseGridRadius; 
        set => _itemUseGridRadius = value; 
    }

    private ItemType _selectedItemType;
    public ItemType SelectedItemType { 
        get => _selectedItemType; 
        set => _selectedItemType = value; 
    }

    private bool _cursorIsEnabled = false;
    public bool CursorIsEnabled { 
        get => _cursorIsEnabled; 
        set => _cursorIsEnabled = value; 
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



	#region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

	#endregion



	#region Public Methods

    public Vector3Int GetGridPositionForCursor() {
        // z is how far the objects are in front of the camera - camera is at -10 so objects are (-)-10 in front = 10
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));  
        
        return grid.WorldToCell(worldPosition);
    }

    public Vector3Int GetGridPositionForPlayer() {
        return grid.WorldToCell(Player.Instance.transform.position);
    }

    public void EnableCursor() {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    public void DisableCursor() {
        cursorImage.color = Color.clear;
        CursorIsEnabled = false;
    }

    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition) {
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);

        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);
    }

    public Vector3 GetWorldPositionForCursor() {
        // We want the grid position for it first, because we want the bottom left coordinate of a cell
        return grid.CellToWorld(GetGridPositionForCursor());
    }

	#endregion



	#region Private Methods

    private void SceneLoaded() {
        grid = GameObject.FindObjectOfType<Grid>();
    }

    private Vector3Int DisplayCursor() {
        if (grid != null) {
            // Get grid position for cursor
            Vector3Int cursorGridPosition = GetGridPositionForCursor();

            // Get grid position for player
            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            // Set cursor sprite
            SetCursorValidity(cursorGridPosition, playerGridPosition);

            // Get rect transform position for cursor
            cursorRectTransform.position = GetRectTransformPositionForCursor(cursorGridPosition);

            return cursorGridPosition;
        } else {
            return Vector3Int.zero;
        }
    }

    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition) {
        SetCursorToValid();

        // Check item use radius is valid
        if(Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius 
                || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius) {

            SetCursorToInvalid();
            return;
        }

        // Get selected item details
        SO_ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.Player);

        if (itemDetails == null) {
            SetCursorToInvalid();
            return;
        }

        // Get grid property details at cursor position
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        if (gridPropertyDetails != null) {
            // Determine cursor validity based on inventory item selected and grid property details
            switch (itemDetails.itemType) {
                case ItemType.Seed:
                    if (!IsCursorValidForSeed(gridPropertyDetails)) {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.Commodity:
                    if (!IsCursorValidForCommodity(gridPropertyDetails)) {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.WateringTool:
                case ItemType.BreakingTool:
                case ItemType.ChoppingTool:
                case ItemType.HoeingTool:
                case ItemType.ReapingTool:
                case ItemType.CollectingTool:
                    if (!IsCursorValidForTool(gridPropertyDetails, itemDetails)) {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                default:
                    break;
            }
        } else {
            SetCursorToInvalid();
            return;
        }
    }

    private void SetCursorToValid() {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
    }

    private void SetCursorToInvalid() {
        cursorImage.sprite = redCursorSprite;
        CursorPositionIsValid = false;
    }

    /// <summary>
    /// Set cursor validity for a seed for the target gridPropertyDetails. Returns true if valid, false if invalid
    /// </summary>
    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails) {
        return gridPropertyDetails.canDropItem;
    }

    /// <summary>
    /// Test cursor validity for a commodity for the target gridPropertyDetails. Returns true if valid, false if invalid
    /// </summary>
    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails) {
        return gridPropertyDetails.canDropItem;
    }

    /// <summary>
    /// Sets the cursor as either valid or invalid for the tool for the target gridPropertyDetails. Returns true if valid or false if invalid
    /// </summary>
    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails, SO_ItemDetails itemDetails) {
        switch (itemDetails.itemType) {
            case ItemType.HoeingTool:
                if (gridPropertyDetails.isDiggable && gridPropertyDetails.daysSinceDug == -1) {
                    // Need to get any items at location so we can check if they are reapable
                    // Get world position for cursor
                    Vector3 cursorWorldPosition = new Vector3(GetWorldPositionForCursor().x + 0.5f, GetWorldPositionForCursor().y + 0.5f, 0f);

                    // Get list of items at cursor location
                    List<Item> itemList = new List<Item>();

                    HelperMethods.GetComponentsAtBoxLocation<Item>(out itemList, cursorWorldPosition, Settings.cursorSize, 0f);

                    // Loop through items found to see if any are reapable type - we are not going to let the player dig where there are reapable scenary items
                    bool foundReapable = false;

                    foreach (Item item in itemList) {
                        if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.ReapableScenary) {
                            foundReapable = true;
                            break;
                        }
                    }

                    return !foundReapable;
                } else {
                    return false;
                }

            case ItemType.WateringTool:
                return (gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.daysSinceWatered == -1);

            case ItemType.ChoppingTool:
            case ItemType.CollectingTool:
            case ItemType.BreakingTool:
                // Check if item can be harvested with item selected, check item is fully grown
                // Check if seed planted
                if (gridPropertyDetails.seedItemCode != -1) {
                    // Get crop details for seed
                    SO_CropDetails cropDetails = so_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);

                    // if crop details found
                    if (cropDetails != null) {
                        // Check if crop fully grown
                        if (gridPropertyDetails.growthDays >= cropDetails.growthDays[cropDetails.growthDays.Length - 1]) {
                            // Check if crop can be harvested with tool selected
                            return (cropDetails.CanUseToolToHarvestCrop(itemDetails.itemCode));
                        }
                        else {
                            return false;
                        }
                    }
                }

                return false;

            default:
                return false;
        }
    }

	#endregion



}
