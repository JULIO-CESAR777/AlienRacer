using UnityEngine;

public class CameraSpringArm : MonoBehaviour
{
    public Transform target; // el coche
    public Transform cameraTransform;

    [Header("Distance")]
    public float maxDistance = 6.5f;
    public float minDistance = 1f;

    [Header("Collision")]
    public float sphereRadius = 0.3f;
    public LayerMask collisionMask;

    [Header("Smooth")]
    public float smoothSpeed = 10f;

    private float currentDistance;

    void Start()
    {
        currentDistance = maxDistance;
    }

    void LateUpdate()
    {
        if (target == null || cameraTransform == null) return;

        Vector3 origin = target.position + Vector3.up * 3.5f;

        // dirección REAL basada en el pivot
        Vector3 direction = -target.forward;
        direction.y = 0f;
        direction.Normalize();

        float targetDistance = maxDistance;

        if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, maxDistance, collisionMask))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
            Debug.Log("Hit: " + hit.collider.name);
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

        Vector3 finalPosition = origin + direction * currentDistance;

        transform.position = finalPosition;
        transform.LookAt(origin);

        Debug.DrawLine(origin, origin + direction * maxDistance, Color.red);
    }
}
