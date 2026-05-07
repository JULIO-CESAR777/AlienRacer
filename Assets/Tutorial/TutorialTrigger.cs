using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private string Mecanica;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica que sea el jugador
        if (other.CompareTag("Player"))
        {
            TutorialManager.instance.MostrarTutorial(Mecanica);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialManager.instance.OcultarTutorial();
        }
    }
}