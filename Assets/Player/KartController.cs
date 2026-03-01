using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class KartController : MonoBehaviour
{
    
    [Header("Movement")]
    public float acceleration = 15f;
    public float maxSpeed = 20f;

    [Header("Steering Settings")]
    [SerializeField] private float minTurnSpeed = 80f;
    [SerializeField] private float maxTurnSpeed = 200f;
    [SerializeField] private float speedTurnReduction = 0.5f;
    [SerializeField] private float rotationSmoothness = 10f;
    [SerializeField] private float reverseTurnMultiplier = 1.6f;

    [Header("Particles")]
    public GameObject[] particlesDrift;

    [Header("Reverse")]
    public float reverseAcceleration = 8f;
    public float maxReverseSpeed = 8f;

    [Header("Drift")]
    public float driftTurnMultiplier = 1.5f;
    public float driftGrip = 0.5f;
    private int driftDirection = 0; 

    [Header("Drift Visual")]
    public Transform visualModel;

    [Header("Advanced Drift Visual")]
    public float driftYawAngle = 35f;
    public float driftRollAngle = 30f;
    public float driftVisualSpeed = 8f;

    private float currentYaw;
    private float currentRoll;

    [Header("Coins Boost")]
    public int coins = 0;
    public float speedPerCoin = 0.5f;
    public int maxCoins = 10;

    [Header("Jump")]
    public float jumpPower = 10f;
    public bool isGrounded = false;

    [Header("Bump Settings")]
    public float bumpDuration = 0.2f;

    private float bumpTimer = 0f;
    private float bumpSpeed = 0f;
    private bool isBumping = false;

    private Rigidbody rb;

    private float currentSpeed;
    private bool isDrifting;

    private float moveInput;
    private float turnInput;

    private Vector3 groundNormal = Vector3.up;
    
    // Manager 
    MainManager gm;
    InputManager input;
    
    // ---- Control & Multipliers (PowerUps los modifican) ----
    private bool controlEnabled = true;
    private float speedMultiplier = 1f;

    // Opcional: si quieres que PowerUps “bloqueen” drift
    private bool driftAllowed = true;

    // Referencia al PowerUpController (para forward de colisiones, etc.)
    private KartPowerUpController powerUps;
    public bool IsBoosting => speedMultiplier > 1.05f;
    
    
    public Rigidbody RB => rb;
    public float CurrentSpeed => currentSpeed;

    private bool isPaused = false;
    private float savedSpeed;
    //Esta es la funcion que quiero que se haga cada vez que pauso o despauso el juego
    public void OnChangeGameStateCallback(GameState newState)
    {
        isPaused = newState != GameState.Play;
        
        if (isPaused)
        {
            // Guarda la velocidad en variable
            savedSpeed = currentSpeed;
            
            // Cambia las propiedades del RB
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            
            // Cancela el drift
            isDrifting = false;
            driftDirection = 0;
        }
        else
        {
            rb.isKinematic = false;
            currentSpeed = savedSpeed;
        }
    }
    
    void Awake()
    {
        gm = MainManager.GetInstance();
        gm.onChangeGameState += OnChangeGameStateCallback;
        input = InputManager.GetInstance();
        
        if (gm.gameState == GameState.Pause)
            isPaused = true;
        
        rb = GetComponent<Rigidbody>();
        powerUps = GetComponent<KartPowerUpController>();
    }


    public Transform shootPoint;
    public GameObject bulletPrefab;
    void Update()
    {

        if (input.IsButtonDown(BUTTONS.START))
        {
            if (isPaused)
            {
                gm.ChangeGameState(GameState.Play);
            }
            else
            {
                gm.ChangeGameState(GameState.Pause);
            }
        }

        if (isPaused) return;
        
        moveInput = 0f; 
        turnInput = 0f;
       
        //moveInput = Input.GetAxis("Vertical");
        moveInput = input.GetAXis(AXIS.LEFT_STICK_VERTICAL);
        
        //turnInput = Input.GetAxis("Horizontal");
        turnInput = input.GetAXis(AXIS.LEFT_STICK_HORIZONTAL);
        

        // Drift (solo si está permitido)
        if (driftAllowed && controlEnabled)
        {
            if (input.IsButtonDown(BUTTONS.B))
            {
                if (Mathf.Abs(turnInput) > 0.2f)
                {
                    isDrifting = true;
                    driftDirection = (int)Mathf.Sign(turnInput);
                }
                SetDriftParticlesGO(true);
            }

            if (input.IsButtonUp(BUTTONS.B))
            {
                SetDriftParticlesGO(false);
                isDrifting = false;
                driftDirection = 0;
            }
        }
        else
        {
            // si te bloquean, apaga drift
            SetDriftParticlesGO(false);
            isDrifting = false;
            driftDirection = 0;
        }

        if (controlEnabled && input.IsButtonDown(BUTTONS.A))
        {
            HandleJump();
        }


        if (Input.GetKeyDown(KeyCode.T))
        {
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().Initialize(shootPoint.forward);
        }
        
    }

    void FixedUpdate()
    {
        if (!isBumping)
        {
            HandleMovement();
            HandleSteering();
        }
        else
        {
            bumpTimer -= Time.fixedDeltaTime;
            currentSpeed = bumpSpeed;

            if (bumpTimer <= 0f)
                isBumping = false;
        }

        HandleDriftVisual();
        HandleBetterGravity();
        CheckGround();

        // Pegado al suelo
        if (isGrounded)
            rb.AddForce(-groundNormal * 10f, ForceMode.Acceleration);

        Vector3 alignedVelocity = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(alignedVelocity.x, rb.linearVelocity.y, alignedVelocity.z);
    }

    void HandleMovement()
    {
        // acelera / frena
        if (moveInput > 0)
        {
            float finalAcceleration = acceleration * speedMultiplier;
            currentSpeed += finalAcceleration * Time.fixedDeltaTime;
        }
        else if (moveInput < 0)
        {
            currentSpeed -= reverseAcceleration * Time.fixedDeltaTime;
        }
        else
        {
            float deceleration = acceleration * 0.8f;
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        // monedas
        float coinBoost = coins * speedPerCoin;

        // maxSpeed final
        float finalMaxSpeed = (maxSpeed + coinBoost) * speedMultiplier;

        // clamp
        currentSpeed = Mathf.Clamp(currentSpeed, -maxReverseSpeed, finalMaxSpeed);

        // aplica al RB
        Vector3 forwardMove = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(forwardMove.x, rb.linearVelocity.y, forwardMove.z);
    }

    void HandleSteering()
    {
        if (!isGrounded) return;
        if (Mathf.Abs(currentSpeed) <= 0.1f) return;

        float steeringInput = turnInput;

        if (isDrifting)
            steeringInput = driftDirection;

        float realMaxSpeed = (maxSpeed + (coins * speedPerCoin)) * speedMultiplier;
        float speedPercent = Mathf.Clamp01(Mathf.Abs(currentSpeed) / Mathf.Max(0.01f, realMaxSpeed));

        float dynamicTurnSpeed = Mathf.Lerp(
            maxTurnSpeed,
            minTurnSpeed,
            speedPercent * speedTurnReduction
        );

       
        dynamicTurnSpeed *= steeringMultiplier;

      
        if (isDrifting)
            dynamicTurnSpeed *= driftTurnMultiplier;

       
        if (currentSpeed < 0)
            dynamicTurnSpeed *= reverseTurnMultiplier;

        float rotationAmount = steeringInput * dynamicTurnSpeed * Time.fixedDeltaTime;

        Quaternion steerRotation = Quaternion.AngleAxis(rotationAmount, groundNormal);
        Quaternion alignToGround = Quaternion.FromToRotation(transform.up, groundNormal) * rb.rotation;
        Quaternion finalRotation = steerRotation * alignToGround;

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                finalRotation,
                rotationSmoothness * Time.fixedDeltaTime
            )
        );

        if (isDrifting)
        {
            Vector3 sidewaysVelocity = Vector3.Project(rb.linearVelocity, transform.right);
            rb.linearVelocity -= sidewaysVelocity * driftGrip;
        }
    }

    void HandleDriftVisual()
    {
        float coinBoost = coins * speedPerCoin;
        float realMaxSpeed = (maxSpeed + coinBoost) * speedMultiplier;

        float speedPercent = Mathf.Abs(currentSpeed) / Mathf.Max(0.01f, realMaxSpeed);
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

        if (visualModel != null)
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
            if (rb.linearVelocity.y > 0) rb.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
            else rb.AddForce(Physics.gravity * 4f, ForceMode.Acceleration);
        }
    }

    public void CheckGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f))
        {
            isGrounded = true;
            groundNormal = hit.normal;

            if (rb.linearVelocity.y < -2f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    // --- API para PowerUps (sin meter lógica de powerups aquí) ---
    public void SetControlEnabled(bool enabled)
    {
        controlEnabled = enabled;
        if (!enabled)
        {
            moveInput = 0f;
            turnInput = 0f;
        }
    }

    public void SetDriftAllowed(bool allowed)
    {
        driftAllowed = allowed;
        if (!allowed)
        {
            SetDriftParticlesGO(false);
            isDrifting = false;
            driftDirection = 0;
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ForceStopHorizontal()
    {
        currentSpeed = 0f;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.angularVelocity = Vector3.zero;
    }

    public void AddCoin()
    {
        coins++;
        coins = Mathf.Clamp(coins, 0, maxCoins);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Forward a PowerUps (Star stun, etc.)
        if (powerUps != null)
            powerUps.OnKartCollision(collision);

        // Bump base (PowerUps puede pedir que se ignore)
        if (powerUps != null && powerUps.IgnoreBumpThisFrame)
            return;

        if (collision.contacts.Length == 0) return;

        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        normal.y = 0f;
        normal.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        float impactDot = Vector3.Dot(forward, -normal);

        if (impactDot > 0.2f)
        {
            isBumping = true;
            bumpTimer = bumpDuration;
            bumpSpeed = -Mathf.Abs(currentSpeed) * 0.7f;
        }
    }

    private void SetDriftParticlesGO(bool on)
    {
        if (particlesDrift == null) return;
        for (int i = 0; i < particlesDrift.Length; i++)
            if (particlesDrift[i] != null)
                particlesDrift[i].SetActive(on);
    }
    
    private float steeringMultiplier = 1f;

    public void SetSteeringMultiplier(float multiplier)
    {
        steeringMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);
    }
}