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

    public void UpdateCoinText(string coins)
    {
        coinText.text = "Coins: " + coins;
    }
    
    
}
