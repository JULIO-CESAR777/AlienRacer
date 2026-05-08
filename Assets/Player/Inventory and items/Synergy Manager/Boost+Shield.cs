using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Synergies/Star Synergy")]
public class StarSynergy : ItemSynergy
{
    public float stunduration = 1.5f;
    public float boostmultiplier = 1.2f;
    public float duration = 5f;
    
    

    public override void Execute(KartController user)
    {
        
        KartPowerUpController kart = user.GetComponent<KartPowerUpController>();
        
        kart.ActivateStar(duration, stunduration);
    
        kart.ApplyBoost(boostmultiplier, duration);
       
    }
}