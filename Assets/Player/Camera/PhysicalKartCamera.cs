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
    [SerializeField] private float topHeight = 8f;
    [SerializeField] private float topBackOffset = 1.3f;
    [SerializeField] private float topViewAngle = 75f;
    [Range(0f, 1f)]
    [SerializeField] private float maxTopBlend = 0.55f;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float sphereRadius = 0.45f;
    [SerializeField] private float wallPadding = 0.25f;
    [SerializeField] private float minDistance = 1.4f;
    [SerializeField] private string[] ignoredTags;

    [Header("Blend Control")]
    [SerializeField] private float topBlendStartDistance = 3.5f;
    [SerializeField] private float topBlendFullDistance = 1.2f;

    [Header("Smooth")]
    [SerializeField] private float positionSharpness = 10f;
    [SerializeField] private float rotationSharpness = 12f;
    [SerializeField] private float topBlendSharpness = 4f;

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

        Vector3 normalDesiredPosition =
            target.position
            - target.forward * normalDistance
            + Vector3.up * normalHeight;

        CameraCollisionResult normalCollision =
            ResolveCameraCollision(lookTarget, normalDesiredPosition);

        float wantedTopBlend = CalculateTopBlend(normalCollision.finalDistance);

        topBlend = Mathf.Lerp(
            topBlend,
            wantedTopBlend,
            1f - Mathf.Exp(-topBlendSharpness * Time.deltaTime)
        );

        Vector3 topDesiredPosition =
            target.position
            - target.forward * topBackOffset
            + Vector3.up * topHeight;

        Vector3 desiredBlendedPosition =
            Vector3.Lerp(normalCollision.safePosition, topDesiredPosition, topBlend);

        CameraCollisionResult finalCollision =
            ResolveCameraCollision(lookTarget, desiredBlendedPosition);

        transform.position = Vector3.Lerp(
            transform.position,
            finalCollision.safePosition,
            1f - Mathf.Exp(-positionSharpness * Time.deltaTime)
        );

        Quaternion normalRotation = Quaternion.LookRotation(lookTarget - normalCollision.safePosition, Vector3.up);

        Quaternion topRotation = Quaternion.Euler(
            topViewAngle,
            target.eulerAngles.y,
            0f
        );

        Quaternion desiredRotation = Quaternion.Slerp(normalRotation, topRotation, topBlend);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            1f - Mathf.Exp(-rotationSharpness * Time.deltaTime)
        );

        HandleFOV();
    }

    private CameraCollisionResult ResolveCameraCollision(Vector3 lookTarget, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - lookTarget;
        float desiredDistance = direction.magnitude;

        if (desiredDistance <= 0.01f)
        {
            return new CameraCollisionResult
            {
                safePosition = desiredPosition,
                finalDistance = 0f,
                hitSomething = false
            };
        }

        direction.Normalize();

        RaycastHit[] hits = Physics.SphereCastAll(
            lookTarget,
            sphereRadius,
            direction,
            desiredDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            if (!IsValidObstacle(hits[i].collider)) continue;

            if (hits[i].distance < closestDistance)
                closestDistance = hits[i].distance;
        }

        if (closestDistance == Mathf.Infinity)
        {
            return new CameraCollisionResult
            {
                safePosition = desiredPosition,
                finalDistance = desiredDistance,
                hitSomething = false
            };
        }

        float safeDistance = Mathf.Clamp(
            closestDistance - wallPadding,
            minDistance,
            desiredDistance
        );

        return new CameraCollisionResult
        {
            safePosition = lookTarget + direction * safeDistance,
            finalDistance = safeDistance,
            hitSomething = true
        };
    }

    private float CalculateTopBlend(float safeDistance)
    {
        float blend = Mathf.InverseLerp(
            topBlendStartDistance,
            topBlendFullDistance,
            safeDistance
        );

        blend = Mathf.Clamp01(blend);
        return blend * maxTopBlend;
    }

    private bool IsValidObstacle(Collider col)
    {
        if (col == null) return false;

        if (col.transform == target || col.transform.IsChildOf(target))
            return false;

        for (int i = 0; i < ignoredTags.Length; i++)
        {
            if (!string.IsNullOrEmpty(ignoredTags[i]) && col.CompareTag(ignoredTags[i]))
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

    private struct CameraCollisionResult
    {
        public Vector3 safePosition;
        public float finalDistance;
        public bool hitSomething;
    }
}