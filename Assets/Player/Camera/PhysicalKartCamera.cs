using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PhysicalKartCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private KartController kart;

    [Header("Normal View")]
    [SerializeField] private float normalDistance = 6.5f;
    [SerializeField] private float normalHeight = 3.2f;
    [SerializeField] private float lookAtHeight = 1.4f;

    [Header("Top View")]
    [SerializeField] private float topHeight = 12f;
    [SerializeField] private float topBackOffset = 1.5f;
    [SerializeField] private float topViewAngle = 80f;

    [Header("Collision Detection")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float sphereRadius = 0.45f;
    [SerializeField] private string[] ignoredTags;

    [Header("Smooth")]
    [SerializeField] private float positionSharpness = 8f;
    [SerializeField] private float rotationSharpness = 10f;
    [SerializeField] private float topBlendSharpness = 6f;

    [Header("FOV")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float topFOV = 70f;
    [SerializeField] private float maxSpeedFOV = 85f;
    [SerializeField] private float fovSharpness = 5f;

    private Camera cam;
    private float topBlend;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (kart == null && target != null)
            kart = target.GetComponent<KartController>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 lookTarget = target.position + Vector3.up * lookAtHeight;

        Vector3 normalPosition =
            target.position
            - target.forward * normalDistance
            + Vector3.up * normalHeight;

        Vector3 topPosition =
            target.position
            - target.forward * topBackOffset
            + Vector3.up * topHeight;

        bool obstacleDetected = HasObstacleBetween(normalPosition, lookTarget);

        float targetBlend = obstacleDetected ? 1f : 0f;

        topBlend = Mathf.Lerp(
            topBlend,
            targetBlend,
            1f - Mathf.Exp(-topBlendSharpness * Time.deltaTime)
        );

        Vector3 finalPosition = Vector3.Lerp(normalPosition, topPosition, topBlend);

        transform.position = Vector3.Lerp(
            transform.position,
            finalPosition,
            1f - Mathf.Exp(-positionSharpness * Time.deltaTime)
        );

        Quaternion normalRotation = Quaternion.LookRotation(lookTarget - normalPosition, Vector3.up);

        Quaternion topRotation = Quaternion.Euler(
            topViewAngle,
            target.eulerAngles.y,
            0f
        );

        Quaternion finalRotation = Quaternion.Slerp(normalRotation, topRotation, topBlend);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            finalRotation,
            1f - Mathf.Exp(-rotationSharpness * Time.deltaTime)
        );

        HandleFOV();
    }

    private bool HasObstacleBetween(Vector3 cameraPosition, Vector3 lookTarget)
    {
        Vector3 direction = cameraPosition - lookTarget;
        float distance = direction.magnitude;

        if (distance <= 0.01f) return false;

        direction.Normalize();

        RaycastHit[] hits = Physics.SphereCastAll(
            lookTarget,
            sphereRadius,
            direction,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            if (IsValidObstacle(hits[i].collider))
                return true;
        }

        return false;
    }

    private bool IsValidObstacle(Collider col)
    {
        if (col == null) return false;

        if (col.transform == target || col.transform.IsChildOf(target))
            return false;

        for (int i = 0; i < ignoredTags.Length; i++)
        {
            if (col.CompareTag(ignoredTags[i]))
                return false;
        }

        return true;
    }

    private void HandleFOV()
    {
        float speedPercent = 0f;

        if (kart != null)
            speedPercent = Mathf.InverseLerp(0f, kart.maxSpeed, Mathf.Abs(kart.currentSpeed));

        float speedFOV = Mathf.Lerp(normalFOV, maxSpeedFOV, speedPercent);
        float targetFOV = Mathf.Lerp(speedFOV, topFOV, topBlend);

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            1f - Mathf.Exp(-fovSharpness * Time.deltaTime)
        );
    }
}
