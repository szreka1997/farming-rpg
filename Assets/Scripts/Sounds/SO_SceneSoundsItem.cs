using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Sounds/Scene Sound Item", fileName = "New so_Scene Sound Item")]
public class SO_SceneSoundsItem : ScriptableObject
{
    public SceneName sceneName;
    public SoundName ambientSoundForScene;
    public SoundName musicForScene;
}