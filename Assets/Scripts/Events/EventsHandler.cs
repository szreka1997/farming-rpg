using System;
using System.Collections.Generic;
using UnityEngine;


public delegate void MovementDelegate(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle, bool isCarrying, 
        ToolEffect toolEffect,
        bool isUsingToolRight, bool isUsingToolLeft, bool isUsingToolUp, bool isUsingToolDown,
        bool isLiftingToolRight, bool isLiftingToolLeft, bool isLiftingToolUp, bool isLiftingToolDown,
        bool isSwingingToolRight, bool isSwingingToolLeft, bool isSwingingToolUp, bool isSwingingToolDown,
        bool isPickingRight, bool isPickingLeft, bool isPickingUp, bool isPickingDown,
        bool idleRight, bool idleLeft, bool idleUp, bool idleDown);


public static class EventHandler
{
    #region Inventory Updated Events

    public static event Action<InventoryLocation, List<InventoryItem>> InventoryUpdatedEvent;

    /// <summary>
	/// Inventory Updated Event Call For Publishers
	/// </summary>
    public static void CallInventoryUpdatedEvent(InventoryLocation inventoryLocation, List<InventoryItem> inventoryItemList) {
        if (InventoryUpdatedEvent != null) {
            InventoryUpdatedEvent(inventoryLocation, inventoryItemList);
        }
    }

    #endregion



    #region Shop Inventory Updated Events

    public static event Action ShopInventoryUpdatedEvent;

    public static void CallShopInventoryUpdatedEvent() {
        if(ShopInventoryUpdatedEvent != null) {
            ShopInventoryUpdatedEvent();
        }
    }


    public static event Action ActiveShopChangedEvent;

    public static void CallActiveShopChangedEvent() {
        if(ActiveShopChangedEvent != null) {
            ActiveShopChangedEvent();
        }
    }

    public static event Action<Shop> ShopOpenCloseStateChangedEvent;

    public static void CallShopOpenCloseStateChangedEvent(Shop currentShop) {
        if(ShopOpenCloseStateChangedEvent != null) {
            ShopOpenCloseStateChangedEvent(currentShop);
        }
    }

    #endregion


    #region Purse Updated Events

    public static event Action PurseUpdatedEvent;

    /// <summary>
	/// Inventory Updated Event Call For Publishers
	/// </summary>
    public static void CallPurseUpdatedEvent() {
        if (PurseUpdatedEvent != null) {
            PurseUpdatedEvent();
        }
    }

    #endregion


    #region Movement Events

    public static event MovementDelegate MovementEvent;

    /// <summary>
	/// Movement Event Call For Publishers
	/// </summary>
    public static void CallMovementEvent(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle, bool isCarrying, 
            ToolEffect toolEffect,
            bool isUsingToolRight, bool isUsingToolLeft, bool isUsingToolUp, bool isUsingToolDown,
            bool isLiftingToolRight, bool isLiftingToolLeft, bool isLiftingToolUp, bool isLiftingToolDown,
            bool isSwingingToolRight, bool isSwingingToolLeft, bool isSwingingToolUp, bool isSwingingToolDown,
            bool isPickingRight, bool isPickingLeft, bool isPickingUp, bool isPickingDown,
            bool idleRight, bool idleLeft, bool idleUp, bool idleDown) {

        if(MovementEvent != null) {
            MovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying, toolEffect,
                    isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                    isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                    isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
                    isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
                    idleRight, idleLeft, idleUp, idleDown);
        }
    }

    #endregion



    #region Time Events

    // Advance Game Minute
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameMinuteEvent;

    public static void CallAdvanceGameMinuteEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, 
                int gameMinute, int gameSecond) {

        if(AdvanceGameMinuteEvent != null) {
            AdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }

    // Advance Game Hour
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameHourEvent;

    public static void CallAdvanceGameHourEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, 
                int gameMinute, int gameSecond) {

        if(AdvanceGameHourEvent != null) {
            AdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }

    // Advance Game Day
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameDayEvent;

    public static void CallAdvanceGameDayEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, 
                int gameMinute, int gameSecond) {

        if (AdvanceGameDayEvent != null) {
            AdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }

    // Advance game season
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameSeasonEvent;

    public static void CallAdvanceGameSeasonEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, 
                int gameMinute, int gameSecond) {

        if (AdvanceGameSeasonEvent != null) {
            AdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }

    // Advance game year
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameYearEvent;

    public static void CallAdvanceGameYearEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, 
                int gameMinute, int gameSecond) {
        if (AdvanceGameYearEvent != null) {
            AdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }

    #endregion



    #region Scene Load Events

    // Before Scene Unload Fade Out Event
    public static event Action BeforeSceneUnloadFadeOutEvent;

    public static void CallBeforeSceneUnloadFadeOutEvent() {
        if (BeforeSceneUnloadFadeOutEvent != null) {
            BeforeSceneUnloadFadeOutEvent();
        }
    }

    // Before Scene Unload Event
    public static event Action BeforeSceneUnloadEvent;

    public static void CallBeforeSceneUnloadEvent() {
        if (BeforeSceneUnloadEvent != null) {
            BeforeSceneUnloadEvent();
        }
    }

    // After Scene Loaded Event
    public static event Action AfterSceneLoadEvent;

    public static void CallAfterSceneLoadEvent() {
        if (AfterSceneLoadEvent != null) {
            AfterSceneLoadEvent();
        }
    }

    // After Scene Load Fade In Event
    public static event Action AfterSceneLoadFadeInEvent;

    public static void CallAfterSceneLoadFadeInEvent() {
        if (AfterSceneLoadFadeInEvent != null) {
            AfterSceneLoadFadeInEvent();
        }
    }

    #endregion



    #region Drop Selected Item Event

    public static event Action DropSelectedItemEvent;

    public static void CallDropSelectedItemEvent() {
        if (DropSelectedItemEvent != null) {
            DropSelectedItemEvent();
        }
    }

    #endregion


    #region Remove selected item from inventory

    public static event Action RemoveSelectedItemFromInventoryEvent;

    public static void CallRemoveSelectedItemFromInventoryEvent() {
        if (RemoveSelectedItemFromInventoryEvent != null) {
            RemoveSelectedItemFromInventoryEvent();
        }
    }

    #endregion


    #region Harvest Action Effect Event

    public static event Action<Vector3, HarvestActionEffect> HarvestActionEffectEvent;

    public static void CallHarvestActionEffectEvent(Vector3 effectPosition, HarvestActionEffect harvestActionEffect) {
        if (HarvestActionEffectEvent != null) {
            HarvestActionEffectEvent(effectPosition, harvestActionEffect);
        }
    }

    #endregion


    #region Instantiate crop prefabs
    public static event Action InstantiateCropPrefabsEvent;

    public static void CallInstantiateCropPrefabsEvent() {
        if (InstantiateCropPrefabsEvent != null) {
            InstantiateCropPrefabsEvent();
        }
    }

    #endregion



}