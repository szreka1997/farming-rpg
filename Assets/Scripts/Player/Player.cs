using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : SingletonMonobehaviour<Player>, ISaveable
{
    #region Variables

    // Movement Animation
    private float xInput;
    private float yInput;
    private bool isWalking;
    private bool isRunning;
    private bool isIdle;
    private bool isCarrying = false;
    private ToolEffect toolEffect = ToolEffect.None;
    private bool isUsingToolRight;
    private bool isUsingToolLeft;
    private bool isUsingToolUp;
    private bool isUsingToolDown;
    private bool isLiftingToolRight;
    private bool isLiftingToolLeft;
    private bool isLiftingToolUp;
    private bool isLiftingToolDown;
    private bool isSwingingToolRight;
    private bool isSwingingToolLeft;
    private bool isSwingingToolUp;
    private bool isSwingingToolDown;
    private bool isPickingRight;
    private bool isPickingLeft;
    private bool isPickingUp;
    private bool isPickingDown;

    

    // Runtime Animation Override
    [Tooltip("Should be populated in the prefab with the equipped item sprite renderer")]
    [SerializeField] private SpriteRenderer equippedItemSpriteRenderer = null;
    
    private AnimationOverrides animationOverrides;
    private List<CharacterAttribute> characterAttributeCustomisationList;

    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;


    // Player Movement
    private float movementSpeed;


    // Player Animation Pause
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds useToolAnimationPause;

    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;

    private WaitForSeconds pickAnimationPause;
    private WaitForSeconds afterPickAnimationPause;

    private bool playerToolUseDisabled = false;


    // Other Variables
    private Rigidbody2D myRigidbody;
    private Camera mainCamera;

    private Direction playerDirection;
    private GridCursor gridCursor;
    private Cursor cursor;

    #endregion


    #region Properties

    private bool _playerInputIsDisabled = false;
    public bool PlayerInputIsDisabled    { get => _playerInputIsDisabled; set => _playerInputIsDisabled = value; }

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID      { get => _iSaveableUniqueID;  set => _iSaveableUniqueID = value; }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

    #endregion


    #region Unity Basic Methods

    protected override void Awake() {
        base.Awake();

        myRigidbody = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        animationOverrides = GetComponentInChildren<AnimationOverrides>();

        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.Arms, PartVariantColour.None, PartVariantType.None);
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.Tool, PartVariantColour.None, PartVariantType.Hoe);
        characterAttributeCustomisationList = new List<CharacterAttribute>();

        // Get unique ID for gameobject and create save data object
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start() {
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();

        useToolAnimationPause      = new WaitForSeconds(Settings.useToolAnimationPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);

        liftToolAnimationPause      = new WaitForSeconds(Settings.liftToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);

        pickAnimationPause = new WaitForSeconds(Settings.pickAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
    }

    private void Update() {     
        if(!PlayerInputIsDisabled) {
            ResetAnimationTriggers();
            PlayerMovementInput();
            PlayerWalkInput();
            PlayerTestInput();
            PlayerClickInput();

            // Send event to any listeners for player movement input
            EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying, toolEffect,
                    isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                    isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                    isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
                    isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
                    false, false, false, false);
        }
    }

    private void FixedUpdate() {
        PlayerMovement();
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        ISaveableRegister();

        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
    }

    private void OnDisable() {
        ISaveableDeregister();

        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
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
        // Nothing required here since the player is on a persistent scene;
    }

    public void ISaveableRestoreScene(string sceneName) {
        // Nothing required here since the player is on a persistent scene;
    }

    public GameObjectSave ISaveableSave() {
        // Delete saveScene for game object if it already exists
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // Create saveScene for game object
        SceneSave sceneSave = new SceneSave();

        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // Add Player position to Vector3 dictionary
        Vector3Serializable vector3Serializable = new Vector3Serializable(transform.position.x, transform.position.y, transform.position.z);
        sceneSave.vector3Dictionary.Add("playerPosition", vector3Serializable);

        // Add Current Scene Name to string dictionary
        sceneSave.stringDictionary.Add("currentScene", SceneManager.GetActiveScene().name);

        // Add Player Direction to string dictionary
        sceneSave.stringDictionary.Add("playerDirection", playerDirection.ToString());

        // Add sceneSave data for player game object
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave) {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave)) {
            // Get save data dictionary for scene
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave)) {
                // Get player position
                if (sceneSave.vector3Dictionary != null && sceneSave.vector3Dictionary.TryGetValue("playerPosition", out Vector3Serializable playerPosition)) {
                    transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                }

                // Get String dictionary
                if (sceneSave.stringDictionary != null) {
                    // Get player scene
                    if (sceneSave.stringDictionary.TryGetValue("currentScene", out string currentScene)) {
                        SceneControllerManager.Instance.FadeAndLoadScene(currentScene, transform.position);
                    }

                    // Get player direction
                    if (sceneSave.stringDictionary.TryGetValue("playerDirection", out string playerDir)) {
                        bool playerDirFound = Enum.TryParse<Direction>(playerDir, true, out Direction direction);

                        if (playerDirFound) {
                            playerDirection = direction;
                            SetPlayerDirection(playerDirection);
                        }
                    }
                }
            }
        }
    }

    #endregion


    #region Public Methods

    /// <summary>
	/// Vector3 viewport position for player ((0,0) viewport bottom left, (1,1) viewport top right)
	/// </summary>
    public Vector3 GetPlayerViewportPosition() {
        return mainCamera.WorldToViewportPoint(transform.position);
    }

    public Vector3 GetPlayerCentrePosition() {
        return new Vector3(transform.position.x, transform.position.y + Settings.playerCentreYOffset, transform.position.z);
    }

    public void EnablePlayerInput() {
        PlayerInputIsDisabled = false;
    }

    public void DisablePlayerInput() {
        PlayerInputIsDisabled = true;
    }

    public void DisablePlayerInputAndResetMovement() {
        DisablePlayerInput();
        ResetMovement();

        // Send event to any listeners for player movement input
        EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying, toolEffect,
            isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
            isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
            isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
            isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
            false, false, false, false);
    }

    public void ClearCarriedItem() {
        equippedItemSpriteRenderer.sprite = null;
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        // Apply base character arms customisation
        armsCharacterAttribute.partVariantType = PartVariantType.None;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(armsCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        isCarrying = false;
    }

    public void ShowCarriedItem(int itemCode) {
        SO_ItemDetails so_ItemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        
        if (so_ItemDetails != null) {
            equippedItemSpriteRenderer.sprite = so_ItemDetails.itemSprite;
            equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            // Apply 'carry' character arms customisation
            armsCharacterAttribute.partVariantType = PartVariantType.Carry;
            characterAttributeCustomisationList.Clear();
            characterAttributeCustomisationList.Add(armsCharacterAttribute);
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

            isCarrying = true;
        }
    }

    #endregion


    #region Private Methods

    /// <summary>
    /// Set the animation triggers to false
    /// </summary>
    private void ResetAnimationTriggers() {
        toolEffect          = ToolEffect.None;
        isUsingToolRight    = false;
        isUsingToolLeft     = false;
        isUsingToolUp       = false;
        isUsingToolDown     = false;
        isLiftingToolRight  = false;
        isLiftingToolLeft   = false;
        isLiftingToolUp     = false;
        isLiftingToolDown   = false;
        isSwingingToolRight = false;
        isSwingingToolLeft  = false;
        isSwingingToolUp    = false;
        isSwingingToolDown  = false;
        isPickingRight      = false;
        isPickingLeft       = false;
        isPickingUp         = false;
        isPickingDown       = false;
    }

    /// <summary>
	/// Set the movement speed and animation trigger if we're running or idleing
	/// </summary>
    private void PlayerMovementInput() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if(xInput != 0 && yInput != 0) {
            xInput = xInput * 0.71f;
            yInput = yInput * 0.71f;
        }

        if(xInput != 0 || yInput != 0) {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;

            // Capture player direction for save game
            if(xInput < 0) {
                playerDirection = Direction.Left;
            } else if(xInput > 0) {
                playerDirection = Direction.Right;
            } else if (yInput < 0) {
                playerDirection = Direction.Down;
            } else {
                playerDirection = Direction.Up;
            }      
        } else if (xInput == 0 && yInput == 0) {
            isRunning = false;
            isWalking = false;
            isIdle = true;
        }
    }

    /// <summary>
	/// Set the movement speed and animation trigger if we're walking
	/// </summary>
    private void PlayerWalkInput() {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            isRunning = false;
            isWalking = true;
            isIdle = false;
            movementSpeed = Settings.walkingSpeed;
        } else {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;
        }
    }

    /// <summary>
	/// Move the player based on xInput and yInput
	/// </summary>
    private void PlayerMovement() {
        Vector2 move = new Vector2(xInput * movementSpeed * Time.deltaTime, yInput * movementSpeed * Time.deltaTime);
        myRigidbody.MovePosition(myRigidbody.position + move);
    }

    private void ResetMovement() {
        xInput = 0f;
        yInput = 0f;
        isWalking = false;
        isRunning = false;
        isIdle = true;
    }    

    private void PlayerClickInput() {
        if(!playerToolUseDisabled) {
            // 0 stands for left click
            // 1 stands for right click
            if(Input.GetMouseButton(0) && (gridCursor.CursorIsEnabled || cursor.CursorIsEnabled)) {
                Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();
                Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();

                ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
            }
        }
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition) {
        ResetMovement();

        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

        // Get Grid property details at cursor position (the GridCursor validation routine ensures that grid property details are not null)
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        // Get Selected item details
        SO_ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.Player);

        if (itemDetails != null) {
            switch (itemDetails.itemType) {
                case ItemType.Seed:
                    if (Input.GetMouseButtonDown(0)) {
                        ProcessPlayerClickInputSeed(gridPropertyDetails, itemDetails);
                    }
                    break;

                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0)) {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;

                case ItemType.WateringTool:
                case ItemType.HoeingTool:
                case ItemType.ReapingTool:
                case ItemType.CollectingTool:
                case ItemType.ChoppingTool:
                case ItemType.BreakingTool:
                    ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerDirection);
                    break;

                default:
                    break;
            }
        }
    }

    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails, SO_ItemDetails itemDetails) {
        if(itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.seedItemCode == -1) {
            PlantSeedAtCursor(gridPropertyDetails, itemDetails);
        }
        else if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid) {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void ProcessPlayerClickInputCommodity(SO_ItemDetails itemDetails) {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid) {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition) {
        if (cursorGridPosition.x > playerGridPosition.x) {
            return Vector3Int.right;
        } else if (cursorGridPosition.x < playerGridPosition.x) {
            return Vector3Int.left;
        } else if (cursorGridPosition.y > playerGridPosition.y) {
            return Vector3Int.up;
        } else {
            return Vector3Int.down;
        }
    }

    private Vector3Int GetPlayerDirection(Vector3 cursorPosition, Vector3 playerPosition) {
        if(cursorPosition.x > playerPosition.x &&
           cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f) &&
           cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)) {

            return Vector3Int.right;
        } 
        else if(cursorPosition.x < playerPosition.x &&
                cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f) &&
                cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)) {

            return Vector3Int.left;
        } 
        else if (cursorPosition.y > playerPosition.y) {
            return Vector3Int.up;
        }
        else {
            return Vector3Int.down;
        }
    }

    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, SO_ItemDetails itemDetails, Vector3Int playerDirection) {
        switch (itemDetails.itemType) {
            case ItemType.HoeingTool:
                if (gridCursor.CursorPositionIsValid) {
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.WateringTool:
                if (gridCursor.CursorPositionIsValid) {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.ReapingTool:
                if (cursor.CursorPositionIsValid) {
                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(), GetPlayerCentrePosition());
                    ReapInPlayerDirectionAtCursor(itemDetails, playerDirection);
                }
                break;

            case ItemType.CollectingTool:
                if (gridCursor.CursorPositionIsValid) {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.ChoppingTool:
                if (gridCursor.CursorPositionIsValid) {
                    ChopInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.BreakingTool:
                if (gridCursor.CursorPositionIsValid) {
                    BreakInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            default:
                break;
        }
    }

    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection) {
        //Play sound
        AudioManager.Instance.PlaySound(SoundName.EffectHoe);

        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails) {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to hoe in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.Hoe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        if (playerDirection == Vector3Int.right) {
            isUsingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left) {
            isUsingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up) {
            isUsingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down) {
            isUsingToolDown = true;
        }

        // its carry on cycling around the game loop and it will only come back into this method after useToolAnimationPause
        yield return useToolAnimationPause;

        // Set Grid property details for dug ground
        if (gridPropertyDetails.daysSinceDug == -1) {
            gridPropertyDetails.daysSinceDug = 0;
        }

        // Display dug grid tiles
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        // Set grid property to dug
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // After animation pause
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection) {
        //Play sound
        AudioManager.Instance.PlaySound(SoundName.EffectWateringCan);

        // Trigger animation
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails) {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to watering can in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.WateringCan;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        // TODO: If there is water in the watering can
        toolEffect = ToolEffect.Watering;

        if (playerDirection == Vector3Int.right) {
            isLiftingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left) {
            isLiftingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up) {
            isLiftingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down) {
            isLiftingToolDown = true;
        }

        yield return liftToolAnimationPause;

        // Set Grid property details for watered ground
        if (gridPropertyDetails.daysSinceWatered == -1) {
            gridPropertyDetails.daysSinceWatered = 0;
        }

        // Display watered grid tiles
        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);

        // Set grid property to watered
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // After animation pause
        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void ReapInPlayerDirectionAtCursor(SO_ItemDetails itemDetails, Vector3Int playerDirection) {
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails, playerDirection));
    }

    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(SO_ItemDetails itemDetails, Vector3Int playerDirection) {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to scythe in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.Scythe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        // Reap in player direction
        UseToolInPlayerDirection(itemDetails, playerDirection);

        yield return useToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, SO_ItemDetails equippedItemDetails, Vector3Int playerDirection) {
        // Play sound
        AudioManager.Instance.PlaySound(SoundName.EffectBasket);

        StartCoroutine(CollectInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator CollectInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, SO_ItemDetails equippedItemDetails, 
                    Vector3Int playerDirection) {

        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return pickAnimationPause;

        // After animation pause
        yield return afterPickAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void ChopInPlayerDirection(GridPropertyDetails gridPropertyDetails, SO_ItemDetails equippedItemDetails, Vector3Int playerDirection) {
        // Play sound
        AudioManager.Instance.PlaySound(SoundName.EffectAxe);
        
        // Trigger animation
        StartCoroutine(ChopInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator ChopInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, SO_ItemDetails equippedItemDetails, Vector3Int playerDirection) {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to axe in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.Axe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        // After animation pause
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void BreakInPlayerDirection(GridPropertyDetails gridPropertyDetails, SO_ItemDetails equippedItemDetails, Vector3Int playerDirection) {
        // Play sound
        AudioManager.Instance.PlaySound(SoundName.EffectPickaxe);        
        
        StartCoroutine(BreakInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator BreakInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, SO_ItemDetails equippedItemDetails, Vector3Int playerDirection) {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to pickaxe in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.Pickaxe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        // After animation pause
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void UseToolInPlayerDirection(SO_ItemDetails equippedItemDetails, Vector3Int playerDirection) {
        if(Input.GetMouseButton(0)) {
            switch(equippedItemDetails.itemType) {
                case ItemType.ReapingTool:
                    if (playerDirection == Vector3Int.right) {
                        isSwingingToolRight = true;
                    }
                    else if (playerDirection == Vector3Int.left) {
                        isSwingingToolLeft = true;
                    }
                    else if (playerDirection == Vector3Int.up) {
                        isSwingingToolUp = true;
                    }
                    else if (playerDirection == Vector3Int.down) {
                        isSwingingToolDown = true;
                    }
                    break;
            }

            // Define centre point of square which will be used for collision testing
            Vector2 point = new Vector2(GetPlayerCentrePosition().x + (playerDirection.x * (equippedItemDetails.itemUseRadius / 2f)), 
                                        GetPlayerCentrePosition().y + playerDirection.y * (equippedItemDetails.itemUseRadius / 2f));

            // Define size of the square which will be used for collision testing
            Vector2 size = new Vector2(equippedItemDetails.itemUseRadius, equippedItemDetails.itemUseRadius);

            // Get Item components with 2D collider located in the square at the centre point defined 
            //      (2d colliders tested limited to maxCollidersToTestPerReapSwing)
            Item[] itemArray = HelperMethods.GetComponentsAtBoxLocationNonAlloc<Item>(Settings.maxCollidersToTestPerReapSwing, point, size, 0f);

            int reapableItemCount = 0;

            // Loop through all items retrieved
            for (int i = itemArray.Length - 1; i >= 0; i--) {
                if (itemArray[i] != null) {
                    // Destroy item game object if reapable
                    if (InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.ReapableScenary) {
                        // Effect position
                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x, 
                                                             itemArray[i].transform.position.y + Settings.gridCellSize / 2f, 
                                                             itemArray[i].transform.position.z);

                        // Trigger reaping effect
                        if(itemArray[i].ItemCode == 10021) {
                            EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.ReapingCactus);
                        } else if(itemArray[i].ItemCode == 10019) {
                            EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.ReapingGrassYellow);
                        } else if(itemArray[i].ItemCode == 10018) {
                            EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.ReapingGrassBlue);
                        }

                        // Play sound
                        AudioManager.Instance.PlaySound(SoundName.EffectScythe);

                        Destroy(itemArray[i].gameObject);
                        reapableItemCount++;

                        if (reapableItemCount >= Settings.maxTargetComponentsToDestroyPerReapSwing) {
                            break;
                        }
                    }
                }
            }
        }
    }

    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, SO_ItemDetails itemDetails) {
        // Process if we have cropdetails for the seed
        if (GridPropertiesManager.Instance.GetCropDetails(itemDetails.itemCode) != null) {
            // Update grid properties with seed details
            gridPropertyDetails.seedItemCode = itemDetails.itemCode;
            gridPropertyDetails.growthDays = 0;

            // Display planted crop at grid property details
            GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

            // Remove item from inventory
            EventHandler.CallRemoveSelectedItemFromInventoryEvent();

            // Make planting sound
            AudioManager.Instance.PlaySound(SoundName.EffectPlantingSound);
        }
    }

    /// <summary>
    /// Method processes crop with equipped item in player direction
    /// </summary>
    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection, SO_ItemDetails equippedItemDetails, 
                GridPropertyDetails gridPropertyDetails) {

        switch (equippedItemDetails.itemType) {
            case ItemType.CollectingTool:
                if (playerDirection == Vector3Int.right) {
                    isPickingRight = true;
                }
                else if (playerDirection == Vector3Int.left) {
                    isPickingLeft = true;
                }
                else if (playerDirection == Vector3Int.up) {
                    isPickingUp = true;
                }
                else if (playerDirection == Vector3Int.down) {
                    isPickingDown = true;
                }
                break;

            case ItemType.ChoppingTool:
            case ItemType.BreakingTool:
                if (playerDirection == Vector3Int.right) {
                    isUsingToolRight = true;
                }
                else if (playerDirection == Vector3Int.left) {
                    isUsingToolLeft = true;
                }
                else if (playerDirection == Vector3Int.up) {
                    isUsingToolUp = true;
                }
                else if (playerDirection == Vector3Int.down) {
                    isUsingToolDown = true;
                }
                break;

            case ItemType.None:
                break;
        }

        // Get crop at cursor grid location
        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        // Execute Process Tool Action For crop
        if (crop != null) {
            switch (equippedItemDetails.itemType) {
                case ItemType.CollectingTool:
                    crop.ProcessToolAction(equippedItemDetails, isPickingRight, isPickingLeft, isPickingDown, isPickingUp);
                    break;

                case ItemType.ChoppingTool:
                case ItemType.BreakingTool:
                    crop.ProcessToolAction(equippedItemDetails, isUsingToolRight, isUsingToolLeft, isUsingToolDown, isUsingToolUp);
                    break;
            }
        }
    }

    private void SetPlayerDirection(Direction playerDirection) {
        switch (playerDirection) {
            case Direction.Up:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, true, false);

                break;

            case Direction.Down:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, true);

                break;

            case Direction.Left:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, true, false, false);

                break;

            case Direction.Right:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               true, false, false, false);
                
                break;

            default:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, false, 
                                               false, false, false, true);
               
                break;
        }
    }


    // TODO: Remove
    /// <summary>
    /// Temp routine for test input
    /// </summary>
    private void PlayerTestInput() {
        if(Input.GetKey(KeyCode.T)) {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        if (Input.GetKeyDown(KeyCode.G)) {
            TimeManager.Instance.TestAdvanceGameDay();
        }
    }

    #endregion


}
