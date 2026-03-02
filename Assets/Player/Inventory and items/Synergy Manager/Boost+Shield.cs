using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Synergies/Star Synergy")]
public class StarSynergy : ItemSynergy
{
    public float boostMultiplier = 1.5f;
    public float duration = 5f;
    
    

    public override void Execute(KartController user)
    {
        user.GetComponent<KartPowerUpController>().ActivateStar(boostMultiplier, duration);
    
        
       
    }
}