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
    [SerializeField] private bool hasStar = false;
    [SerializeField] private float starTimer = 0f;
    [SerializeField] private float starStunSeconds = 1.5f;

    [SerializeField] private float starHitCooldown = 0.5f;
    private readonly Dictionary<int, float> starHitCdByTarget = new Dictionary<int, float>();

    [Header("Boost")]
    [SerializeField] private bool hasBoost = false;
    [SerializeField] private float boostTimer = 0f;
    [SerializeField] private float boostMultiplier = 1f;

    [Header("Jump")]
    [SerializeField] private bool hasJump = false;
    [SerializeField] private float boostedJumpForce = 15f; // la fuerza temporal que quieres
    [SerializeField] private float jumpTimer = 0f;
    
    [Header("Iman")]
    [SerializeField] private Transform puntoDisparoRayo;
    [SerializeField] private LayerMask capasDetectablesRayo;
    [SerializeField] private LineRenderer lineRendererRayo;

    private Coroutine rutinaRayo;
    
    

    private float originalJumpForce;

    [Header("Spawn Points")]
    public Transform behindSpawnPoint;
    public Transform shootPoint;
    public bool IgnoreBumpThisFrame { get; private set; }

    void Awake()
    {
        kart = GetComponent<KartController>();

        // Guardamos la fuerza original del salto del kart
        originalJumpForce = kart.GetJumpForce();
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
    }

    public void ActivateStar(float duration, float stunSeconds = 1.5f)
    {
        hasStar = true;
        starTimer = Mathf.Max(starTimer, duration);
        starStunSeconds = stunSeconds;

        hasShield = true;
        shieldTimer = Mathf.Max(shieldTimer, duration);
    }

    public void ApplyBoost(float multiplier, float duration)
    {
        hasBoost = true;
        boostMultiplier = Mathf.Max(multiplier, 1f);
        boostTimer = Mathf.Max(boostTimer, duration);

        kart.SetSpeedMultiplier(boostMultiplier);
    }

    // Aquí aplicas la nueva fuerza de salto por X segundos
    public void ApplyJump(float newJumpForce, float duration)
    {
        hasJump = true;
        boostedJumpForce = newJumpForce;
        jumpTimer = Mathf.Max(jumpTimer, duration);

        kart.SetJumpForce(boostedJumpForce);
    }

    public void Stun(float seconds, bool refresh = true)
    {
        if (seconds <= 0f) return;

        if (hasShield) return;

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

    #region rayoralentizador

     public void DispararRayoRalentizador(
    float distanciaRayo,
    float duracionRalentizacion,
    float multiplicadorRalentizacion,
    Color colorRayo,
    float anchoRayo,
    float tiempoVisible
)
{
    PrepararLineRendererRayo(colorRayo, anchoRayo);

    Transform origen = puntoDisparoRayo != null ? puntoDisparoRayo : transform;

    Vector3 posicionInicio = origen.position;
    Vector3 direccion = origen.forward;
    Vector3 posicionFinal = posicionInicio + direccion * distanciaRayo;

    if (Physics.Raycast(
        posicionInicio,
        direccion,
        out RaycastHit hit,
        distanciaRayo,
        capasDetectablesRayo,
        QueryTriggerInteraction.Ignore))
    {
        posicionFinal = hit.point;

        KartObstaculosIA rival = hit.collider.GetComponentInParent<KartObstaculosIA>();

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

    MostrarRayo(posicionInicio, posicionFinal, tiempoVisible);
}

private void PrepararLineRendererRayo(Color colorRayo, float anchoRayo)
{
    if (lineRendererRayo == null)
    {
        lineRendererRayo = gameObject.AddComponent<LineRenderer>();
    }

    lineRendererRayo.positionCount = 2;
    lineRendererRayo.useWorldSpace = true;

    lineRendererRayo.startWidth = anchoRayo;
    lineRendererRayo.endWidth = anchoRayo * 0.35f;

    lineRendererRayo.startColor = colorRayo;
    lineRendererRayo.endColor = new Color(colorRayo.r, colorRayo.g, colorRayo.b, 0f);

    if (lineRendererRayo.material == null)
    {
        Shader shader = Shader.Find("Sprites/Default");
        lineRendererRayo.material = new Material(shader);
    }

    lineRendererRayo.enabled = false;
}

private void MostrarRayo(Vector3 inicio, Vector3 fin, float tiempoVisible)
{
    if (rutinaRayo != null)
    {
        StopCoroutine(rutinaRayo);
    }

    rutinaRayo = StartCoroutine(RutinaMostrarRayo(inicio, fin, tiempoVisible));
}

private IEnumerator RutinaMostrarRayo(Vector3 inicio, Vector3 fin, float tiempoVisible)
{
    lineRendererRayo.SetPosition(0, inicio);
    lineRendererRayo.SetPosition(1, fin);

    lineRendererRayo.enabled = true;

    yield return new WaitForSeconds(tiempoVisible);

    lineRendererRayo.enabled = false;
}

    #endregion
   

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
        }
    }

    private void TickStar()
    {
        if (!hasStar) return;

        starTimer -= Time.deltaTime;
        if (starTimer <= 0f)
        {
            hasStar = false;
            starTimer = 0f;
            starHitCdByTarget.Clear();
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

            // Regresa la fuerza normal al terminar los 6 segundos
            kart.SetJumpForce(originalJumpForce);
        }
    }
}