using UnityEngine;
using System.Collections;

public class OilSpillAppear : MonoBehaviour
{
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float startSize = 0.05f;

    private Vector3 finalScale;

    private void Start()
    {
        finalScale = transform.localScale;

        transform.localScale = new Vector3(
            finalScale.x * startSize,
            finalScale.y,
            finalScale.z * startSize
        );

        StartCoroutine(AppearOil());
    }

    private IEnumerator AppearOil()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = new Vector3(
                Mathf.Lerp(finalScale.x * startSize, finalScale.x, t),
                finalScale.y,
                Mathf.Lerp(finalScale.z * startSize, finalScale.z, t)
            );

            yield return null;
        }

        transform.localScale = finalScale;
    }
}