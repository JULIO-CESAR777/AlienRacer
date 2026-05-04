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

    [Header("Tutorial")]
    [SerializeField] private bool useTutorialOrder = false;
    [SerializeField] private List<ItemBase> tutorialOrder;
    private int tutorialIndex = 0;

    [Header("UI Roulette")]
    [SerializeField] private ItemRouletteUI rouletteUI;
    [SerializeField] private List<ItemBase> ribbonVisualPool;

    private KartController kart;
    private KartInventory inv;
    private InputManager input;
    private MainManager gm;

    private bool isPaused = false;

    private void Awake()
    {
        kart = GetComponent<KartController>();
        inv = GetComponent<KartInventory>();
    }

    private void Start()
    {
        input = InputManager.GetInstance();

        gm = MainManager.GetInstance();
        if (gm != null)
        {
            gm.onChangeGameState += OnChangeGameStateCallback;

            // Igual que tu lógica general de pausa:
            isPaused = gm.gameState != GameState.Play;
        }
        else
        {
            Debug.LogError("MainManager is NULL in CoinGachaBuyer");
        }
    }

    private void OnDestroy()
    {
        if (gm != null)
        {
            gm.onChangeGameState -= OnChangeGameStateCallback;
        }
    }

    public void OnChangeGameStateCallback(GameState newState)
    {
        isPaused = newState != GameState.Play;
    }

    private void Update()
    {
        if (isPaused) return;
        if (rouletteUI != null && rouletteUI.IsSpinning) return;
        if (input == null) return;

        if (input.IsButtonDown(BUTTONS.X))
        {
            TryBuy();
        }
    }

    private void TryBuy()
    {
        if (isPaused) return;
        if (lootTable == null || rouletteUI == null) return;
        if (kart == null || inv == null) return;

        if (!kart.TrySpendCoins(coinCost))
            return;

        ItemBase result = GetTutorialItem();

        if (result == null)
        {
            float c = 60f, u = 25f, r = 10f, e = 4f, l = 1f;

            if (usePositionWeights && positionConfig != null && GestorPosiciones.Instancia != null)
            {
                int total = Mathf.Max(1, GestorPosiciones.Instancia.ObtenerTotalCorredores());
                int posActual = GestorPosiciones.Instancia.ObtenerPosicionDe(transform);
                int pos = Mathf.Clamp(posActual == 0 ? 1 : posActual, 1, total);

                positionConfig.GetWeights(pos, total, out c, out u, out r, out e, out l);
            }

            result = lootTable.RollWithRarityWeights(c, u, r, e, l);
        }

        if (result == null) return;

        List<ItemBase> visualPool = (ribbonVisualPool != null && ribbonVisualPool.Count > 0)
            ? ribbonVisualPool
            : LootTableToItemList(lootTable);

        rouletteUI.Spin(result, visualPool, (finalItem) =>
        {
            inv.TryAddItem(finalItem);
        });
    }

    private ItemBase GetTutorialItem()
    {
        if (!useTutorialOrder) return null;
        if (tutorialOrder == null || tutorialOrder.Count == 0) return null;
        if (tutorialIndex >= tutorialOrder.Count) return null;

        ItemBase item = tutorialOrder[tutorialIndex];
        tutorialIndex++;

        return item;
    }

    private List<ItemBase> LootTableToItemList(LootTable table)
    {
        List<ItemBase> list = new List<ItemBase>();

        foreach (var e in table.entries)
        {
            if (e.item != null && !list.Contains(e.item))
                list.Add(e.item);
        }

        return list;
    }
}