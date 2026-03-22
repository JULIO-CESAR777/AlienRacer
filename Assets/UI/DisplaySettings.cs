using System;
using UnityEngine;
using TMPro;

public class DisplaySettings : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    
    [Header("Resolution Settings")]
    [SerializeField] private int targetWidth = 1920;
    [SerializeField] private int targetHeight = 1080;
    [SerializeField] private int refreshRate = 60;

    private void Start()
    {
        InitializeDropdown();
        LoadCurrentSetting();
    }

    private void InitializeDropdown()
    {
        screenModeDropdown.ClearOptions();
        
        var options = new System.Collections.Generic.List<string>
        {
            "FullScreen",
            "Window"
        };
        
        screenModeDropdown.AddOptions(options);
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
    }

    private void LoadCurrentSetting()
    {
        // Check if we have a saved preference
        if (PlayerPrefs.HasKey("ScreenMode"))
        {
            // Use saved preference
            int savedMode = PlayerPrefs.GetInt("ScreenMode");
            screenModeDropdown.value = savedMode;
            screenModeDropdown.RefreshShownValue();
            OnScreenModeChanged(savedMode);
        }
        else
        {
            // No saved preference - default to FullScreen (0)
            screenModeDropdown.value = 0;
            screenModeDropdown.RefreshShownValue();
            SetFullScreen(); // Apply fullscreen without triggering save yet
        }
    }

    private void OnScreenModeChanged(int index)
    {
        PlayerPrefs.SetInt("ScreenMode", index);
        PlayerPrefs.Save();
        
        switch (index)
        {
            case 0:
                SetFullScreen();
                break;
            case 1:
                SetWindowed();
                break;
        }
    }

    public void SetFullScreen()
    {
        Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.FullScreenWindow, refreshRate);
        Debug.Log("Switched to FullScreen");
    }

    public void SetWindowed()
    {
        Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed, refreshRate);
        Debug.Log("Switched to Windowed");
    }

    private void OnDestroy()
    {
        if (screenModeDropdown != null)
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);
    }
}