using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AStar))]
public class NPCManager : SingletonMonobehaviour<NPCManager>
{
    #region Variables

    [SerializeField] private SO_SceneRouteList so_SceneRouteList = null;

    [HideInInspector] public NPC[] npcArray;

    private Dictionary<string, SO_SceneRoute> so_sceneRouteDictionary;
    private AStar aStar;

    #endregion


    #region Unity Basic Methods

    protected override void Awake() {
        base.Awake();

        // Create sceneRoute dictionary
        so_sceneRouteDictionary = new Dictionary<string, SO_SceneRoute>();

        if (so_SceneRouteList.sceneRouteList.Count > 0) {
            foreach (SO_SceneRoute so_sceneRoute in so_SceneRouteList.sceneRouteList) {
                // Check for duplicate routes in dictionary
                if (so_sceneRouteDictionary.ContainsKey(so_sceneRoute.fromSceneName.ToString() + so_sceneRoute.toSceneName.ToString())) {
                    Debug.Log("** Duplicate Scene Route Key Found ** Check for duplicate routes in the scriptable object scene route list");
                    continue;
                }

                // Add route to dictionary
                so_sceneRouteDictionary.Add(so_sceneRoute.fromSceneName.ToString() + so_sceneRoute.toSceneName.ToString(), so_sceneRoute);
            }
        }

        aStar = GetComponent<AStar>();
        npcArray = FindObjectsOfType<NPC>();
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    #endregion


    #region Public Methods 

    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition, Stack<NPCMovementStep> npcMovementStepStack) {
        return (aStar.BuildPath(sceneName, startGridPosition, endGridPosition, npcMovementStepStack));
    }

    public SO_SceneRoute GetSceneRoute(string fromSceneName, string toSceneName) {
        SO_SceneRoute sceneRoute;

        // Get scene route from dictionary
        if (so_sceneRouteDictionary.TryGetValue(fromSceneName + toSceneName, out sceneRoute)) {
            return sceneRoute;
        }
        else {
            return null;
        }
    }

    #endregion



    private void AfterSceneLoad() {
        SetNPCsActiveStatus();
    }

    private void SetNPCsActiveStatus() {
        foreach (NPC npc in npcArray){
            NPCMovement npcMovement = npc.GetComponent<NPCMovement>();

            if (npcMovement.npcCurrentScene.ToString() == SceneManager.GetActiveScene().name) {
                npcMovement.SetNPCActiveInScene();
            }
            else {
                npcMovement.SetNPCInactiveInScene();
            }
        }
    }


}