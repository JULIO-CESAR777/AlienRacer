/* Instead of calling the function like that (unsafe, can throw NullReferenceException silently)
    GameManager.Instance.DeathEvent();

    To call the function in general safely, use the null-conditional operator like this:
    GameManager.GetInstance()?.DeathEvent();
    The ?. (null-conditional) means if GetInstance() returns null, it won't crash —
    it'll just skip the call, and you'll see the LogError in the console telling you why*/

using System;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static GameManager GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogError("GameManager instance is null! Make sure it exists in the scene.");
        }
        return Instance;
    }


    public void DeathEvent()
    {
        Debug.Log("Player has died!");
    }

    public void WinEvent()
    {
        Debug.Log("Player has won!");
    }
    public  void LoseEvent()
    {
        Debug.Log("Player has lost!");
    }
}