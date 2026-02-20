using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Items/Oil Barrel")]
public class OilBarrelItem : ItemBase
{
    public GameObject oilPrefab;
    public override void Use(KartController user)
    {
        Instantiate(oilPrefab, user.transform.position - user.transform.forward * 2f, Quaternion.identity);
    }
}
