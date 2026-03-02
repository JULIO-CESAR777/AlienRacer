using UnityEngine;

[RequireComponent(typeof(Camera))]
public class KartCamera : MonoBehaviour
{
    private Camera cam;
    private KartController kart;

    [Header("FOV")]
    public float baseFOV = 60f;
    public float maxFOV = 85f;
    public float boostExtraFOV = 10f;
    public float fovSharpness = 5f;

    [Header("Tilt")]
    public float maxTiltAngle = 12f;
    public float tiltSharpness = 6f;

    [Header("Boost Kickback")]
    public float boostBackAmount = 0.8f;
    public float boostSharpness = 8f;

    private float baseX;
    private float baseY;
    private Vector3 baseLocalPos;

    private float currentTilt;
    private float currentBackOffset;

    void Start()
    {
        cam = GetComponent<Camera>();
        kart = GetComponentInParent<KartController>();

        baseX = transform.localEulerAngles.x;
        baseY = transform.localEulerAngles.y;
        baseLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (kart == null) return;

        HandleFOV();
        HandleTilt();
        HandleBoostKick();
    }

    void HandleFOV()
    {
        float speedPercent = Mathf.InverseLerp(0f, kart.maxSpeed, Mathf.Abs(kart.CurrentSpeed));

        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedPercent);

        if (kart.IsBoosting)
            targetFOV += boostExtraFOV;

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            1f - Mathf.Exp(-fovSharpness * Time.deltaTime)
        );
    }

    void HandleTilt()
    {
        float turnAmount = kart.RB.angularVelocity.y;

        float targetTilt = Mathf.Clamp(
            -turnAmount * 4f,
            -maxTiltAngle,
            maxTiltAngle
        );

        currentTilt = Mathf.Lerp(
            currentTilt,
            targetTilt,
            1f - Mathf.Exp(-tiltSharpness * Time.deltaTime)
        );

        transform.localRotation = Quaternion.Euler(
            baseX,
            baseY,
            currentTilt
        );
    }

    void HandleBoostKick()
    {
        float targetOffset = kart.IsBoosting ? -boostBackAmount : 0f;

        currentBackOffset = Mathf.Lerp(
            currentBackOffset,
            targetOffset,
            1f - Mathf.Exp(-boostSharpness * Time.deltaTime)
        );

        transform.localPosition =
            baseLocalPos + new Vector3(0f, 0f, currentBackOffset);
    }
}