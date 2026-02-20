using UnityEngine;  

public interface IItem
{
    void Use(KartController user);
}

public abstract class ItemBase : ScriptableObject, IItem
{
    public string itemName;
    public Sprite icon;

    public abstract void Use(KartController user);
}