using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ObscuringItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;


    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    public void FadeOut() {
        if(gameObject.activeSelf) {
            StartCoroutine(FadeOutRoutine());
        }
    }

    public void FadeIn() {
        if(gameObject.activeSelf) {
            StartCoroutine(FadeInRoutine());
        }
    }


    private IEnumerator FadeOutRoutine() {
        // a - alpha
        float currentAlpha = spriteRenderer.color.a;  
        float distance = currentAlpha - Settings.targetAlpha;

        while(currentAlpha - Settings.targetAlpha > Mathf.Epsilon) {
            currentAlpha -= distance / Settings.fadeOutSeconds * Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            
            yield return null;
        }

        spriteRenderer.color = new Color(1f, 1f, 1f, Settings.targetAlpha);
    }

    private IEnumerator FadeInRoutine() {
        float currentAlpha = spriteRenderer.color.a;
        float distance = 1f - currentAlpha;

        while(1f - currentAlpha > Mathf.Epsilon) {
            currentAlpha += distance / Settings.fadeInSeconds * Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            
            yield return null;
        }

        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }


}
