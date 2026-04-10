using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Principal")]
    public AudioMixer mainMixer;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource uiSource;

    [Header("Background Music")]
    public AudioClip bgmClip;

    [Header("UI Sounds")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    public AudioClip pauseMenuSound;

    private const string MUSIC_PREF_KEY = "MusicVolume";
    private const string SFX_PREF_KEY = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        PlayBGM();
        LoadVolumePreferences();
    }

    // --- MÉTODOS DE VOLUMEN (Globales) ---

    public void SetMusicVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat(MUSIC_PREF_KEY, sliderValue);
        mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat(SFX_PREF_KEY, sliderValue);

        float dbVolume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;

        mainMixer.SetFloat("SFXVol", dbVolume);
        mainMixer.SetFloat("UIVol", dbVolume);

        PlayerPrefs.Save();
    }

    private void LoadVolumePreferences()
    {
        float savedMusicVol = PlayerPrefs.GetFloat(MUSIC_PREF_KEY, 1f);
        SetMusicVolume(savedMusicVol);

        float savedSFXVol = PlayerPrefs.GetFloat(SFX_PREF_KEY, 1f);
        SetSFXVolume(savedSFXVol);
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_PREF_KEY, 1f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_PREF_KEY, 1f);
    }

    // --- MÉTODOS PARA LA MÚSICA Y UI ---

    public void PlayBGM()
    {
        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayUIClick()
    {
        if (buttonClickSound != null && uiSource != null)
            uiSource.PlayOneShot(buttonClickSound);
    }

    public void PlayUIHover()
    {
        if (buttonHoverSound != null && uiSource != null)
            uiSource.PlayOneShot(buttonHoverSound);
    }

    public void PlayPauseSound()
    {
        if (pauseMenuSound != null && uiSource != null)
            uiSource.PlayOneShot(pauseMenuSound);
    }

    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }
}