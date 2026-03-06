using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    public float timedDestroy;  
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject, timedDestroy);
        }
    }
}