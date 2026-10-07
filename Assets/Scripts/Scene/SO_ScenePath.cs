using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Scene/Scene Path", fileName = "New so_Scene Path")]
public class SO_ScenePath : ScriptableObject
{
    public SceneName sceneName;
    public GridCoordinate fromGridCell;
    public GridCoordinate toGridCell;
}