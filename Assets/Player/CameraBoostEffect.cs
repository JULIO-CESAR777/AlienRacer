using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraBoostEffect : MonoBehaviour
{
    private CinemachineCamera cam;
    private CinemachineFollow follow;

    private Vector3 originalOffset;
    private float originalFOV;

    private Coroutine transitionRoutine;

    [Header("Boost Settings")]
    public float boostFOVIncrease = 10f;
    public float boostZOffset = -2f;
    public float boostYOffset = -0.5f;
    public float transitionSpeed = 6f;

    void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        follow = GetComponent<CinemachineFollow>();

        originalOffset = follow.FollowOffset;
        originalFOV = cam.Lens.FieldOfView;
    }

    public void OnBoostStart()
    {
        StartTransition(true);
    }

    public void OnBoostEnd()
    {
        StartTransition(false);
    }

    private void StartTransition(bool boosting)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(SmoothTransition(boosting));
    }

    private IEnumerator SmoothTransition(bool boosting)
    {
        Vector3 targetOffset = boosting
            ? new Vector3(
                originalOffset.x,
                originalOffset.y + boostYOffset,
                originalOffset.z + boostZOffset)
            : originalOffset;

        float targetFOV = boosting
            ? originalFOV + boostFOVIncrease
            : originalFOV;

        while (Vector3.Distance(follow.FollowOffset, targetOffset) > 0.01f ||
               Mathf.Abs(cam.Lens.FieldOfView - targetFOV) > 0.01f)
        {
            follow.FollowOffset = Vector3.Lerp(
                follow.FollowOffset,
                targetOffset,
                Time.deltaTime * transitionSpeed);

            cam.Lens.FieldOfView = Mathf.Lerp(
                cam.Lens.FieldOfView,
                targetFOV,
                Time.deltaTime * transitionSpeed);

            yield return null;
        }

        follow.FollowOffset = targetOffset;
        cam.Lens.FieldOfView = targetFOV;
    }
}