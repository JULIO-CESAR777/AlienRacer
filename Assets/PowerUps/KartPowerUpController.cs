using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartController))]
public class KartPowerUpController : MonoBehaviour
{
    private KartController kart;

    // ---- STUN ----
    [Header("Stun")]
    [SerializeField] private bool isStunned = false;
    [SerializeField] private float stunTimer = 0f;

    // ---- SHIELD ----
    [Header("Shield")]
    [SerializeField] private bool hasShield = false;
    [SerializeField] private float shieldTimer = 0f;

    // ---- STAR ----
    [Header("Star")]
    [SerializeField] private bool hasStar = false;
    [SerializeField] private float starTimer = 0f;
    [SerializeField] private float starStunSeconds = 1.5f;

    [SerializeField] private float starHitCooldown = 0.5f;
    private readonly Dictionary<int, float> starHitCdByTarget = new Dictionary<int, float>();

    // ---- BOOST ----
    [Header("Boost")]
    [SerializeField] private bool hasBoost = false;
    [SerializeField] private float boostTimer = 0f;
    [SerializeField] private float boostMultiplier = 1f;

    
    public bool IgnoreBumpThisFrame { get; private set; }

    void Awake()
    {
        kart = GetComponent<KartController>();
    }

    void Update()
    {
        IgnoreBumpThisFrame = false;

        TickShield();
        TickStar();
        TickBoost();
        TickStun();
    }

    // ======================
    // PUBLIC API (Items/Synergies)
    // ======================

    public void ActivateShield(float duration)
    {
        Debug.Log("Escudo");
        hasShield = true;
        shieldTimer = Mathf.Max(shieldTimer, duration);
    }

    public void ActivateStar(float duration, float stunSeconds = 1.5f)
    {
        Debug.Log("Estrella");
        hasStar = true;
        starTimer = Mathf.Max(starTimer, duration);
        starStunSeconds = stunSeconds;

     
        hasShield = true;
        shieldTimer = Mathf.Max(shieldTimer, duration);
    }

    public void ApplyBoost(float multiplier, float duration)
    {
        Debug.Log("Boost");
        hasBoost = true;
        boostMultiplier = Mathf.Max(multiplier, 1f);
        boostTimer = Mathf.Max(boostTimer, duration);

        kart.SetSpeedMultiplier(boostMultiplier);
    }

    public void Stun(float seconds, bool refresh = true)
    {
        if (seconds <= 0f) return;

        // Inmunidad por escudo (y si quieres, también por star)
        if (hasShield) return;

        if (isStunned)
        {
            stunTimer = refresh ? Mathf.Max(stunTimer, seconds) : (stunTimer + seconds);
            return;
        }

        isStunned = true;
        stunTimer = seconds;

        // Bloquear control + frenar
        kart.SetControlEnabled(false);
        kart.SetDriftAllowed(false);
        kart.ForceStopHorizontal();
    }

    public bool IsStunned() => isStunned;
    public bool HasShield() => hasShield;
    public bool HasStar() => hasStar;
    public bool HasBoost() => hasBoost;

    // ======================
    // COLLISION HOOK (Star hits)
    // ======================

    public void OnKartCollision(Collision collision)
    {
        if (!hasStar) return;

        // Mario-style: con star no quieres que te haga bump
        IgnoreBumpThisFrame = true;

       
        KartPowerUpController other = collision.collider.GetComponentInParent<KartPowerUpController>();
        if (other == null || other == this) return;

        int id = other.GetInstanceID();
        float now = Time.time;

        if (!starHitCdByTarget.TryGetValue(id, out float nextAllowed) || now >= nextAllowed)
        {
            other.Stun(starStunSeconds, refresh: true);
            starHitCdByTarget[id] = now + starHitCooldown;
        }
    }

    // ======================
    // TIMERS
    // ======================

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

            // regresar multiplicador a normal
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

            // regresar control
            kart.SetControlEnabled(true);
            kart.SetDriftAllowed(true);
        }
    }
}