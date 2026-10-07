using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;

public class LightingControl : MonoBehaviour
{
    #region Variables

    [SerializeField] private SO_LightingSchedule so_lightingSchedule;
    [SerializeField] private bool isLightFlicker = false;
    [SerializeField] [Range(0f, 1f)] private float lightFlickerIntensity;
    [SerializeField] [Range(0f, 0.2f)] private float lightFlickerTimeMin;
    [SerializeField] [Range(0f, 0.2f)] private float lightFlickerTimeMax;

    // string key  - Season + Hour
    // float value - Light Intensity 
    private Dictionary<string, float> lightingBrightnessDictionary = new Dictionary<string, float>();

    private Light2D light2D;
    private Coroutine fadeInLightRoutine;
    private float currentLightIntensity;
    private float lightFlickerTimer = 0f;

    #endregion


    #region Unity Basic Methods

    private void Awake() {
        // Get 2D light
        light2D = GetComponentInChildren<Light2D>();

        // disable if no light2D
        if (light2D == null) {
            enabled = false;
        }

        // populate lighting brightness dictionary
        foreach (SO_LightingBrightness lightingBrightness in so_lightingSchedule.lightingBrightnessArray) {
            string key = lightingBrightness.season.ToString() + lightingBrightness.hour.ToString();

            if(!lightingBrightnessDictionary.ContainsKey(key)) {
                lightingBrightnessDictionary.Add(key, lightingBrightness.lightIntensity);
            }
        }
         
    }

    private void Update() {
        if (isLightFlicker) {
            lightFlickerTimer -= Time.deltaTime;
        }
    }
    
    private void LateUpdate() {
        if (lightFlickerTimer <= 0f && isLightFlicker) {
            LightFlicker();
        }
        else {
            light2D.intensity = currentLightIntensity;
        }
    }

    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.AdvanceGameHourEvent += AdvanceGameHourEvent;
        EventHandler.AfterSceneLoadFadeInEvent += AfterSceneLoadEvent;
    }

    private void OnDisable() {
        EventHandler.AdvanceGameHourEvent -= AdvanceGameHourEvent;
        EventHandler.AfterSceneLoadFadeInEvent -= AfterSceneLoadEvent;
    }

    #endregion


    #region Event Subscriber Methods

    private void AdvanceGameHourEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond) {
        string key = gameSeason.ToString() + (gameHour).ToString();

        if (gameHour == 0 && !lightingBrightnessDictionary.TryGetValue(key, out float targetLightingIntensity)) {
            SetLightingIntensity(gameSeason, gameHour, false);
        } else {
            SetLightingIntensity(gameSeason, gameHour, true);
        }
    }

    private void AfterSceneLoadEvent() {
        SetLightingAfterSceneLoaded();
    }

    #endregion


    

    /// <summary>
    /// After the scene is loaded get the season and hour to set the light intensity
    /// </summary>
    private void SetLightingAfterSceneLoaded() {
        Season gameSeason = TimeManager.Instance.GetGameSeason();
        int gameHour      = TimeManager.Instance.GetGameTime().Hours;


        // Set light intensity immediately without fading in
        SetLightingIntensity(gameSeason, gameHour, false);
    }

    /// <summary>
    /// Set the light intensity based on season and game hour
    /// </summary>
    private void SetLightingIntensity(Season gameSeason, int gameHour, bool fadein) {
        int i = 0;

        // Get light intensity for nearest game hour that is less than or equal to the current game hour for the same season
        while (i <= 23) {
            // check dictionary for value
            string key = gameSeason.ToString() + (gameHour).ToString();

            if (lightingBrightnessDictionary.TryGetValue(key, out float targetLightingIntensity)) {
                if (fadein) {
                    // stop fade in coroutine if already running
                    if (fadeInLightRoutine != null) {
                        StopCoroutine(fadeInLightRoutine);
                    }

                    // fade in to new light intensity level
                    fadeInLightRoutine = StartCoroutine(FadeLightRoutine(targetLightingIntensity));
                }
                else {
                    currentLightIntensity = targetLightingIntensity;
                }

                break;
            }

            i++;
            gameHour--;

            if (gameHour < 0) {
                gameHour = 23;
            }

        }

    }

    private IEnumerator FadeLightRoutine(float targetLightingIntensity) {
        float fadeDuration = 5f;

        // Calculate how fast the light should fade based current and target intensity and duration
        float fadeSpeed = Mathf.Abs(currentLightIntensity - targetLightingIntensity) / fadeDuration;

        // loop while fading
        while (!Mathf.Approximately(currentLightIntensity, targetLightingIntensity)) {
            // move the light intensity towards it's target intensity.
            currentLightIntensity = Mathf.MoveTowards(currentLightIntensity, targetLightingIntensity, fadeSpeed * Time.deltaTime);

            yield return null;
        }

        currentLightIntensity = targetLightingIntensity;
    }

    private void LightFlicker() {
        // calculate a random flicker intensity
        light2D.intensity = Random.Range(currentLightIntensity, currentLightIntensity + (currentLightIntensity * lightFlickerIntensity));

        // if the light is to flicker calculate a random flicker interval
        lightFlickerTimer = Random.Range(lightFlickerTimeMin, lightFlickerTimeMax);

    }
}
