// HOW TO CALL FUNCTIONS FROM THIS CONTROLLER:
// Instead of using Instance directly, always use GetInstance() for safety.
//
// Example:
//      UIController.GetInstance()?.NameOfTheFunction();
//
// The ?. (null-conditional operator) ensures that if the controller is missing
// from the scene, the call is simply skipped instead of crashing the game.
// You will also see a descriptive error in the Console telling you what's missing.
//
// NEVER do this:
//      UIController.Instance.NameOfTheFunction(); // can throw a NullReferenceException

using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [Header("Main Menu UI Panels")]
        [SerializeField] private GameObject _mainMenuPanel = default;

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
        
        public void Play()
        {
            Debug.Log("Play button clicked!");
            _mainMenuPanel.SetActive(false);
        }

        public void ToSettingsPanel()
        {
            Debug.Log("To settings panel clicked!");
        }
        
        public void ExitGameUIButton()
        {
            Debug.Log("Exit game button clicked!");
            Application.Quit();
        }
    }
}
