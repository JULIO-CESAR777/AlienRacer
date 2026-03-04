using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _loadingScreenPanel;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TMP_Text _progressText;

    private void OnEnable()
    {
        SceneLoader.OnLoadProgress += HandleProgress;
        SceneLoader.OnLoadComplete += HandleLoadComplete;
    }

    private void OnDisable()
    {
        SceneLoader.OnLoadProgress -= HandleProgress;
        SceneLoader.OnLoadComplete -= HandleLoadComplete;
    }
    
    private void HandleProgress(float progress)
    {
        _loadingScreenPanel.SetActive(true);
        _progressBar.value = progress;
        _progressText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
    }
    
    private void HandleLoadComplete()
    {
        _loadingScreenPanel.SetActive(false);
    }
}
