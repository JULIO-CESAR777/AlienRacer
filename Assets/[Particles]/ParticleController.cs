// HOW TO CALL FUNCTIONS FROM THIS CONTROLLER:
// Instead of using Instance directly, always use GetInstance() for safety.
//
// Example:
//      ParticleController.GetInstance()?.NameOfTheFunction();
//
// The ?. (null-conditional operator) ensures that if the controller is missing
// from the scene, the call is simply skipped instead of crashing the game.
// You will also see a descriptive error in the Console telling you what's missing.
//
// NEVER do this:
//      ParticleController.Instance.NameOfTheFunction(); // can throw a NullReferenceException

using UnityEngine;

[DisallowMultipleComponent]
public class ParticleController : MonoBehaviour
{
    public static ParticleController Instance { get; private set; }

    private void Awake()
    {
        // TODO: If particles are persistent across scenes, add back the duplicate
        //  check (if Instance != null && Instance != this) and DontDestroyOnLoad(gameObject)
        Instance = this;
    }

    public static ParticleController GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogError("ParticleController instance is null! Make sure it exists in the scene.");
        }

        return Instance;
    }
}