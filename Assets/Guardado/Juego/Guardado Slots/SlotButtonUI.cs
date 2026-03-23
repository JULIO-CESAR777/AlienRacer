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

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        SlotSaveData data = SlotSaveSystem.LoadSlot(slotIndex);

        if (!data.used)
        {
            label.text = $"Slot {slotIndex + 1} - Vacío";
            return;
        }

        if (data.lastCompletedLevel >= 2)
        {
            label.text = $"Slot {slotIndex + 1} - Última carrera";
            return;
        }

        label.text = $"Slot {slotIndex + 1} - Carrera {data.nextLevelToPlay}";
    }
}