using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Sounds/Scene Sounds List", fileName = "New so_Scene Sounds List")]
public class SO_SceneSoundsList : ScriptableObject
{
    [SerializeField] public List<SO_SceneSoundsItem> sceneSoundsDetails;
}