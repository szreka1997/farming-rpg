using System.Collections.Generic;

public class NPCScheduleEventSort : IComparer<SO_NPCScheduleEvent>
{
    public int Compare(SO_NPCScheduleEvent npcScheduleEvent1, SO_NPCScheduleEvent npcScheduleEvent2) {
        // ? means : (Avoid null reference error)
        // if(npcScheduleEcent1 == null) {
        //     return null;       
        // }
        // return npcScheduleEvent.Time     // Igy ha null akkor nem akarja majd lekérni a Time-ot
        if (npcScheduleEvent1?.Time == npcScheduleEvent2?.Time) {
            if (npcScheduleEvent1?.priority < npcScheduleEvent2?.priority) {
                return -1;
            }
            else {
                return 1;
            }
        }
        else if (npcScheduleEvent1?.Time > npcScheduleEvent2?.Time) {
            return 1;
        }
        else if (npcScheduleEvent1?.Time < npcScheduleEvent2?.Time) {
            return -1;
        }
        else {
            return 0;
        }
    }
}