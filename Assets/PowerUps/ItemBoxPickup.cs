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

        // MODIFICACIÓN: Ya no buscamos el componente ParticipanteCarrera.
        if (!other.CompareTag("Player") && !other.CompareTag("Bot")) return;

        if (!lootTable || !positionConfig || !rouletteUI) return;
        if (rouletteUI.IsSpinning) return;

        // MODIFICACIÓN: Obtenemos el total y la posición desde el Singleton 'Instancia'
        int total = 1;
        int pos = 1;

        if (GestorPosiciones.Instancia != null)
        {
            total = GestorPosiciones.Instancia.ObtenerTotalCorredores();
            // Usamos transform.root o GetComponentInParent para asegurar que pasamos el objeto raíz registrado
            pos = GestorPosiciones.Instancia.ObtenerPosicionDe(other.transform.root);
            if (pos <= 0) pos = 1;
        }
        positionConfig.GetWeights(pos, total, out float c, out float u, out float r, out float e, out float l);

        ItemBase result = lootTable.RollWithRarityWeights(c, u, r, e, l);
        if (result == null) return;

        List<ItemBase> visualPool = (ribbonVisualPool != null && ribbonVisualPool.Count > 0)
            ? ribbonVisualPool
            : LootTableToItemList(lootTable);

        //  la ruleta 
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