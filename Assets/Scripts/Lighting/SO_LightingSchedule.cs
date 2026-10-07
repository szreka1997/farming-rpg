using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Lighting/Lighting Schedule", fileName = "New so_Lighting Schedule")]
public class SO_LightingSchedule : ScriptableObject
{
    public SO_LightingBrightness[] lightingBrightnessArray;
}

