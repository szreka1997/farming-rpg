using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Lighting/Lighting Brightness", fileName = "New so_Lighting Brightness")]
public class SO_LightingBrightness : ScriptableObject 
{
    public Season season;
    public int hour;
    public float lightIntensity;
}