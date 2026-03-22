using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotButtonHandler : MonoBehaviour
{
    [Header("Nombres exactos de escenas")]
    [SerializeField] private string level1SceneName;
    [SerializeField] private string level2SceneName;

    public void SelectSlot(int slotIndex)
    {
        LevelProgressSave.SetActiveSlot(slotIndex);

        int nextLevel = LevelProgressSave.GetNextLevelToPlay();

        switch (nextLevel)
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
        LevelProgressSave.ResetSlot(slotIndex);
    }
}