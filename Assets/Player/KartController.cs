using UnityEngine;

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

    private Vector3 smoothedGroundNormal = Vector3.up;
    
    [Header("Bump Settings")]
    public float bumpDuration = 0.2f;

    private float bumpTimer = 0f;
    private float bumpSpeed = 0f;
    private bool isBumping = false;

    public Rigidbody rb;

    public float currentSpeed;
    public bool isDrifting;

    private float moveInput;
    private float turnInput;

    private Vector3 groundNormal = Vector3.up;
    
    // Manager 
    MainManager gm;
    InputManager input;
    UiManagerPlayer uiManager;
    
    // ---- Control & Multipliers (PowerUps los modifican) ----
    private bool controlEnabled = true;
    private float speedMultiplier = 1f;

    // Opcional: si quieres que PowerUps “bloqueen” drift
    private bool driftAllowed = true;

    // Referencia al PowerUpController (para forward de colisiones, etc.)
    private KartPowerUpController powerUps;
    public bool IsBoosting => speedMultiplier > 1.05f;

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
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        powerUps = GetComponent<KartPowerUpController>();

        gm = MainManager.GetInstance();
        if (gm != null)
        {
            gm.onChangeGameState += OnChangeGameStateCallback;

            if (gm.gameState == GameState.Pause)
                isPaused = true;
        }
        else
        {
            Debug.LogError("MainManager is NULL in KartController");
        }

        input = InputManager.GetInstance();
        if (input == null)
        {
            Debug.LogError("InputManager is NULL in KartController");
        }
        
        uiManager = UiManagerPlayer.GetInstance();
        
        uiManager.UpdateCoinText(coins.ToString());

    }


   
   

    private float accelerate;
    private float brake;
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
        //moveInput = input.GetAXis(AXIS.LEFT_STICK_VERTICAL);
        accelerate = input.IsButton(BUTTONS.R2) ? 1f : 0f;
        brake = input.IsButton(BUTTONS.L2) ? 1f : 0f;
        moveInput = accelerate - brake;
        
        //turnInput = Input.GetAxis("Horizontal");
        turnInput = input.GetAXis(AXIS.LEFT_STICK_HORIZONTAL);
        

        // Drift (solo si está permitido)
        if (driftAllowed && controlEnabled)
        {
            if (input.IsButtonDown(BUTTONS.A))
            {
                if (Mathf.Abs(turnInput) > 0.2f)
                {
                    isDrifting = true;
                    driftDirection = (int)Mathf.Sign(turnInput);
                    SetDriftParticlesGO(true);
                }
               
            }

            if (input.IsButtonUp(BUTTONS.A))
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

        if (controlEnabled && input.IsButtonDown(BUTTONS.B) && !isDrifting)
        {
            HandleJump();
        }
        
    }

    void FixedUpdate()
    {

        if (isPaused) return;
        
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
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            rb.AddForce(-smoothedGroundNormal * 3f, ForceMode.Acceleration);
        }
        
        Vector3 verticalVelocity = Vector3.up * rb.linearVelocity.y;

        // Base forward
        Vector3 forwardVelocity = transform.forward * currentSpeed;

        if (isBumping)
            return;
        
        // Si NO hay input lateral y NO está drifteando
        if (Mathf.Abs(turnInput) < 0.05f && !isDrifting)
        {
            // eliminar cualquier componente lateral
            rb.linearVelocity = forwardVelocity + verticalVelocity;
        }
        else
        {
            // permitir que la física conserve parte del lateral controlado
            Vector3 projectedForward = Vector3.Project(rb.linearVelocity, transform.forward);
            rb.linearVelocity = projectedForward + verticalVelocity;
        }

        // eliminar torque no deseado
        rb.angularVelocity = Vector3.zero;
        
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
            if (currentSpeed > 0)
            {
                // Frenar fuerte si vas hacia adelante
                currentSpeed -= acceleration * 1.5f * Time.fixedDeltaTime;
            }
            else
            {
                // Reversa cuando ya estás en negativo
                currentSpeed -= reverseAcceleration * Time.fixedDeltaTime;
            }
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
        if (Mathf.Abs(turnInput) < 0.05f && !isDrifting) return;

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

        // Construir rotación limpia desde cero
        Vector3 forwardProjected = Vector3.ProjectOnPlane(transform.forward, smoothedGroundNormal).normalized;

        Quaternion baseRotation = Quaternion.LookRotation(forwardProjected, smoothedGroundNormal);

        Quaternion steerRotation = Quaternion.AngleAxis(rotationAmount, smoothedGroundNormal);

        Quaternion finalRotation = steerRotation * baseRotation;

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
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Physics.gravity * 1.5f, ForceMode.Acceleration);
            else
                rb.AddForce(Physics.gravity * 5f, ForceMode.Acceleration);
        }
    }

    public float groundCheckDistance = 1.2f;
    public float groundSphereRadius = 0.4f;

    public void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(origin, groundSphereRadius, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            
            if (hit.normal.y < 0.3f)
            {
                isGrounded = false;
                return;
            }
            
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // Si es demasiado vertical, es pared/banqueta grande
            if (slopeAngle > 50f)
            {
                isGrounded = false;
                return;
            }

            isGrounded = true;

            groundNormal = hit.normal;
            smoothedGroundNormal = Vector3.Slerp(smoothedGroundNormal, groundNormal, 12f * Time.deltaTime);
            
        }
        else
        {
            isGrounded = false;
            smoothedGroundNormal = Vector3.Slerp(smoothedGroundNormal, Vector3.up, 5f * Time.deltaTime);
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
        uiManager.UpdateCoinText(coins.ToString());
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Wall")) return;
        
        print("choco");
        
        // Forward a PowerUps (Star stun, etc.)
        if (powerUps != null)
            powerUps.OnKartCollision(collision);

        // Bump base (PowerUps puede pedir que se ignore)
        if (powerUps != null && powerUps.IgnoreBumpThisFrame)
            return;

        if (collision.contacts.Length == 0) return;

        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;
        
        if (Mathf.Abs(normal.y) > 0.7f) return;

        normal.y = 0f;
        normal.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        float impactDot = Vector3.Dot(forward, -normal);

        if (impactDot > 0.2f)
        {
            // Cancelar movimiento hacia la pared
            Vector3 velocity = rb.linearVelocity;

            // Quitar componente en dirección de la normal
            Vector3 pushDir = Vector3.Project(velocity, -normal);
            velocity -= pushDir;

            // Matar vertical
            velocity.y = 0f;

            rb.linearVelocity = velocity;
            rb.angularVelocity = Vector3.zero;

            currentSpeed = 0f;

            isBumping = true;
            bumpTimer = bumpDuration;
            bumpSpeed = -Mathf.Abs(currentSpeed) * 0.7f;
        }
    }

    
    // COSAS JULIO
    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (coins < amount) return false;

        coins -= amount;
        coins = Mathf.Clamp(coins, 0, maxCoins);
        uiManager.UpdateCoinText(coins.ToString());
        return true;
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