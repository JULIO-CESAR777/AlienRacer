using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotButtonHandler : MonoBehaviour
{
    [SerializeField] private string level1SceneName;
    [SerializeField] private string level2SceneName;

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
                SceneManager.LoadScene(level1SceneName);
                break;

            case 2:
                SceneManager.LoadScene(level2SceneName);
                break;

            default:
                SceneManager.LoadScene(level1SceneName);
                break;
        }
    }

    public void DeleteSlot(int slotIndex)
    {
        SlotSaveSystem.DeleteSlot(slotIndex);
    }
}