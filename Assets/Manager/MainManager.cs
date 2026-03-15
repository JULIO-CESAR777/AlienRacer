using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    // ----- SingleTon ---------
    #region Singleton
    public static MainManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public static MainManager GetInstance() => instance;

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion
    // ------ Fin del singleton  ---------

    public GameState gameState;
    public Action<GameState> onChangeGameState;
    
    public bool canPause;

    // Begin Race
    [SerializeField] public TextMeshProUGUI countDownText; 
    public bool countDownActive;
    
    
    private void Start()
    {
        gameState = GameState.Play;
        canPause = true;
        
        BeginRace();
    }

    public void PauseGame()
    {
        if (canPause)
        {
            if (gameState == GameState.Pause)
            {
                ChangeGameState(GameState.Play);
            }
            else if (gameState == GameState.Play)
            {
                ChangeGameState(GameState.Pause);
            }
        }
    }

    public void ChangeGameState(GameState newGameState)
    {
        if (!canPause) return;
        
        gameState = newGameState;
        onChangeGameState?.Invoke(gameState);
    }
    
    public void BeginRace()
    {
        StartCoroutine(RaceInit());
    }

    IEnumerator RaceInit()
    {
        countDownActive = true;
        ChangeGameState(GameState.Pause);
        
        for (int i = 3; i > 0; i--)
        {
            countDownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        countDownText.text = "GO!";

        yield return new WaitForSecondsRealtime(1f);

        countDownText.gameObject.SetActive(false);
        ChangeGameState(GameState.Play);
        countDownActive = false;
        
    }
}


public enum GameState
{
    Play,
    Pause,
    GameOver
}