using System;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    
    public static LanguageManager instance;
    public static LanguageManager GetInstance() => instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] public LANGUAGES currentLanguage;
    public Action<LANGUAGES> OnLanguageChanged;
    
    
    public LANGUAGES CurrentLanguage => currentLanguage;

    public byte GetCurrentLanguageByte()
    {
        return (byte)currentLanguage;
    }

    public void ChangeLanguage(LANGUAGES newLanguage)
    {
        currentLanguage = newLanguage;
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
    
}


public enum LANGUAGES
{
    SPANISH,
    ENGLISH,
    END
}
