using UnityEngine;
using System;

public class DisplaySettings : MonoBehaviour
{
    public static DisplaySettings instance;

    public Action<int> OnScreenModeChanged;

    [Header("Resolution Settings")]
    [SerializeField] private int targetWidth = 1920;
    [SerializeField] private int targetHeight = 1080;
    [SerializeField] private int refreshRate = 60;

    private int currentMode; // 0 = Fullscreen, 1 = Windowed

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadCurrentSetting();
    }

    private void LoadCurrentSetting()
    {
        if (PlayerPrefs.HasKey("ScreenMode"))
        {
            currentMode = PlayerPrefs.GetInt("ScreenMode");
        }
        else
        {
            currentMode = 0;
        }

        ApplyMode();
    }

    public void ChangeMode(bool right)
    {
        if (right)
            currentMode++;
        else
            currentMode--;

        if (currentMode > 1) currentMode = 0;
        if (currentMode < 0) currentMode = 1;

        PlayerPrefs.SetInt("ScreenMode", currentMode);
        PlayerPrefs.Save();

        ApplyMode();
    }

    private void ApplyMode()
    {
        switch (currentMode)
        {
            case 0:
                Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.FullScreenWindow, refreshRate);
                break;

            case 1:
                Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed, refreshRate);
                break;
        }

        OnScreenModeChanged?.Invoke(currentMode);
    }

    public int GetCurrentMode()
    {
        return currentMode;
    }
}