using System.Collections.Generic;
using UnityEngine;


public class ItemSynergyManager : MonoBehaviour
{
    public List<ItemSynergy> synergies;

    public bool TryExecuteSynergy(
        ItemBase slot1,
        ItemBase slot2,
        KartController user)
    {
        foreach (var synergy in synergies)
        {
            if (synergy.CanSynergize(slot1, slot2))
            {
                synergy.Execute(user);
                return true;
            }
        }

        return false;
    }
}
