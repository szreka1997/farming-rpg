using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISaveable
{
    #region Variables

    [SerializeField] private SO_GridProperties[] so_GridPropertiesArray = null;
    [SerializeField] private SO_CropDetailsList so_CropDetailsList = null;
    [SerializeField] private Tile[] dugGroundTileArray = null;
    [SerializeField] private Tile[] wateredGroundTileArray = null;

    private Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
    private string iSaveableUniqueID;
    private GameObjectSave gameObjectSave;
    private Transform cropParentTransform;

    private Grid grid;
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;

    private bool isFirstTimeSceneLoaded = true;

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

    private void Start() {
        InitialiseGridProperties();
    }

    #endregion



    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        ISaveableRegister();
        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }

    private void OnDisable() {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
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
        GameObjectSave.sceneData.Remove(sceneName);
        SceneSave sceneSave = new SceneSave();

        // create & add dict grid property details dictionary
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDetailsDictionary;

        // create & add bool dictionary for first time scene loaded
        sceneSave.boolDictionary = new Dictionary<string, bool>();
        sceneSave.boolDictionary.Add("isFirstTimeSceneLoaded", isFirstTimeSceneLoaded);

        // Add scene save to game object scene data
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    public void ISaveableRestoreScene(string sceneName) {
        // Get sceneSave for scene - it exists since we created it in initialise
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave)) {
            // get grid property details dictionary - it exists since we created it in initialise
            if (sceneSave.gridPropertyDetailsDictionary != null) {
                gridPropertyDetailsDictionary = sceneSave.gridPropertyDetailsDictionary;
            }

            // get dictionary of bools - it exists since we created it in initialise
            if (sceneSave.boolDictionary != null && sceneSave.boolDictionary.TryGetValue("isFirstTimeSceneLoaded", out bool storedIsFirstTimeSceneLoaded)) {
                isFirstTimeSceneLoaded = storedIsFirstTimeSceneLoaded;
            }

            // Instantiate any crop prefabs initially present in the scene
            if (isFirstTimeSceneLoaded) {
                EventHandler.CallInstantiateCropPrefabsEvent();
            }

            // If grid properties exist
            if (gridPropertyDetailsDictionary.Count > 0)
            {
                // grid property details found for the current scene destroy existing ground decoration
                ClearDisplayGridPropertyDetails();

                // Instantiate grid property details for current scene
                DisplayGridPropertyDetails();
            }

            // Update first time scene loaded bool
            if (isFirstTimeSceneLoaded) {
                isFirstTimeSceneLoaded = false;
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

    /// <summary>
    /// Returns the gridPropertyDetails at the gridlocation for the supplied dictionary, or null if no properties exist at that location.
    /// </summary>
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY, Dictionary<string, GridPropertyDetails> gridPropertyDictionary) {
        // Construct key from coordinate
        string key = "x" + gridX + "y" + gridY;
        GridPropertyDetails gridPropertyDetails;

        // Check if grid property details exist forcoordinate and retrieve
        if (!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails)) {
            return null;
        } else {
            return gridPropertyDetails;
        }
    }

    
    /// <summary>
    /// Get the grid property details for the tile at (gridX,gridY).  If no grid property details exist null is returned and can assume that all grid property details values are null or false
    /// </summary>
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY) {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDetailsDictionary);
    }

    /// <summary>
    /// Set the grid property details to gridPropertyDetails for the tile at (gridX,gridY) for the gridpropertyDictionary.
    /// </summary>
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, 
                                       Dictionary<string, GridPropertyDetails> gridPropertyDictionary) {

        // Construct key from coordinate
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        // Set value
        gridPropertyDictionary[key] = gridPropertyDetails;
    }

    /// <summary>
    /// Set the grid property details to gridPropertyDetails for the tile at (gridX,gridY) for current scene
    /// </summary>
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails) {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDetailsDictionary);
    }

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails) {
        if (gridPropertyDetails.daysSinceDug > -1) {
            ConnectDugGround(gridPropertyDetails);
        }
    }

    public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails) {
        if (gridPropertyDetails.daysSinceWatered > -1) {
            ConnectWateredGround(gridPropertyDetails);
        }
    }

    public void DisplayPlantedCrop(GridPropertyDetails gridPropertyDetails) {
        // if greater than -1, then the grid is planted
        if (gridPropertyDetails.seedItemCode > -1) {
            SO_CropDetails so_cropDetails = so_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);

            if(so_cropDetails != null) {
                // prefab to use
                GameObject cropPrefab;

                // instantiate crop prefab at grid location
                int growthStages = so_cropDetails.growthDays.Length;

                int currentGrowthStage = 0;

                for (int i = growthStages - 1; i >= 0; i--) {
                    if (gridPropertyDetails.growthDays >= so_cropDetails.growthDays[i]) {
                        currentGrowthStage = i;
                        break;
                    }
                }

                cropPrefab = so_cropDetails.growthPrefab[currentGrowthStage];
                Sprite growthSprite = so_cropDetails.growthSprite[currentGrowthStage];

                Vector3 worldPosition = groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));

                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y + Settings.gridCellSize / 4, worldPosition.z);

                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity);

                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;
                cropInstance.transform.SetParent(cropParentTransform);
                cropInstance.GetComponent<Crop>().cropGridPosition = new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
            }
        }
    }

    /// <summary>
    ///  Returns the Crop object at the gridX, gridY position or null if no crop was found
    /// </summary>
    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails) {
        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(worldPosition);

        // Loop through colliders to get crop game object
        Crop crop = null;

        for (int i = 0; i < collider2DArray.Length; i++) {
            crop = collider2DArray[i].gameObject.GetComponentInParent<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY)) {
                break;
            }

            crop = collider2DArray[i].gameObject.GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY)) {
                break;
            }
        }

        return crop;
    }

    /// <summary>
    /// Returns Crop Details for the provided seedItemCode
    /// </summary>
    public SO_CropDetails GetCropDetails(int seedItemCode) {
        return so_CropDetailsList.GetCropDetails(seedItemCode);
    }

    /// <summary>
    /// for sceneName this method returns a Vector2Int with the grid dimensions for that scene, or Vector2Int.zero if scene not found
    /// </summary>
    public bool GetGridDimensions(SceneName sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin) {
        gridDimensions = Vector2Int.zero;
        gridOrigin = Vector2Int.zero;

        // loop through scenes
        foreach (SO_GridProperties so_GridProperties in so_GridPropertiesArray) {
            if (so_GridProperties.sceneName == sceneName) {
                gridDimensions.x = so_GridProperties.gridWidth;
                gridDimensions.y = so_GridProperties.gridHeight;

                gridOrigin.x = so_GridProperties.originX;
                gridOrigin.y = so_GridProperties.originY;

                return true;
            }
        }

        return false;
    }

    #endregion



    #region Private Methods

    private void AfterSceneLoaded() {
        grid = GameObject.FindObjectOfType<Grid>();
        
        if (GameObject.FindGameObjectWithTag(Tags.CropsParentTransform) != null) {
            cropParentTransform = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform).transform;
        }
        else {
            cropParentTransform = null;
        }
        
        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
    }

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond) {
        // Clear Display All Grid Property Details
        ClearDisplayGridPropertyDetails();

        // Loop through all scenes - by looping through all gridproperties in the array
        foreach (SO_GridProperties so_GridProperties in so_GridPropertiesArray) {
            // Get gridpropertydetails dictionary for scene
            if (GameObjectSave.sceneData.TryGetValue(so_GridProperties.sceneName.ToString(), out SceneSave sceneSave)) {
                if (sceneSave.gridPropertyDetailsDictionary != null) {
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count - 1; i >= 0; i--) {
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);

                        GridPropertyDetails gridPropertyDetails = item.Value;

                        // If ground is watered, then clear water
                        if (gridPropertyDetails.daysSinceWatered > -1) {
                            gridPropertyDetails.daysSinceWatered = -1;
                        }

                        // If a crop is planted
                        if (gridPropertyDetails.growthDays > -1) {
                            gridPropertyDetails.growthDays += 1;
                        }

                        // Set gridpropertydetails
                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails, 
                                               sceneSave.gridPropertyDetailsDictionary);
                    }
                }
            }
        }

        // Display grid property details to reflect changed values
        DisplayGridPropertyDetails();
    }

    /// <summary>
    /// This initialises the grid property dictionary with the values from the SO_GridProperties assets and stores the values for each scene in
    /// GameObjectSave sceneData
    /// </summary>
    private void InitialiseGridProperties() {
        foreach(SO_GridProperties so_GridProperties in so_GridPropertiesArray) {
            Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary = new Dictionary<string, GridPropertyDetails>();

            // Populate grid property dictionary - Iterate through all the grid properties in the so gridproperties list
            foreach (GridProperty gridProperty in so_GridProperties.gridPropertyList) {
                GridPropertyDetails gridPropertyDetails;
                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetailsDictionary);

                if (gridPropertyDetails == null) {
                    gridPropertyDetails = new GridPropertyDetails();
                }

                switch (gridProperty.gridBoolProperty) {
                    case GridBoolProperty.Diggable:
                        gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.CanDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.CanPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.IsPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.IsNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;

                    default:
                        break;
                }

                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDetailsDictionary);
            }

            // Create scene save for this gameobject
            SceneSave sceneSave = new SceneSave();

            // Add grid property dictionary to scene save data
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDetailsDictionary;

            // If starting scene set the gridPropertyDictionary member variable to the current iteration
            if (so_GridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString()) {
                this.gridPropertyDetailsDictionary = gridPropertyDetailsDictionary;
            }

            // Add bool dictionary and set first time scene loaded to true
            sceneSave.boolDictionary = new Dictionary<string, bool>();
            sceneSave.boolDictionary.Add("isFirstTimeSceneLoaded", true);

            // Add scene save to game object scene data
            GameObjectSave.sceneData.Add(so_GridProperties.sceneName.ToString(), sceneSave);
        }
    }

    private void ClearDisplayGroundDecorations() {
        // Remove ground decorations
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }

    private void ClearDisplayGridPropertyDetails() {
        ClearDisplayGroundDecorations();
        ClearDisplayAllPlantedCrops();
    }

    private void ClearDisplayAllPlantedCrops() {
        Crop[] cropArray;
        cropArray = FindObjectsOfType<Crop>();

        foreach (Crop crop in cropArray) {
            Destroy(crop.gameObject);
        }
    }

    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails) {
        // Select tile based on surrounding dug tiles

        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);

        // Set 4 tiles if dug surrounding current tile - up, down, left, right now that this central tile has been dug

        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), dugTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), dugTile2);
        }
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), dugTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), dugTile4);
        }
    }

    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails) {
        // Select tile based on surrounding watered tiles

        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);

        // Set 4 tiles if watered surrounding current tile - up, down, left, right now that this central tile has been watered

        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1) {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1) {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1) {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), wateredTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1) {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), wateredTile4);
        }
    }

    private Tile SetDugTile(int xGrid, int yGrid) {
        //Get whether surrounding tiles (up,down,left, and right) are dug or not

        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);

        if (!upDug && !downDug && !rightDug && !leftDug) {
            return dugGroundTileArray[0];
        }
        else if (!upDug && downDug && rightDug && !leftDug) {
            return dugGroundTileArray[1];
        }
        else if (!upDug && downDug && rightDug && leftDug) {
            return dugGroundTileArray[2];
        }
        else if (!upDug && downDug && !rightDug && leftDug) {
            return dugGroundTileArray[3];
        }
        else if (!upDug && downDug && !rightDug && !leftDug) {
            return dugGroundTileArray[4];
        }
        else if (upDug && downDug && rightDug && !leftDug) {
            return dugGroundTileArray[5];
        }
        else if (upDug && downDug && rightDug && leftDug) {
            return dugGroundTileArray[6];
        }
        else if (upDug && downDug && !rightDug && leftDug) {
            return dugGroundTileArray[7];
        }
        else if (upDug && downDug && !rightDug && !leftDug) {
            return dugGroundTileArray[8];
        }
        else if (upDug && !downDug && rightDug && !leftDug) {
            return dugGroundTileArray[9];
        }
        else if (upDug && !downDug && rightDug && leftDug) {
            return dugGroundTileArray[10];
        }
        else if (upDug && !downDug && !rightDug && leftDug) {
            return dugGroundTileArray[11];
        }
        else if (upDug && !downDug && !rightDug && !leftDug) {
            return dugGroundTileArray[12];
        }
        else if (!upDug && !downDug && rightDug && !leftDug) {
            return dugGroundTileArray[13];
        }
        else if (!upDug && !downDug && rightDug && leftDug) {
            return dugGroundTileArray[14];
        }
        else if (!upDug && !downDug && !rightDug && leftDug) {
            return dugGroundTileArray[15];
        }

        return null;
    }

    private Tile SetWateredTile(int xGrid, int yGrid) {
        // Get whether surrounding tiles (up,down,left, and right) are watered or not

        bool upWatered = IsGridSquareWatered(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareWatered(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareWatered(xGrid - 1, yGrid);
        bool rightWatered = IsGridSquareWatered(xGrid + 1, yGrid);

        if (!upWatered && !downWatered && !rightWatered && !leftWatered) {
            return wateredGroundTileArray[0];
        }
        else if (!upWatered && downWatered && rightWatered && !leftWatered) {
            return wateredGroundTileArray[1];
        }
        else if (!upWatered && downWatered && rightWatered && leftWatered) {
            return wateredGroundTileArray[2];
        }
        else if (!upWatered && downWatered && !rightWatered && leftWatered) {
            return wateredGroundTileArray[3];
        }
        else if (!upWatered && downWatered && !rightWatered && !leftWatered) {
            return wateredGroundTileArray[4];
        }
        else if (upWatered && downWatered && rightWatered && !leftWatered) {
            return wateredGroundTileArray[5];
        }
        else if (upWatered && downWatered && rightWatered && leftWatered) {
            return wateredGroundTileArray[6];
        }
        else if (upWatered && downWatered && !rightWatered && leftWatered) {
            return wateredGroundTileArray[7];
        }
        else if (upWatered && downWatered && !rightWatered && !leftWatered) {
            return wateredGroundTileArray[8];
        }
        else if (upWatered && !downWatered && rightWatered && !leftWatered) {
            return wateredGroundTileArray[9];
        }
        else if (upWatered && !downWatered && rightWatered && leftWatered) {
            return wateredGroundTileArray[10];
        }
        else if (upWatered && !downWatered && !rightWatered && leftWatered) {
            return wateredGroundTileArray[11];
        }
        else if (upWatered && !downWatered && !rightWatered && !leftWatered) {
            return wateredGroundTileArray[12];
        }
        else if (!upWatered && !downWatered && rightWatered && !leftWatered) {
            return wateredGroundTileArray[13];
        }
        else if (!upWatered && !downWatered && rightWatered && leftWatered) {
            return wateredGroundTileArray[14];
        }
        else if (!upWatered && !downWatered && !rightWatered && leftWatered) {
            return wateredGroundTileArray[15];
        }

        return null;
    }

    private bool IsGridSquareDug(int xGrid, int yGrid) {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null) {
            return false;
        } else if (gridPropertyDetails.daysSinceDug > -1) {
            return true;
        } else {
            return false;
        }
    }

    private bool IsGridSquareWatered(int xGrid, int yGrid) {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null) {
            return false;
        }
        else if (gridPropertyDetails.daysSinceWatered > -1) {
            return true;
        } 
        else {
            return false;
        }
    }

    private void DisplayGridPropertyDetails() {
        // Loop through all grid items
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDetailsDictionary) {
            GridPropertyDetails gridPropertyDetails = item.Value;

            DisplayDugGround(gridPropertyDetails);
            DisplayWateredGround(gridPropertyDetails);
            DisplayPlantedCrop(gridPropertyDetails);
        }
    }

    #endregion

    

    
}
