using System;
using System.Collections;
using UnityEngine;

//This is a persistent object it does not gets
//destroyed when loading a new scene, 
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    
    // Events for the UILoadingScreen to Subscribe to
    public static event Action<float> OnLoadProgress;
    public static event Action OnLoadComplete;

    private bool _isLoading = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName, float minLoadTime = 1.5f)
    {
        if (_isLoading) return;
        {
            
        }
    }

    /*private IEnumerator LoadSceneRoutine(object sceneRef, float minLoadTime)
    {
        _isLoading = true;
        // Start the async operation but does not activate the secene yet
    }*/
}
