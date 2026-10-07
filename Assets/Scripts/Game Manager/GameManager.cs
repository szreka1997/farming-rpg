using UnityEngine;

public class GameManager : SingletonMonobehaviour<GameManager>
{
    public Weather currentWeather;


    protected override void Awake() {
        base.Awake();

        // Set starting weather
        currentWeather = Weather.Dry;

        //TODO: Need a resolution settings options screen
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow, 0);
    }


    
}