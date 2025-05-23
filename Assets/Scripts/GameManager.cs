﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using unityroom.Api;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("カウントダウン設定")]
    [SerializeField] private float countdownTime = 3f;
    [SerializeField] private CountdownUI countdownUI;

    private float survivalTime = 0f;
    private bool isGameOver = false;
    private bool hasStarted = false;

    public float SurvivalTime => survivalTime;
    public float FinalSurvivalTime { get; private set; }
    public bool IsPaused { get; private set; }

    public bool IsGameStarted => hasStarted;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    IEnumerator Start()
    {
        AudioManager.Instance?.PlayBGM();
        yield return CountdownStart();
        hasStarted = true;
    }

    void Update()
    {
        if (!isGameOver && hasStarted)
        {
            survivalTime += Time.deltaTime;
        }
    }



public void GameOver()
{
    isGameOver = true;
    FinalSurvivalTime = survivalTime;

    // ✅ ランキング送信
    UnityroomApiClient.Instance.SendScore(
        1,
        FinalSurvivalTime,
        ScoreboardWriteMode.HighScoreDesc
    );

    SceneManager.LoadScene("GameOver");
}





private IEnumerator CountdownStart()
    {
        for (int i = (int)countdownTime; i >= 1; i--)
        {
            countdownUI?.ShowCountdown(i);
            yield return new WaitForSeconds(1f);
        }

        countdownUI?.ShowStart();
        yield return new WaitForSeconds(1f);

        hasStarted = true;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGame")
        {
            countdownUI = FindAnyObjectByType<CountdownUI>();
            ResetGameState();
            StartCoroutine(CountdownStart());
        }
        else if (scene.name == "Title")
        {
            ResetGameState();
        }
    }




    public void SetPaused(bool pause)
    {
        IsPaused = pause;
        Time.timeScale = pause ? 0f : 1f;
        Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pause;
    }

    public void ResetGameState()
    {
        survivalTime = 0f;
        FinalSurvivalTime = 0f;
        hasStarted = false;
        isGameOver = false;
    }
}
