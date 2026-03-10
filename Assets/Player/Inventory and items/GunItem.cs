using UnityEngine;



[CreateAssetMenu(menuName = "Kart/Items/GunItem")]
public class GunItem : ItemBase
{
    public GameObject gunPrefab;
    public override void Use(KartController user)
    {
        KartPowerUpController powerUp = user.GetComponent<KartPowerUpController>();
        if (powerUp == null) return;

        Transform spawnPoint = powerUp.shootPoint;
        if (spawnPoint == null) return;

        GameObject bulletObj = Instantiate(gunPrefab, spawnPoint.position, spawnPoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        
        bullet.Initialize(spawnPoint.forward);
        
    }
}