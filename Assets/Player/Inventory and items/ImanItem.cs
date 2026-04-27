using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Items/Iman")]
public class ImanTiem : ItemBase
{
    [Header("Raycast")]
    public float distanciaRayo = 35f;

    [Header("Ralentización")]
    public float duracionRalentizacion = 3f;

    [Range(0.1f, 1f)]
    public float multiplicadorRalentizacion = 0.45f;

    [Header("Visual")]
    public Color colorRayo = Color.cyan;
    public float anchoRayo = 0.18f;
    public float tiempoVisible = 0.18f;

    public override void Use(KartController user)
    {
        KartPowerUpController powerUpController = user.GetComponent<KartPowerUpController>();

        if (powerUpController == null)
        {
            Debug.LogWarning("El kart no tiene KartPowerUpController.");
            return;
        }

        powerUpController.DispararRayoRalentizador(
            distanciaRayo,
            duracionRalentizacion,
            multiplicadorRalentizacion,
            colorRayo,
            anchoRayo,
            tiempoVisible
        );
    }
}
