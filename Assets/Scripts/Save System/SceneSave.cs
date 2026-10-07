using System.Collections.Generic;

[System.Serializable]
public class SceneSave
{
    // Saving Scene Items
    public List<SceneItem> listSceneItem;


    // Saving Grid Property Details
    // string key is the grid coordinate
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;


    // Initializing Crops for the first time
    // string key is an identifier name we choose for this list
    public Dictionary<string, bool> boolDictionary;    


    // Saving Player
    // string value is the scene name and the player direction
    public Dictionary<string, string> stringDictionary;
    public Dictionary<string, Vector3Serializable> vector3Dictionary;


    // Saving Inventory
    public List<InventoryItem>[] listInvItemArray;
    // capacities
    public Dictionary<string, int[]> intArrayDictionary;


    // Saving Time
    public Dictionary<string, int> intDictionary;
}