using UnityEngine;

[DisallowMultipleComponent]
public class UINavigationController : MonoBehaviour
{
    public static UINavigationController Instance { get; private set; }

    public enum UIDestination { None, Settings, MainMenu }
    public UIDestination CurrentDestination { get; private set; } = UIDestination.None;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static UINavigationController GetInstance()
    {
        if (Instance == null)
            Debug.LogError("UINavigationManager instance is null! Make sure it exists in the Boot Scene.");
        return Instance;
    }

    public void SetDestination(UIDestination destination) => CurrentDestination = destination;
    public void ClearDestination()                        => CurrentDestination = UIDestination.None;
}
