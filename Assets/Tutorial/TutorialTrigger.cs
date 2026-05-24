using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial UI")]
    [SerializeField] private string Mecanica;

    [Header("Registrar Zona")]
    [SerializeField] private bool registerZoneOnEnter = false;

    [SerializeField] private string zoneID;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log($"[TutorialTrigger] Player entró al trigger: {gameObject.name}");

        // =====================================
        // MOSTRAR TUTORIAL
        // =====================================

        if (!string.IsNullOrEmpty(Mecanica))
        {
            Debug.Log($"[TutorialTrigger] Mostrando tutorial: {Mecanica}");

            TutorialManager.instance.MostrarTutorial(Mecanica);
        }

        // =====================================
        // REGISTRAR ZONA
        // =====================================

        if (registerZoneOnEnter)
        {
            Debug.Log($"[TutorialTrigger] Registrando zona: {zoneID}");

            TutorialManager.instance.CompleteTutorial(zoneID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[TutorialTrigger] Player salió del trigger: {gameObject.name}");

            if (!string.IsNullOrEmpty(Mecanica))
            {
                TutorialManager.instance.OcultarTutorial();
            }
        }
    }
}