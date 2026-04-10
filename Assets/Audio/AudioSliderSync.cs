using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AudioSliderSync : MonoBehaviour
{
    public enum AudioType { Music, SFX }
    public AudioType targetAudio;

    private Slider mySlider;
    private bool isInitializing = true;

    void OnEnable()
    {
        mySlider = GetComponent<Slider>();
        isInitializing = true;

        if (AudioManager.Instance != null)
        {
            if (targetAudio == AudioType.Music)
            {
                mySlider.value = AudioManager.Instance.GetMusicVolume();
            }
            else if (targetAudio == AudioType.SFX)
            {
                mySlider.value = AudioManager.Instance.GetSFXVolume();
            }
        }

        isInitializing = false;
    }

    void Start()
    {
        mySlider.onValueChanged.AddListener(HandleSliderChange);
    }

    void HandleSliderChange(float value)
    {
        if (isInitializing || AudioManager.Instance == null) return;

        if (targetAudio == AudioType.Music)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        else if (targetAudio == AudioType.SFX)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }
}