using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(KartController))]
public class KartPowerUpController : MonoBehaviour
{
    private KartController kart;

    [Header("Stun")]
    [SerializeField] private bool isStunned = false;
    [SerializeField] private float stunTimer = 0f;

    [Header("Shield")]
    [SerializeField] private bool hasShield = false;
    [SerializeField] private float shieldTimer = 0f;

    [Header("Star")]
    [SerializeField] public bool hasStar = false;
    [SerializeField] private float starTimer = 0f;
    [SerializeField] public float starStunSeconds = 1.5f;

    [SerializeField] private float starHitCooldown = 0.5f;
    private readonly Dictionary<int, float> starHitCdByTarget = new Dictionary<int, float>();

    [Header("Boost")]
    [SerializeField] private bool hasBoost = false;
    [SerializeField] private float boostTimer = 0f;
    [SerializeField] private float boostMultiplier = 1f;

    [Header("Jump")]
    [SerializeField] private bool hasJump = false;
    [SerializeField] private float boostedJumpForce = 15f;
    [SerializeField] private float jumpTimer = 0f;

    [Header("Rayo Ralentizador")]
    [SerializeField] private Transform puntoDisparoRayo;
    [SerializeField] private LayerMask capasDetectablesRayo;
    [SerializeField] private LineRenderer lineRendererRayo;

    [Header("Carga del Rayo")]
    [SerializeField] private bool usarVibracionCarga = true;
    [SerializeField] private float intensidadVibracionCarga = 0.12f;
    [SerializeField] private bool mostrarDebugRayo = true;

    [Header("Glow / Emisivo del Rayo")]
    [SerializeField] private LineRenderer lineRendererGlowRayo;
    [SerializeField] private float intensidadEmisionRayo = 4f;
    [SerializeField] private float multiplicadorAnchoGlow = 3.5f;
    [SerializeField] private float alphaGlow = 0.45f;

    private Coroutine rutinaRayo;

    private float originalJumpForce;


    [Header("Shield Feel")]
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private ParticleSystem shieldParticles;
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip shieldActivateSfx;
    [SerializeField] private AudioClip shieldBreakSfx;
    [SerializeField] private AudioClip shieldBlockSfx;
    
    [Header("Star Feel")]
    [SerializeField] private Renderer[] starRenderers;
    [SerializeField] private float starColorSpeed = 8f;
    [SerializeField] private float starEmissionIntensity = 3f;

    private Material[][] originalMaterials;
    private Material[][] starMaterials;
    private bool starVisualActive = false;

    [Header("Controlador de Audio Externo")]
    public PowerUpAudioController powerUpAudio;

    [Header("Spawn Points")]
    public Transform behindSpawnPoint;
    public Transform shootPoint;
    public bool IgnoreBumpThisFrame { get; private set; }

    void Awake()
    {
        kart = GetComponent<KartController>();

        originalJumpForce = kart.GetJumpForce();

        PrepararLineRendererRayo(Color.cyan, 0.18f);

        GuardarMaterialesOriginalesEstrella();
    }

    void Update()
    {
        IgnoreBumpThisFrame = false;

        TickShield();
        TickStar();
        TickBoost();
        TickStun();
        TickJump();
    }

    public void ActivateShield(float duration)
    {
        hasShield = true;
        shieldTimer = Mathf.Max(shieldTimer, duration);

        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        if (shieldParticles != null)
            shieldParticles.Play();

        if (powerUpAudio != null)
            powerUpAudio.PlayEscudo();
        else if (audioSource != null && shieldActivateSfx != null) 
            audioSource.PlayOneShot(shieldActivateSfx);
    }

    public void ActivateStar(float duration, float stunSeconds)
    {
        hasStar = true;
        starTimer = Mathf.Max(starTimer, duration);
        starStunSeconds = stunSeconds;

        hasShield = true;
        shieldTimer = Mathf.Max(shieldTimer, duration);

        StartStarVisual();
        if (powerUpAudio != null)
            powerUpAudio.PlayEstrella();
    }
    
