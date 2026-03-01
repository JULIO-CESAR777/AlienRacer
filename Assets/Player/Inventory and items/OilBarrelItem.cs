using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Items/Oil Barrel")]
public class OilBarrelItem : ItemBase
{
    public GameObject oilPrefab;
    public override void Use(KartController user)
    {
        KartPowerUpController powerUp = user.GetComponent<KartPowerUpController>();
        if (powerUp == null) return;

        Transform spawnPoint = powerUp.behindSpawnPoint;
        if (spawnPoint == null) return;

        Object.Instantiate(oilPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
