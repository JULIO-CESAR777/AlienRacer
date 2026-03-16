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
        [SerializeField] private GameObject _winMenuPanel = default;
        [SerializeField] private GameObject _loseMenuPanel = default;
        [SerializeField] private GameObject _settingsPanel = default;
        [SerializeField] private GameObject _pausePanel = default;

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
                Debug.LogWarning("UIController instance is null! Make sure it exists in the scene.");
            }
            return Instance;
        }
        
        public void Play()
        {
            Debug.Log("Play button clicked!");
            _mainMenuPanel.SetActive(false);
            SceneLoader.GetInstance()?.LoadScene(1);
        }

        public void ToSettingsPanel()
        {
            Debug.Log("To settings panel clicked!");
            _mainMenuPanel.SetActive(false);
            _settingsPanel.SetActive(true);
        }
        
        public void ExitGameUIButton()
        {
            Debug.Log("Exit game button clicked!");
            Application.Quit();
        }

        public void FromWinToMainMenu()
        {
            Debug.Log("Changing from Win Menu to Main Menu Panel");
            _winMenuPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
        }

        public void ReloadButton()
        {
            Debug.Log(" Reload button clicked!");
            SceneLoader.GetInstance()?.LoadScene(1);
        }

        public void FromLoseToMainMenu()
        {
            Debug.Log("Changing from Lose Menu to Main Menu Panel");
            _loseMenuPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
        }
        public void ToUIScene()
        {
            SceneLoader.GetInstance()?.LoadScene("Mapa1_V2");
        }

        public void UIPause()
        {
            _pausePanel.SetActive(true);
        }

        public void FromUIPauseToPlay()
        {
            _pausePanel.SetActive(false);
        }

        public void FromPauseToSettings()
        {
            UINavigationController.GetInstance()?.SetDestination(UINavigationController.UIDestination.Settings);
            SceneLoader.GetInstance()?.LoadScene(0);
        }
    }
}
