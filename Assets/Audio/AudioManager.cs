using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    [Header("Conexiones")]
    public AudioMixer mainMixer; 
    public AudioSource bgmSource; 

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CambiarMusica(AudioClip nuevaCancion)
    {
        if (bgmSource.clip == nuevaCancion) return; 

        bgmSource.Stop();
        bgmSource.clip = nuevaCancion;
        bgmSource.Play();
    }

    public void CambiarVolumenMusica(float nivelVolumen)
    {
        mainMixer.SetFloat("VolumenBGM", Mathf.Log10(nivelVolumen) * 20f);
    }
}