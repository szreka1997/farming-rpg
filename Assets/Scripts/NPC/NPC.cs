using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCMovement))]
[RequireComponent(typeof(GenerateGUID))]
public class NPC : MonoBehaviour, ISaveable
{
    #region Variables

    private NPCMovement npcMovement;

    #endregion


    #region Properties

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get => _iSaveableUniqueID;  set => _iSaveableUniqueID = value; }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave;  set => _gameObjectSave = value;  }

    #endregion


    #region Unity Basic Methods

    private void Awake() {
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start() {
        npcMovement = GetComponent<NPCMovement>();
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        ISaveableRegister();
    }

    private void OnDisable() {
        ISaveableDeregister();
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
        // Nothing required here since on persistent scene
    }

    public void ISaveableRestoreScene(string sceneName) {
        // Nothing required here since on persistent scene
    }

    public GameObjectSave ISaveableSave() {
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        SceneSave sceneSave = new SceneSave();
        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // Store target grid position, target world position, and target scene
        sceneSave.vector3Dictionary.Add("npcTargetGridPosition", 
                new Vector3Serializable(npcMovement.npcTargetGridPosition.x, npcMovement.npcTargetGridPosition.y, npcMovement.npcTargetGridPosition.z));

        sceneSave.vector3Dictionary.Add("npcTargetWorldPosition", 
                new Vector3Serializable(npcMovement.npcTargetWorldPosition.x, npcMovement.npcTargetWorldPosition.y, npcMovement.npcTargetWorldPosition.z));

        sceneSave.stringDictionary.Add("npcTargetScene", npcMovement.npcTargetScene.ToString());

        // Add scene save to game object
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave) {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave)) {
            GameObjectSave = gameObjectSave;

            if (GameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave)) {
                if (sceneSave.vector3Dictionary != null && sceneSave.stringDictionary != null) {
                    // target grid position
                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetGridPosition", out Vector3Serializable savedNPCTargetGridPosition)) {
                        npcMovement.npcTargetGridPosition = 
                                new Vector3Int((int)savedNPCTargetGridPosition.x, (int)savedNPCTargetGridPosition.y, (int)savedNPCTargetGridPosition.z);

                        npcMovement.npcCurrentGridPosition = npcMovement.npcTargetGridPosition;
                    }

                    // target world position
                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetWorldPosition", out Vector3Serializable savedNPCTargetWorldPosition)) {
                        npcMovement.npcTargetWorldPosition = 
                                new Vector3(savedNPCTargetWorldPosition.x, savedNPCTargetWorldPosition.y, savedNPCTargetWorldPosition.z);

                        transform.position = npcMovement.npcTargetWorldPosition;
                    }

                    // target scene
                    if (sceneSave.stringDictionary.TryGetValue("npcTargetScene", out string savedTargetScene)) {
                        if (Enum.TryParse<SceneName>(savedTargetScene, out SceneName sceneName)) {
                            npcMovement.npcTargetScene = sceneName;
                            npcMovement.npcCurrentScene = npcMovement.npcTargetScene;
                        }
                    }

                    // Clear any current NPC movement
                    npcMovement.CancelNPCMovement();
                }
            }
        }
    }

    #endregion

}