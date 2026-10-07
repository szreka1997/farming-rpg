using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerObscuringItemFader : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        /* Get the gameobject we have collided with, 
                and then get all the Obscuring Item Fader components on it and its children - and then trigger the fade out */

        ObscuringItemFader[] obscuringItemFaders = other.GetComponentsInChildren<ObscuringItemFader>();

        if(obscuringItemFaders.Length > 0) {
            for(int i = 0; i < obscuringItemFaders.Length; i++) {
                obscuringItemFaders[i].FadeOut();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        /* Get the gameobject we have collided with, 
                and then get all the Obscuring Item Fader components on it and its children - and then trigger the fade in */      
        
        ObscuringItemFader[] obscuringItemFaders = other.GetComponentsInChildren<ObscuringItemFader>();

        if(obscuringItemFaders.Length > 0) {
            for(int i = 0; i < obscuringItemFaders.Length; i++) {
                obscuringItemFaders[i].FadeIn();
            }
        }
    }


}
