using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Scene/Scene Route", fileName = "New so_Scene Route")]
public class SO_SceneRoute : ScriptableObject
{
    public SceneName fromSceneName;
    public SceneName toSceneName;
    public List<SO_ScenePath> scenePathList;
}