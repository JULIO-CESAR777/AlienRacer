using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Synergies/TPSinergy")]
public class TPSinergy : ItemSynergy
{
    [Header("Raycast")]
    public float distanciaRayo = 35f;

    [Header("Visual")]
    public Color colorRayo = Color.magenta;
    public float anchoFinalRayo = 0.18f;
    public float tiempoVisibleRayoFinal = 0.18f;

    public override void Execute(KartController user)
    {
        KartPowerUpController powerUp = user.GetComponent<KartPowerUpController>();

        if (powerUp == null)
        {
            Debug.LogWarning("No se encontró KartPowerUpController en el usuario.");
            return;
        }

        powerUp.DispararRayoIntercambio(
            distanciaRayo,
            colorRayo,
            anchoFinalRayo,
            tiempoVisibleRayoFinal
        );
    }
}