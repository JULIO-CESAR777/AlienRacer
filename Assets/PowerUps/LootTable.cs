using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Loot/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<LootEntry> entries = new List<LootEntry>();

    public ItemBase RollWithRarityWeights(float commonW, float uncommonW, float rareW, float epicW, float legendaryW)
    {
        if (entries == null || entries.Count == 0) return null;

        ItemRarity chosen = RollRarity(commonW, uncommonW, rareW, epicW, legendaryW);

        // pool por rareza
        List<LootEntry> pool = entries.FindAll(e => e.item != null && e.rarity == chosen);
        if (pool.Count == 0)
            pool = entries.FindAll(e => e.item != null);

        return RollFromPool(pool);
    }

    private ItemRarity RollRarity(float c, float u, float r, float e, float l)
    {
        float total = c + u + r + e + l;
        if (total <= 0f) return ItemRarity.Common;

        float x = Random.Range(0f, total);

        if (x < c) return ItemRarity.Common; x -= c;
        if (x < u) return ItemRarity.Uncommon; x -= u;
        if (x < r) return ItemRarity.Rare; x -= r;
        if (x < e) return ItemRarity.Epic;

        return ItemRarity.Legendary;
    }

    private ItemBase RollFromPool(List<LootEntry> pool)
    {
        float total = 0f;
        for (int i = 0; i < pool.Count; i++)
            total += Mathf.Max(0f, pool[i].weight);

        if (total <= 0f)
            return pool[Random.Range(0, pool.Count)].item;

        float x = Random.Range(0f, total);
        for (int i = 0; i < pool.Count; i++)
        {
            float w = Mathf.Max(0f, pool[i].weight);
            if (x < w) return pool[i].item;
            x -= w;
        }

        return pool[pool.Count - 1].item;
    }
}