    private void GuardarMaterialesOriginalesEstrella()
{
    if (starRenderers == null || starRenderers.Length == 0) return;

    originalMaterials = new Material[starRenderers.Length][];
    starMaterials = new Material[starRenderers.Length][];

    for (int i = 0; i < starRenderers.Length; i++)
    {
        if (starRenderers[i] == null) continue;

        originalMaterials[i] = starRenderers[i].materials;
        starMaterials[i] = new Material[originalMaterials[i].Length];

        for (int j = 0; j < originalMaterials[i].Length; j++)
        {
            starMaterials[i][j] = new Material(originalMaterials[i][j]);

            if (starMaterials[i][j].HasProperty("_EmissionColor"))
            {
                starMaterials[i][j].EnableKeyword("_EMISSION");
            }
        }
    }
}

private void StartStarVisual()
{
    if (starRenderers == null || starRenderers.Length == 0) return;

    starVisualActive = true;

    for (int i = 0; i < starRenderers.Length; i++)
    {
        if (starRenderers[i] == null) continue;
        if (starMaterials == null || starMaterials[i] == null) continue;

        starRenderers[i].materials = starMaterials[i];
    }
}

private void UpdateStarVisual()
{
    if (!starVisualActive) return;
    if (starMaterials == null) return;

    float hue = Mathf.Repeat(Time.time * starColorSpeed * 0.1f, 1f);
    Color starColor = Color.HSVToRGB(hue, 1f, 1f);

    Color emissionColor = starColor * starEmissionIntensity;

    for (int i = 0; i < starMaterials.Length; i++)
    {
        if (starMaterials[i] == null) continue;

        for (int j = 0; j < starMaterials[i].Length; j++)
        {
            Material mat = starMaterials[i][j];

            if (mat == null) continue;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", starColor);

            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", starColor);

            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", emissionColor);
        }
    }
}

private void StopStarVisual()
{
    if (!starVisualActive) return;

    starVisualActive = false;

    if (starRenderers == null || originalMaterials == null) return;

    for (int i = 0; i < starRenderers.Length; i++)
    {
        if (starRenderers[i] == null) continue;
        if (originalMaterials[i] == null) continue;

        starRenderers[i].materials = originalMaterials[i];
    }
}

    public void ApplyBoost(float multiplier, float duration)
    {
        hasBoost = true;
        boostMultiplier = Mathf.Max(multiplier, 1f);
        boostTimer = Mathf.Max(boostTimer, duration);

        kart.SetSpeedMultiplier(boostMultiplier);
        VFXController.GetInstance()?.StartBoostVFX();

        if (powerUpAudio != null)
            powerUpAudio.PlayBoost();
    }

    public void ApplyJump(float newJumpForce, float duration)
    {
        hasJump = true;
        boostedJumpForce = newJumpForce;
        jumpTimer = Mathf.Max(jumpTimer, duration);

        kart.SetJumpForce(boostedJumpForce);
        kart.HandleJump();
    }

    public void Stun(float seconds, bool refresh = true)
    {
        if (seconds <= 0f) return;

        if (hasShield)
        {
            PlayShieldBlockFeedback();
            return;
        }

        if (isStunned)
        {
            stunTimer = refresh ? Mathf.Max(stunTimer, seconds) : (stunTimer + seconds);
            return;
        }

        isStunned = true;
        stunTimer = seconds;

        kart.SetControlEnabled(false);
        kart.SetDriftAllowed(false);
        kart.ForceStopHorizontal();
    }

    #region Rayo Ralentizador

