using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenerateGUID))]
public class SceneItemsManager : SingletonMonobehaviour<SceneItemsManager>, ISaveable
{
    #region Variables

    [SerializeField] private GameObject itemPrefab = null;
    
    private Transform parentItem;
    private string iSaveableUniqueID;
    private GameObjectSave gameObjectSave;

    public string ISaveableUniqueID { 
        get => iSaveableUniqueID; 
        set => iSaveableUniqueID = value; 
    }

    public GameObjectSave GameObjectSave { 
        get => gameObjectSave; 
        set => gameObjectSave = value; 
    }

    #endregion



    #region Unity Basic Methods

    protected override void Awake() {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    #endregion



    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        ISaveableRegister();
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    #endregion



    #region Interface Methods

    public void ISaveableRegister() {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister() {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableStoreScene(string sceneName) {
        // Remove old scene save for gameObject if exists
        GameObjectSave.sceneData.Remove(sceneName);

        // Get all items in the scene
        List<SceneItem> sceneItemList = new List<SceneItem>();
        Item[] itemsInScene = FindObjectsOfType<Item>();

        // Loop through all scene items
        foreach (Item item in itemsInScene) {
            SceneItem sceneItem = new SceneItem();

            sceneItem.itemCode = item.ItemCode;
            sceneItem.position = new Vector3Serializable(item.transform.position.x, item.transform.position.y, item.transform.position.z);
            sceneItem.itemName = item.name;

            // Add scene item to list
            sceneItemList.Add(sceneItem);
        }

        // Create list scene items in scene save and set to scene item list
        SceneSave sceneSave = new SceneSave();
        sceneSave.listSceneItem = sceneItemList;

        // Add scene save to gameobject
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    public void ISaveableRestoreScene(string sceneName) {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave)) {
            if (sceneSave.listSceneItem != null) {
                // scene list items found - destroy existing items in scene
                DestroySceneItems();

                // now instantiate the list of scene items
                InstantiateSceneItems(sceneSave.listSceneItem);
            }
        }
    }

    public GameObjectSave ISaveableSave() {
        // Store current scene data
        ISaveableStoreScene(SceneManager.GetActiveScene().name);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave) {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave)) {
            GameObjectSave = gameObjectSave;

            // Restore data for current scene
            ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }

    #endregion



    #region Public Methods

    public void InstantiateSceneItem(int itemCode, Vector3 itemPosition) {
        GameObject instance = Instantiate(itemPrefab, itemPosition, Quaternion.identity, parentItem);
        Item item = instance.GetComponent<Item>();
        item.Init(itemCode);
    }

    #endregion



    #region Private Methods

    private void AfterSceneLoad() {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

    /// <summary>
    /// Destroy items currently in the scene
    /// </summary>
    private void DestroySceneItems() {
        // Get all items in the scene
        Item[] itemsInScene = GameObject.FindObjectsOfType<Item>();

        // Loop through all scene items and destroy them
        for(int i = itemsInScene.Length - 1; i > -1; i--) {
            Destroy(itemsInScene[i].gameObject);
        }
    }

    private void InstantiateSceneItems(List<SceneItem> sceneItemList) {
        GameObject instance;

        foreach(SceneItem sceneItem in sceneItemList) {
            instance = Instantiate(itemPrefab, 
                                   new Vector3(sceneItem.position.x, sceneItem.position.y, sceneItem.position.z), 
                                   Quaternion.identity, 
                                   parentItem);

            Item item = instance.GetComponent<Item>();
            item.ItemCode = sceneItem.itemCode;
            item.name = sceneItem.itemName;
        }
    }

    #endregion

    

    

    

    

    

   

    
    


}
