using UnityEngine;

public class ResultScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private void Start()
    {
        var result = GameResultManager.Instance.CurrentResult;

        winPanel.SetActive(result  == GameResultManager.RaceResult.Win);
        losePanel.SetActive(result == GameResultManager.RaceResult.Lose);

        GameResultManager.GetInstance()?.ClearResult(); // Clean up for next race
    }
}
