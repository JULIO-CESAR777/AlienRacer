using UnityEngine;

[DisallowMultipleComponent]
public class AnimationController : MonoBehaviour
{
    public static AnimationController Instance { get; private set; }

    private void Awake()
    {
        // TODO: If animations are persistent across scenes, add back the duplicate
        //  check (if Instance != null && Instance != this) and DontDestroyOnLoad(gameObject)
        Instance = this;
    }

    public static AnimationController GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogError("AnimationController instance is null! Make sure it exists in the scene.");
        }
        return Instance;
    }
}