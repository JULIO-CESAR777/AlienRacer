using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Items/Boost")]
public class BoostItem : ItemBase
{
    public float boostForce = 25f;
    public float duration = 2f;

    public override void Use(KartController user)
    {
        
        
        user.GetComponent<KartPowerUpController>().ApplyBoost(boostForce, duration);
    }
}
