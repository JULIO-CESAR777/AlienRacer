using System;
using UnityEngine;

public class KartInventory : MonoBehaviour
{
    public ItemBase slot1;
    public ItemBase slot2;

    private KartController kart;
    public ItemSynergyManager synergyManager;

    private InputManager input;

    
    public event Action<ItemBase, ItemBase> OnInventoryChanged;

    private void Awake()
    {
        kart = GetComponent<KartController>();
    }

    private void Start()
    {
        input = InputManager.GetInstance();
        NotifyChanged(); // refresca UI al iniciar
    }

    void Update()
    {
        if (input.IsButtonDown(BUTTONS.L1)) UseSlot(1);
        if (input.IsButtonDown(BUTTONS.R1)) UseSlot(2);

        if (InputManager.GetInstance().IsButtonDown(BUTTONS.Y))
            TryUseSynergy();
    }

    void TryUseSynergy()
    {
        if (!slot1 || !slot2) return;

        if (synergyManager != null && synergyManager.TryExecuteSynergy(slot1, slot2, kart))
        {
            slot1 = null;
            slot2 = null;
            NotifyChanged();
        }
    }

    void UseSlot(int slotIndex)
    {
        ItemBase item = slotIndex == 1 ? slot1 : slot2;
        if (!item) return;

        item.Use(kart);

        // consume
        if (slotIndex == 1) slot1 = null;
        else slot2 = null;

        NotifyChanged();
    }

    public bool TryAddItem(ItemBase item)
    {
        if (item == null) return false;

        if (slot1 == null) { slot1 = item; NotifyChanged(); return true; }
        if (slot2 == null) { slot2 = item; NotifyChanged(); return true; }

        // reemplaza slot1
        slot1 = item;
        NotifyChanged();
        return true;
    }

    private void NotifyChanged()
    {
        OnInventoryChanged?.Invoke(slot1, slot2);
    }
}