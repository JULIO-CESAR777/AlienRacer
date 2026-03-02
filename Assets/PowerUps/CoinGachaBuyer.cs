using System.Collections.Generic;
using UnityEngine;


public class CoinGachaBuyer : MonoBehaviour
{
    [Header("Cost")]
    [SerializeField] private int coinCost = 5;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    [SerializeField] private PositionBasedLootConfig positionConfig; // opcional
    [SerializeField] private bool usePositionWeights = true;

    [Header("UI Roulette")]
    [SerializeField] private ItemRouletteUI rouletteUI;
    [SerializeField] private List<ItemBase> ribbonVisualPool; // opcional (si no, usa lootTable entries)

    [Header("Input")]
    [SerializeField] private BUTTONS buyButton = BUTTONS.X; // cambia al que quieras

    private KartController kart;
    private KartInventory inv;
    private ParticipanteCarrera participante;

    private void Awake()
    {
        kart = GetComponent<KartController>();
        inv = GetComponent<KartInventory>();
        participante = GetComponent<ParticipanteCarrera>(); // si lo tienes
    }

    private void Update()
    {
        if (rouletteUI != null && rouletteUI.IsSpinning) return;

        if (InputManager.GetInstance().IsButtonDown(buyButton))
        {
            TryBuy();
        }
    }

    private void TryBuy()
    {
        if (lootTable == null || rouletteUI == null) return;

        // 1) Cobrar
        if (!kart.TrySpendCoins(coinCost))
        {
            // Debug.Log("No tienes coins suficientes");
            return;
        }

        // 2) Calcular pesos (por posición o default)
        float c = 60, u = 25, r = 10, e = 4, l = 1;

        if (usePositionWeights && positionConfig != null && participante != null && GestorPosiciones.instancia != null)
        {
            int total = Mathf.Max(1, GestorPosiciones.instancia.participantes.Count);
            int pos = Mathf.Clamp(participante.posicionActual, 1, total);

            positionConfig.GetWeights(pos, total, out c, out u, out r, out e, out l);
        }

        // 3) Roll
        ItemBase result = lootTable.RollWithRarityWeights(c, u, r, e, l);
        if (result == null) return;

        // 4) Pool visual para la cinta
        List<ItemBase> visualPool = (ribbonVisualPool != null && ribbonVisualPool.Count > 0)
            ? ribbonVisualPool
            : LootTableToItemList(lootTable);

        // 5) Animación + dar item
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