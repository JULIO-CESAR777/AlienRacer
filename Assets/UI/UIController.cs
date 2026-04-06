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
            // Asignación de instancia para el Singleton
            Instance = this;
        }

        public static UIController GetInstance()
        {
            if (Instance == null)
            {
                Debug.LogWarning("UIController instance is null! Asegúrate de que esté en la escena.");
            }
            return Instance;
        }

        // --- MÉTODOS DE NAVEGACIÓN Y MENÚ ---

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
            _winMenuPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
        }

        public void ReloadButton()
        {
            SceneLoader.GetInstance()?.LoadScene(1);
        }

        public void FromLoseToMainMenu()
        {
            _loseMenuPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
        }

        /// <summary>
        /// Este método es vital para GestorPosiciones. 
        /// Carga la escena principal o de interfaz (Scene 0).
        /// </summary>
        public void ToUIScene()
        {
            SceneLoader.GetInstance()?.LoadScene(0);
        }

        // --- MÉTODOS DE PAUSA ---

        public void UIPause()
        {
            if (_pausePanel != null) _pausePanel.SetActive(true);
        }

        public void FromUIPauseToPlay()
        {
            if (_pausePanel != null) _pausePanel.SetActive(false);
        }

        public void FromPauseToSettings()
        {
            UINavigationController.GetInstance()?.SetDestination(UINavigationController.UIDestination.Settings);
            SceneLoader.GetInstance()?.LoadScene(0);
        }
    }
}