using UnityEngine;

public class KartMovement : MonoBehaviour
{
    [Header("Ajustes del Kart")]
    public Rigidbody sphereRB;
    public float speed = 50f;
    public float turnSpeed = 60f;
    public float jumpForce = 10f;
    public float gravity = 10f; // Gravedad extra para pegarlo al suelo
    
    [Header("Visuales")]
    public Transform kartModel; // El modelo 3D del coche
    public Transform groundCheck;
    public LayerMask groundLayer;

    private float moveInput;
    private float turnInput;
    private bool isGrounded;
    private bool isDrifting;

    void Update()
    {
        // 1. Inputs
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");
        
        // 2. Salto y Drift
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            sphereRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isDrifting = true;
        }
        
        if (Input.GetKeyUp(KeyCode.Space)) isDrifting = false;

        // 3. Alinear el modelo con el suelo
        transform.position = sphereRB.transform.position - new Vector3(0, 0.4f, 0); // Ajuste de altura
    }

    void FixedUpdate()
    {
        // Detectar suelo
        isGrounded = Physics.Raycast(groundCheck.position, -transform.up, 1.0f, groundLayer);

        if (isGrounded)
        {
            // Aceleración
            if (Mathf.Abs(moveInput) > 0)
            {
                sphereRB.AddForce(transform.forward * moveInput * speed, ForceMode.Acceleration);
            }

            // Giro (Rotamos el objeto completo, no la esfera fisica)
            float driftFactor = isDrifting ? 1.5f : 1f; // Gira más rápido si derrapas
            if (moveInput != 0) // Solo girar si nos movemos
            {
                transform.Rotate(Vector3.up * turnInput * turnSpeed * driftFactor * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Gravedad extra para caer rápido
            sphereRB.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        }
        
        // Simular Drift visual (Rotar el modelo hijo)
        if (isDrifting && turnInput != 0)
        {
            float driftAngle = turnInput > 0 ? 30 : -30;
            // Lerp para suavizar la inclinación del modelo
            kartModel.localRotation = Quaternion.Lerp(kartModel.localRotation, Quaternion.Euler(0, driftAngle, 0), Time.deltaTime * 5f);
        }
        else
        {
            // Volver a rotación 0
            kartModel.localRotation = Quaternion.Lerp(kartModel.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }
    }
}
