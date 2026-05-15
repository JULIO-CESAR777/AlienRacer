using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    public float timedDestroy;  
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bot"))
        {
            Destroy(gameObject, timedDestroy);
        }
    }
}