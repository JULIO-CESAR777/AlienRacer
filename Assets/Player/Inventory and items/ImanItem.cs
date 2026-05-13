using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Items/Iman")]
public class ImanTiem : ItemBase
{
    [Header("Raycast")]
    public float distanciaRayo = 35f;

    [Header("Ralentización")]
    public float duracionRalentizacion = 3f;
    [Range(0.1f, 1f)] public float multiplicadorRalentizacion = 0.45f;

    [Header("Visual")]
    public Color colorRayo = Color.cyan;
    public float anchoRayoFinal = 0.18f;
    public float tiempoVisibleRayoFinal = 0.18f;

 
    public override void Use(KartController user)
    {
        KartPowerUpController powerUp = user.GetComponent<KartPowerUpController>();

        if (powerUp == null)
        {
            Debug.LogWarning("No se encontró KartPowerUpController.");
            return;
        }

        powerUp.DispararRayoRalentizador(
            distanciaRayo,
            duracionRalentizacion,
            multiplicadorRalentizacion,
            colorRayo,
            anchoRayoFinal,
            tiempoVisibleRayoFinal
        );
    }
}
