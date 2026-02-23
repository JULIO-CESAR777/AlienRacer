using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        private void Awake()
        {
            // TODO: If the UI is a persistent HUD across scenes, add back the duplicate 
            //  check (if Instance != null && Instance != this) and DontDestroyOnLoad(gameObject)
            Instance = this;
        }

        public static UIController GetInstance()
        {
            if (Instance == null)
            {
                Debug.LogError("UIController instance is null! Make sure it exists in the scene.");
            }
            return Instance;
        }
    }
}
