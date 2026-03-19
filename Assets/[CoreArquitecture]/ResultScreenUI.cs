using UnityEngine;

public class ResultScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private MenuCameraController _menuCameraController;

    private void Start()
    {
        var result = GameResultManager.Instance.CurrentResult;

        winPanel.SetActive(result  == GameResultManager.RaceResult.Win);
        losePanel.SetActive(result == GameResultManager.RaceResult.Lose);

        GameResultManager.GetInstance()?.ClearResult(); // Clean up for next race
        
        // Navigation - Settings - from PauseUI to UI Scene - Settings Panel.
        var destination = UINavigationController.GetInstance()?.CurrentDestination;
        if (destination == UINavigationController.UIDestination.Settings)
        {
            Debug.Log("Woooked");
            _menuCameraController?.GoToSettings();
        }
        UINavigationController.Instance.ClearDestination();
    }
}
