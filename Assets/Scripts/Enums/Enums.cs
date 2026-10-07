public enum ToolEffect {
    None,
    Watering
}

public enum Direction {
    Right,
    Left,
    Up,
    Down,
    None
}

public enum ItemType {
    Seed,
    Commodity,
    WateringTool,
    HoeingTool,
    ChoppingTool,
    BreakingTool,
    ReapingTool,
    CollectingTool,
    ReapableScenary,
    Furniture,
    None,
    Count
}

public enum InventoryLocation {
    Player,
    Chest,
    Count
}

public enum AnimationName {
    UseToolRight,
    UseToolLeft,
    UseToolUp,
    UseToolDown,
    LiftToolRight,
    LiftToolLeft,
    LiftToolUp,
    LiftToolDown,
    SwingToolRight,
    SwingToolLeft,
    SwingToolUp,
    SwingToolDown,
    HoldToolRight,
    HoldToolLeft,
    HoldToolUp,
    HoldToolDown,
    PickRight,
    PickLeft,
    PickDown,
    PickUp,
    IdleRight,
    IdleLeft,
    IdleDown,
    IdleUp,
    WalkRight,
    WalkLeft,
    WalkUp,
    WalkDown,
    RunRight,
    RunLeft,
    RunUp,
    RunDown,
    Count
}

public enum CharacterPartAnimator {
    Body,
    Arms,
    Hair,
    Tool,
    Hat,
    Count
}

public enum PartVariantColour {
    None,
    Count
}

public enum PartVariantType {
    None,
    Carry,
    Hoe,
    Pickaxe,
    Axe,
    Scythe,
    WateringCan,
    Count
}

public enum Season {
    Spring,
    Summer,
    Autumn,
    Winter,
    None,
    Count
}

public enum SceneName {
    Scene1_Farm,
    Scene2_Field,
    Scene3_Cabin
}

public enum GridBoolProperty {
    Diggable,
    CanDropItem,
    CanPlaceFurniture,
    IsPath,
    IsNPCObstacle
}

public enum HarvestActionEffect {
    KotShogLeavesFalling,
    FleckLeavesFalling,
    ChoppingTreeTrunkPurple,
    ChoppingTreeTrunkBlue,
    BreakingStone,
    ReapingGrassYellow,
    ReapingGrassBlue,
    ReapingCactus,
    None
}

public enum Facing {
    None,
    Front,
    Back,
    Right
}

public enum Weather {
    Dry,
    Raining,
    Snowing,
    None,
    Count
}

public enum SoundName {
    None = 0,
    EffectFootstepSoftGround = 10,
    EffectFootstepHardGround = 20,
    EffectAxe = 30,
    EffectPickaxe = 40,
    EffectScythe = 50,
    EffectHoe = 60,
    EffectWateringCan = 70,
    EffectBasket = 80,
    EffectPickupSound = 90,
    EffectRustle = 100,
    EffectTreeFalling = 110,
    EffectPlantingSound = 120,
    EffectPluck = 130,
    EffectStoneShatter = 140,
    EffectWoodSplinters = 150,
    AmbientCountryside1 = 1000,
    AmbientCountryside2 = 1010,
    AmbientIndoors1 = 1020,
    MusicCalm3 = 2000,
    MusicCalm1 = 2010
}

public enum ItemCategory {
    None,
    Seed,
    Commodity,
    Tool,
    Count
}

public enum CursorType{
    None,
    Movement,
    Combat,
    UI,
    Pickup,
    FullPickup,
    Dialogue,
    Shop
}

