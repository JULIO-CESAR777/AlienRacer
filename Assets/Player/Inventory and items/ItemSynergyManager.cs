using System.Collections.Generic;
using UnityEngine;

public class ItemSynergyManager : MonoBehaviour
{
    public List<ItemSynergy> synergies;
    private Dictionary<string, ItemSynergy> map;

    private void Awake()
    {
        map = new Dictionary<string, ItemSynergy>(synergies.Count);

        foreach (var s in synergies)
        {
            if (s == null || s.itemA == null || s.itemB == null) continue;

            string key = MakeKey(s.itemA, s.itemB);

            if (map.ContainsKey(key))
                Debug.LogWarning($"Synergy duplicada: {s.itemA.name} + {s.itemB.name}");

            map[key] = s;
        }
    }

    private string MakeKey(ItemBase a, ItemBase b)
    {
        // ordena por nombre 
        return string.CompareOrdinal(a.name, b.name) <= 0
            ? $"{a.name}|{b.name}"
            : $"{b.name}|{a.name}";
    }

    public bool TryExecuteSynergy(ItemBase slot1, ItemBase slot2, KartController user)
    {
        if (slot1 == null || slot2 == null) return false;

        string key = MakeKey(slot1, slot2);

        if (map.TryGetValue(key, out var synergy))
        {
            synergy.Execute(user);
            return true;
        }

        return false;
    }
    
    
    public ItemSynergy GetSynergy(ItemBase slot1, ItemBase slot2)
    {
        if (slot1 == null || slot2 == null) return null;

        string key = MakeKey(slot1, slot2);

        if (map != null && map.TryGetValue(key, out var synergy))
            return synergy;

        return null;
    }
}