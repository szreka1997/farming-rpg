using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NPCPath))]
public class NPCMovement : MonoBehaviour
{
    #region Variables

    public SceneName npcCurrentScene;
    public Direction npcFacingDirectionAtDestination;
    
    [HideInInspector] public SceneName npcTargetScene;
    [HideInInspector] public Vector3Int npcCurrentGridPosition;
    [HideInInspector] public Vector3Int npcTargetGridPosition;
    [HideInInspector] public Vector3 npcTargetWorldPosition;
    [HideInInspector] public AnimationClip npcTargetAnimationClip;
    [HideInInspector] public bool npcActiveInScene = false;

    [Header("NPC Movement")]
    [SerializeField] private float npcMinSpeed = 1f;
    [SerializeField] private float npcMaxSpeed = 3f;
    public float npcNormalSpeed = 2f;
    private bool npcIsMoving = false;

    [Header("NPC Animation")]
    [SerializeField] private AnimationClip blankAnimation = null;

    private SceneName npcPreviousMovementStepScene;
    private Vector3Int npcNextGridPosition;
    private Vector3 npcNextWorldPosition;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private CapsuleCollider2D capsuleCollider2D;
    private Rigidbody2D rigidBody2D;
    private Animator animator;

    private Grid grid;
    private AnimatorOverrideController animatorOverrideController;
    private NPCPath npcPath;

    private WaitForFixedUpdate waitForFixedUpdate;
    private Coroutine moveToGridPositionRoutine;

    private int lastMoveAnimationParameter;
    private bool npcInitialised = false;
    private bool sceneLoaded = false;

    #endregion


    #region Unity Basic Methods

    private void Awake() {
        spriteRenderer    = GetComponent<SpriteRenderer>();
        boxCollider2D     = GetComponent<BoxCollider2D>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        rigidBody2D       = GetComponent<Rigidbody2D>();
        animator          = GetComponent<Animator>();
        npcPath           = GetComponent<NPCPath>();

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        // Initialise target world position, target grid position & target scene to current
        npcTargetScene         = npcCurrentScene;
        npcTargetGridPosition  = npcCurrentGridPosition;
        npcTargetWorldPosition = transform.position;
    }

    private void Start() {
        waitForFixedUpdate = new WaitForFixedUpdate();
        SetIdleAnimation();
    }

    private void FixedUpdate() {
        if (sceneLoaded) {
            if (!npcIsMoving) {
                // set npc current and next grid position - to take into account the npc might be animating
                npcCurrentGridPosition = GetGridPosition(transform.position);
                npcNextGridPosition = npcCurrentGridPosition;

                if (npcPath.npcMovementStepStack.Count > 0) {
                    NPCMovementStep npcMovementStep = npcPath.npcMovementStepStack.Peek();    // This won't remove the top element
                    npcCurrentScene = npcMovementStep.sceneName;

                    // If NPC is about the move to a new scene reset position to starting point in new scene and update the step times
                    if (npcCurrentScene != npcPreviousMovementStepScene) {
                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);
                        npcPreviousMovementStepScene = npcCurrentScene;
                        npcPath.UpdateTimesOnPath();
                    }

                    // If NPC is in current scene then set NPC to active to make visible, pop the movement step off the stack and then call method to move NPC
                    if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name) {
                        SetNPCActiveInScene();

                        npcMovementStep = npcPath.npcMovementStepStack.Pop();     // Removes it from the Stack
                        npcNextGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);

                        MoveToGridPosition(npcNextGridPosition, npcMovementStepTime, TimeManager.Instance.GetGameTime());
                    }
                    // else if NPC is not in current scene then set NPC to inactive to make invisible
                    // - once the movement step time is less than game time (in the past) 
                    //         then pop movement step off the stack and set NPC position to movement step position
                    else {
                        SetNPCInactiveInScene();

                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);

                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);
                        TimeSpan gameTime = TimeManager.Instance.GetGameTime();

