using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinGachaBuyer : MonoBehaviour
{
    [Header("Cost")]
    [SerializeField] private int coinCost = 5;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    [SerializeField] private PositionBasedLootConfig positionConfig;
    [SerializeField] private bool usePositionWeights = true;

    [Header("UI Roulette")]
    [SerializeField] private ItemRouletteUI rouletteUI;
    [SerializeField] private List<ItemBase> ribbonVisualPool;
    

    private KartController kart;
    private KartInventory inv;
    private InputManager input;

    private void Awake()
    {
        kart = GetComponent<KartController>();
        inv = GetComponent<KartInventory>();
    }

    private void Start()
    {
        input = InputManager.GetInstance();
    }

    private void Update()
    {
        if (rouletteUI != null && rouletteUI.IsSpinning) return;

        if (input.IsButtonDown(BUTTONS.X))
        {
            TryBuy();
        }
    }

    private void TryBuy()
    {
        if (lootTable == null || rouletteUI == null) return;


        if (!kart.TrySpendCoins(coinCost))
            return;


        float c = 60, u = 25, r = 10, e = 4, l = 1;

        // MODIFICACIÓN: Ahora le pedimos la posición al GestorPosiciones.Instancia directamente
        if (usePositionWeights && positionConfig != null && GestorPosiciones.Instancia != null)
        {
            // Obtenemos el total y la posición desde el Singleton usando nuestro transform
            int total = Mathf.Max(1, GestorPosiciones.Instancia.ObtenerTotalCorredores());
            int posActual = GestorPosiciones.Instancia.ObtenerPosicionDe(transform);

            // Si por alguna razón no está en la lista (pos 0), lo tratamos como posición 1
            int pos = Mathf.Clamp(posActual == 0 ? 1 : posActual, 1, total);

            positionConfig.GetWeights(pos, total, out c, out u, out r, out e, out l);
        }


        ItemBase result = lootTable.RollWithRarityWeights(c, u, r, e, l);
        if (result == null) return;

        // pool visual
        List<ItemBase> visualPool = (ribbonVisualPool != null && ribbonVisualPool.Count > 0)
            ? ribbonVisualPool
            : LootTableToItemList(lootTable);

        // lo gira y da el item
        rouletteUI.Spin(result, visualPool, (finalItem) =>
        {
            inv.TryAddItem(finalItem);
        });
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