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
    }


    public void SetMusicVolume(float sliderValue)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20f);
    }

    public void SetSFXVolume(float sliderValue)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(sliderValue) * 20f);
    }


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
}