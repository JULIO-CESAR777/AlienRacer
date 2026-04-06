using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class EngineAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private Rigidbody rb;

    [Header("Configuración de Sonido")]
    public float pitchMinimo = 0.8f; 
    public float pitchMaximo = 2.5f; 
    public float velocidadParaPitchMaximo = 20f; 

    [Header("Suavizado")]
    public float velocidadTransicion = 3f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        audioSource.pitch = pitchMinimo;
    }

    void Update()
    {
        float porcentajeVelocidad = rb.linearVelocity.magnitude / velocidadParaPitchMaximo;

        porcentajeVelocidad = Mathf.Clamp01(porcentajeVelocidad);

        float pitchObjetivo = Mathf.Lerp(pitchMinimo, pitchMaximo, porcentajeVelocidad);

        audioSource.pitch = Mathf.Lerp(audioSource.pitch, pitchObjetivo, Time.deltaTime * velocidadTransicion);
    }
}