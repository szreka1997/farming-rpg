using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    // Different objects can be here, but they all need to implement the ISaveable interface
    public List<ISaveable> iSaveableObjectList;
    public GameSave gameSave;



    protected override void Awake() {
        base.Awake();
        iSaveableObjectList = new List<ISaveable>();
    }



    public void StoreCurrentSceneData() {
        // loop through all ISaveable objects and trigger store scene data for each
        foreach (ISaveable iSaveableObject in iSaveableObjectList) {
            iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void RestoreCurrentSceneData() {
        // loop through all ISaveable objects and trigger restore scene data for each
        foreach (ISaveable iSaveableObject in iSaveableObjectList) {
            iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void LoadDataFromFile() {
        // We gonna use this to serialize our data
        BinaryFormatter bf = new BinaryFormatter();

        // if we have a save game already saved
        if (File.Exists(Application.persistentDataPath + "/WildHopeCreek.dat")) {
            gameSave = new GameSave();

            // Opens the file stream for reading
            FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.dat", FileMode.Open);

            gameSave = (GameSave)bf.Deserialize(file);

            // loop through all ISaveable objects and apply save data
            for (int i = iSaveableObjectList.Count - 1; i > -1; i--) {
                if (gameSave.gameObjectData.ContainsKey(iSaveableObjectList[i].ISaveableUniqueID)) {
                    iSaveableObjectList[i].ISaveableLoad(gameSave);
                }
                // else if iSaveableObject unique ID is not in the game object data then destroy object
                else {
                    Component component = (Component)iSaveableObjectList[i];
                    Destroy(component.gameObject);
                }
            }

            file.Close();
        }

        UIManager.Instance.DisablePauseMenu();
        UIManager.Instance.DisableInventoryMenu();
        UIManager.Instance.DisableShopMenu();
    }

    public void SaveDataToFile() {
        gameSave = new GameSave();

        // loop through all ISaveable objects and generate save data
        foreach (ISaveable iSaveableObject in iSaveableObjectList) {
            gameSave.gameObjectData.Add(iSaveableObject.ISaveableUniqueID, iSaveableObject.ISaveableSave());
        }

        BinaryFormatter bf = new BinaryFormatter();

        // Create - if the file already exist then this method will override it
        FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.dat", FileMode.Create);

        bf.Serialize(file, gameSave);

        file.Close();

        UIManager.Instance.DisablePauseMenu();
        UIManager.Instance.DisableInventoryMenu();
        UIManager.Instance.DisableShopMenu();
    }


}
