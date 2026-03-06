using UnityEngine;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance { get; private set; }

    public enum RaceResult { None, Win, Lose }
    public RaceResult CurrentResult { get; private set; } = RaceResult.None;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static GameResultManager GetInstance()
    {
        if (Instance == null)
            Debug.LogError("GameResultManager instance is null! Make sure it exists in the Boot Scene.");
        return Instance;
    }

    public void SetResult(RaceResult result) => CurrentResult = result;
    public void ClearResult()               => CurrentResult = RaceResult.None;
    
}
