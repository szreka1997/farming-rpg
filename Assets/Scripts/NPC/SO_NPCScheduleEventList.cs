using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/NPC/NPC Schedule Event List", fileName = "New so_NPC Schedule Event List")]
public class SO_NPCScheduleEventList : ScriptableObject
{
    [SerializeField] public List<SO_NPCScheduleEvent> npcScheduleEventList;
}