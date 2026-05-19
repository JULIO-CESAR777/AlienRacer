using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AudioSliderSync : MonoBehaviour
{
    public enum AudioType { Music, SFX }
    public AudioType targetAudio;

    [Header("Consistencia de Input")]
    [Tooltip("El tamaño del 'salto' para que el mouse y el control se sientan igual. 0.1f = 10%")]
    public float stepSize = 0.1f;

    private Slider mySlider;
    private bool isInitializing = true;

    void Awake()
    {
        mySlider = GetComponent<Slider>();
    }

    void Start()
    {
        isInitializing = true;

        mySlider.onValueChanged.RemoveAllListeners();

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
        else
        {
            Debug.LogWarning("AudioManager no encontrado al intentar sincronizar el Slider: " + gameObject.name);
        }

        mySlider.onValueChanged.AddListener(HandleSliderChange);

        isInitializing = false;
    }

    void HandleSliderChange(float value)
    {
        if (isInitializing || AudioManager.Instance == null) return;

        float snappedValue = Mathf.Round(value / stepSize) * stepSize;

        if (Mathf.Abs(mySlider.value - snappedValue) > 0.001f)
        {
            mySlider.value = snappedValue;
            return; 
        }

        if (targetAudio == AudioType.Music)
        {
            AudioManager.Instance.SetMusicVolume(snappedValue);
        }
        else if (targetAudio == AudioType.SFX)
        {
            AudioManager.Instance.SetSFXVolume(snappedValue);
        }
    }
}