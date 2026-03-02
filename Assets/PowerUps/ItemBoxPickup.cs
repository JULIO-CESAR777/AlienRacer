using System.Collections.Generic;
using UnityEngine;

public class ItemBoxPickup : MonoBehaviour
{
    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    [SerializeField] private PositionBasedLootConfig positionConfig;

    [Header("UI")]
    [SerializeField] private ItemRouletteUI rouletteUI;

    [Header("Visual pool for ribbon (optional)")]
    [SerializeField] private List<ItemBase> ribbonVisualPool;

    private void OnTriggerEnter(Collider other)
    {
        KartInventory inv = other.GetComponentInParent<KartInventory>();
        if (!inv) return;

        ParticipanteCarrera p = other.GetComponentInParent<ParticipanteCarrera>();
        if (!p) return;

        if (!lootTable || !positionConfig || !rouletteUI) return;
        if (rouletteUI.IsSpinning) return;

        int total = (GestorPosiciones.instancia != null) ? GestorPosiciones.instancia.participantes.Count : 1;
        int pos = p.posicionActual;

        positionConfig.GetWeights(pos, total, out float c, out float u, out float r, out float e, out float l);

        ItemBase result = lootTable.RollWithRarityWeights(c, u, r, e, l);
        if (result == null) return;

        // si no pasas pool visual, usa todos los items de la tabla como pool (simple)
        List<ItemBase> visualPool = (ribbonVisualPool != null && ribbonVisualPool.Count > 0)
            ? ribbonVisualPool
            : LootTableToItemList(lootTable);

        rouletteUI.Spin(result, visualPool, (finalItem) =>
        {
            inv.TryAddItem(finalItem);
        });

        gameObject.SetActive(false);
    }

    private List<ItemBase> LootTableToItemList(LootTable table)
    {
        List<ItemBase> list = new List<ItemBase>();
        foreach (var e in table.entries)
            if (e.item != null && !list.Contains(e.item))
                list.Add(e.item);
        return list;
    }
}