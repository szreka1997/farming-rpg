using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Item/Item List", fileName = "New so_Item List")]
public class SO_ItemList : ScriptableObject
{
    [SerializeField] public List<SO_ItemDetails> itemDetails = new List<SO_ItemDetails>();
}
