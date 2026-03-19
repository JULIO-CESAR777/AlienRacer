using UnityEngine;
using UnityEngine.UI;

public class MainMenuLevels : MonoBehaviour
{
    [Header("Botones de niveles")]
    public Button level1Button;
    public Button level2Button;

    [Header("Opcional - Candados o texto bloqueado")]
    public GameObject level2LockedObject;

    private void Start()
    {
        RefreshButtons();
    }

    public void RefreshButtons()
    {
        bool level1Unlocked = LevelProgressSave.IsLevelUnlocked(1);
        bool level2Unlocked = LevelProgressSave.IsLevelUnlocked(2);

        if (level1Button != null)
            level1Button.interactable = level1Unlocked;

        if (level2Button != null)
            level2Button.interactable = level2Unlocked;

        if (level2LockedObject != null)
            level2LockedObject.SetActive(!level2Unlocked);
    }

    public void ResetAllProgress()
    {
        LevelProgressSave.ResetProgress();
        RefreshButtons();
    }
}