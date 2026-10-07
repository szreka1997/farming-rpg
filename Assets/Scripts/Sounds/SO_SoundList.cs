using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Sounds/Sound List", fileName = "New so_Sound List")]
public class SO_SoundList : ScriptableObject
{
    [SerializeField] public List<SO_SoundItem> soundDetails;
}