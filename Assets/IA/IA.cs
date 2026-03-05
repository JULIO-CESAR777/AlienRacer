using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KartObstaculosIA : MonoBehaviour
{
    [Header("Ruta y Navegaci�n")]
    public Transform[] waypoints;
    public float distanciaCambio = 6f;
    public float anticipacionCurva = 12f;

    [Header("Motor y F�sicas")]
    public float velocidadMaxima = 16f;
    public float velocidadMinima = 7f;
    public float aceleracion = 8f;
    public float velocidadGiro = 7f;
    public float factorDerrape = 6f;
    public float fuerzaGravedadExtra = 20f;

    [Header("Competitividad (Overtake & Boost)")]
    public float boostRecta = 1.2f;
    public float boostRebufo = 1.35f;
    public float distanciaRebufo = 20f;
    public float fuerzaAdelantamiento = 4.5f;

    [Header("Sensores y Evasi�n")]
    public float longitudSensor = 8f;
    public float anguloSensores = 25f;
    public float fuerzaEvasionMuro = 4f;
    public LayerMask capaObstaculos;

    [Header("Sistema Anti-Atascos")]
    public float distanciaChoqueFrontal = 1.5f;
    public float tiempoReversa = 1.2f;
    public float velocidadReversa = 6f;

    private int indiceWaypoint = 0;
    private float velocidadActual = 0f;
    private float offsetCompetitivo = 0f;

    private bool enReversa = false;
    private float temporizadorReversa = 0f;
    private float adnVelocidad;
    private float adnAceleracion;
    private float direccionRebase = 1f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        adnVelocidad = Random.Range(0.98f, 1.05f);
        adnAceleracion = Random.Range(0.95f, 1.08f);
        direccionRebase = Random.value > 0.5f ? 1f : -1f;
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        if (enReversa)
        {
            EjecutarReversa();
            return;
        }

        ProcesarConduccion();
    }

    void ProcesarConduccion()
    {
        Transform objetivoActual = waypoints[indiceWaypoint];
        Vector3 posicionObjetivo = objetivoActual.position;
        posicionObjetivo.y = transform.position.y;

        float distanciaAlPunto = Vector3.Distance(transform.position, posicionObjetivo);

        if (distanciaAlPunto <= distanciaCambio)
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

        if (Physics.Raycast(origenRayo, dirFrente, out RaycastHit hitCaza, distanciaRebufo))
        {
            if (hitCaza.collider.CompareTag("Racer IA") || hitCaza.collider.CompareTag("Player"))
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

        if (Physics.Raycast(origenRayo, dirFrente, out RaycastHit hitFrente, longitudSensor, capaObstaculos))
        {
            if (!hitFrente.collider.CompareTag("Racer IA") && !hitFrente.collider.CompareTag("Player"))
            {
                if (hitFrente.distance <= distanciaChoqueFrontal)
                {
                    enReversa = true;
                    temporizadorReversa = tiempoReversa;
                    return;
                }
                frenarPorMuroFrontal = true;
                float intensidad = 1f - (hitFrente.distance / longitudSensor);
                multiplicadorGiroEvasion -= (fuerzaEvasionMuro * intensidad) * direccionRebase;
            }
        }

        if (Physics.Raycast(origenRayo, dirDer, out RaycastHit hitDer, longitudSensor, capaObstaculos))
        {
            if (!hitDer.collider.CompareTag("Racer IA") && !hitDer.collider.CompareTag("Player"))
            {
                float intensidad = 1f - (hitDer.distance / longitudSensor);
                multiplicadorGiroEvasion -= (fuerzaEvasionMuro * intensidad);
                if (cazandoRival && direccionRebase > 0) direccionRebase = -1f;
            }
        }

        if (Physics.Raycast(origenRayo, dirIzq, out RaycastHit hitIzq, longitudSensor, capaObstaculos))
        {
            if (!hitIzq.collider.CompareTag("Racer IA") && !hitIzq.collider.CompareTag("Player"))
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
        velocidadActual = Mathf.Lerp(velocidadActual, velocidadObjetivo, (aceleracion * adnAceleracion) * Time.fixedDeltaTime);

        Vector3 normalSuelo = Vector3.up;
        bool tocandoSuelo = false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hitSuelo, 1.5f, capaObstaculos))
        {
            normalSuelo = hitSuelo.normal;
            tocandoSuelo = true;
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

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, impulsoDeseado, factorDerrape * Time.fixedDeltaTime);

        if (direccionFinal != Vector3.zero)
        {
            bool necesitaGiroRapido = frenarPorMuroFrontal || Mathf.Abs(multiplicadorGiroEvasion) > 0.5f;
            float velocidadGiroDinamica = necesitaGiroRapido ? velocidadGiro * 1.8f : velocidadGiro;

            Vector3 direccionVisual = Vector3.ProjectOnPlane(direccionFinal, normalSuelo);
            if (direccionVisual == Vector3.zero) direccionVisual = direccionFinal;

            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionVisual, normalSuelo);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rotacionObjetivo, velocidadGiroDinamica * Time.fixedDeltaTime));
        }
    }

    void EjecutarReversa()
    {
        temporizadorReversa -= Time.fixedDeltaTime;

        Vector3 normalSuelo = Vector3.up;
        bool tocandoSuelo = false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hitSuelo, 1.5f, capaObstaculos))
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
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rotacionNormal, 10f * Time.fixedDeltaTime));
        }

        transform.Rotate(Vector3.up, -velocidadGiro * 10f * Time.fixedDeltaTime);

        if (temporizadorReversa <= 0)
        {
            enReversa = false;
        }
    }
}