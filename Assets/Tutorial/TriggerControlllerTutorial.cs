using UnityEngine;

public class TriggerControladorTutorial : MonoBehaviour
{
    [Header("Configuración de la UI")]
    [Tooltip("El nombre exacto de la mecánica configurada en tu TutorialManager (ej: 'MecanicaTurbo')")]
    [SerializeField] private string nombreMecanicaUI;

    [Header("Configuración de la Gacha")]
    [Tooltip("¿Este trigger obliga a la gacha a dar un ítem específico del tutorial?")]
    [SerializeField] private bool activarPoolTutorial = true;

    [Tooltip("El índice del ítem en la lista 'tutorialOrder' de la Gacha (0 = Primer ítem, 1 = Segundo, etc.)")]
    [SerializeField] private int indiceItemGacha = 0;

    private void OnTriggerEnter(Collider other)
    {
        // Detectar si el carro del jugador entró al trigger
        if (other.CompareTag("Player"))
        {
            // 1. Abre el Canvas de UI usando tu TutorialManager original
            if (TutorialManager.instance != null && !string.IsNullOrEmpty(nombreMecanicaUI))
            {
                TutorialManager.instance.MostrarTutorial(nombreMecanicaUI);
            }

            // 2. Configura el comportamiento de la Gacha del jugador
            CoinGachaBuyer gacha = other.GetComponent<CoinGachaBuyer>();
            if (gacha != null)
            {
                gacha.ConfigurarGachaTutorial(activarPoolTutorial, indiceItemGacha);
            }
        }
    }
}