using UnityEngine;

public class PowerUpAudioController : MonoBehaviour
{
    [Header("Reproductor")]
    [Tooltip("El Audio Source de tu Kart dedicado a los SFX")]
    public AudioSource sfxSource;

    [Header("Efectos de Sonido")]
    public AudioClip clipBoost;
    public AudioClip clipEstrella;
    public AudioClip clipEscudo;

    public void PlayBoost()
    {
        if (sfxSource != null && clipBoost != null)
            sfxSource.PlayOneShot(clipBoost);
    }

    public void PlayEstrella()
    {
        if (sfxSource != null && clipEstrella != null)
            sfxSource.PlayOneShot(clipEstrella);
    }

    public void PlayEscudo()
    {
        if (sfxSource != null && clipEscudo != null)
            sfxSource.PlayOneShot(clipEscudo);
    }
}