    public void DispararRayoRalentizador(
        float distanciaRayo,
        float duracionRalentizacion,
        float multiplicadorRalentizacion,
        Color colorRayo,
        float anchoRayo,
        float tiempoVisible
    )
    {
        if (rutinaRayo != null)
        {
            StopCoroutine(rutinaRayo);

            if (lineRendererRayo != null)
                lineRendererRayo.enabled = false;

            if (lineRendererGlowRayo != null)
                lineRendererGlowRayo.enabled = false;
        }

        rutinaRayo = StartCoroutine(RutinaRayoRalentizadorInstantaneo(
            distanciaRayo,
            duracionRalentizacion,
            multiplicadorRalentizacion,
            colorRayo,
            anchoRayo,
            tiempoVisible
        ));
    }
    private IEnumerator RutinaRayoRalentizadorInstantaneo(
        float distanciaRayo,
        float duracionRalentizacion,
        float multiplicadorRalentizacion,
        Color colorRayo,
        float anchoRayo,
        float tiempoVisible
    )
    {
        PrepararLineRendererRayo(colorRayo, anchoRayo);

        lineRendererRayo.enabled = true;

        if (lineRendererGlowRayo != null)
            lineRendererGlowRayo.enabled = true;

        lineRendererRayo.startColor = colorRayo;
        lineRendererRayo.endColor = new Color(colorRayo.r, colorRayo.g, colorRayo.b, 0f);

        lineRendererRayo.startWidth = anchoRayo;
        lineRendererRayo.endWidth = anchoRayo * 0.35f;

        float tiempo = 0f;

        while (tiempo < tiempoVisible)
        {
            tiempo += Time.deltaTime;

            ObtenerDatosRayo(
                distanciaRayo,
                out Vector3 inicio,
                out Vector3 fin,
                out RaycastHit hit,
                out bool hayHit
            );

            lineRendererRayo.SetPosition(0, inicio);
            lineRendererRayo.SetPosition(1, fin);

            if (lineRendererGlowRayo != null)
            {
                lineRendererGlowRayo.SetPosition(0, inicio);
                lineRendererGlowRayo.SetPosition(1, fin);
            }

            if (mostrarDebugRayo)
            {
                Debug.DrawLine(inicio, fin, colorRayo);
            }

            if (hayHit)
            {
                KartObstaculosIA rival = hit.collider.GetComponentInParent<KartObstaculosIA>();

                if (rival != null)
                {
                    rival.AplicarRalentizacion(
                        duracionRalentizacion,
                        multiplicadorRalentizacion
                    );

                    Debug.Log("Rayo ralentizó a: " + rival.name);

                    break;
                }
            }

            yield return null;
        }

        lineRendererRayo.enabled = false;

        if (lineRendererGlowRayo != null)
            lineRendererGlowRayo.enabled = false;

        rutinaRayo = null;
    }


    public void DispararRayoRalentizadorConCarga(
        float distanciaRayo,
        float duracionRalentizacion,
        float multiplicadorRalentizacion,
        Color colorRayo,
        float anchoInicialCarga,
        float anchoFinalRayo,
        float tiempoCarga,
        float tiempoVisibleRayoFinal
    )
    {
        if (rutinaRayo != null)
        {
            StopCoroutine(rutinaRayo);

            lineRendererRayo.enabled = false;

            if (lineRendererGlowRayo != null)
            {
                lineRendererGlowRayo.enabled = false;
            }
        }

        rutinaRayo = StartCoroutine(RutinaRayoConCarga(
            distanciaRayo,
            duracionRalentizacion,
            multiplicadorRalentizacion,
            colorRayo,
            anchoInicialCarga,
            anchoFinalRayo,
            tiempoCarga,
            tiempoVisibleRayoFinal
        ));
    }

    private IEnumerator RutinaRayoConCarga(
        float distanciaRayo,
        float duracionRalentizacion,
        float multiplicadorRalentizacion,
        Color colorRayo,
        float anchoInicialCarga,
        float anchoFinalRayo,
        float tiempoCarga,
        float tiempoVisibleRayoFinal
    )
    {
        PrepararLineRendererRayo(colorRayo, anchoFinalRayo);

        lineRendererRayo.enabled = true;

        if (lineRendererGlowRayo != null)
        {
            lineRendererGlowRayo.enabled = true;
        }
        // ============================
        // FASE 1: CARGA VISUAL
        // ============================
        // Aquí el rayo se ve grande y se va juntando.
        // Todavía NO aplica la ralentización.

        float tiempo = 0f;
        float tiempoCargaSeguro = Mathf.Max(0.01f, tiempoCarga);

        while (tiempo < tiempoCargaSeguro)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / tiempoCargaSeguro);

            ObtenerDatosRayo(
                distanciaRayo,
                out Vector3 inicioCarga,
                out Vector3 finCarga,
                out RaycastHit hitCarga,
                out bool hayHitCarga
            );

            float anchoActual = Mathf.Lerp(anchoInicialCarga, anchoFinalRayo, t);

            if (usarVibracionCarga)
            {
                Transform origen = puntoDisparoRayo != null ? puntoDisparoRayo : transform;

                float fuerzaVibracion = intensidadVibracionCarga * (1f - t);
                float vibracion = Mathf.Sin(Time.time * 45f) * fuerzaVibracion;

                finCarga += origen.right * vibracion;
            }

            float alphaInicio = Mathf.Lerp(0.35f, 1f, t);

            Color colorInicio = new Color(
                colorRayo.r,
                colorRayo.g,
                colorRayo.b,
                alphaInicio
            );

            Color colorFinal = new Color(
                colorRayo.r,
                colorRayo.g,
                colorRayo.b,
                0f
            );

