using UnityEngine;
using UnityEngine.UI;

public class KartInventoryUI : MonoBehaviour
{
    [SerializeField] private KartInventory inventory;

    [Header("Item icons")]
    [SerializeField] private Image slot1Icon;
    [SerializeField] private Image slot2Icon;

    [Header("Synergy icon")]
    [SerializeField] private Image synergyIcon;

    private void OnEnable()
    {
        inventory.OnInventoryChanged += Refresh;
        Refresh(inventory.slot1, inventory.slot2);
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= Refresh;
    }

    void Refresh(ItemBase s1, ItemBase s2)
    {
        ApplyItemIcon(slot1Icon, s1);
        ApplyItemIcon(slot2Icon, s2);

       
        ItemSynergy syn = null;
        if (inventory.synergyManager != null)
            syn = inventory.synergyManager.GetSynergy(s1, s2);

        if (syn != null && syn.icon != null)
        {
            synergyIcon.enabled = true;
            synergyIcon.sprite = syn.icon;
        }
        else
        {
            synergyIcon.enabled = false;
            synergyIcon.sprite = null;
        }
    }

    void ApplyItemIcon(Image img, ItemBase item)
    {
        if (item != null && item.icon != null)
        {
            img.enabled = true;
            img.sprite = item.icon;
        }
        else
        {
            img.enabled = false;
            img.sprite = null;
        }
    }
}