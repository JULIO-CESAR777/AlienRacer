// HOW TO CALL FUNCTIONS FROM THIS CONTROLLER:
// Instead of using Instance directly, always use GetInstance() for safety.
//
// Example:
//      SoundController.GetInstance()?.NameOfTheFunction();
//
// The ?. (null-conditional operator) ensures that if the controller is missing
// from the scene, the call is simply skipped instead of crashing the game.
// You will also see a descriptive error in the Console telling you what's missing.
//
// NEVER do this:
//      SoundController.Instance.NameOfTheFunction(); // can throw a NullReferenceException

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
