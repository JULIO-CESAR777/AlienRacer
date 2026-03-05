using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Synergies/Boost Synergy")]
public class BoostSynergy : ItemSynergy
{
    public float boostMultiplier = 1.5f;
    public float duration = 2f;
    
    public int coins = 0;

    public override void Execute(KartController user)
    {
        user.GetComponent<KartPowerUpController>().ApplyBoost(boostMultiplier, duration);
    

        for (int i = 0; i < coins; i++)
        {
            user.AddCoin();
            
        }
    }
}