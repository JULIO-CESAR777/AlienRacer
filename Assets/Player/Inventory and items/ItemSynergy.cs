using UnityEngine;

[CreateAssetMenu(menuName =  "Kart/Items/Synergy")]
public class ItemSynergy: ScriptableObject, ISynergy
{
    public ItemBase itemA;
    public ItemBase itemB;

    public  float specialBoostForce = 50f;
    
    public bool CanSynergize(IItem a, IItem b)
    {
        return ((itemA == a &&  itemB == b) || 
                (itemA == b && itemB == a));
    }

    public void Execute(KartController user)
    {
        user.StartCoroutine(user.ApplyBoost(specialBoostForce, 3f));
    }
    
}
