using UnityEngine;


public class OilSlick : MonoBehaviour
{
    [Header("Effect")]
    [Range(0.1f, 1f)]
    public float steeringMultiplierInOil = 0.4f;

    private void OnTriggerEnter(Collider other)
    {
        KartController kart = other.GetComponentInParent<KartController>();
        if (kart == null) return;

        kart.SetSteeringMultiplier(steeringMultiplierInOil);
    }

    private void OnTriggerExit(Collider other)
    {
        KartController kart = other.GetComponentInParent<KartController>();
        if (kart == null) return;

        kart.SetSteeringMultiplier(1f);
    }
}