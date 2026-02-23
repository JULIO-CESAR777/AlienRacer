using UnityEngine;

[DisallowMultipleComponent]
public class SoundController : MonoBehaviour
{
    public static SoundController Instance { get; private set; }

    private void Awake()
    {
        //TODO: If music is not persistent across scenes, remove the duplicate check
        // (if Instance != null && Instance != this) and DontDestroyOnLoad(gameObject) and just set Instance = this;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static SoundController GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogError("SoundController instance is null! Make sure it exists in the scene.");
        }
        return Instance;
    }
}
