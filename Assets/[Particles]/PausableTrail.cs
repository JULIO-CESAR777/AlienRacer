using UnityEngine;

public class PausableTrail : MonoBehaviour
{
    private ParticleSystem ps;
    private bool wasPaused = false;
    private MainManager gm;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        gm = MainManager.GetInstance();
    }

    private void Update()
    {
        if (gm == null) return;

        bool isPaused = gm.gameState != GameState.Play;

        if (isPaused && !wasPaused)
        {
            ps.Pause();
            wasPaused = true;
        }
        else if (!isPaused && wasPaused)
        {
            ps.Play();
            wasPaused = false;
        }
    }
}
