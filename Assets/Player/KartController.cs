using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
public class KartController : MonoBehaviour
{
    [Header("Camera")]
    public CinemachineCamera vcam;
    public float baseFov = 75f;
    public float maxFov = 90f;
    
    [Header("Camera Tilt")]
    public float maxTilt = 8f;
    public float tiltSpeed = 5f;
    private float currentTilt;
    
    [Header("Movement")]
    public float acceleration = 15f;
    public float maxSpeed = 20f;
    public float turnSpeed = 120f;
    
    [Header("Steering Settings")]
    [SerializeField] private float baseTurnSpeed = 120f;
    [SerializeField] private float minTurnSpeed = 80f;
    [SerializeField] private float maxTurnSpeed = 200f;
    [SerializeField] private float speedTurnReduction = 0.5f;
    [SerializeField] private float rotationSmoothness = 10f;
    [SerializeField] private float reverseTurnMultiplier = 1.6f;
    
    
    
    [Header("Particles")]
    public  GameObject[] particlesDrift;
    
    [Header("Reverse")]
    public float reverseAcceleration = 8f;
    public float maxReverseSpeed = 8f;

    [Header("Drift")]
    public float driftTurnMultiplier = 1.5f;
    public float driftGrip = 0.5f;
    private int driftDirection = 0; // -1 izquierda, 1 derecha
    
    [Header("Drift Visual")]
    public Transform visualModel;
    
    [Header("Advanced Drift Visual")]
    public float driftYawAngle = 35f;      // Giro horizontal exagerado
    public float driftRollAngle = 30f;     // Inclinación lateral (2 ruedas)
    public float driftVisualSpeed = 8f;    // Qué tan rápido rota

    private float currentYaw;
    private float currentRoll;

    [Header("Coins Boost")]
    public int coins = 0;
    public float speedPerCoin = 0.5f;
    public int maxCoins = 10;
    
    [Header("Jump")]
    public float jumpPower = 10f;
    public float gravityMultiplier = 2f;
    public bool isGrounded = false;
    
    [Header("Boost")]
    private bool isBoosting;
    private float boostMultiplier = 1f;
    
    private Rigidbody rb;

    private float currentSpeed;
    private bool isDrifting;

    private float moveInput;
    private float turnInput;

  
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Obtencion de inputs pero se va a cambiar seguramente
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        // Drifting
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
          
