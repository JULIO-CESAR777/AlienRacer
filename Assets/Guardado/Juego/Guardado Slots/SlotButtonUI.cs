using TMPro;
using UnityEngine;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private int slotIndex;
    [SerializeField] private TextMeshProUGUI label;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        SlotSaveData data = SlotSaveSystem.LoadSlot(slotIndex);

        if (!data.used)
        {
            label.text = $"Slot {slotIndex + 1} - Vacío";
        }
        else
        {
            label.text = $"Slot {slotIndex + 1} - Carrera {data.nextLevelToPlay}";
        }
    }
}