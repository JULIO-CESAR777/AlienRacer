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
        Debug.Log("Synergy " + itemA.itemName + " " + itemB.itemName + "puede hacer el super boost");
        // Hacer el boost con sinergia (velocidad, duracion, con o sin sinergia)
        //user.StartCoroutine(user.ApplyBoost(specialBoostForce, 2f, true));
    }
    
}
