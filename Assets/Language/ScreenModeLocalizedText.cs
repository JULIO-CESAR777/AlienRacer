using TMPro;
using UnityEngine;

public class ScreenModeLocalizedText : MonoBehaviour
{
    private TextMeshProUGUI textUI;
    private DisplaySettings displaySettings;
    private LanguageManager languageManager;

    [Header("Texts by Language [ES, EN]")]
    [Tooltip("Textos para el modo FullScreen. El índice debe coincidir con el enum LANGUAGES.")]
    [TextArea]
    [SerializeField] private string[] fullScreenTexts;

    [Tooltip("Textos para el modo Windowed. El índice debe coincidir con el enum LANGUAGES.")]
    [TextArea]
    [SerializeField] private string[] windowedTexts;

    private void Start()
    {
        
        displaySettings = DisplaySettings.GetInstance();
        languageManager = LanguageManager.GetInstance();
        
        if (textUI == null)
            textUI = GetComponent<TextMeshProUGUI>();

        UpdateText();

        if (displaySettings != null)
            displaySettings.OnScreenModeChanged += OnScreenModeChanged;

        if (languageManager != null)
            languageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnScreenModeChanged(int mode)
    {
        UpdateText();
    }

    private void OnLanguageChanged(LANGUAGES lang)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (textUI == null || displaySettings == null || languageManager == null)
            return;

        int mode = displaySettings.GetCurrentMode();
        int langIndex = (int)languageManager.CurrentLanguage;

        string[] sourceArray = mode == 0 ? fullScreenTexts : windowedTexts;

        if (sourceArray == null || sourceArray.Length == 0)
        {
            textUI.text = "";
            return;
        }

        if (langIndex < 0 || langIndex >= sourceArray.Length)
        {
            textUI.text = sourceArray[0];
            return;
        }

        textUI.text = sourceArray[langIndex];
    }

    private void OnDestroy()
    {
        if (displaySettings != null)
            displaySettings.OnScreenModeChanged -= OnScreenModeChanged;

        if (languageManager != null)
            languageManager.OnLanguageChanged -= OnLanguageChanged;
    }
}