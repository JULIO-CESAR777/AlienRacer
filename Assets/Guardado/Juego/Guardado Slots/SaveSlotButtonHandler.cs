using UnityEngine;

public class SaveSlotButtonHandler : MonoBehaviour
{
    [Header("UI de los slots")]
    [SerializeField] private SaveSlotUI[] slotUIs;

    public void SelectSlot(int slotIndex)
    {
        SlotSaveSystem.SetActiveSlot(slotIndex);

        if (!SlotSaveSystem.SlotHasData(slotIndex))
        {
            SlotSaveSystem.CreateNewSlot(slotIndex);
            RefreshAllSlots();
        }

        int levelToLoad = SlotSaveSystem.GetLevelToPlay(slotIndex);

        switch (levelToLoad)
        {
            case 1:
                SceneLoader.GetInstance()?.LoadScene(1);
                break;

            case 2:
                SceneLoader.GetInstance()?.LoadScene(2);
                break;

            default:
                SceneLoader.GetInstance()?.LoadScene(1);
                break;
        }
    }

    public void DeleteSlot(int slotIndex)
    {
        Debug.Log($"Entré a DeleteSlot desde: {gameObject.name} | Slot: {slotIndex}");

        if (!SlotSaveSystem.SlotHasData(slotIndex))
        {
            Debug.Log("Este slot está vacío, no se puede borrar.");
            return;
        }

        SlotSaveSystem.DeleteSlot(slotIndex);

        RefreshAllSlots();
    }

    private void RefreshAllSlots()
    {
        

        if (slotUIs == null || slotUIs.Length == 0)
        {
            Debug.LogWarning("slotUIs está vacío. Buscando SaveSlotUI automáticamente en la escena...");

            slotUIs = FindObjectsByType<SaveSlotUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            Debug.Log($"Encontré automáticamente: {slotUIs.Length} SaveSlotUI");
        }

        foreach (SaveSlotUI slotUI in slotUIs)
        {
            if (slotUI != null)
            {
                Debug.Log($"Refrescando: {slotUI.gameObject.name}");
                slotUI.Refresh();
            }
            else
            {
                Debug.LogWarning("Hay un slotUI nulo.");
            }
        }
    }
}