using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// An editor class (this class) must be in an "Editor" folder!
// There can be multiple "Editor" folder in multiple places
//
// We need to sign that which custom property attribute we want to draw
[CustomPropertyDrawer(typeof(ItemCodeDescriptionAttribute))]
public class ItemCodeDescriptionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // Change the returned property height to be double to cater for the additional item code description that we will draw
        return EditorGUI.GetPropertyHeight(property) * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Because ItemCode is an integer
        if(property.propertyType == SerializedPropertyType.Integer) {
            // Start of check for changed velues
            EditorGUI.BeginChangeCheck();  

            // Draw ItemCode (as default)
            var newValue = EditorGUI.IntField(new Rect(position.x, position.y, position.width, position.height / 2), 
                                                label, property.intValue);

            // Draw Item Description
            EditorGUI.LabelField(new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2),
                                            "Item Description", GetItemDescription(property.intValue));

            // If ItemCode value has changed, then set value to new value
            if(EditorGUI.EndChangeCheck()) {
                property.intValue = newValue;
            }
        }

        EditorGUI.EndProperty();
    }



    private string GetItemDescription(int itemCode) {
        SO_ItemList itemList;
        itemList = AssetDatabase.LoadAssetAtPath("Assets/Scriptable Object Assets/Item/so_Item List.asset", typeof(SO_ItemList)) as SO_ItemList;

        List<SO_ItemDetails> itemDetailsList = itemList.itemDetails;
        SO_ItemDetails itemDetail = itemDetailsList.Find(x => x.itemCode == itemCode);

        if(itemDetail != null) {
            return itemDetail.itemDescription;
        } else {
            return "";
        }
    }


}
