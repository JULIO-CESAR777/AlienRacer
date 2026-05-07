using System.Collections;
using UnityEngine;

public class ResultScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private MenuCameraController _menuCameraController;
    
    [Header("Victory Fireworks")]
    [SerializeField] private float _timeBetweenFireworks = 0.4f;
    [SerializeField] private float _spawnRadius = 3f;
    [SerializeField] private int _maxFireworks = 20;
    
    private Coroutine _fireworksCoroutine;

    private void Start()
    {
        var result = GameResultManager.Instance.CurrentResult;

        winPanel.SetActive(result  == GameResultManager.RaceResult.Win);
        losePanel.SetActive(result == GameResultManager.RaceResult.Lose);
        
        if (result == GameResultManager.RaceResult.Win)
            _fireworksCoroutine = StartCoroutine(PlayVictoryFireworks());

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
    private IEnumerator PlayVictoryFireworks()
    {
        for (int i = 0; i < _maxFireworks; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-_spawnRadius, _spawnRadius),
                Random.Range(-_spawnRadius, _spawnRadius),
                0f
            );

            VFXController.GetInstance()?.SpawnVictoryVFX(transform.position + randomOffset);
            yield return new WaitForSeconds(_timeBetweenFireworks);
        }
    }

    private void OnDisable()
    {
        if (_fireworksCoroutine != null)
            StopCoroutine(_fireworksCoroutine);
    }
}