                        if (npcMovementStepTime < gameTime) {
                            npcMovementStep = npcPath.npcMovementStepStack.Pop();

                            npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                            npcNextGridPosition = npcCurrentGridPosition;
                            transform.position = GetWorldPosition(npcCurrentGridPosition);
                        }
                    }
                }
                // else if no more NPC movement steps
                else {
                    ResetMoveAnimation();
                    SetNPCFacingDirection();
                    SetNPCEventAnimation();
                }
            }
        }
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloaded;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloaded;
    }

    #endregion


    #region Event Subscriber Methods

    private void AfterSceneLoad() {
        grid = GameObject.FindObjectOfType<Grid>();

        if (!npcInitialised) {
            InitialiseNPC();
            npcInitialised = true;
        }

        sceneLoaded = true;
    }

    private void BeforeSceneUnloaded() {
        sceneLoaded = false;
    }

    #endregion


    #region Public Methods

    public void SetNPCActiveInScene() {
        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;
        capsuleCollider2D.enabled = true;
        npcActiveInScene = true;
    }

    public void SetNPCInactiveInScene() {
        spriteRenderer.enabled = false;
        boxCollider2D.enabled = false;
        capsuleCollider2D.enabled = false;
        npcActiveInScene = false;
    }

    public void SetScheduleEventDetails(SO_NPCScheduleEvent npcScheduleEvent) {
        npcTargetScene         = npcScheduleEvent.toSceneName;
        npcTargetGridPosition  = (Vector3Int)npcScheduleEvent.toGridCoordinate;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);

        npcFacingDirectionAtDestination = npcScheduleEvent.npcFacingDirectionAtDestination;
        npcTargetAnimationClip          = npcScheduleEvent.animationAtDestination;

        ClearNPCEventAnimation();
    }

    public void ClearNPCEventAnimation() {
        animatorOverrideController[blankAnimation] = blankAnimation;
        animator.SetBool(Settings.eventAnimation, false);

        transform.rotation = Quaternion.identity;
    }

    public void CancelNPCMovement() {
        npcPath.ClearPath();
        npcNextGridPosition = Vector3Int.zero;
        npcNextWorldPosition = Vector3.zero;
        npcIsMoving = false;

        if (moveToGridPositionRoutine != null) {
            StopCoroutine(moveToGridPositionRoutine);
        }

        ResetMoveAnimation();

        // Clear event animation
        ClearNPCEventAnimation();
        npcTargetAnimationClip = null;


        ResetIdleAnimation();
        SetIdleAnimation();
    }

    #endregion


    #region Private Methods

    private void SetIdleAnimation() {
        animator.SetBool(Settings.idleDown, true);
    }

    /// <summary>
    /// returns the grid position given the worldPosition
    /// </summary>
    private Vector3Int GetGridPosition(Vector3 worldPosition) {
        if (grid != null) {
            return grid.WorldToCell(worldPosition);
        }
        else {
            return Vector3Int.zero;
        }
    }

    /// <summary>
    ///  returns the world position (centre of grid square) from gridPosition
    /// </summary>
    public Vector3 GetWorldPosition(Vector3Int gridPosition) {
        Vector3 worldPosition = grid.CellToWorld(gridPosition);

        // Get centre of grid square
        return new Vector3(worldPosition.x + Settings.gridCellSize / 2f, worldPosition.y + Settings.gridCellSize / 2f, worldPosition.z);
    }

    private void MoveToGridPosition(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime) {
        moveToGridPositionRoutine = StartCoroutine(MoveToGridPositionRoutine(gridPosition, npcMovementStepTime, gameTime));
    }

    private IEnumerator MoveToGridPositionRoutine(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime) {
        npcIsMoving = true;
        SetMoveAnimation(gridPosition);
        npcNextWorldPosition = GetWorldPosition(gridPosition);

        // If movement step time is in the future, otherwise skip and move NPC immediately to position
        if (npcMovementStepTime > gameTime) {
            //calculate time difference in seconds
            float timeToMove = (float)(npcMovementStepTime.TotalSeconds - gameTime.TotalSeconds);
            float npcCalculatedSpeed = Mathf.Max(npcMinSpeed, 
                                                 Vector3.Distance(transform.position, npcNextWorldPosition) / timeToMove / Settings.secondsPerGameSecond);

            // If speed is less than npc max speed  then process, otherwise skip and move NPC immediately to position
            if (npcCalculatedSpeed <= npcMaxSpeed) {
                while (Vector3.Distance(transform.position, npcNextWorldPosition) > Settings.pixelSize) {
                    Vector3 unitVector = Vector3.Normalize(npcNextWorldPosition - transform.position);

                    Vector2 move = new Vector2(unitVector.x * npcCalculatedSpeed * Time.fixedDeltaTime, 
                                               unitVector.y * npcCalculatedSpeed * Time.fixedDeltaTime);

                    rigidBody2D.MovePosition(rigidBody2D.position + move);

                    yield return waitForFixedUpdate;
                }
            }
        }

        rigidBody2D.position = npcNextWorldPosition;
        npcCurrentGridPosition = gridPosition;
        npcNextGridPosition = npcCurrentGridPosition;
        npcIsMoving = false;
    }

    private void SetMoveAnimation(Vector3Int gridPosition) {
        ResetIdleAnimation();
        ResetMoveAnimation();

        Vector3 toWorldPosition = GetWorldPosition(gridPosition);
        Vector3 directionVector = toWorldPosition - transform.position;

        if (Mathf.Abs(directionVector.x) >= Mathf.Abs(directionVector.y)) {
            if (directionVector.x > 0) {
                animator.SetBool(Settings.walkRight, true);
            }
            else {
                animator.SetBool(Settings.walkLeft, true);
            }
        }
        else {
            if (directionVector.y > 0) {
                animator.SetBool(Settings.walkUp, true);
            }
            else {
                animator.SetBool(Settings.walkDown, true);
            }
        }
    }

    private void ResetIdleAnimation() {
        animator.SetBool(Settings.idleRight, false);
        animator.SetBool(Settings.idleLeft, false);
        animator.SetBool(Settings.idleUp, false);
        animator.SetBool(Settings.idleDown, false);
    }

    private void ResetMoveAnimation() {
        animator.SetBool(Settings.walkRight, false);
        animator.SetBool(Settings.walkLeft, false);
        animator.SetBool(Settings.walkUp, false);
        animator.SetBool(Settings.walkDown, false);
    }

    private void SetNPCFacingDirection() {
        ResetIdleAnimation();

        switch (npcFacingDirectionAtDestination) {
            case Direction.Right:
                animator.SetBool(Settings.idleRight, true);
                break;

            case Direction.Left:
                animator.SetBool(Settings.idleLeft, true);
                break;

            case Direction.Up:
                animator.SetBool(Settings.idleUp, true);
                break;

            case Direction.Down:
                animator.SetBool(Settings.idleDown, true);
                break;

            default:
                break;
        }
    }

    private void SetNPCEventAnimation() {
        if (npcTargetAnimationClip != null) {
            ResetIdleAnimation();
            animatorOverrideController[blankAnimation] = npcTargetAnimationClip;
            animator.SetBool(Settings.eventAnimation, true);
        }
        else {
            animatorOverrideController[blankAnimation] = blankAnimation;
            animator.SetBool(Settings.eventAnimation, false);
        }
    }

    private void InitialiseNPC() {
        // Active in scene
        if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name) {
            SetNPCActiveInScene();
        }
        else {
            SetNPCInactiveInScene();
        }
        
        npcPreviousMovementStepScene = npcCurrentScene;

        // Get NPC Current Grid Position
        npcCurrentGridPosition = GetGridPosition(transform.position);

        // Set Next Grid Position and Target Grid Position to current Grid Position
        npcNextGridPosition    = npcCurrentGridPosition;
        npcTargetGridPosition  = npcCurrentGridPosition;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);

        // Get NPC WorldPosition
        npcNextWorldPosition = GetWorldPosition(npcCurrentGridPosition);
    }

    #endregion



}