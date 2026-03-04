using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

//This is a persistent object it does not gets
//destroyed when loading a new scene, 
[DisallowMultipleComponent]
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
            StartCoroutine(LoadSceneRoutine(sceneName, minLoadTime));
        }
    }
    
    public void LoadScene(int buildIndex, float minLoadTime = 1.5f)
    {
        if (_isLoading) return;
        {
            StartCoroutine(LoadSceneRoutine(buildIndex, minLoadTime));
        }
    }

    private IEnumerator LoadSceneRoutine(object sceneRef, float minLoadTime)
    {
        _isLoading = true;
        // Start the async operation but does not activate the secene yet
        
        AsyncOperation asyncOperation = sceneRef is string name
            ? SceneManager.LoadSceneAsync(name)
            : SceneManager.LoadSceneAsync((int)sceneRef);
        asyncOperation.allowSceneActivation = false;
        
        float elapsed = 0f;
        //
        while (!asyncOperation.isDone)
        {
            elapsed += Time.unscaledDeltaTime;
            
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            OnLoadProgress?.Invoke(progress);
            
            bool assetsDone = asyncOperation.progress >= 0.9f;
            bool timeDone = elapsed >= minLoadTime;

            if (assetsDone && timeDone)
            {
                OnLoadProgress?.Invoke(1f);
                yield return null;
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
        OnLoadComplete?.Invoke();
        _isLoading = false;
    }
}
