using UnityEngine;

public static class Settings
{
    // Money
    public const float sellingValuePercentage = 0.8f;

    // Tilemap
    public const float gridCellSize = 1f;                // Grid cell size in unity units
    public const float gridCellDiagonalSize = 1.41f;     // diagonal distance between unity cell centres
    public static Vector2 cursorSize = Vector2.one;
    public const int maxGridWidth  = 99999;
    public const int maxGridHeight = 99999;

    // Obscuring Item Fading - ObscuringItemFader
    public const float fadeInSeconds  = 0.25f;
    public const float fadeOutSeconds = 0.35f;
    public const float targetAlpha    = 0.45f;

    // Player Movement
    public const float runningSpeed = 5.333f;
    public const float walkingSpeed = 2.666f;

    // Player Animation Pause
    public static float useToolAnimationPause = 0.25f;
    public static float afterUseToolAnimationPause = 0.2f;

    public static float liftToolAnimationPause = 0.4f;
    public static float afterLiftToolAnimationPause = 0.4f;

    public static float pickAnimationPause = 1f;
    public static float afterPickAnimationPause = 0.2f;

    // Inventory
    public static int playerInitialInventoryCapacity = 12;
    public static int playerMaximumInventoryCapacity = 48;

    // Tools
    public const string HoeingTool     = "Hoe";
    public const string ChoppingTool   = "Axe";
    public const string BreakingTool   = "Pickaxe";
    public const string ReapingTool    = "Scythe";
    public const string WateringTool   = "Watering Can";
    public const string CollectingTool = "Basket";

    // Reaping
    public const int maxCollidersToTestPerReapSwing = 15;
    public const int maxTargetComponentsToDestroyPerReapSwing = 2;

    // Time System
    public const float secondsPerGameSecond = 0.012f;

    // Player
    public static float playerCentreYOffset = 0.875f;

    // Scenes
    public const string PersistentScene = "PersistentScene";

    // Player Animation Parameters
    public static int xInput;
    public static int yInput;
    public static int isWalking;
    public static int isRunning;
    public static int toolEffect;
    public static int isUsingToolRight;
    public static int isUsingToolLeft;
    public static int isUsingToolUp;
    public static int isUsingToolDown;
    public static int isLiftingToolRight;
    public static int isLiftingToolLeft;
    public static int isLiftingToolUp;
    public static int isLiftingToolDown;
    public static int isSwingingToolRight;
    public static int isSwingingToolLeft;
    public static int isSwingingToolUp;
    public static int isSwingingToolDown;
    public static int isPickingRight;
    public static int isPickingLeft;
    public static int isPickingUp;
    public static int isPickingDown;

    // Shared Animation Parameters
    public static int idleRight;
    public static int idleLeft;
    public static int idleUp;
    public static int idleDown;

    //NPC Movement
    public static float pixelSize = 0.0625f;

    // NPC Animation Parameters
    public static int walkRight;
    public static int walkLeft;
    public static int walkUp;
    public static int walkDown;
    public static int eventAnimation;

    
    /// <summary>
	/// Static constructor
	/// </summary>
    static Settings() {
        // Player Animation Parameters
        xInput              = Animator.StringToHash("xInput");
        yInput              = Animator.StringToHash("yInput");
        isWalking           = Animator.StringToHash("isWalking");
        isRunning           = Animator.StringToHash("isRunning");
        toolEffect          = Animator.StringToHash("toolEffect");
        isUsingToolRight    = Animator.StringToHash("isUsingToolRight");
        isUsingToolLeft     = Animator.StringToHash("isUsingToolLeft");
        isUsingToolUp       = Animator.StringToHash("isUsingToolUp");
        isUsingToolDown     = Animator.StringToHash("isUsingToolDown");
        isLiftingToolRight  = Animator.StringToHash("isLiftingToolRight");
        isLiftingToolLeft   = Animator.StringToHash("isLiftingToolLeft");
        isLiftingToolUp     = Animator.StringToHash("isLiftingToolUp");
        isLiftingToolDown   = Animator.StringToHash("isLiftingToolDown");
        isSwingingToolRight = Animator.StringToHash("isSwingingToolRight");
        isSwingingToolLeft  = Animator.StringToHash("isSwingingToolLeft");
        isSwingingToolUp    = Animator.StringToHash("isSwingingToolUp");
        isSwingingToolDown  = Animator.StringToHash("isSwingingToolDown");
        isPickingRight      = Animator.StringToHash("isPickingRight");
        isPickingLeft       = Animator.StringToHash("isPickingLeft");
        isPickingUp         = Animator.StringToHash("isPickingUp");
        isPickingDown       = Animator.StringToHash("isPickingDown");

        // Shared Animation Parameters
        idleRight = Animator.StringToHash("idleRight");
        idleLeft  = Animator.StringToHash("idleLeft");
        idleUp    = Animator.StringToHash("idleUp");
        idleDown  = Animator.StringToHash("idleDown");

        // NPC Animation parameters
        walkRight      = Animator.StringToHash("walkRight");
        walkLeft       = Animator.StringToHash("walkLeft");
        walkUp         = Animator.StringToHash("walkUp");
        walkDown       = Animator.StringToHash("walkDown");
        eventAnimation = Animator.StringToHash("eventAnimation");
    }



}
