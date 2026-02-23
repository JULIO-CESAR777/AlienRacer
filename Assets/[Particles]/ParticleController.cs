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