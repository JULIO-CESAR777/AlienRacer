using System;
using TMPro;
using UnityEngine;

public class UiManagerPlayer : MonoBehaviour
{
    
    #region singleton

    public static UiManagerPlayer instance { get; private set; }

    public static UiManagerPlayer GetInstance() => instance;

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
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    
    #endregion
    
    [Header("Coins")]
    [SerializeField] public TextMeshProUGUI coinText;

    [Header("Pause")]
    [SerializeField] public GameObject pausePanel;
    [SerializeField] public GameObject settingsPanel;


    private void Start()
    {
        pausePanel.SetActive(false);
    }

    public void PauseGame()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void GoToSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
    
    public void UpdateCoinText(string coins)
    {
        coinText.text = coins;
    }
    
    
}
