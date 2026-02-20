using System;
using UnityEngine;

public class CoinObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<KartController>().AddCoin();
            Destroy(gameObject);
        }
    }
}
