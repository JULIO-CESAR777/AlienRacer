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

    [Header("Wheels Visual")]
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;

    [SerializeField] private float maxSteeringAngle = 20f;
    [SerializeField] private float wheelSteerSmooth = 10f;

    private Quaternion initialRotFL;
    private Quaternion initialRotFR;

    private float currentWheelSteer;

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
    
    [Header("Collision Bounce")]
    [SerializeField] private float wallBounceForce = 6f;
    [SerializeField] private float wallBounceUpForce = 0.5f;
    [SerializeField] private float minImpactSpeedForBounce = 2f;
    [SerializeField] private float wallStickPreventionForce = 8f;
    [SerializeField] private float botBounceForce = 3f;

    private float bumpTimer = 0f;
    private float bumpSpeed = 0f;
    private bool isBumping = false;

    public Rigidbody rb;

    public float currentSpeed;
    public bool isDrifting;

    private float moveInput;
    private float turnInput;

    private Vector3 groundNormal = Vector3.up;

    MainManager gm;
    InputManager input;
    UiManagerPlayer uiManager;

    private bool controlEnabled = true;
    private float speedMultiplier = 1f;

    private bool driftAllowed = true;

    private KartPowerUpController powerUps;
    public bool IsBoosting => speedMultiplier > 1.05f;

    private bool isPaused = false;
    private float savedSpeed;

    private float timerSalto = 0f;
    private float recuperacionAgarre = 15f;

    private bool empujandoKartMuerto = false;

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
            jumpBlockTimer = jumpBlockAfterResume;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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

        if (frontLeftWheel != null)
            initialRotFL = frontLeftWheel.localRotation;

        if (frontRightWheel != null)
            initialRotFR = frontRightWheel.localRotation;

    }

    private float accelerate;
    private float brake;

    [Header("To Ground")]
    public float timerToGround;
    private float timer;
    
    [Header("Pause Input Buffer")]
    [SerializeField] private float jumpBlockAfterResume = 0.2f;
    private float jumpBlockTimer = 0f;
    
    void Update()
    {
        
        if (jumpBlockTimer > 0f)
        {
            jumpBlockTimer -= Time.deltaTime;
        }

        if (input.IsButtonDown(BUTTONS.START) && !gm.countDownActive)
        {
            if (isPaused)
            {
                gm.ChangeGameState(GameState.Play);
                uiManager.ResumeGame();
            }
            else
            {
                gm.ChangeGameState(GameState.Pause);
                uiManager.PauseGame();
            }
        }

        if (isPaused) return;

        if (isGrounded)
        {
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
            if (timer >= timerToGround)
            {
                print("volando");
                return;
            }
        }

        moveInput = 0f;
        turnInput = 0f;

        if (input.currentInputType == INPUT_TYPE.XBOX)
        {
            accelerate = input.GetAXis(AXIS.RIGHT_TRIGGER); // XBOX controller
        }
        else
        {
            accelerate = input.IsButton(BUTTONS.R2) ? 1f : 0f; // Keyboard and playstation controller
        }

        if (input.currentInputType == INPUT_TYPE.XBOX)
        {
            brake = input.GetAXis(AXIS.LEFT_TRIGGER); // XBOX controller
        }
        else
        {
            brake = input.IsButton(BUTTONS.L2) ? 1f : 0f; // Keyboard and playstation controller
        }

        moveInput = accelerate - brake;

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

        if (controlEnabled && jumpBlockTimer <= 0f && input.IsButtonDown(BUTTONS.B) && !isDrifting)
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

        if (timerSalto > 0f) timerSalto -= Time.fixedDeltaTime;
        float limiteSalto = (timerSalto > 0f) ? (jumpPower + 2f) : 2f;

        float velocidadYActual = rb.linearVelocity.y;

        if (velocidadYActual > limiteSalto)
        {
            velocidadYActual = limiteSalto;
        }

        Vector3 verticalVelocity = Vector3.up * velocidadYActual;

        // Base forward
        Vector3 forwardVelocity = transform.forward * currentSpeed;

        if (isBumping)
        {
            rb.linearVelocity = forwardVelocity + verticalVelocity;
        }
        else
        {
            // Si NO hay input lateral y NO está drifteando
            if (Mathf.Abs(turnInput) < 0.05f && !isDrifting)
            {
                // eliminar cualquier componente lateral suavemente
                Vector3 targetVelocity = forwardVelocity;
                targetVelocity.y = velocidadYActual;
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, recuperacionAgarre * Time.fixedDeltaTime);
            }
            else if (isDrifting)
            {
                Vector3 proyectedSidewaysVelocity = Vector3.Project(rb.linearVelocity, transform.right);
                rb.linearVelocity -= proyectedSidewaysVelocity * driftGrip;
            }
            else
            {
                // permitir que la física conserve parte del lateral controlado
                Vector3 projectedForward = Vector3.Project(rb.linearVelocity, transform.forward);
                Vector3 targetVelocity = projectedForward;
                targetVelocity.y = velocidadYActual;
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 12f * Time.fixedDeltaTime);
            }
        }

        if (rb.linearVelocity.y > limiteSalto)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, limiteSalto, rb.linearVelocity.z);
        }

        // eliminar torque no deseado
        rb.angularVelocity = Vector3.zero;

        // Rotacion de las ruedas
        HandleWheelSteering();

        Vector3 euler = rb.rotation.eulerAngles;
        rb.MoveRotation(Quaternion.Euler(0f, euler.y, 0f));

        // Se resetea al final del frame. Si seguimos chocando, el OnCollisionStay la volverá a prender.
        empujandoKartMuerto = false;
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

        if (empujandoKartMuerto)
        {
            finalMaxSpeed = 4f; // Limitamos la velocidad máxima drásticamente como si empujaras algo pesado
            if (currentSpeed > finalMaxSpeed)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, finalMaxSpeed, 15f * Time.fixedDeltaTime);
            }
        }

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

        // Si está drifteando, usa la dirección bloqueada del drift
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

        // En reversa, invertir la dirección del giro
        if (currentSpeed < 0f && !isDrifting)
        {
            steeringInput *= -1f;
            dynamicTurnSpeed *= reverseTurnMultiplier;
        }

        float rotationAmount = steeringInput * dynamicTurnSpeed * Time.fixedDeltaTime;

        float targetY = rb.rotation.eulerAngles.y + rotationAmount;
        Quaternion targetRotation = Quaternion.Euler(0f, targetY, 0f);

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
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

    void HandleJump()
    {
        if (!isGrounded) return;
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        timerSalto = 0.5f;
    }

    void HandleBetterGravity()
    {
        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Physics.gravity * 1.5f, ForceMode.Acceleration);
            else
                rb.AddForce(Physics.gravity * 5f, ForceMode.Acceleration);
        }
    }

    void HandleWheelSteering()
    {
        if (frontLeftWheel == null || frontRightWheel == null) return;

        float targetAngle = turnInput * maxSteeringAngle;

        // Si está en drift, usa la dirección fija
        if (isDrifting)
            targetAngle = driftDirection * maxSteeringAngle;

        // suavizado visual
        currentWheelSteer = Mathf.Lerp(currentWheelSteer, targetAngle, Time.deltaTime * wheelSteerSmooth);

        Quaternion steerRot = Quaternion.Euler(0f, currentWheelSteer, 0f);

        frontLeftWheel.localRotation = initialRotFL * steerRot;
        frontRightWheel.localRotation = initialRotFR * steerRot;
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
        bool isWall = collision.gameObject.layer == LayerMask.NameToLayer("Wall");
        bool isBot = collision.gameObject.CompareTag("Bot");

        if (!isWall && !isBot) return;

        if (powerUps != null)
            powerUps.OnKartCollision(collision);

        if (powerUps != null && powerUps.IgnoreBumpThisFrame)
            return;

        if (collision.contactCount == 0) return;

        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;

        // Ignorar suelo/techo
        if (Mathf.Abs(normal.y) > 0.7f) return;

        // Solo normal horizontal
        Vector3 flatNormal = normal;
        flatNormal.y = 0f;

        if (flatNormal.sqrMagnitude < 0.001f) return;
        flatNormal.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        // Qué tan frontal fue el choque
        float impactDot = Vector3.Dot(forward, -flatNormal);

        // Solo reaccionar a choques medio frontales
        if (impactDot <= 0.15f) return;

        // Guardar velocidad previa
        float impactSpeed = Mathf.Abs(currentSpeed);

        // 1. Quitar componente de velocidad hacia la pared
        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityIntoWall = Vector3.Project(velocity, -flatNormal);
        velocity -= velocityIntoWall;

        // 2. Evitar que trepe la pared
        if (velocity.y > 0f)
            velocity.y = 0f;

        rb.linearVelocity = velocity;
        rb.angularVelocity = Vector3.zero;

        // 3. Dirección de rebote: alejarse de la pared
        Vector3 bounceDirection = flatNormal;

        // 4. Si fue pared
        if (isWall)
        {
            if (impactSpeed >= minImpactSpeedForBounce)
            {
                // Empujón instantáneo lejos de la pared
                rb.AddForce(
                    bounceDirection * wallBounceForce + Vector3.up * wallBounceUpForce,
                    ForceMode.VelocityChange
                );
            }

            // Retroceso controlado estilo kart
            currentSpeed = 0f;
            isBumping = true;
            bumpTimer = bumpDuration;
            bumpSpeed = -Mathf.Clamp(impactSpeed * 0.35f, 1.5f, maxReverseSpeed * 0.5f);
        }
        // 5. Si fue bot
        else if (isBot)
        {
            currentSpeed *= 0.6f;

            rb.AddForce(
                bounceDirection * botBounceForce,
                ForceMode.VelocityChange
            );
        }
    }

    void OnCollisionStay(Collision collision)
    {
        bool isWall = collision.gameObject.layer == LayerMask.NameToLayer("Wall");
        if (!isWall) return;

        if (collision.contactCount == 0) return;

        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;

        if (Mathf.Abs(normal.y) > 0.7f) return;

        Vector3 flatNormal = normal;
        flatNormal.y = 0f;

        if (flatNormal.sqrMagnitude < 0.001f) return;
        flatNormal.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        float pushingIntoWall = Vector3.Dot(forward, -flatNormal);

        // Si el jugador sigue acelerando hacia la pared
        if (moveInput > 0.1f && pushingIntoWall > 0.2f)
        {
            // Empuje continuo para despegarlo un poco
            rb.AddForce(flatNormal * wallStickPreventionForce, ForceMode.Acceleration);

            // Cancelar componente de velocidad contra la pared
            Vector3 velocity = rb.linearVelocity;
            Vector3 intoWall = Vector3.Project(velocity, -flatNormal);
            rb.linearVelocity = velocity - intoWall;

            // Evitar subir la pared
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
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