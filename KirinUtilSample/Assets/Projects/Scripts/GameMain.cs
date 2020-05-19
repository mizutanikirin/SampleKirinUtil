using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KirinUtil;
using UnityEngine.UI;
using com.rfilkov.kinect;
using System;

public class GameMain : MonoBehaviour
{
    // Public Vars
    public float GameTime = 30f;

    public CountDown CountDown;

    public Text CountDownText;
    public Text TimerText;
    public Text ScoreText;
    public Text ResultText;

    public Timer UserTimer;
    public Timer GameTimer;

    public CharController[] CharControllers;

    // Private Vars
    enum Scene
    {
        Loading, Idling, Countdown, GamePlaying, Finished
    }
    private Scene CurrentScene;

    private KinectManager _Kinect;

    private int score;

    // Start is called before the first frame update
    void Start()
    {
        Util.BasicSetting(new Vector2(1920f, 1080f), true, 60, false);

        CurrentScene = Scene.Loading;

        _Kinect = KinectManager.Instance;

        UserTimer = new Timer();
        GameTimer = new Timer();

        // Add event listener
        foreach (var _CharController in CharControllers)
        {
            _CharController.HitEvent.AddListener(CharHitEvent);
        }

        // Load contents
        Util.image.LoadPlayImages();
        Util.sound.LoadSounds();
    }

    // Update is called once per frame
    void Update()
    {
        // Key Bindings
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Swith Update Method by Scene
        switch (CurrentScene)
        {
            case Scene.Idling:
                {
                    IdlingUpdate();
                }
                break;

            case Scene.Countdown:
                {
                    CountdownUpdate();
                }
                break;

            case Scene.GamePlaying:
                {
                    GamePlayingUpdate();
                }
                break;
        }
    }

    void OnDestroy()
    {
        // Remove event listener
        foreach (var _CharController in CharControllers)
        {
            _CharController.HitEvent.RemoveListener(CharHitEvent);
        }
    }

    public void SoundLoadFinished()
    {
        Debug.Log(gameObject.name + ":SoundLoadFinished()");

        StartIdling();
    }

    private void StartIdling()
    {
        Debug.Log(gameObject.name + ":Idling()");

        CurrentScene = Scene.Idling;

        // Hide UIs
        Util.media.FadeOutUI(TimerText.gameObject, 0.5f, 0);
        Util.media.FadeOutUI(ScoreText.gameObject, 0.5f, 0);

        // Hide characters
        foreach (var _CharController in CharControllers)
        {
            _CharController.StopCharacter();
        }

        // Start user detection timer
        UserTimer.LimitTime = 3f;
    }

    private void IdlingUpdate()
    {
        if (_Kinect.IsInitialized() && _Kinect.IsUserDetected(0))
        {
            // Start countdown if user is in area for 3 seconds
            if (UserTimer.Update())
            {
                StartCountdown();
            }
        }
        else
        {
            UserTimer = new Timer();
            UserTimer.LimitTime = 3f;
        }
    }

    private void StartCountdown()
    {
        Debug.Log(gameObject.name + ":StartCountdown()");

        CurrentScene = Scene.Countdown;

        // Show and start countdown
        Util.media.FadeInUI(CountDownText.gameObject, 0.5f, 0);
        CountDown.SetCountDown(3, CountDownText, false, 1, 3);
    }

    private void CountdownUpdate()
    {
        if (CountDown.Update2())
        {
            StartGamePlaying();
        }
    }

    private void StartGamePlaying()
    {
        Debug.Log(gameObject.name + ":StartGame()");

        CurrentScene = Scene.GamePlaying;

        // Hide countdown
        Util.media.FadeOutUI(CountDownText.gameObject, 0.5f, 0);

        // Show UIs
        Util.media.FadeInUI(TimerText.gameObject, 0.5f, 0);
        Util.media.FadeInUI(ScoreText.gameObject, 0.5f, 0);

        // Start SE
        Util.sound.PlaySE(2);

        // Start BGM
        Util.sound.PlayBGM(0);

        // Start moving characters
        foreach (var _CharController in CharControllers)
        {
            _CharController.StartMoving();
        }

        score = 0;

        GameTimer.LimitTime = GameTime;
    }

    private void GamePlayingUpdate()
    {
        // Display time and score
        TimerText.text = "Timer: " + GameTimer.RemainingTime.ToString("0.0");
        ScoreText.text = "Score: " + score.ToString("0");

        if (GameTimer.Update())
        {
            // To finish scene if timer is stopped
            FinishGame();
        }
        else
        {
            // Show character if it is hidden
            foreach (var _CharController in CharControllers)
            {
                if (_CharController.gameObject.activeSelf == false)
                {
                    _CharController.StartMoving();
                }
            }
        }
    }

    private void CharHitEvent(int char_score)
    {
        if (CurrentScene == Scene.GamePlaying)
        {
            // Add score
            score += char_score;
        }
    }

    private void FinishGame()
    {
        Debug.Log(gameObject.name + ":FinishGame()");

        CurrentScene = Scene.Finished;

        // Finish SE
        Util.sound.PlaySE(2);

        // Stop BGM
        Util.sound.BGMFadeout();

        // Show result
        ResultText.text = "Your Score!\n" + score + " points";
        Util.media.FadeInUI(ResultText.gameObject, 1f, 0);

        // Stop characters
        foreach (var _CharController in CharControllers)
        {
            _CharController.StopMoving();
            _CharController.HideCharacter();
        }

        // Delay to idling
        StartCoroutine(DelayToIdling());
    }

    IEnumerator DelayToIdling()
    {
        yield return new WaitForSeconds(5f);

        // Hide result
        Util.media.FadeOutUI(ResultText.gameObject, 1f, 0);

        yield return new WaitForSeconds(1f);

        StartIdling();
    }
}
