using System.Collections.Generic;
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

    [Header("Spawn Points")]
    public Transform behindSpawnPoint;
    public Transform shootPoint;
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

    public bool IsStunned() => isStunned;
    public bool HasShield() => hasShield;
    public bool HasStar() => hasStar;
    public bool HasBoost() => hasBoost;

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
}