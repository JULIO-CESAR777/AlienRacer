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

    [Tooltip("El Audio Source de tu Kart dedicado a los SFX")]
    public AudioSource sfxSource;

    [Header("Efectos de Sonido")]
    public AudioClip wrongGacha;

    private bool isPaused = false;

    // =========================================
    // NUEVO
    // =========================================

    private bool jumpPhaseCompleted = false;

    // =========================================

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
        if (isPaused)
            return;

        if (rouletteUI != null && rouletteUI.IsSpinning)
            return;

        if (input == null)
            return;

        if (input.IsButtonDown(BUTTONS.X))
        {
            TryBuy();
        }
    }

    private void TryBuy()
    {
        if (
            isPaused ||
            lootTable == null ||
            rouletteUI == null ||
            kart == null ||
            inv == null ||
            inv.IsFull
        )
        {
            if (sfxSource != null && wrongGacha != null)
            {
                sfxSource.PlayOneShot(wrongGacha);
            }

            return;
        }

        if (!kart.TrySpendCoins(coinCost))
            return;

        // =====================================
        // NUEVA LÓGICA TUTORIAL
        // =====================================

        ItemBase result = GetTutorialItem();

        // =====================================
        // SI YA NO HAY TUTORIAL
        // =====================================

        if (result == null)
        {
            float c = 60f, u = 25f, r = 10f, e = 4f, l = 1f;

            if (
                usePositionWeights &&
                positionConfig != null &&
                GestorPosiciones.Instancia != null
            )
            {
                int total =
                    Mathf.Max(
                        1,
                        GestorPosiciones.Instancia.ObtenerTotalCorredores()
                    );

                int posActual =
                    GestorPosiciones.Instancia.ObtenerPosicionDe(transform);

                int pos =
                    Mathf.Clamp(posActual == 0 ? 1 : posActual, 1, total);

                positionConfig.GetWeights(
                    pos,
                    total,
                    out c,
                    out u,
                    out r,
                    out e,
                    out l
                );
            }

            result = lootTable.RollWithRarityWeights(c, u, r, e, l);

            Debug.Log("[Gacha] Usando pool random.");
        }

        if (result == null)
            return;

        List<ItemBase> visualPool =
            (ribbonVisualPool != null && ribbonVisualPool.Count > 0)
            ? ribbonVisualPool
            : LootTableToItemList(lootTable);

        rouletteUI.Spin(result, visualPool, (finalItem) =>
        {
            inv.TryAddItem(finalItem);
        });
    }

    private ItemBase GetTutorialItem()
    {
        if (!useTutorialOrder)
        {
            Debug.Log("[Gacha] Tutorial Order desactivado.");
            return null;
        }

        if (tutorialOrder == null || tutorialOrder.Count == 0)
        {
            Debug.Log("[Gacha] Tutorial Order vacío.");
            return null;
        }

        // =========================================
        // FASE DEL SALTO
        // =========================================

        if (!jumpPhaseCompleted)
        {
            bool passedJumpZone =
                TutorialManager.instance != null &&
                TutorialManager.instance.IsTutorialCompleted("Monedas10");

            Debug.Log($"[Gacha] ¿Pasó Monedas10?: {passedJumpZone}");

            // TODAVÍA NO PASA EL MURO
            if (!passedJumpZone)
            {
                Debug.Log("[Gacha] Forzando JumpItem");

                return tutorialOrder[0];
            }

            // YA PASÓ EL MURO
            jumpPhaseCompleted = true;

            tutorialIndex = 1;

            Debug.Log(
                "[Gacha] Jump completado. Continuando tutorial normal."
            );
        }

        // =========================================
        // TERMINÓ EL TUTORIAL
        // =========================================

        if (tutorialIndex >= tutorialOrder.Count)
        {
            useTutorialOrder = false;

            Debug.Log(
                "[Gacha] Tutorial terminado. Activando pool random."
            );

            return null;
        }

        // =========================================
        // CONTINUAR ORDEN NORMAL
        // =========================================

        ItemBase item = tutorialOrder[tutorialIndex];

        Debug.Log($"[Gacha] Entregando item: {item.name}");

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

    public void ConfigurarGachaTutorial(
        bool activarTutorial,
        int indiceItem
    )
    {
        useTutorialOrder = activarTutorial;

        if (
            activarTutorial &&
            tutorialOrder != null &&
            indiceItem >= 0 &&
            indiceItem < tutorialOrder.Count
        )
        {
            tutorialIndex = indiceItem;

            Debug.Log(
                $"[Gacha Tutorial] Activado. Ítem actual fijado: {tutorialOrder[tutorialIndex].name}"
            );
        }
        else if (!activarTutorial)
        {
            Debug.Log(
                "[Gacha Tutorial] Desactivado. Usando pool aleatoria normal."
            );
        }
    }
}