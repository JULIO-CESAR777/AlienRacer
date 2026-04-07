using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    public ParticleSystem monedaParticles;
    public GameObject visualModel;
    private bool yaRecogida = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !yaRecogida)
        {
            yaRecogida = true; 
            Recolectar();
        }
    }

    void Recolectar()
    {
        if (monedaParticles != null)
        {
            monedaParticles.Play();
        }
        if (visualModel != null) visualModel.SetActive(false);
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 2.0f);
    }
}