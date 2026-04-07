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
    void Update()
    {

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
            Vector3 velocity = rb.linearVelocity;
            Vector3 pushDir = Vector3.Project(velocity, -normal);
            velocity -= pushDir;
            velocity.y = 0f; // Magia anti-rampa inicial
            rb.linearVelocity = velocity;
            rb.angularVelocity = Vector3.zero;

            float impactoVelocidad = currentSpeed;

            if (isWall)
            {
                currentSpeed = 0f;
                isBumping = true;
                bumpTimer = bumpDuration * 0.5f;
                bumpSpeed = -Mathf.Abs(impactoVelocidad) * 0.1f;
            }
            else if (isBot)
            {
                // Solo perdemos un poco de velocidad al impactar inicialmente
                currentSpeed *= 0.5f;
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (isBumping) return;

        if (collision.gameObject.CompareTag("Bot"))
        {
            if (collision.contacts.Length == 0) return;

            Vector3 normal = collision.contacts[0].normal;
            if (Mathf.Abs(normal.y) > 0.7f) return; // Ignorar si ya estamos de alguna forma arriba

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            float impactDot = Vector3.Dot(forward, -normal);

            // Si estamos empujando hacia adelante contra el bot
            if (impactDot > 0.2f)
            {
                Rigidbody colRb = collision.gameObject.GetComponent<Rigidbody>();

                // Si el bot va muy lento (apagado o esperando la salida)
                if (colRb != null && colRb.linearVelocity.magnitude < 2f)
                {
                    empujandoKartMuerto = true;

                    // Matamos el eje Y agresivamente para que las llantas no intenten trepar
                    Vector3 velSegura = rb.linearVelocity;
                    if (velSegura.y > 0.1f)
                    {
                        velSegura.y = 0f;
                        rb.linearVelocity = velSegura;
                    }
                }
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