using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _loadingScreenPanel;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TMP_Text _progressText;
    [SerializeField] private float _smoothSpeed = 0.8f;
    
    private float _targetProgress = 0f;
    private float _displayedProgress = 0f;
    private bool _isLoading = false;
        
    private void Start()
    {
        _loadingScreenPanel.SetActive(false);
    }
    
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

    private void Update()
    {
        if (!_isLoading) return;
        _displayedProgress = Mathf.MoveTowards(_displayedProgress,
            _targetProgress, _smoothSpeed * Time.unscaledDeltaTime);
        
        _progressBar.value = _displayedProgress;
        _progressText.text = $"Loading... {Mathf.RoundToInt(_displayedProgress * 100)}%";
    }

    private void HandleProgress(float progress)
    {
        if (!_isLoading)
        {
            _isLoading = true;
            _displayedProgress = 0f;
            _loadingScreenPanel.SetActive(true);
        }
        _targetProgress = progress;
    }
    
    private void HandleLoadComplete()
    {
        _isLoading = false;
        _targetProgress = 1f;
        _loadingScreenPanel.SetActive(false);
    }
}
