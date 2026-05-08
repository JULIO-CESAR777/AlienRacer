using UnityEngine;

[RequireComponent(typeof(KartController))]
public class AudioMotorKart : MonoBehaviour
{
    public AudioSource motorAudioSource;
    public Rigidbody kartRb;

    [Header("Configuración de Marchas")]
    public int numeroMarchas = 6;
    public float tiempoPorMarcha = 0.6f;
    public float caidaPitchCambio = 0.7f;

    [Header("Rangos de Tono")]
    public float pitchMinimo = 0.8f;
    public float pitchMaximo = 2.4f;
    public float suavizadoPitch = 15f;

    private int marchaActual = 0;
    private float temporizadorMarcha = 0f;
    private float pitchObjetivo;
    private bool estaAcelerando;

    private MainManager manager;
    private KartController kartController;

    void Start()
    {
        manager = MainManager.GetInstance();
        kartController = GetComponent<KartController>();
    }

    void Update()
    {
        if (motorAudioSource == null || kartRb == null || manager == null || kartController == null) return;

        if (manager.gameState != GameState.Play)
        {
            if (motorAudioSource.isPlaying)
            {
                motorAudioSource.Pause();
            }
            return;
        }
        else
        {
            if (!motorAudioSource.isPlaying)
            {
                motorAudioSource.UnPause();
            }
        }

        float velocidadActual = kartRb.linearVelocity.magnitude;

        estaAcelerando = kartController.IsAccelerating;

        float marchaSegunVelocidad = (velocidadActual / 22f) * numeroMarchas;

        if (estaAcelerando)
        {
            if (marchaActual < numeroMarchas)
            {
                temporizadorMarcha += Time.deltaTime;

                if (temporizadorMarcha >= tiempoPorMarcha)
                {
                    if (marchaActual < numeroMarchas - 1)
                    {
                        marchaActual++;
                        temporizadorMarcha = 0f;
                        motorAudioSource.pitch *= 0.85f;
                    }
                }
            }
        }
        else
        {
            temporizadorMarcha = Mathf.Lerp(temporizadorMarcha, 0f, Time.deltaTime * 5f);

            if (marchaActual > (int)marchaSegunVelocidad)
            {
                marchaActual = (int)marchaSegunVelocidad;
            }
        }

        CalcularPitch();
    }

    void CalcularPitch()
    {
        float progreso = temporizadorMarcha / tiempoPorMarcha;

        float basePitch = pitchMinimo + (marchaActual * 0.1f);
        float factorSubida = (marchaActual == numeroMarchas - 1) ? caidaPitchCambio * 0.4f : caidaPitchCambio;

        pitchObjetivo = basePitch + (progreso * factorSubida);
        pitchObjetivo = Mathf.Min(pitchObjetivo, pitchMaximo);

        motorAudioSource.pitch = Mathf.Lerp(motorAudioSource.pitch, pitchObjetivo, Time.deltaTime * suavizadoPitch);
    }
}