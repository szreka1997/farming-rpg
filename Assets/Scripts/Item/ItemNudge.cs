using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemNudge : MonoBehaviour
{
    private WaitForSeconds pause;
    private bool isAnimating = false;


    private void Awake() {
        pause = new WaitForSeconds(0.04f);
    }


    private void OnTriggerEnter2D(Collider2D other) {
        if (isAnimating == false) {
            if (transform.position.x < other.transform.position.x) {
                StartCoroutine(RotateAntiClock());
            } else {
                StartCoroutine(RotateClock());
            }

            //Play rustle sound if player
            if (other.gameObject.tag == "Player") {
                AudioManager.Instance.PlaySound(SoundName.EffectRustle);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (isAnimating == false) {
            if (transform.position.x > other.transform.position.x) {
                StartCoroutine(RotateAntiClock());
            } else {
                StartCoroutine(RotateClock());
            }

            //Play rustle sound if player
            if (other.gameObject.tag == "Player")
            {
                AudioManager.Instance.PlaySound(SoundName.EffectRustle);
            }
        }
    }

    private IEnumerator RotateAntiClock() {
        isAnimating = true;

        for(int i = 0; i < 4; i++) {
            transform.GetChild(0).Rotate(0f, 0f, 2f);
            yield return pause;
        }

        for(int i = 0; i < 5; i++) {
            transform.GetChild(0).Rotate(0f, 0f, -2f);
            yield return pause;
        }

        transform.GetChild(0).Rotate(0f, 0f, 2f);
        yield return pause;

        isAnimating = false;
    }

    private IEnumerator RotateClock() {
        isAnimating = true;

        for(int i = 0; i < 4; i++) {
            transform.GetChild(0).Rotate(0f, 0f, -2f);
            yield return pause;
        }

        for(int i = 0; i < 5; i++) {
            transform.GetChild(0).Rotate(0f, 0f, 2f);
            yield return pause;
        }

        transform.GetChild(0).Rotate(0f, 0f, -2f);
        yield return pause;

        isAnimating = false;
    }


}
