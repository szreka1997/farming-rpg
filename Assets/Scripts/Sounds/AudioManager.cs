using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : SingletonMonobehaviour<AudioManager>
{
    #region Variables

    [Header("Prefabs")]
    [SerializeField] private GameObject soundPrefab = null;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSoundAudioSource = null;
    [SerializeField] private AudioSource gameMusicAudioSource = null;

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer gameAudioMixer = null;

    [Header("Audio Snapshots")]
    [SerializeField] private AudioMixerSnapshot gameMusicSnapshot = null;
    [SerializeField] private AudioMixerSnapshot gameAmbientSnapshot = null;

    [Header("Other")]
    [SerializeField] private SO_SoundList so_soundList = null;
    [SerializeField] private SO_SceneSoundsList so_sceneSoundsList = null;
    [SerializeField] private float defaultSceneMusicPlayTimeSeconds = 120f;
    [SerializeField] private float sceneMusicStartMinSecs = 20f;
    [SerializeField] private float sceneMusicStartMaxSecs = 40f;
    [SerializeField] private float musicTransitionSecs = 8f;

    private Dictionary<SoundName, SO_SoundItem> soundDictionary;
    private Dictionary<SceneName, SO_SceneSoundsItem> sceneSoundsDictionary;

    private Coroutine playSceneSoundsCoroutine;

    #endregion


    #region Unity Basic Methods

    protected override void Awake() {
        base.Awake();

        soundDictionary       = new Dictionary<SoundName, SO_SoundItem>();
        sceneSoundsDictionary = new Dictionary<SceneName, SO_SceneSoundsItem>();

        foreach (SO_SoundItem soundItem in so_soundList.soundDetails) {
            soundDictionary.Add(soundItem.soundName, soundItem);
        }

        foreach (SO_SceneSoundsItem sceneSoundsItem in so_sceneSoundsList.sceneSoundsDetails) {
            sceneSoundsDictionary.Add(sceneSoundsItem.sceneName, sceneSoundsItem);
        }
    }
    
    #endregion


    #region Unity Event Related (On...) Methods

    private void OnEnable() {
        EventHandler.AfterSceneLoadEvent += PlaySceneSounds;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadEvent -= PlaySceneSounds;
    }

    #endregion


    #region Public Methods

    public void PlaySound(SoundName soundName) {
        if (soundDictionary.TryGetValue(soundName, out SO_SoundItem soundItem) && soundPrefab != null) {
            GameObject soundGameObject = PoolManager.Instance.ReuseObject(soundPrefab, Vector3.zero, Quaternion.identity);

            Sound sound = soundGameObject.GetComponent<Sound>();

            sound.SetSound(soundItem);
            soundGameObject.SetActive(true);
            StartCoroutine(DisableSound(soundGameObject, soundItem.soundClip.length));
        }
    }

    #endregion



    private void PlaySceneSounds() {
        SO_SoundItem musicSoundItem = null;
        SO_SoundItem ambientSoundItem = null;

        float musicPlayTime = defaultSceneMusicPlayTimeSeconds;

        // Try Get current scene
        if (Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, true, out SceneName currentSceneName)) {
            // Get Music and Ambient Sounds For Scene
            if (sceneSoundsDictionary.TryGetValue(currentSceneName, out SO_SceneSoundsItem sceneSoundsItem)) {
                soundDictionary.TryGetValue(sceneSoundsItem.musicForScene, out musicSoundItem);
                soundDictionary.TryGetValue(sceneSoundsItem.ambientSoundForScene, out ambientSoundItem);
            }
            else {
                return;
            }

            // Stop any scene sounds already playing
            if (playSceneSoundsCoroutine != null) {
                StopCoroutine(playSceneSoundsCoroutine);
            }    

            // Play scene ambient sounds and music
            playSceneSoundsCoroutine = StartCoroutine(PlaySceneSoundsRoutine(musicPlayTime, musicSoundItem, ambientSoundItem));
        }
    }

    private IEnumerator PlaySceneSoundsRoutine(float musicPlaySeconds, SO_SoundItem musicSoundItem, SO_SoundItem ambientSoundItem) {
        if (musicSoundItem != null && ambientSoundItem != null) {
            // Start with ambient sound
            PlayAmbientSoundClip(ambientSoundItem, 0f);

            // Wait for random range of seconds before playing music
            yield return new WaitForSeconds(UnityEngine.Random.Range(sceneMusicStartMinSecs, sceneMusicStartMaxSecs));

            // Play music
            PlayMusicSoundClip(musicSoundItem, musicTransitionSecs);

            // Wait for music play seconds before transitioning to ambient sounds
            yield return new WaitForSeconds(musicPlaySeconds);

            // Play ambient sound clip
            PlayAmbientSoundClip(ambientSoundItem, musicTransitionSecs);
        }
    }

    private void PlayMusicSoundClip(SO_SoundItem musicSoundItem, float transitionTimeSeconds) {
        // Set Volume
        gameAudioMixer.SetFloat("MusicVolume", ConvertSoundVolumeDecimalFractionToDecibels(musicSoundItem.soundVolume));

        // Set clip & play
        gameMusicAudioSource.clip = musicSoundItem.soundClip;
        gameMusicAudioSource.Play();

        // Transition to music snapshot
        gameMusicSnapshot.TransitionTo(transitionTimeSeconds);
    }

    private void PlayAmbientSoundClip(SO_SoundItem ambientSoundItem, float transitionTimeSeconds) {
        // Set Volume
        gameAudioMixer.SetFloat("AmbientVolume", ConvertSoundVolumeDecimalFractionToDecibels(ambientSoundItem.soundVolume));

        // Set clip & play
        ambientSoundAudioSource.clip = ambientSoundItem.soundClip;
        ambientSoundAudioSource.Play();

        // Transition to ambient
        gameAmbientSnapshot.TransitionTo(transitionTimeSeconds);
    }

    /// <summary>
    /// Convert volumeDecimalFraction to -80 to +20 decibel range
    /// </summary>
    private float ConvertSoundVolumeDecimalFractionToDecibels(float volumeDecimalFraction) {
        // Convert volume from decimal fraction to -80 to +20 decibel range
        return (volumeDecimalFraction * 100f - 80f);
    }

    private IEnumerator DisableSound(GameObject soundGameObject, float soundDuration) {
        yield return new WaitForSeconds(soundDuration);
        soundGameObject.SetActive(false);
    }

    
}