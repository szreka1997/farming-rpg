using UnityEngine;

public class AStarTest : MonoBehaviour
{
    //private AStar aStar;

    [SerializeField] private NPCPath npcPath = null;
    [SerializeField] private Vector2Int finishPosition;
    [SerializeField] private AnimationClip idleDownAnimationClip = null;
    [SerializeField] private AnimationClip eventAnimationClip = null;
    [SerializeField] private SceneName sceneName = SceneName.Scene1_Farm;
    [SerializeField] private bool moveNPC = false;

    private NPCMovement npcMovement;


    private void Start() {
        npcMovement = npcPath.GetComponent<NPCMovement>();

        npcMovement.npcFacingDirectionAtDestination = Direction.Down;
        npcMovement.npcTargetAnimationClip = idleDownAnimationClip;

    }

    private void Update() {
        if (moveNPC) {
            moveNPC = false;

            SO_NPCScheduleEvent npcScheduleEvent = new SO_NPCScheduleEvent(0, 0, 0, 0, Weather.None, Season.None, sceneName, 
                                                                     new GridCoordinate(finishPosition.x, finishPosition.y), eventAnimationClip);

            npcPath.BuildPath(npcScheduleEvent);
        }
    }

    
}
