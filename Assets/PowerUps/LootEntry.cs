using System;
using UnityEngine;

[Serializable]
public class LootEntry
{
    public ItemBase item;
    public ItemRarity rarity = ItemRarity.Common;

    [Min(0f)]
    public float weight = 1f; // peso dentro de su rareza
}