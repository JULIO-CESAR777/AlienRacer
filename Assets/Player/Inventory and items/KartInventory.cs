using UnityEngine;

public class KartInventory : MonoBehaviour
{
    public ItemBase slot1;
    public ItemBase slot2;

    private KartController kart;
    public ItemSynergyManager synergyManager;

    private void Awake()
    {
        kart = GetComponent<KartController>();
    }

    void Update()
    {
        if (InputManager.GetInstance().IsButtonDown(BUTTONS.L1)) UseSlot(1);
        if (InputManager.GetInstance().IsButtonDown(BUTTONS.R1)) UseSlot(2);

        if (InputManager.GetInstance().IsButtonDown(BUTTONS.Y))
            TryUseSynergy();
    }

    void TryUseSynergy()
    {
        print("sinergia");
        if (!slot1 || !slot2) return;

        if (synergyManager.TryExecuteSynergy(slot1, slot2, kart))
        {
            slot1 = null;
            slot2 = null;
        }
    }

    void UseSlot(int slotIndex)
    {
        ItemBase item = slotIndex == 1 ? slot1 : slot2;
        if (!item) return;

        item.Use(kart);
        // Decide si se consume:
        // if(slotIndex==1) slot1=null; else slot2=null;
    }
}