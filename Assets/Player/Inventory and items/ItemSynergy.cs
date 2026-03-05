using UnityEngine;

public abstract class ItemSynergy : ScriptableObject
{
    public ItemBase itemA;
    public ItemBase itemB;
        
    public Sprite icon;
    public bool CanSynergize(ItemBase a, ItemBase b)
    {
        return (itemA == a && itemB == b) || (itemA == b && itemB == a);
    }

    public abstract void Execute(KartController user);
}