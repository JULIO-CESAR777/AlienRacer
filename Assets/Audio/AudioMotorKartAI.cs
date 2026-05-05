using UnityEngine;

public class AudioMotorKartAI : MonoBehaviour
{
    public AudioSource motorAudioSource;
    public Rigidbody kartRb;

    // Puedes vincular esto a la lógica de navegación de tu IA 
    // (ej: si está yendo a un waypoint, estaAcelerandoAI = true)
    public bool estaAcelerandoAI = false;

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

    // --- NUEVO: Referencia al cerebro de tu juego ---
    private MainManager manager;

    void Start()
    {
        // Conectamos con tu mánager al iniciar
        manager = MainManager.GetInstance();
    }

    void Update()
    {
        if (motorAudioSource == null || kartRb == null || manager == null) return;

        // --- LÓGICA DE PAUSA BASADA EN TU MAIN MANAGER ---
        // Si el estado NO es Play (ej. Pausa, Menú, Fin de carrera)
        if (manager.gameState != GameState.Play)
        {
            if (motorAudioSource.isPlaying)
            {
                motorAudioSource.Pause();
            }
            return; // Cortamos el código aquí para no seguir calculando marchas
        }
        else
        {
            // Si volvemos a Play y estaba pausado, reanudamos
            if (!motorAudioSource.isPlaying)
            {
                motorAudioSource.UnPause();
            }
        }
        // -------------------------------------------------

        float velocidadActual = kartRb.linearVelocity.magnitude;

        // Calculamos la marcha según la velocidad real como referencia para el regreso
        float marchaSegunVelocidad = (velocidadActual / 22f) * numeroMarchas;

        if (estaAcelerandoAI)
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