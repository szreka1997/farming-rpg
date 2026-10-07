using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>, ISaveable
{
    #region Variables

    private int gameYear = 1;
    private Season gameSeason = Season.Spring;
    private int gameDay = 1;
    private int gameHour = 6;
    private int gameMinute = 30;
    private int gameSecond = 0;
    private string gameDayOfWeek = "Mon";

    private bool gameClockPaused = false;
    private float gameTick = 0f;

    #endregion


    #region Properties

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID      { get => _iSaveableUniqueID;  set => _iSaveableUniqueID = value; }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave;  set => _gameObjectSave = value; }

    #endregion


    #region Unity Basic Methods

    protected override void Awake() {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start() {
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private void Update() {
        if (!gameClockPaused) {
            GameTick();
        }
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        ISaveableRegister();

        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoadFadeIn;
    }

    private void OnDisable() {
        ISaveableDeregister();

        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoadFadeIn;
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
        // Nothing required here since Time Manager is running on the persistent scene
    }

    public void ISaveableRestoreScene(string sceneName) {
        // Nothing required here since Time Manager is running on the persistent scene
    }

    public GameObjectSave ISaveableSave() {
        // Delete existing scene save if exists
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // Create new scene save
        SceneSave sceneSave = new SceneSave();

        // Create new int dictionary
        sceneSave.intDictionary = new Dictionary<string, int>();

        // Create new string dictionary
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // Add values to the int dictionary
        sceneSave.intDictionary.Add("gameYear", gameYear);
        sceneSave.intDictionary.Add("gameDay", gameDay);
        sceneSave.intDictionary.Add("gameHour", gameHour);
        sceneSave.intDictionary.Add("gameMinute", gameMinute);
        sceneSave.intDictionary.Add("gameSecond", gameSecond);

        // Add values to the string dictionary
        sceneSave.stringDictionary.Add("gameDayOfWeek", gameDayOfWeek);
        sceneSave.stringDictionary.Add("gameSeason", gameSeason.ToString());

        // Add scene save to game object for persistent scene
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave) {
        // Get saved gameobject from gameSave data
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave)) {
            GameObjectSave = gameObjectSave;

            // Get savedscene data for gameObject
            if (GameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave)) {
                // if int and string dictionaries are found
                if (sceneSave.intDictionary != null && sceneSave.stringDictionary != null)
                {
                    // populate saved int values
                    if (sceneSave.intDictionary.TryGetValue("gameYear", out int savedGameYear)) {
                        gameYear = savedGameYear;
                    }

                    if (sceneSave.intDictionary.TryGetValue("gameDay", out int savedGameDay)) {
                        gameDay = savedGameDay;
                    }

                    if (sceneSave.intDictionary.TryGetValue("gameHour", out int savedGameHour)) {
                        gameHour = savedGameHour;
                    }

                    if (sceneSave.intDictionary.TryGetValue("gameMinute", out int savedGameMinute)) {
                        gameMinute = savedGameMinute;
                    }

                    if (sceneSave.intDictionary.TryGetValue("gameSecond", out int savedGameSecond)) {
                        gameSecond = savedGameSecond;
                    }

                    // populate string saved values
                    if (sceneSave.stringDictionary.TryGetValue("gameDayOfWeek", out string savedGameDayOfWeek)) {
                        gameDayOfWeek = savedGameDayOfWeek;
                    }

                    if (sceneSave.stringDictionary.TryGetValue("gameSeason", out string savedGameSeason)) {
                        if (Enum.TryParse<Season>(savedGameSeason, out Season season)) {
                            gameSeason = season;
                        }
                    }

                    // Zero gametick
                    gameTick = 0f;

                    // Trigger advance minute event
                    EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

                    // Refresh game clock
                }
            }
        }
    }

    #endregion


    #region Public Methods

    public string GetDayOfWeek() {
        int totalGameDaysInActualGameYear = (((int)gameSeason) * 30) + gameDay;
        int dayOfWeek = totalGameDaysInActualGameYear % 7;

        switch(dayOfWeek) {
            case 1:  return "Mon";
            case 2:  return "Tue";
            case 3:  return "Wed";
            case 4:  return "Thu";
            case 5:  return "Fri";
            case 6:  return "Sat";
            case 0:  return "Sun";
            default: return "";
        }
    }

    public TimeSpan GetGameTime() {
        TimeSpan gameTime = new TimeSpan(gameHour, gameMinute, gameSecond);

        return gameTime;
    }

    public Season GetGameSeason() {
        return gameSeason;
    }

    //TODO:Remove
    /// <summary>
    /// Advance 1 game minute
    /// </summary>
    public void TestAdvanceGameMinute() {
        for (int i = 0; i < 60; i++) {
            UpdateGameSecond();
        }
    }

    //TODO:Remove
    /// <summary>
    /// Advance 1 day
    /// </summary>
    public void TestAdvanceGameDay() {
        for (int i = 0; i < 86400; i++) {
            UpdateGameSecond();
        }
    }

    #endregion



    #region Private Methods

    private void BeforeSceneUnloadFadeOut() {
        gameClockPaused = true;
    }

    private void AfterSceneLoadFadeIn() {
        gameClockPaused = false;
    }

    private void GameTick() {
        gameTick += Time.deltaTime;

        if(gameTick >= Settings.secondsPerGameSecond) {
            gameTick -= Settings.secondsPerGameSecond;
            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond() {
        gameSecond++;

        if(gameSecond > 59) {
            gameSecond = 0;
            gameMinute++;

            if(gameMinute > 59) {
                gameMinute = 0;
                gameHour++;

                if(gameHour > 23) {
                    gameHour = 0;
                    gameDay++;

                    if(gameDay > 30) {
                        gameDay = 1;

                        int gameSeasonAsNumber = (int)gameSeason;
                        gameSeasonAsNumber++;

                        gameSeason = (Season)gameSeasonAsNumber;

                        if(gameSeasonAsNumber > 3) {
                            gameSeasonAsNumber = 0;
                            gameSeason = (Season)gameSeasonAsNumber;
                            gameYear++;

                            if (gameYear > 99999) {
                                gameYear = 1;
                            }

                            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                        }

                        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                    }

                    gameDayOfWeek = GetDayOfWeek();
                    EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }

                EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }

            EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }

        // Call to advance game second event would go here if required
    }

    #endregion



}
