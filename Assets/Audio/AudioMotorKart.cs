using UnityEngine;

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
    public float pitchMaximo = 2.4f; // Reducido un poco para evitar el exceso
    public float suavizadoPitch = 15f;

    private int marchaActual = 0;
    private float temporizadorMarcha = 0f;
    private float pitchObjetivo;
    private bool estaAcelerando;

    void Update()
    {
        if (motorAudioSource == null || kartRb == null) return;

        float velocidadActual = kartRb.linearVelocity.magnitude;
        estaAcelerando = Input.GetAxis("Vertical") > 0.1f;

        // CORRECCIÓN: Sincronizar marcha con velocidad cuando se frena
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
                        motorAudioSource.pitch *= 0.85f; // Golpe de embrague
                    }
                }
            }
        }
        else
        {
            // Regreso rápido de marchas basado en la velocidad real
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

        // Base de pitch que sube por marcha
        float basePitch = pitchMinimo + (marchaActual * 0.1f);

        // CORRECCIÓN: Si es la última marcha, el pitch sube más lento (techo)
        float factorSubida = (marchaActual == numeroMarchas - 1) ? caidaPitchCambio * 0.4f : caidaPitchCambio;

        pitchObjetivo = basePitch + (progreso * factorSubida);

        // Cap absoluto para evitar el sonido "roto"
        pitchObjetivo = Mathf.Min(pitchObjetivo, pitchMaximo);

        motorAudioSource.pitch = Mathf.Lerp(motorAudioSource.pitch, pitchObjetivo, Time.deltaTime * suavizadoPitch);
    }
}