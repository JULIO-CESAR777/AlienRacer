using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Principal")]
    public AudioMixer mainMixer;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource uiSource;

    [Header("Música Dinámica (Tareas 4 y 5)")]
    [Tooltip("Música para la pantalla de inicio (Escena 0)")]
    public AudioClip bgmMenu;
    [Tooltip("Música con temática mexicana para las pistas de carreras")]
    public AudioClip bgmGameplayMexicano;

    [Header("UI Sounds")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    public AudioClip pauseMenuSound;

    private const string MUSIC_PREF_KEY = "MusicVolume";
    private const string SFX_PREF_KEY = "SFXVolume";

    private bool juegoEstabaPausado = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadVolumePreferences();

        if (uiSource != null) uiSource.ignoreListenerPause = true;
        if (bgmSource != null) bgmSource.ignoreListenerPause = true;
    }

    private void Update()
    {
        bool juegoEstaPausado = (Time.timeScale == 0f);

        if (juegoEstaPausado && !juegoEstabaPausado)
        {
            PauseBGM();
            juegoEstabaPausado = true;
        }
        else if (!juegoEstaPausado && juegoEstabaPausado)
        {
            ResumeBGM();
            juegoEstabaPausado = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            CambiarMusica(bgmMenu);
        }
        else
        {
            CambiarMusica(bgmGameplayMexicano);
        }
    }

    private void CambiarMusica(AudioClip nuevaMusica)
    {
        if (bgmSource == null || nuevaMusica == null) return;

        if (bgmSource.clip == nuevaMusica && bgmSource.isPlaying) return;

        bgmSource.Stop();
        bgmSource.clip = nuevaMusica;
        bgmSource.loop = true;
        bgmSource.Play();
    }

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

        if (mainMixer.GetFloat("UIVol", out _))
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
        if (bgmSource != null)
        {
            bgmSource.volume = 0.3f; 
        }
        AudioListener.pause = true; 
    }

    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = 1f;
        }
        AudioListener.pause = false; 
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}