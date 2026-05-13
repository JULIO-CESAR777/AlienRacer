using TMPro;
using UnityEngine;

public class MultiLanguageText : MonoBehaviour
{
    TextMeshProUGUI uiText;
    [TextArea]
    [SerializeField] public string[] texts;

    bool setUp = false;

    private void Start()
    {
        uiText = GetComponent<TextMeshProUGUI>();
        uiText.text = texts[LanguageManager.GetInstance().GetCurrentLanguageByte()];
        LanguageManager.GetInstance().OnLanguageChanged += OnLanguageChanged;
        setUp = true;
    }

    private void OnEnable()
    {
        if (setUp == false) return;

        // También es buena práctica validar aquí por si acaso
        if (LanguageManager.GetInstance() != null)
        {
            uiText.text = texts[LanguageManager.GetInstance().GetCurrentLanguageByte()];
            LanguageManager.GetInstance().OnLanguageChanged += OnLanguageChanged;
        }
    }

    void OnLanguageChanged(LANGUAGES newLanguage)
    {
        uiText.text = texts[(byte)newLanguage];
    }

    private void OnDisable()
    {
        if (LanguageManager.GetInstance() != null)
        {
            LanguageManager.GetInstance().OnLanguageChanged -= OnLanguageChanged;
        }
    }
}