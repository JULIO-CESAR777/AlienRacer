using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotButtonHandler : MonoBehaviour
{
    public void SelectSlot(int slotIndex)
    {
        SlotSaveSystem.SetActiveSlot(slotIndex);

        if (!SlotSaveSystem.SlotHasData(slotIndex))
        {
            SlotSaveSystem.CreateNewSlot(slotIndex);
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
        SlotSaveSystem.DeleteSlot(slotIndex);
    }
}