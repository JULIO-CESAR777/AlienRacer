using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Nombres de escenas")]
    public string level1SceneName = "Mapa1_V2";
    public string level2SceneName = "Mapa_2";

    public void LoadLevel1()
    {
        if (LevelProgressSave.IsLevelUnlocked(1))
        {
            SceneManager.LoadScene(level1SceneName);
        }
    }

    public void LoadLevel2()
    {
        if (LevelProgressSave.IsLevelUnlocked(2))
        {
            SceneManager.LoadScene(level2SceneName);
        }
    }
}