            lineRendererRayo.startColor = colorInicio;
            lineRendererRayo.endColor = colorFinal;

            lineRendererRayo.startWidth = anchoActual;
            lineRendererRayo.endWidth = anchoActual * 0.35f;

            lineRendererRayo.SetPosition(0, inicioCarga);
            lineRendererRayo.SetPosition(1, finCarga);
            if (lineRendererGlowRayo != null)
            {
                lineRendererGlowRayo.SetPosition(0, inicioCarga);
                lineRendererGlowRayo.SetPosition(1, finCarga);
            }

            yield return null;
        }

        // ============================
        // FASE 2: DISPARO REAL
        // ============================
        // Aquí sí se hace el raycast final y se aplica la ralentización.

        ObtenerDatosRayo(
            distanciaRayo,
            out Vector3 inicioFinal,
            out Vector3 finFinal,
            out RaycastHit hitFinal,
            out bool hayHitFinal
        );

        if (hayHitFinal)
        {
            KartObstaculosIA rival = hitFinal.collider.GetComponentInParent<KartObstaculosIA>();

            if (rival != null)
            {
                rival.AplicarRalentizacion(duracionRalentizacion, multiplicadorRalentizacion);
                Debug.Log("Rayo ralentizó a: " + rival.name);
            }
            else
            {
                Debug.Log("El rayo golpeó algo, pero no era un rival.");
            }
        }

        lineRendererRayo.startColor = colorRayo;
        lineRendererRayo.endColor = new Color(colorRayo.r, colorRayo.g, colorRayo.b, 0f);

        lineRendererRayo.startWidth = anchoFinalRayo;
        lineRendererRayo.endWidth = anchoFinalRayo * 0.35f;

        lineRendererRayo.SetPosition(0, inicioFinal);
        lineRendererRayo.SetPosition(1, finFinal);
        if (lineRendererGlowRayo != null)
        {
            lineRendererGlowRayo.SetPosition(0, inicioFinal);
            lineRendererGlowRayo.SetPosition(1, finFinal);
        }

        if (mostrarDebugRayo)
        {
            Debug.DrawLine(inicioFinal, finFinal, colorRayo, 1f);
        }

        yield return new WaitForSeconds(tiempoVisibleRayoFinal);

        lineRendererRayo.enabled = false;

        if (lineRendererGlowRayo != null)
        {
            lineRendererGlowRayo.enabled = false;
        }
        rutinaRayo = null;
    }

    private void PrepararLineRendererRayo(Color colorRayo, float anchoRayo)
    {
        if (lineRendererRayo == null)
        {
            lineRendererRayo = gameObject.AddComponent<LineRenderer>();
        }

        if (lineRendererGlowRayo == null)
        {
            GameObject glowObj = new GameObject("Glow Rayo");
            glowObj.transform.SetParent(transform);
            glowObj.transform.localPosition = Vector3.zero;
            glowObj.transform.localRotation = Quaternion.identity;

            lineRendererGlowRayo = glowObj.AddComponent<LineRenderer>();
        }

        Material materialRayo = CrearMaterialEmisivo(colorRayo);
        Material materialGlow = CrearMaterialEmisivo(colorRayo);

        // Rayo principal
        ConfigurarLineRendererBase(
            lineRendererRayo,
            materialRayo,
            colorRayo,
            anchoRayo,
            anchoRayo * 0.35f
        );

        // Glow externo
        Color colorGlowInicio = new Color(
            colorRayo.r * intensidadEmisionRayo,
            colorRayo.g * intensidadEmisionRayo,
            colorRayo.b * intensidadEmisionRayo,
            alphaGlow
        );

        Color colorGlowFinal = new Color(
            colorRayo.r * intensidadEmisionRayo,
            colorRayo.g * intensidadEmisionRayo,
            colorRayo.b * intensidadEmisionRayo,
            0f
        );

        lineRendererGlowRayo.positionCount = 2;
        lineRendererGlowRayo.useWorldSpace = true;
        lineRendererGlowRayo.enabled = false;

        lineRendererGlowRayo.material = materialGlow;

        lineRendererGlowRayo.startWidth = anchoRayo * multiplicadorAnchoGlow;
        lineRendererGlowRayo.endWidth = anchoRayo * multiplicadorAnchoGlow * 0.35f;

        lineRendererGlowRayo.startColor = colorGlowInicio;
        lineRendererGlowRayo.endColor = colorGlowFinal;

        lineRendererGlowRayo.numCapVertices = 8;
        lineRendererGlowRayo.numCornerVertices = 8;
    }


    private Material CrearMaterialEmisivo(Color colorRayo)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material mat = new Material(shader);

        Color colorHDR = new Color(
            colorRayo.r * intensidadEmisionRayo,
            colorRayo.g * intensidadEmisionRayo,
            colorRayo.b * intensidadEmisionRayo,
            colorRayo.a
        );

        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", colorHDR);
        }

        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", colorHDR);
        }

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", colorHDR);
        }

        return mat;
    }

    private void ConfigurarLineRendererBase(
        LineRenderer lr,
        Material material,
        Color colorRayo,
        float anchoInicio,
        float anchoFinal
    )
    {
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.enabled = false;

        lr.material = material;

        lr.startWidth = anchoInicio;
        lr.endWidth = anchoFinal;

        Color colorHDRInicio = new Color(
            colorRayo.r * intensidadEmisionRayo,
            colorRayo.g * intensidadEmisionRayo,
            colorRayo.b * intensidadEmisionRayo,
            colorRayo.a
        );

        Color colorHDRFinal = new Color(
            colorRayo.r * intensidadEmisionRayo,
            colorRayo.g * intensidadEmisionRayo,
            colorRayo.b * intensidadEmisionRayo,
            0f
        );

        lr.startColor = colorHDRInicio;
        lr.endColor = colorHDRFinal;

        lr.numCapVertices = 8;
        lr.numCornerVertices = 8;
    }
    private void ObtenerDatosRayo(
        float distanciaRayo,
        out Vector3 inicio,
        out Vector3 fin,
        out RaycastHit hit,
        out bool hayHit
    )
    {
        Transform origen = puntoDisparoRayo != null ? puntoDisparoRayo : transform;

        inicio = origen.position;
        Vector3 direccion = origen.forward;

        fin = inicio + direccion * distanciaRayo;

        int mascara = capasDetectablesRayo.value == 0
            ? Physics.DefaultRaycastLayers
            : capasDetectablesRayo.value;

        hayHit = Physics.Raycast(
            inicio,
            direccion,
            out hit,
            distanciaRayo,
            mascara,
            QueryTriggerInteraction.Ignore
        );

        if (hayHit)
        {
            fin = hit.point;
        }
    }

    #endregion


    // =========================================================
    // RAYO DE INTERCAMBIO DE LUGAR
    // =========================================================

    public void DispararRayoIntercambio(
        float distanciaRayo,
        Color colorRayo,
        float anchoRayo,
        float tiempoVisible
    )
    {
        if (rutinaRayo != null)
        {
            StopCoroutine(rutinaRayo);

            if (lineRendererRayo != null)
                lineRendererRayo.enabled = false;

            if (lineRendererGlowRayo != null)
                lineRendererGlowRayo.enabled = false;
        }

        rutinaRayo = StartCoroutine(RutinaRayoIntercambioInstantaneo(
            distanciaRayo,
            colorRayo,
            anchoRayo,
            tiempoVisible
        ));
    }
    
    
    private IEnumerator RutinaRayoIntercambioInstantaneo(
        float distanciaRayo,
        Color colorRayo,
        float anchoRayo,
        float tiempoVisible
    )
    {
        PrepararLineRendererRayo(colorRayo, anchoRayo);

        lineRendererRayo.enabled = true;

        if (lineRendererGlowRayo != null)
            lineRendererGlowRayo.enabled = true;

        lineRendererRayo.startColor = colorRayo;
        lineRendererRayo.endColor = new Color(colorRayo.r, colorRayo.g, colorRayo.b, 0f);

        lineRendererRayo.startWidth = anchoRayo;
        lineRendererRayo.endWidth = anchoRayo * 0.35f;

        float tiempo = 0f;

        while (tiempo < tiempoVisible)
        {
            tiempo += Time.deltaTime;

            bool encontroObjetivo = BuscarPrimerObjetivoIntercambio(
                distanciaRayo,
                out Collider objetivo,
                out Vector3 inicio,
                out Vector3 fin
            );

            lineRendererRayo.SetPosition(0, inicio);
            lineRendererRayo.SetPosition(1, fin);

            if (lineRendererGlowRayo != null)
            {
                lineRendererGlowRayo.SetPosition(0, inicio);
                lineRendererGlowRayo.SetPosition(1, fin);
            }

            Debug.DrawLine(inicio, fin, colorRayo);

            if (encontroObjetivo && objetivo != null)
            {
                IntercambiarLugarCon(objetivo);
                break;
            }

            yield return null;
        }

        lineRendererRayo.enabled = false;

        if (lineRendererGlowRayo != null)
            lineRendererGlowRayo.enabled = false;

        rutinaRayo = null;
    }

    public void DispararRayoIntercambioConCarga(
        float distanciaRayo,
        Color colorRayo,
        float anchoInicialCarga,
        float anchoFinalRayo,
        float tiempoCarga,
        float tiempoVisibleRayoFinal
    )
    {
        if (rutinaRayo != null)
        {
            StopCoroutine(rutinaRayo);

            lineRendererRayo.enabled = true;

            if (lineRendererGlowRayo != null)
            {
                lineRendererGlowRayo.enabled = true;
            }
        }

        rutinaRayo = StartCoroutine(RutinaRayoIntercambioConCarga(
            distanciaRayo,
            colorRayo,
            anchoInicialCarga,
            anchoFinalRayo,
            tiempoCarga,
            tiempoVisibleRayoFinal
        ));
    }

    private IEnumerator RutinaRayoIntercambioConCarga(
        float distanciaRayo,
        Color colorRayo,
        float anchoInicialCarga,
        float anchoFinalRayo,
        float tiempoCarga,
        float tiempoVisibleRayoFinal
    )
    {
        PrepararLineRendererRayo(colorRayo, anchoFinalRayo);

        lineRendererRayo.enabled = true;

        if (lineRendererGlowRayo != null)
        {
            lineRendererGlowRayo.enabled = true;
        }

        // ============================
        // FASE 1: CARGA VISUAL
        // ============================
        float tiempo = 0f;
        float tiempoCargaSeguro = Mathf.Max(0.01f, tiempoCarga);

        while (tiempo < tiempoCargaSeguro)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / tiempoCargaSeguro);

            ObtenerLineaRayoIntercambio(
                distanciaRayo,
                out Vector3 inicioCarga,
                out Vector3 finCarga
            );

            float anchoActual = Mathf.Lerp(anchoInicialCarga, anchoFinalRayo, t);

            Transform origen = puntoDisparoRayo != null ? puntoDisparoRayo : transform;

            float fuerzaVibracion = 0.12f * (1f - t);
            float vibracion = Mathf.Sin(Time.time * 45f) * fuerzaVibracion;

            finCarga += origen.right * vibracion;

            float alphaInicio = Mathf.Lerp(0.35f, 1f, t);

            Color colorInicio = new Color(
                colorRayo.r,
                colorRayo.g,
                colorRayo.b,
                alphaInicio
            );

            Color colorFinal = new Color(
                colorRayo.r,
                colorRayo.g,
                colorRayo.b,
                0f
            );

            lineRendererRayo.startColor = colorInicio;
            lineRendererRayo.endColor = colorFinal;

            lineRendererRayo.startWidth = anchoActual;
            lineRendererRayo.endWidth = anchoActual * 0.35f;

            lineRendererRayo.SetPosition(0, inicioCarga);
            lineRendererRayo.SetPosition(1, finCarga);
            if (lineRendererGlowRayo != null)
            {
                lineRendererGlowRayo.SetPosition(0, inicioCarga);
                lineRendererGlowRayo.SetPosition(1, finCarga);
            }

            yield return null;
        }

        // ============================
        // FASE 2: DISPARO REAL
        // ============================

        bool encontroObjetivo = BuscarPrimerObjetivoIntercambio(
            distanciaRayo,
            out Collider objetivo,
            out Vector3 inicioFinal,
            out Vector3 finFinal
        );

        if (encontroObjetivo && objetivo != null)
        {
            IntercambiarLugarCon(objetivo);
        }

        lineRendererRayo.startColor = colorRayo;
        lineRendererRayo.endColor = new Color(colorRayo.r, colorRayo.g, colorRayo.b, 0f);

        lineRendererRayo.startWidth = anchoFinalRayo;
        lineRendererRayo.endWidth = anchoFinalRayo * 0.35f;

        lineRendererRayo.SetPosition(0, inicioFinal);
        lineRendererRayo.SetPosition(1, finFinal);
        if (lineRendererGlowRayo != null)
        {
            lineRendererGlowRayo.SetPosition(0, inicioFinal);
            lineRendererGlowRayo.SetPosition(1, finFinal);
        }

        Debug.DrawLine(inicioFinal, finFinal, colorRayo, 1f);

        yield return new WaitForSeconds(tiempoVisibleRayoFinal);

        lineRendererRayo.enabled = false;

        if (lineRendererGlowRayo != null)
        {
            lineRendererGlowRayo.enabled = false;
        }
        rutinaRayo = null;
    }

    private void ObtenerLineaRayoIntercambio(
        float distanciaRayo,
        out Vector3 inicio,
        out Vector3 fin
    )
    {
        Transform origen = puntoDisparoRayo != null ? puntoDisparoRayo : transform;

        inicio = origen.position;
        Vector3 direccion = origen.forward;

        fin = inicio + direccion * distanciaRayo;

        int mascara = capasDetectablesRayo.value == 0
            ? Physics.DefaultRaycastLayers
            : capasDetectablesRayo.value;

        RaycastHit[] hits = Physics.RaycastAll(
            inicio,
            direccion,
            distanciaRayo,
            mascara,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0) return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // Evita que el rayo se pegue a sí mismo
            if (hit.collider.transform.IsChildOf(transform)) continue;

            fin = hit.point;
            return;
        }
    }

    private bool BuscarPrimerObjetivoIntercambio(
        float distanciaRayo,
        out Collider objetivo,
        out Vector3 inicio,
        out Vector3 fin
    )
    {
        objetivo = null;

        Transform origen = puntoDisparoRayo != null ? puntoDisparoRayo : transform;

        inicio = origen.position;
        Vector3 direccion = origen.forward;

        fin = inicio + direccion * distanciaRayo;

        int mascara = capasDetectablesRayo.value == 0
            ? Physics.DefaultRaycastLayers
            : capasDetectablesRayo.value;

        RaycastHit[] hits = Physics.RaycastAll(
            inicio,
            direccion,
            distanciaRayo,
            mascara,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // Evita golpearse a sí mismo
            if (hit.collider.transform.IsChildOf(transform)) continue;

            fin = hit.point;

            KartPowerUpController otroPlayer = hit.collider.GetComponentInParent<KartPowerUpController>();
            KartObstaculosIA otraIA = hit.collider.GetComponentInParent<KartObstaculosIA>();

            bool esOtroPlayer = otroPlayer != null && otroPlayer != this;
            bool esIA = otraIA != null;

            if (esOtroPlayer || esIA)
            {
                objetivo = hit.collider;
                return true;
            }

            // Si golpeó una pared u obstáculo antes que a un rival,
            // el rayo se detiene ahí y no intercambia lugar.
            return false;
        }

        return false;
    }

    private void IntercambiarLugarCon(Collider objetivoCollider)
    {
        KartPowerUpController otroPlayer = objetivoCollider.GetComponentInParent<KartPowerUpController>();
        KartObstaculosIA otraIA = objetivoCollider.GetComponentInParent<KartObstaculosIA>();

        Transform objetivoTransform = null;

        if (otroPlayer != null && otroPlayer != this)
        {
            if (otroPlayer.HasShield())
            {
                Debug.Log("El objetivo tenía escudo. No se intercambió lugar.");
                return;
            }

            objetivoTransform = otroPlayer.transform;
        }
        else if (otraIA != null)
        {
            objetivoTransform = otraIA.transform;
        }

        if (objetivoTransform == null || objetivoTransform == transform)
        {
            return;
        }

        Rigidbody rbUsuario = GetComponent<Rigidbody>();
        Rigidbody rbObjetivo = objetivoTransform.GetComponent<Rigidbody>();

        Vector3 posicionUsuario = transform.position;
        Quaternion rotacionUsuario = transform.rotation;

        Vector3 posicionObjetivo = objetivoTransform.position;
        Quaternion rotacionObjetivo = objetivoTransform.rotation;

        Vector3 velocidadUsuario = rbUsuario != null ? rbUsuario.linearVelocity : Vector3.zero;
        Vector3 velocidadAngularUsuario = rbUsuario != null ? rbUsuario.angularVelocity : Vector3.zero;

        Vector3 velocidadObjetivo = rbObjetivo != null ? rbObjetivo.linearVelocity : Vector3.zero;
        Vector3 velocidadAngularObjetivo = rbObjetivo != null ? rbObjetivo.angularVelocity : Vector3.zero;

        // Usuario va al lugar del objetivo
        MoverKartParaIntercambio(
            transform,
            rbUsuario,
            posicionObjetivo,
            rotacionObjetivo
        );

        // Objetivo va al lugar del usuario
        MoverKartParaIntercambio(
            objetivoTransform,
            rbObjetivo,
            posicionUsuario,
            rotacionUsuario
        );

        // Intercambiamos también velocidades para que no se sienta raro físicamente
        if (rbUsuario != null)
        {
            rbUsuario.linearVelocity = velocidadObjetivo;
            rbUsuario.angularVelocity = velocidadAngularObjetivo;
        }

        if (rbObjetivo != null)
        {
            rbObjetivo.linearVelocity = velocidadUsuario;
            rbObjetivo.angularVelocity = velocidadAngularUsuario;
        }

        Physics.SyncTransforms();

        Debug.Log("Intercambio de lugar realizado con: " + objetivoTransform.name);
    }

    private void MoverKartParaIntercambio(
        Transform kartTransform,
        Rigidbody kartRigidbody,
        Vector3 nuevaPosicion,
        Quaternion nuevaRotacion
    )
    {
        if (kartRigidbody != null)
        {
            kartRigidbody.position = nuevaPosicion;
            kartRigidbody.rotation = nuevaRotacion;

            kartRigidbody.transform.SetPositionAndRotation(
                nuevaPosicion,
                nuevaRotacion
            );
        }
        else
        {
            kartTransform.SetPositionAndRotation(
                nuevaPosicion,
                nuevaRotacion
            );
        }
    }

    public bool IsStunned() => isStunned;
    public bool HasShield() => hasShield;
    public bool HasStar() => hasStar;
    public bool HasBoost() => hasBoost;
    public bool HasJump() => hasJump;

    public void OnKartCollision(Collision collision)
    {
        if (!hasStar) return;

        IgnoreBumpThisFrame = true;

        KartPowerUpController otherPlayer = collision.collider.GetComponentInParent<KartPowerUpController>();
        KartObstaculosIA otherIA = collision.collider.GetComponentInParent<KartObstaculosIA>();

        if ((otherPlayer == null && otherIA == null) || otherPlayer == this) return;

        int id = collision.collider.gameObject.GetInstanceID();
        float now = Time.time;

        if (!starHitCdByTarget.TryGetValue(id, out float nextAllowed) || now >= nextAllowed)
        {
            if (otherPlayer != null)
            {
                otherPlayer.Stun(starStunSeconds, true);
            }
            else if (otherIA != null)
            {
                otherIA.AplicarStun(starStunSeconds);
            }

            starHitCdByTarget[id] = now + starHitCooldown;
        }
    }

    private void TickShield()
    {
        if (!hasShield) return;

        shieldTimer -= Time.deltaTime;

        if (shieldTimer <= 0f)
        {
            hasShield = false;
            shieldTimer = 0f;

            if (shieldVisual != null)
                shieldVisual.SetActive(false);

            if (shieldParticles != null)
                shieldParticles.Stop();

            if (audioSource != null && shieldBreakSfx != null)
                audioSource.PlayOneShot(shieldBreakSfx);
        }
    }

    private void TickStar()
    {
        if (!hasStar) return;

        UpdateStarVisual();

        starTimer -= Time.deltaTime;

        if (starTimer <= 0f)
        {
            hasStar = false;
            starTimer = 0f;
            starHitCdByTarget.Clear();

            StopStarVisual();
        }
    }

    private void TickBoost()
    {
        if (!hasBoost) return;

        boostTimer -= Time.deltaTime;

        if (boostTimer <= 0f)
        {
            hasBoost = false;
            boostTimer = 0f;
            boostMultiplier = 1f;

            kart.SetSpeedMultiplier(1f);

            //Stop VFX Boost Function call
            VFXController.GetInstance()?.StopBoostVFX();
        }
    }

    private void TickStun()
    {
        if (!isStunned) return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            isStunned = false;
            stunTimer = 0f;

            kart.SetControlEnabled(true);
            kart.SetDriftAllowed(true);
        }
    }

    private void TickJump()
    {
        if (!hasJump) return;

        jumpTimer -= Time.deltaTime;

        if (jumpTimer <= 0f)
        {
            hasJump = false;
            jumpTimer = 0f;

            kart.SetJumpForce(originalJumpForce);
           
        }
    }

    private void PlayShieldBlockFeedback()
    {
        if (audioSource != null && shieldBlockSfx != null)
            audioSource.PlayOneShot(shieldBlockSfx);
    }
}