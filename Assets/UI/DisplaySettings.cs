using System;
using UnityEngine;

public class DisplaySettings : MonoBehaviour
{
    public static DisplaySettings instance;
    public static DisplaySettings GetInstance() => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
    }

    [Header("Resolution Settings")]
    [SerializeField] private int targetWidth = 1920;
    [SerializeField] private int targetHeight = 1080;
    [SerializeField] private int refreshRate = 60;

    public Action<int> OnScreenModeChanged;

    private int currentMode; // 0 = FullScreen, 1 = Window

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

        ApplyCurrentMode(false);
    }

    public void ChangeMode(bool right)
    {
        if (right)
        {
            currentMode++;
        }
        else
        {
            currentMode--;
        }

        if (currentMode > 1)
            currentMode = 0;
        else if (currentMode < 0)
            currentMode = 1;

        ApplyCurrentMode(true);
    }

    public void SetMode(int mode)
    {
        currentMode = Mathf.Clamp(mode, 0, 1);
        ApplyCurrentMode(true);
    }

    private void ApplyCurrentMode(bool save)
    {
        switch (currentMode)
        {
            case 0:
                SetFullScreen();
                break;

            case 1:
                SetWindowed();
                break;
        }

        if (save)
        {
            PlayerPrefs.SetInt("ScreenMode", currentMode);
            PlayerPrefs.Save();
        }

        OnScreenModeChanged?.Invoke(currentMode);
    }

    public void SetFullScreen()
    {
        Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.FullScreenWindow, refreshRate);
    }

    public void SetWindowed()
    {
        Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed, refreshRate);
    }

    public int GetCurrentMode()
    {
        return currentMode;
    }

    public string GetCurrentModeText()
    {
        return currentMode == 0 ? "FullScreen" : "Window";
    }
}