            if (Mathf.Abs(turnInput) > 0.2f)
            {
                isDrifting = true;
                driftDirection = (int)Mathf.Sign(turnInput);
            }
            SetDriftParticlesGO(true);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            SetDriftParticlesGO(false);
            isDrifting = false;
            driftDirection = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleJump();
        }
        
        
        // ----- Cosas que afectan a la camara -------
        float speedPercent = currentSpeed / maxSpeed;
        
        // Cambio de FOV por velocidad
        vcam.Lens.FieldOfView = Mathf.Lerp(baseFov, maxFov, speedPercent);
        
        // Agregar una inclinacion en la camara cuando se gira
        float targetTilt = -turnInput * maxTilt;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Rotación solo en Z
        vcam.transform.localRotation = Quaternion.Euler(0f, 0f, currentTilt);

        
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleSteering();
        HandleDriftVisual();
        HandleBetterGravity();
        CheckGround();
        
        // Mantener pegado al suelo
        if (isGrounded)
        {
            rb.AddForce(-groundNormal * 10f, ForceMode.Acceleration);
        }
    }
    
    #region HandleMovement

    void HandleMovement()
    {

        // Si hay input agrega movimiento
        if (moveInput > 0)
        {
            float finalAcceleration = acceleration * boostMultiplier;
            currentSpeed += finalAcceleration * Time.fixedDeltaTime;
        }
        // Si no, desacelera
        else if(moveInput < 0)
        {
            currentSpeed -= reverseAcceleration * Time.fixedDeltaTime;
        }
        else
        {
            float deceleration = acceleration * 0.8f; // puedes ajustar
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }
        
        // Toma en cuenta el boost que otorga las monedas y da la velocidad max final
        float coinBoost = coins * speedPerCoin;
        float finalMaxSpeed = (maxSpeed + coinBoost) * boostMultiplier;
        
        // Hace un clamp en la velocidad
        currentSpeed = Mathf.Clamp(currentSpeed, -maxReverseSpeed, finalMaxSpeed);

        // Agrega velocidad al RigidBody
        Vector3 forwardMove = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(forwardMove.x, rb.linearVelocity.y, forwardMove.z);
    }

    void HandleSteering()
    {
        // Para rotar checamos si esta en el piso
        if (!isGrounded) return;
        // y que tenga la suficiente velocidad
        if (Mathf.Abs(currentSpeed) <= 0.1f) return;
        
        float steeringInput = turnInput;

        // Si esta haciendo drift bloquea cambiar de direccion
        if (isDrifting)
        {
            steeringInput = driftDirection;
        }

        // Calculo de la velocidad total (monedas, boost e inputs)
        float realMaxSpeed = (maxSpeed + (coins * speedPerCoin)) * boostMultiplier;
        float speedPercent = Mathf.Clamp01(Mathf.Abs(currentSpeed) / realMaxSpeed);

        // Potencia de giro dependiendo de la velocidad
        float dynamicTurnSpeed = Mathf.Lerp(
            maxTurnSpeed,
            minTurnSpeed,
            speedPercent * speedTurnReduction
        );

        // Mas giro mientras este el drift
        if (isDrifting)
        {
            dynamicTurnSpeed *= driftTurnMultiplier;
        }

        // Esto es para que cuando vaya en reversa gire mas
        if (currentSpeed < 0)
        {
            dynamicTurnSpeed *= reverseTurnMultiplier;
        }

        // La cantidad de giro que va a tener
        float rotationAmount =
            steeringInput *
            dynamicTurnSpeed *
            Time.fixedDeltaTime;

        // Consigue rotacion que debe de tener el coche (esto es para las pendientes)
        Quaternion steerRotation = Quaternion.AngleAxis(rotationAmount, groundNormal);
        // Esto basicamente va a conseguir los datos de cuanto a rotas entre su rotacion actual y a donde tiene que rotar
        Quaternion alignToGround = Quaternion.FromToRotation(transform.up, groundNormal) * rb.rotation;
        // Suma de rotaciones
        Quaternion finalRotation = steerRotation * alignToGround;
        // La rotacion - Todo esto principalmente lo hice para cuando sube colinas o algo asi
        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                finalRotation,
                rotationSmoothness * Time.fixedDeltaTime
            )
        );

        // Controlacion de velocidades en el drift
        if (isDrifting)
        {
            /*
             * Aquí mentiria si digo que lo entendi al 100 pero esto sirve para reducir la velocidad del eje Z para cuando va de lado
             * El tema del Vector3.Project otorga la proyeccion de cierto vector del eje que le pidas y asi lo reducimos para que este
             * no termine girando como loco por el exceso de velocidad lateral
             */
            Vector3 sidewaysVelocity =
                Vector3.Project(rb.linearVelocity, transform.right);

            rb.linearVelocity -= sidewaysVelocity * driftGrip;
        }
    }
    
    
    void HandleDriftVisual()
    {
        float coinBoost = coins * speedPerCoin;
        float realMaxSpeed = (maxSpeed + coinBoost) * boostMultiplier;

        float speedPercent = Mathf.Abs(currentSpeed) / realMaxSpeed;
        speedPercent = Mathf.Clamp01(speedPercent);

        if (isDrifting)
        {
            float direction = driftDirection;

            float targetYaw = direction * driftYawAngle;
            float targetRoll = -direction * driftRollAngle * speedPercent;

            currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * driftVisualSpeed);
            currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * driftVisualSpeed);
        }
        else
        {
            currentYaw = Mathf.Lerp(currentYaw, 0f, Time.deltaTime * driftVisualSpeed);
            currentRoll = Mathf.Lerp(currentRoll, 0f, Time.deltaTime * driftVisualSpeed);
        }

        visualModel.localRotation = Quaternion.Euler(0f, currentYaw, currentRoll);
    }

    public void HandleJump()
    {
        if (!isGrounded) return;
        
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }

    public void HandleBetterGravity()
    {
        if (!isGrounded)
        {
            // Subiendo
            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
            }
            // Cayendo
            else
            {
                rb.AddForce(Physics.gravity * 4f, ForceMode.Acceleration);
            }
        }
    }
    
    private Vector3 groundNormal = Vector3.up;
    
    public void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.5f))
        {
            isGrounded = true;
            groundNormal = hit.normal;
            if (rb.linearVelocity.y < -2f)
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    0f,
                    rb.linearVelocity.z);
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
        
    }
    
    #endregion
    
    // Agrega las monedas
    public void AddCoin()
    {
        coins++;
        coins = Mathf.Clamp(coins, 0, maxCoins);
    }

    #region items Use

    public IEnumerator ApplyBoost(float boostForce, float duration, bool withSynergy)
    {
        if (isBoosting)
            yield break;

        isBoosting = true;

        float timer = 0f;

        while (timer < duration)
        {
            boostMultiplier = Mathf.Lerp(boostForce, 1f, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        boostMultiplier = 1f;
        isBoosting = false;
    }

    #endregion
    
    private void SetDriftParticlesGO(bool on)
    {
        for (int i = 0; i < particlesDrift.Length; i++)
            if (particlesDrift[i] != null)
                particlesDrift[i].SetActive(on);
    }
}
