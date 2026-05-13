using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class KartObstaculosIA : MonoBehaviour
{
    [Header("Ruta y Navegación")]
    public Transform[] waypoints;
    public float distanciaCambio = 6f;
    public float anticipacionCurva = 15f; // Aumentado para tomar curvas antes

    [Header("Motor y Físicas")]
    public float velocidadMaxima = 16f;
    public float velocidadMinima = 7f;
    public float aceleracion = 8f;
    public float velocidadGiro = 8.5f; // Aumentado para mayor respuesta en curvas
    public float factorDerrape = 5f; // Reducido para mayor agarre
    public float fuerzaGravedadExtra = 20f;

    [Header("Competitividad (Overtake & Boost)")]
    public float boostRecta = 1.2f;
    public float boostRebufo = 1.35f;
    public float distanciaRebufo = 30f; // Aumentado para aprovechar más el rebufo
    public float fuerzaAdelantamiento = 6f; // Aumentado para rebases más agresivos

    [Header("Sensores y Evasión")]
    public float longitudSensor = 8f;
    public float anguloSensores = 25f;
    public float fuerzaEvasionMuro = 4f;
    public LayerMask capaObstaculos;

    [Header("Sistema Anti-Atascos")]
    public float distanciaChoqueFrontal = 1.5f;
    public float tiempoReversa = 1.2f;
    public float velocidadReversa = 6f;

    [Header("Asistente Todoterreno (Bordes)")]
    public float alturaRayoBajo = 0.15f;
    public float distanciaRayoBajo = 0.8f;
    public float fuerzaSaltoBorde = 4f;
    public float inclinacionSubida = -15f;
    private float pitchVisualActual = 0f;

    private int indiceWaypoint = 0;
    private float velocidadActual = 0f;
    private float offsetCompetitivo = 0f;

    private bool enReversa = false;
    private float temporizadorReversa = 0f;
    private float adnVelocidad;
    private float adnAceleracion;
    private float direccionRebase = 1f;

    private float tiempoStun = 0f;
    private float tiempoBoost = 0f;
    private float multiplicadorBoostActual = 1f;
    private float tiempoEscudo = 0f;

    private Rigidbody rb;
    private MainManager manager;

    private bool isFrozen = false;
    private Vector3 savedVelocity;
    private Vector3 savedAngularVelocity;

    [Header("Efectos de Sonido")]
    public AudioSource sfxAudioSource;
    public AudioClip sonidoChoqueMuro;
    public AudioClip sonidoReversa;
    public AudioClip sonidoDerrape;
    public AudioClip sonidoImpactoBala;

    [Header("Efectos Procedurales (Charco de Aceite)")]
    public float intensidadVaiven = 120f;
    public float velocidadVaiven = 15f;
    private float tiempoResbalando = 0f;

    private float tiempoRalentizado = 0f;
    private float multiplicadorRalentizacion = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        // Genética mejorada: el peor bot corre a la velocidad base, el mejor la supera por 10%-12%
        adnVelocidad = Random.Range(1.0f, 1.10f);
        adnAceleracion = Random.Range(1.0f, 1.12f);

        direccionRebase = Random.value > 0.5f ? 1f : -1f;

        manager = MainManager.GetInstance();
    }

    void FixedUpdate()
    {
        if (manager.gameState != GameState.Play)
        {
            if (!isFrozen)
            {
                savedVelocity = rb.linearVelocity;
                savedAngularVelocity = rb.angularVelocity;
                rb.isKinematic = true;
                isFrozen = true;
            }
            return;
        }
        else
        {
            if (isFrozen)
            {
                rb.isKinematic = false;
                rb.linearVelocity = savedVelocity;
                rb.angularVelocity = savedAngularVelocity;
                isFrozen = false;
            }
        }

        if (waypoints.Length == 0) return;

        ActualizarTimersPowerUps();

        if (tiempoStun > 0)
        {
            ProcesarStun();
            return;
        }

        if (tiempoResbalando > 0)
        {
            ProcesarResbalonProcedural();
            return;
        }

        if (enReversa)
        {
            EjecutarReversa();
            return;
        }

        ProcesarConduccion();
    }

    void ActualizarTimersPowerUps()
    {
        if (tiempoStun > 0) tiempoStun -= Time.fixedDeltaTime;
        if (tiempoBoost > 0) tiempoBoost -= Time.fixedDeltaTime;
        if (tiempoEscudo > 0) tiempoEscudo -= Time.fixedDeltaTime;
        if (tiempoResbalando > 0) tiempoResbalando -= Time.fixedDeltaTime;

        if (tiempoRalentizado > 0)
        {
            tiempoRalentizado -= Time.fixedDeltaTime;
            if (tiempoRalentizado <= 0)
            {
                multiplicadorRalentizacion = 1f;
            }
        }
    }

    void ProcesarStun()
    {
        Vector3 velocidadStun = rb.linearVelocity;
        velocidadStun.x = Mathf.Lerp(velocidadStun.x, 0f, Time.fixedDeltaTime * 4f);
        velocidadStun.z = Mathf.Lerp(velocidadStun.z, 0f, Time.fixedDeltaTime * 4f);
        velocidadStun.y -= fuerzaGravedadExtra * 1.5f * Time.fixedDeltaTime;
        rb.linearVelocity = velocidadStun;

        Vector3 normalSuelo = Vector3.up;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hitSuelo, 1.5f, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            normalSuelo = hitSuelo.normal;
        }

        Quaternion rotacionGiro = Quaternion.AngleAxis(1080f * Time.fixedDeltaTime, normalSuelo);
        rb.MoveRotation(rb.rotation * rotacionGiro);
    }

    void ProcesarConduccion()
    {
        Transform objetivoActual = waypoints[indiceWaypoint];
        Vector3 posicionObjetivo = objetivoActual.position;
        posicionObjetivo.y = transform.position.y;

        float distanciaAlPunto = Vector3.Distance(transform.position, posicionObjetivo);
        Vector3 direccionHaciaPunto = (posicionObjetivo - transform.position).normalized;
        float anguloAlWaypoint = Vector3.Angle(transform.forward, direccionHaciaPunto);

        if (distanciaAlPunto <= distanciaCambio || (anguloAlWaypoint > 90f && distanciaAlPunto <= distanciaCambio * 1.5f))
        {
            indiceWaypoint = (indiceWaypoint + 1) % waypoints.Length;
            objetivoActual = waypoints[indiceWaypoint];
            posicionObjetivo = objetivoActual.position;
            posicionObjetivo.y = transform.position.y;
            offsetCompetitivo = 0f;
        }

        Vector3 direccionBase = (posicionObjetivo - transform.position).normalized;

        if (distanciaAlPunto <= anticipacionCurva)
        {
            Transform siguienteObjetivo = waypoints[(indiceWaypoint + 1) % waypoints.Length];
            Vector3 dirSiguiente = (siguienteObjetivo.position - transform.position).normalized;
            dirSiguiente.y = 0;
            direccionBase = Vector3.Lerp(direccionBase, dirSiguiente, 1f - (distanciaAlPunto / anticipacionCurva)).normalized;
        }

        Vector3 origenRayo = transform.position + (Vector3.up * 0.5f);
        Vector3 dirFrente = transform.forward;
        Vector3 dirDer = Quaternion.AngleAxis(anguloSensores, transform.up) * transform.forward;
        Vector3 dirIzq = Quaternion.AngleAxis(-anguloSensores, transform.up) * transform.forward;

        bool frenarPorMuroFrontal = false;
        float multiplicadorGiroEvasion = 0f;
        float multiVelocidadDinamica = 1f;
        bool cazandoRival = false;

        float anguloCurvaReal = Vector3.Angle(transform.forward, direccionBase);

        if (Physics.Raycast(origenRayo, dirFrente, out RaycastHit hitCaza, distanciaRebufo, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (hitCaza.collider.CompareTag("Bot") || hitCaza.collider.CompareTag("Player"))
            {
                cazandoRival = true;
                multiVelocidadDinamica = boostRebufo;
                offsetCompetitivo = Mathf.Lerp(offsetCompetitivo, fuerzaAdelantamiento * direccionRebase, Time.fixedDeltaTime * 3f);
            }
        }

        if (!cazandoRival)
        {
            offsetCompetitivo = Mathf.Lerp(offsetCompetitivo, 0f, Time.fixedDeltaTime * 2f);
        }

        if (Physics.Raycast(origenRayo, dirFrente, out RaycastHit hitFrente, longitudSensor, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            if (!hitFrente.collider.CompareTag("Bot") && !hitFrente.collider.CompareTag("Player"))
            {
                if (hitFrente.distance <= distanciaChoqueFrontal)
                {
                    enReversa = true;
                    temporizadorReversa = tiempoReversa;
                    return;
                }
                frenarPorMuroFrontal = true;
                float intensidad = 1f - (hitFrente.distance / longitudSensor);
                float dirHaciaRuta = Mathf.Sign(Vector3.SignedAngle(transform.forward, direccionBase, Vector3.up));
                multiplicadorGiroEvasion += (fuerzaEvasionMuro * intensidad) * dirHaciaRuta;
            }
        }

        if (Physics.Raycast(origenRayo, dirDer, out RaycastHit hitDer, longitudSensor, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            if (!hitDer.collider.CompareTag("Bot") && !hitDer.collider.CompareTag("Player"))
            {
                float intensidad = 1f - (hitDer.distance / longitudSensor);
                multiplicadorGiroEvasion -= (fuerzaEvasionMuro * intensidad);
                if (cazandoRival && direccionRebase > 0) direccionRebase = -1f;
            }
        }

        if (Physics.Raycast(origenRayo, dirIzq, out RaycastHit hitIzq, longitudSensor, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            if (!hitIzq.collider.CompareTag("Bot") && !hitIzq.collider.CompareTag("Player"))
            {
                float intensidad = 1f - (hitIzq.distance / longitudSensor);
                multiplicadorGiroEvasion += (fuerzaEvasionMuro * intensidad);
                if (cazandoRival && direccionRebase < 0) direccionRebase = 1f;
            }
        }

        Vector3 direccionFinal = direccionBase + (transform.right * offsetCompetitivo) + (transform.right * multiplicadorGiroEvasion);
        direccionFinal.Normalize();

        float velocidadObjetivo = velocidadMaxima * adnVelocidad;

        if (frenarPorMuroFrontal || anguloCurvaReal > 35f)
        {
            velocidadObjetivo = velocidadMinima;
        }
        else if (anguloCurvaReal < 10f && !frenarPorMuroFrontal)
        {
            velocidadObjetivo *= boostRecta;
        }

        velocidadObjetivo *= multiVelocidadDinamica;

        if (tiempoBoost > 0)
        {
            velocidadObjetivo *= multiplicadorBoostActual;
        }

        if (tiempoRalentizado > 0)
        {
            velocidadObjetivo *= multiplicadorRalentizacion;
        }

        float aceleracionFinal = tiempoBoost > 0 ? aceleracion * 1.5f : aceleracion;
        velocidadActual = Mathf.Lerp(velocidadActual, velocidadObjetivo, (aceleracionFinal * adnAceleracion) * Time.fixedDeltaTime);

        Vector3 normalSuelo = Vector3.up;
        bool tocandoSuelo = false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hitSuelo, 1.5f, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            normalSuelo = hitSuelo.normal;
            tocandoSuelo = true;
        }

        bool saltandoBorde = false;
        Vector3 origenRayoBajo = transform.position + (Vector3.up * alturaRayoBajo);

        if (Physics.Raycast(origenRayoBajo, transform.forward, out RaycastHit hitBorde, distanciaRayoBajo, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            if (!frenarPorMuroFrontal && !hitBorde.collider.CompareTag("Bot") && !hitBorde.collider.CompareTag("Player"))
            {
                saltandoBorde = true;
                rb.MovePosition(rb.position + (Vector3.up * 0.08f));
            }
        }

        Vector3 direccionMovimientoReal = Vector3.ProjectOnPlane(transform.forward, normalSuelo).normalized;
        Vector3 impulsoDeseado = direccionMovimientoReal * velocidadActual;

        if (tocandoSuelo)
        {
            impulsoDeseado -= normalSuelo * fuerzaGravedadExtra;
        }
        else
        {
            impulsoDeseado.y = rb.linearVelocity.y;
        }

        float limiteY = saltandoBorde ? fuerzaSaltoBorde * 1.5f : 2f;

        if (saltandoBorde)
        {
            impulsoDeseado.y = fuerzaSaltoBorde;
        }

        if (impulsoDeseado.y > limiteY)
        {
            impulsoDeseado.y = limiteY;
        }

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, impulsoDeseado, factorDerrape * Time.fixedDeltaTime);

        float pitchObjetivo = saltandoBorde ? inclinacionSubida : 0f;
        pitchVisualActual = Mathf.Lerp(pitchVisualActual, pitchObjetivo, Time.fixedDeltaTime * 12f);

        if (direccionFinal != Vector3.zero)
        {
            bool necesitaGiroRapido = frenarPorMuroFrontal || Mathf.Abs(multiplicadorGiroEvasion) > 0.5f;
            float velocidadGiroDinamica = necesitaGiroRapido ? velocidadGiro * 1.8f : velocidadGiro;

            Vector3 direccionVisual = Vector3.ProjectOnPlane(direccionFinal, normalSuelo);
            if (direccionVisual == Vector3.zero) direccionVisual = direccionFinal;

            Quaternion rotacionBase = Quaternion.LookRotation(direccionVisual, normalSuelo);
            Quaternion rotacionConPitch = rotacionBase * Quaternion.Euler(pitchVisualActual, 0f, 0f);

            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rotacionConPitch, velocidadGiroDinamica * Time.fixedDeltaTime));
        }
    }

    void EjecutarReversa()
    {
        temporizadorReversa -= Time.fixedDeltaTime;

        Vector3 normalSuelo = Vector3.up;
        bool tocandoSuelo = false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hitSuelo, 1.5f, capaObstaculos, QueryTriggerInteraction.Ignore))
        {
            normalSuelo = hitSuelo.normal;
            tocandoSuelo = true;
        }

        Vector3 direccionMovimientoReal = Vector3.ProjectOnPlane(-transform.forward, normalSuelo).normalized;
        Vector3 impulsoDeseado = direccionMovimientoReal * velocidadReversa;

        if (tocandoSuelo)
        {
            impulsoDeseado -= normalSuelo * fuerzaGravedadExtra;
        }
        else
        {
            impulsoDeseado.y = rb.linearVelocity.y;
        }

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, impulsoDeseado, factorDerrape * Time.fixedDeltaTime);

        Vector3 direccionVisual = Vector3.ProjectOnPlane(transform.forward, normalSuelo);
        if (direccionVisual != Vector3.zero)
        {
            Quaternion rotacionNormal = Quaternion.LookRotation(direccionVisual, normalSuelo);
            rb.MoveRotation(rotacionNormal * Quaternion.Euler(0f, -velocidadGiro * 15f * Time.fixedDeltaTime, 0f));
        }

        if (temporizadorReversa <= 0)
        {
            enReversa = false;
        }
    }

    public void AplicarStun(float duracion)
    {
        if (tiempoEscudo > 0)
        {
            tiempoEscudo = 0f;
            return;
        }

        tiempoStun = duracion;
        enReversa = false;
    }

    public void AplicarBoost(float duracion, float multiplicador)
    {
        tiempoBoost = duracion;
        multiplicadorBoostActual = multiplicador;
    }

    public void AplicarEscudo(float duracion)
    {
        tiempoEscudo = duracion;
    }

    public void AplicarRalentizacion(float duracion, float multiplicador)
    {
        //aqui pones las particulas"
       
        if (tiempoEscudo > 0 || tiempoStun > 0) return;
        tiempoRalentizado = duracion;
        multiplicadorRalentizacion = multiplicador;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Oil"))
        {
            AplicarResbalon(1.5f);
        }
       
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            AplicarImpactoBala(1.3f);
        }
        else if(other.gameObject.CompareTag("Player"))
        {
            KartPowerUpController kartpower = other.gameObject.GetComponent<KartPowerUpController>();

            if (kartpower != null)
            {
                if(kartpower.hasStar)
                {

                    AplicarStun(kartpower.starStunSeconds);

                }

            }

        }
    }

    public void AplicarResbalon(float duracion)
    {
        if (tiempoEscudo > 0 || tiempoStun > 0) return;

        tiempoResbalando = duracion;
        enReversa = false;
    }

    public void AplicarImpactoBala(float duracion)
    {
        if (tiempoEscudo > 0) return;

        if (tiempoStun <= 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.3f, 6.5f, rb.linearVelocity.z * 0.3f);

            if (sfxAudioSource != null && sonidoImpactoBala != null)
            {
                sfxAudioSource.PlayOneShot(sonidoImpactoBala);
            }
        }

        tiempoStun = duracion;
        tiempoResbalando = 0f;
        enReversa = false;
    }

    void ProcesarResbalonProcedural()
    {
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, rb.linearVelocity * 0.95f, Time.fixedDeltaTime * 2f);

        float velocidadGiroResbalon = Mathf.Sin(Time.time * velocidadVaiven) * intensidadVaiven;

        Quaternion giroWobble = Quaternion.AngleAxis(velocidadGiroResbalon * Time.fixedDeltaTime, Vector3.up);
        rb.MoveRotation(rb.rotation * giroWobble);
    }
}