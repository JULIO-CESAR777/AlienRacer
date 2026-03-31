using System;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    
    public static LanguageManager instance;
    public static LanguageManager GetInstance() => instance;

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
        LoadLanguage();
    }

    [SerializeField] public LANGUAGES currentLanguage;
    public Action<LANGUAGES> OnLanguageChanged;
    
    private const string LANGUAGE_PREF_KEY = "Language";
    
    public LANGUAGES CurrentLanguage => currentLanguage;

    public byte GetCurrentLanguageByte()
    {
        return (byte)currentLanguage;
    }

    public void ChangeLanguage(LANGUAGES newLanguage)
    {
        currentLanguage = newLanguage;
        SaveLanguage();
        OnLanguageChanged?.Invoke(currentLanguage);
    }

    public void ChangeLanguage(bool right)
    {
        if (right)
        {
            if (currentLanguage < LANGUAGES.END-1)
            {
                currentLanguage++;
            }
            else
            {
                currentLanguage = 0;
            }
        }else
        {
            if (currentLanguage <= 0)
            {
                currentLanguage = LANGUAGES.END - 1;
            }
            else
            {
                currentLanguage--;
            }
        }
        ChangeLanguage(currentLanguage);
    }
    
    private void SaveLanguage()
    {
        PlayerPrefs.SetInt(LANGUAGE_PREF_KEY, (int)currentLanguage);
        PlayerPrefs.Save();
    }
    
    private void LoadLanguage()
    {
        if (!PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
            return;

        int savedLanguage = PlayerPrefs.GetInt(LANGUAGE_PREF_KEY);

        if (savedLanguage >= 0 && savedLanguage < (int)LANGUAGES.END)
        {
            currentLanguage = (LANGUAGES)savedLanguage;
        }
    }
    
}

public enum LANGUAGES
{
    SPANISH,
    ENGLISH,
    END
}
