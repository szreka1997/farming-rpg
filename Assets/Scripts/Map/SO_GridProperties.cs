using System.Collections.Generic;
using UnityEngine;

// We're gonna have 1 of these scriptable object asset for every scene

[CreateAssetMenu(menuName = "Scriptable Objects/Grid Properties", fileName = "New so_Grid Properties")]
public class SO_GridProperties : ScriptableObject
{
    public SceneName sceneName;
    public int gridWidth;    // Tilemap width
    public int gridHeight;   // Tilemap height
    public int originX;      // Bottom Left
    public int originY;

    [SerializeField] public List<GridProperty> gridPropertyList;
}