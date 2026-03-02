using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Items/Shield")]
public class ShieldItem : ItemBase
{
 
    public float duration = 5f;
    public override void Use(KartController user)
    {
        
        user.GetComponent<KartPowerUpController>().ActivateShield(duration);
    }
}
