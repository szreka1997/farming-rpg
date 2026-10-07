using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Crop/Crop Details List", fileName = "New so_Crop Details List")]
public class SO_CropDetailsList : ScriptableObject
{
    [SerializeField] public List<SO_CropDetails> cropDetails;


    public SO_CropDetails GetCropDetails(int seedItemCode) {
        return cropDetails.Find(x => x.seedItemCode == seedItemCode);
    }

}
