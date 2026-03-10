using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public int maxBounces = 5;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private int bounceCount = 0;

    // Sistema de pausa
    MainManager gm;
    bool isPaused;

    public void OnChangeGameStateCallback(GameState newState)
    {
        isPaused = newState != GameState.Play;

        if (isPaused)
            rb.linearVelocity = Vector3.zero;
        else
            rb.linearVelocity = moveDirection * speed;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        gm = MainManager.GetInstance();
        gm.onChangeGameState += OnChangeGameStateCallback;

        if (gm.gameState == GameState.Pause)
            isPaused = true;

       
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(Vector3 direction)
    {
        moveDirection = direction.normalized;
        rb.linearVelocity = moveDirection * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isPaused) return;
        
        if(collision.gameObject.layer != LayerMask.NameToLayer("Wall")) Destroy(gameObject);

        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
            return;
        }

        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        // Reflejar dirección
        moveDirection = Vector3.Reflect(moveDirection, normal).normalized;

        rb.linearVelocity = moveDirection * speed;

        bounceCount++;
    }
}