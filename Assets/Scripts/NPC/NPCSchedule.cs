using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCPath))]
public class NPCSchedule : MonoBehaviour
{
    #region Variables

    [SerializeField] private SO_NPCScheduleEventList so_NPCScheduleEventList = null;

    private SortedSet<SO_NPCScheduleEvent> npcScheduleEventSet;
    private NPCPath npcPath;

    #endregion


    #region Unity Basic Methods

    private void Awake() {
        // Implement the comparison functionally that we defined in the NPCScheduleEventSort class as part of the SortedSet
        // Every value that we add in the sorting set will automatically be sorted by the logic that we included
        npcScheduleEventSet = new SortedSet<SO_NPCScheduleEvent>(new NPCScheduleEventSort());

        foreach (SO_NPCScheduleEvent npcScheduleEvent in so_NPCScheduleEventList.npcScheduleEventList) {
            npcScheduleEventSet.Add(npcScheduleEvent);
        }

        // Get NPC Path Component
        npcPath = GetComponent<NPCPath>();
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.AdvanceGameMinuteEvent += GameTimeSystem_AdvanceMinute;
    }

    private void OnDisable() {
        EventHandler.AdvanceGameMinuteEvent -= GameTimeSystem_AdvanceMinute;
    }

    #endregion


    #region Event Subscriber Methods

    private void GameTimeSystem_AdvanceMinute(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond) {
        int time = (gameHour * 100) + gameMinute;

        //Attempt to get matching schedule
        SO_NPCScheduleEvent matchingNPCScheduleEvent = null;

        foreach (SO_NPCScheduleEvent npcScheduleEvent in npcScheduleEventSet) {
            if (npcScheduleEvent.Time == time) {
                // Time match now check if parameters match
                if (npcScheduleEvent.day != 0 && npcScheduleEvent.day != gameDay) {
                    continue;
                }

                if (npcScheduleEvent.season != Season.None && npcScheduleEvent.season != gameSeason) {
                    continue;
                }

                if (npcScheduleEvent.weather != Weather.None && npcScheduleEvent.weather != GameManager.Instance.currentWeather) {
                    continue;
                }

                //Schdule matches
                // Debug.Log("Schedule Matches! " + npcScheduleEvent);
                matchingNPCScheduleEvent = npcScheduleEvent;
                break;
            }
            else if (npcScheduleEvent.Time > time) {
                break;
            }
        }

        // Now test is matchingSchedule!=null and do something;
        if (matchingNPCScheduleEvent != null) {
            // Build path for matching schedule
            npcPath.BuildPath(matchingNPCScheduleEvent);
        }
    }

    #endregion



}