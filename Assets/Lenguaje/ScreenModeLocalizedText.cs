using TMPro;
using UnityEngine;

public class ScreenModeLocalizedText : MonoBehaviour
{
    private TextMeshProUGUI text;

    [Header("Texts [ES, EN]")]
    public string[] fullscreenTexts;
    public string[] windowedTexts;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        if (DisplaySettings.instance != null)
        {
            UpdateText(DisplaySettings.instance.GetCurrentMode());
            DisplaySettings.instance.OnScreenModeChanged += UpdateText;
        }

        if (LanguageManager.instance != null)
        {
            LanguageManager.instance.OnLanguageChanged += OnLanguageChanged;
        }
    }
    

    void UpdateText(int mode)
    {
        if (LanguageManager.instance == null) return;

        int lang = LanguageManager.instance.GetCurrentLanguageByte();

        if (mode == 0)
            text.text = fullscreenTexts[lang];
        else
            text.text = windowedTexts[lang];
    }

    void OnLanguageChanged(LANGUAGES lang)
    {
        if (DisplaySettings.instance == null) return;

        UpdateText(DisplaySettings.instance.GetCurrentMode());
    }

    private void OnDisable()
    {
        if (DisplaySettings.instance != null)
            DisplaySettings.instance.OnScreenModeChanged -= UpdateText;

        if (LanguageManager.instance != null)
            LanguageManager.instance.OnLanguageChanged -= OnLanguageChanged;
    }
}