// We're gonna have a SceneItem instance for every item that we have in our scene

[System.Serializable]
public class SceneItem
{
    public int itemCode;
    public Vector3Serializable position;
    public string itemName;


    public SceneItem() {
        position = new Vector3Serializable();
    }
}