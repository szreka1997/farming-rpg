using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Scene/Scene Route List", fileName = "New so_Scene Route List")]
public class SO_SceneRouteList : ScriptableObject
{
    public List<SO_SceneRoute> sceneRouteList;
}